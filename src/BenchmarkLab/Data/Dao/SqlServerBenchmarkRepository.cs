using JetBrains.Annotations;
using MeasureThat.Net.Models;
using System.Collections.Generic;

namespace MeasureThat.Net.Data.Dao
{
    using BenchmarkLab.Data.Models;
    using BenchmarkLab.Models;
    using Exceptions;
    using Logic.Validation;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class SqlServerBenchmarkRepository
    {
        const string titles_cache_key = "titles";
        private readonly ApplicationDbContext m_db;
        private readonly IMemoryCache memoryCache;

        public SqlServerBenchmarkRepository([NotNull] ApplicationDbContext db, [NotNull] IMemoryCache memoryCache)
        {
            this.m_db = db;
            this.memoryCache = memoryCache;
        }

        public virtual async Task<long> Add([NotNull] BenchmarkDto entity)
        {
            var newEntity = new Benchmark()
            {
                Name = entity.BenchmarkName,
                Description = entity.Description,
                OwnerId = entity.OwnerId,
                HtmlPreparationCode = entity.HtmlPreparationCode,
                ScriptPreparationCode = entity.ScriptPreparationCode,
                BenchmarkTests = new List<BenchmarkTest>(),
                WhenCreated = DateTime.UtcNow,
            };

            foreach (var test in entity.TestCases)
            {
                var newTest = new BenchmarkTest()
                {
                    TestName = test.TestCaseName,
                    BenchmarkText = test.BenchmarkCode,
                };
                newEntity.BenchmarkTests.Add(newTest);
            }

            this.Validate(newEntity);

            this.m_db.Benchmarks.Add(newEntity);
            await this.m_db.SaveChangesAsync().ConfigureAwait(false);

            InvalidateCache();

            return newEntity.Id;
        }

        public virtual async Task<long> DeleteById(long id, ApplicationUser user)
        {
            var entity = await this.m_db.Benchmarks
                .SingleOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            if (entity != null)
            {
                this.m_db.Benchmarks.Where(a => a.Id == id && a.OwnerId == user.Id).ExecuteDelete();
            }

            return id;
        }

        public virtual async Task<BenchmarkDto> FindById(long id)
        {
            var entity = await this.m_db.Benchmarks
                .Include(b => b.BenchmarkTests)
                .Include(b => b.GenAidescriptions)
                .FirstOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            if (entity == null)
            {
                return null;
            }

            var result = DbEntityToModel(entity);

            return result;
        }

        public virtual async Task<IList<BenchmarkDto>> ListAllForSitemap(int maxEntities, int page)
        {
            Preconditions.ToBePositive(maxEntities);
            Preconditions.ToBeNonNegative(page);

            var entities = await this.m_db.Benchmarks
                .OrderByDescending(t => t.WhenCreated)
                .Skip(maxEntities * page)
                .Take(maxEntities)
                .ToListAsync()
                .ConfigureAwait(false);

            return ProcessQueryResult(entities);
        }

        // Returns total number of benchmarks
        public virtual async Task<int> CountAll()
        {
            return await this.m_db.Benchmarks.CountAsync();
        }

        // Returns total number of benchmarks for the given user
        public virtual async Task<int> CountUserBenchmarks(string userId)
        {
            Preconditions.NonEmptyString(userId);
            return await this.m_db.Benchmarks.CountAsync(t => t.OwnerId == userId);
        }

        // Returns list of benchmarks just for the index (title and when created.)
        public virtual async Task<IList<BenchmarkDtoForIndex>> ListAllForIndex(int maxEntities, int page)
        {
            Preconditions.ToBePositive(maxEntities);
            Preconditions.ToBeNonNegative(page);

            var entities = await this.m_db.Benchmarks
                .OrderByDescending(t => t.WhenCreated)
                .Skip(maxEntities * page)
                .Take(maxEntities)
                .Select(x => new { x.Id, x.Name, x.Description, x.WhenCreated, x.Version, x.OwnerId })
                .ToListAsync()
                .ConfigureAwait(false);

            var result = new List<BenchmarkDtoForIndex>();
            foreach (var item in entities)
            {
                result.Add(new BenchmarkDtoForIndex
                {
                    Id = item.Id,
                    BenchmarkName = item.Name,
                    Description = item.Description,
                    WhenCreated = item.WhenCreated,
                    Version = item.Version,
                    OwnerId = item.OwnerId
                });
            }


            return result;
        }

        private void Validate(Benchmark newEntity)
        {
            if (newEntity == null)
            {
                throw new ValidationException("New entity is null");
            }

            if (string.IsNullOrWhiteSpace(newEntity.Name))
            {
                throw new ValidationException("Benchmark name is mandatory");
            }

            if (newEntity.BenchmarkTests == null || newEntity.BenchmarkTests.Count == 0)
            {
                throw new ValidationException("Test cases were not specified");
            }

            foreach (BenchmarkTest benchmarkTest in newEntity.BenchmarkTests)
            {
                if (benchmarkTest == null)
                {
                    throw new ValidationException("Test Case is empty");
                }

                if (string.IsNullOrWhiteSpace(benchmarkTest.BenchmarkText))
                {
                    throw new ValidationException("Test case does not have test definition (code filed is empty)");
                }

                if (string.IsNullOrWhiteSpace(benchmarkTest.TestName))
                {
                    throw new ValidationException("Test case name is mandatory");
                }
            }
        }

        public static BenchmarkDto DbEntityToModel([NotNull] Benchmark entity)
        {
            var result = new BenchmarkDto()
            {
                Id = entity.Id,
                BenchmarkName = entity.Name,
                Description = entity.Description,
                HtmlPreparationCode = entity.HtmlPreparationCode,
                OwnerId = entity.OwnerId,
                ScriptPreparationCode = entity.ScriptPreparationCode,
                TestCases = new List<TestCaseDto>(),
                WhenCreated = entity.WhenCreated,
                WhenUpdated = entity.WhenUpdated,
                Version = entity.Version,
                RelatedIds = entity.RelatedBenchmarks,
                LLMSummaries = new List<BenchmarkLab.Data.Models.GenAidescription>()
            };

            foreach (var test in entity.BenchmarkTests)
            {
                var testCase = new TestCaseDto()
                {
                    TestCaseName = test.TestName,
                    BenchmarkCode = test.BenchmarkText
                };
                result.TestCases.Add(testCase);
            }

            if (entity.GenAidescriptions != null)
            {
                foreach (var llmSummary in entity.GenAidescriptions)
                {
                    result.LLMSummaries.Add(llmSummary);
                }
            }

            return result;
        }

        public virtual async Task<IList<BenchmarkDtoForIndex>> ListByUser(string userId, int page, int numOfItems)
        {
            var entities = await this.m_db.Benchmarks
                .Where(t => t.OwnerId == userId)
                .Include(b => b.BenchmarkTests)
                .Skip(page * numOfItems)
                .Take(numOfItems)
                .OrderByDescending(b => b.WhenCreated)
                .Select(x => new { x.Id, x.Name, x.Description, x.WhenCreated, x.Version, x.OwnerId })
                .ToListAsync()
                .ConfigureAwait(false);

            var result = new List<BenchmarkDtoForIndex>();
            foreach (var item in entities)
            {
                result.Add(new BenchmarkDtoForIndex
                {
                    Id = item.Id,
                    BenchmarkName = item.Name,
                    Description = item.Description,
                    WhenCreated = item.WhenCreated,
                    Version = item.Version,
                    OwnerId = item.OwnerId
                });
            }

            return result;
        }

        public async Task<BenchmarkDto> Update([NotNull] BenchmarkDto model,
            [NotNull] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UserIdEmptyException("User Id is empty");
            }

            var entity = await this.m_db.Benchmarks
                .Include(m => m.BenchmarkTests)
                .FirstOrDefaultAsync(m => m.Id == model.Id && m.OwnerId == userId)
                .ConfigureAwait(false);

            if (entity == null)
            {
                throw new UnableToFindBenchmarkException("Unable to find benchmark by Id and owner id");
            }

            if (entity.BenchmarkTests == null)
            {
                // Just sanity check
                throw new ValidationException("Empty test collection");
            }

            entity.Version++;
            entity.Description = model.Description;
            entity.Name = model.BenchmarkName;
            entity.HtmlPreparationCode = model.HtmlPreparationCode;
            entity.ScriptPreparationCode = model.ScriptPreparationCode;

            var entityTestsList = entity.BenchmarkTests.ToList();
            if (entityTestsList.Count() > model.TestCases.Count)
            {
                // Remove extra test cases from the entity
                for (int i = model.TestCases.Count; i < entity.BenchmarkTests.Count; i++)
                {
                    this.m_db.BenchmarkTests.Remove(entityTestsList[i]);
                    entity.BenchmarkTests.Remove(entityTestsList[i]);
                }
            }
            else if (entityTestsList.Count() < model.TestCases.Count)
            {
                for (int i = entity.BenchmarkTests.Count; i < model.TestCases.Count; i++)
                {
                    entity.BenchmarkTests.Add(new BenchmarkTest());
                }
            }


            // Now both collections of test cases should have same number of elements
            int index = 0;
            foreach (var benchmarkTest in entity.BenchmarkTests)
            {
                benchmarkTest.BenchmarkText = model.TestCases[index].BenchmarkCode;
                benchmarkTest.TestName = model.TestCases[index].TestCaseName;
                index++;
            }

            this.m_db.Benchmarks.Update(entity);
            await this.m_db.SaveChangesAsync().ConfigureAwait(false);

            InvalidateCache();

            return DbEntityToModel(entity);
        }

        public async Task<Dictionary<string, long>> GetTitles()
        {
            Dictionary<string, long> result;
            if (this.memoryCache.TryGetValue(titles_cache_key, out result))
            {
                return result;
            }

            var entities = await this.m_db.Benchmarks
               .OrderByDescending(b => b.WhenCreated)
               .Select(x => new { x.Id, x.Name })
               .ToListAsync()
               .ConfigureAwait(false);
            result = new Dictionary<string, long>();
            foreach (var entity in entities)
            {
                result[entity.Name.ToLower()] = entity.Id;
            }

            var expirationOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(1));
            this.memoryCache.Set(titles_cache_key, result, expirationOptions);
            return result;
        }

        private void InvalidateCache()
        {
            this.memoryCache.Remove(titles_cache_key);
        }

        private static IList<BenchmarkDto> ProcessQueryResult(IEnumerable<Benchmark> entities)
        {
            var result = new List<BenchmarkDto>();
            foreach (var benchmark in entities)
            {
                BenchmarkDto model = DbEntityToModel(benchmark);
                result.Add(model);
            }

            return result;
        }
    }
}
