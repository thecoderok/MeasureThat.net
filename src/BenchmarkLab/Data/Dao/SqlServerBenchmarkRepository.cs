using System;
using System.Collections.Generic;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using BenchmarkLab.Data.Models;

namespace BenchmarkLab.Data.Dao
{
    using System.Linq;
    using System.Threading.Tasks;
    using Logic.Exceptions;
    using Microsoft.EntityFrameworkCore;

    public class SqlServerBenchmarkRepository :  IEntityRepository<NewBenchmarkModel, long>
    {
        private readonly ApplicationDbContext m_db;

        public SqlServerBenchmarkRepository([NotNull] ApplicationDbContext db)
        {
            this.m_db = db;
        }

        public async Task<long> Add([NotNull] NewBenchmarkModel entity)
        {
            var newEntity = new Benchmark()
            {
                Name = entity.BenchmarkName,
                Description = entity.Description,
                OwnerId = entity.OwnerId,
                HtmlPreparationCode = entity.HtmlPreparationCode,
                ScriptPreparationCode = entity.ScriptPreparationCode,
                BenchmarkTest = new List<BenchmarkTest>()
            };

            foreach (var test in entity.TestCases)
            {
                var newTest = new BenchmarkTest()
                {
                    TestName = test.TestCaseName,
                    BenchmarkText = test.BenchmarkCode,
                };
                newEntity.BenchmarkTest.Add(newTest);
            }

            this.Validate(newEntity);

            this.m_db.Benchmark.Add(newEntity);
            await this.m_db.SaveChangesAsync();

            return newEntity.Id;
        }

        public async Task<long> DeleteById(long id)
        {
            var entity = await this.m_db.Benchmark.SingleOrDefaultAsync(m => m.Id == id);
            if (entity != null)
            {
                this.m_db.Benchmark.Remove(entity);
                await this.m_db.SaveChangesAsync();
            }

            return id;
        }

        public async Task<NewBenchmarkModel> FindById(long id)
        {
            var entity = await this.m_db.Benchmark.Include(b => b.BenchmarkTest).FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null)
            {
                return null;
            }

            var result = DbEntityToModel(entity);

            return result;
        }

        public async Task<IEnumerable<NewBenchmarkModel>> ListAll(uint maxEntities)
        {
            var entities = await this.m_db.Benchmark
                .Include(b => b.BenchmarkTest)
                .Take((int)maxEntities)
                .ToListAsync();
            var result = new List<NewBenchmarkModel>();
            foreach (var benchmark in entities)
            {
                NewBenchmarkModel model = DbEntityToModel(benchmark);
                result.Add(model);
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

            if (string.IsNullOrWhiteSpace(newEntity.OwnerId))
            {
                throw new ValidationException("Benchmark must have owner");
            }

            if (newEntity.BenchmarkTest == null || newEntity.BenchmarkTest.Count == 0)
            {
                throw new ValidationException("Test cases were not specified");
            }

            foreach (BenchmarkTest benchmarkTest in newEntity.BenchmarkTest)
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

        public static NewBenchmarkModel DbEntityToModel([NotNull] Benchmark entity)
        {
            var result = new NewBenchmarkModel()
            {
                Id = entity.Id,
                BenchmarkName = entity.Name,
                Description = entity.Description,
                HtmlPreparationCode = entity.HtmlPreparationCode,
                OwnerId = entity.OwnerId,
                ScriptPreparationCode = entity.ScriptPreparationCode,
                TestCases = new List<TestCase>()
            };

            foreach (var test in entity.BenchmarkTest)
            {
                var testCase = new TestCase()
                {
                    TestCaseName = test.TestName,
                    BenchmarkCode = test.BenchmarkText
                };
                result.TestCases.Add(testCase);
            }
            return result;
        }

        public async Task<IEnumerable<NewBenchmarkModel>> ListByUser(uint maxEntities, string userId)
        {
            var entities = await this.m_db.Benchmark
                .Where(t=> t.OwnerId == userId)
                .Include(b => b.BenchmarkTest)
                .Take((int)maxEntities)
                .ToListAsync();
            var result = new List<NewBenchmarkModel>();
            foreach (var benchmark in entities)
            {
                NewBenchmarkModel model = DbEntityToModel(benchmark);
                result.Add(model);
            }

            return result;
        }
    }
}
