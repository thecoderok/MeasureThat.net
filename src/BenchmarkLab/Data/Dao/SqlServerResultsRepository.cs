using BenchmarkLab.Data.Models;
using JetBrains.Annotations;
using MeasureThat.Net.Exceptions;
using MeasureThat.Net.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeasureThat.Net.Data.Dao
{
    public class RankedTest
    {
        public long ResultId
        {
            get; set;
        }
        public string TestName
        {
            get; set;
        }
        public string Browser
        {
            get; set;
        }
        public int Rank
        {
            get; set;
        }
    }

    public class FastestCountResult
    {
        public string Browser
        {
            get; set;
        }
        public string TestName
        {
            get; set;
        }
        public int FastestCount
        {
            get; set;
        }
    }

    public class SqlServerResultsRepository
    {
        private readonly ApplicationDbContext m_db;

        public SqlServerResultsRepository([NotNull] ApplicationDbContext db)
        {
            this.m_db = db;
        }

        public virtual async Task<long> Add(BenchmarkResultDto entity)
        {
            var newEntity = new Result()
            {
                BenchmarkId = entity.BenchmarkId,
                Browser = entity.Browser,
                DevicePlatform = entity.DevicePlatform,
                OperatingSystem = entity.OS,
                RawUastring = entity.RawUserAgenString,
                UserId = entity.UserId,
                ResultRows = new List<ResultRow>(),
                Version = entity.BenchmarkVersion,
                Created = DateTime.UtcNow
            };

            foreach (var row in entity.ResultRows)
            {
                var dbrow = new ResultRow()
                {
                    ExecutionsPerSecond = row.ExecutionsPerSecond,
                    TestName = row.TestName,
                    NumberOfSamples = row.NumberOfSamples,
                    RelativeMarginOfError = row.RelativeMarginOfError
                };
                newEntity.ResultRows.Add(dbrow);
            }

            this.Validate(newEntity);

            this.m_db.Results.Add(newEntity);
            await this.m_db.SaveChangesAsync().ConfigureAwait(false);

            return newEntity.Id;
        }

        public virtual async Task<long> DeleteById(long id)
        {
            var entity = await this.m_db.Results.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (entity != null)
            {
                this.m_db.Results.Remove(entity);
                await this.m_db.SaveChangesAsync().ConfigureAwait(false);
            }

            return id;
        }

        public virtual async Task<BenchmarkResultDto> FindById(long id)
        {
            var entity = await this.m_db.Results
                .Include(b => b.ResultRows)
                .FirstOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            if (entity == null)
            {
                return null;
            }

            var result = DbEntityToModel(entity);

            return result;
        }

        public virtual async Task<ShowResultModel> GetResultWithBenchmark(long id)
        {
            var entity = await this.m_db.Results
                .Include(b => b.ResultRows)
                .Include(b => b.Benchmark)
                .Include(b => b.Benchmark.BenchmarkTests)
                .FirstOrDefaultAsync(m => m.Id == id)
                .ConfigureAwait(false);
            if (entity == null)
            {
                return null;
            }

            BenchmarkResultDto result = DbEntityToModel(entity);
            BenchmarkDto benchmark = SqlServerBenchmarkRepository.DbEntityToModel(entity.Benchmark);

            return new ShowResultModel(result, benchmark);
        }

        public virtual async Task<BenchmarkResultDto> GetLatestResultForBenchmark(long benchmarkId)
        {
            var entity = await this.m_db.Results
                .OrderByDescending(m => m.Created)
                .Include(b => b.ResultRows)
                .FirstOrDefaultAsync(m => m.BenchmarkId == benchmarkId)
                .ConfigureAwait(false);
            if (entity == null)
            {
                return null;
            }

            BenchmarkResultDto result = DbEntityToModel(entity);

            return result;
        }

        public virtual async Task<List<Benchmark>> GetBenchmarksByIds(HashSet<long> benchmarksIds)
        {
            var list = await this.m_db.Benchmarks
                .Where(t => benchmarksIds.Contains(t.Id))
                .ToListAsync()
                .ConfigureAwait(false);

            return list;
        }

        /// <summary>
        /// Returns total number of results for given benchmark
        /// Total # is needed to create pagination
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IList<BenchmarkResultDto>> ListAll(int benchmarkId)
        {
            var list = await this.m_db.Results
                .Where(t => t.BenchmarkId == benchmarkId)
                .OrderByDescending(t => t.Created)
                .ToListAsync()
                .ConfigureAwait(false);

            return ProcessQueryResult(list);
        }

        public List<FastestCountResult> GetFastestCountsPerBrowser(long benchmarkId)
        {
            var rankedTests = m_db.ResultRows
                .Where(rr => rr.Result.BenchmarkId == benchmarkId)
                .GroupBy(rr => new { rr.ResultId, rr.TestName, rr.Result.Browser })
                .Select(g => new RankedTest
                {
                    ResultId = g.Key.ResultId,
                    TestName = g.Key.TestName,
                    Browser = g.Key.Browser,
                    Rank = g.OrderByDescending(rr => rr.ExecutionsPerSecond)
                            .Select((rr, index) => new { rr, index })
                            .FirstOrDefault().index + 1
                });

            var fastestCounts = rankedTests
                .GroupBy(rt => new { rt.Browser, rt.TestName })
                .Select(g => new FastestCountResult
                {
                    Browser = g.Key.Browser,
                    TestName = g.Key.TestName,
                    FastestCount = g.Count(rt => rt.Rank == 1)
                })
                .OrderBy(fc => fc.Browser)
                .ThenByDescending(fc => fc.TestName)
                .ToList();

            return fastestCounts;
        }


        public List<FastestCountResult> GetFastestCountsByTestName(long benchmarkId)
        {
            var rankedTests = m_db.ResultRows
                .Where(rr => rr.Result.BenchmarkId == benchmarkId)
                .GroupBy(rr => new { rr.ResultId, rr.TestName })
                .SelectMany(g => g
                    .OrderByDescending(rr => rr.ExecutionsPerSecond)
                    .Select((rr, index) => new
                    {
                        rr.ResultId,
                        rr.TestName,
                        Rank = index + 1
                    })
                );

            var result = rankedTests
                .Where(rt => rt.Rank == 1)
                .GroupBy(rt => rt.TestName)
                .Select(g => new FastestCountResult
                {
                    TestName = g.Key,
                    FastestCount = g.Count()
                })
                .OrderByDescending(fc => fc.FastestCount)
                .ToList();

            return result;
        }


        private IList<BenchmarkResultDto> ProcessQueryResult(IEnumerable<Result> entities)
        {
            var result = new List<BenchmarkResultDto>();
            foreach (var benchmark in entities)
            {
                BenchmarkResultDto model = DbEntityToModel(benchmark);
                result.Add(model);
            }

            return result;
        }

        private BenchmarkResultDto DbEntityToModel([NotNull] Result entity)
        {
            var result = new BenchmarkResultDto()
            {
                Id = entity.Id,
                BenchmarkId = entity.BenchmarkId,
                Browser = entity.Browser,
                DevicePlatform = entity.DevicePlatform,
                OS = entity.OperatingSystem,
                RawUserAgenString = entity.RawUastring,
                UserId = entity.UserId,
                ResultRows = new List<ResultsRowModel>(),
                WhenCreated = entity.Created
            };

            foreach (var row in entity.ResultRows)
            {
                var rowModel = new ResultsRowModel()
                {
                    ExecutionsPerSecond = row.ExecutionsPerSecond,
                    NumberOfSamples = row.NumberOfSamples,
                    RelativeMarginOfError = row.RelativeMarginOfError,
                    TestName = row.TestName
                };
                result.ResultRows.Add(rowModel);
            }
            return result;
        }

        private void Validate(Result newEntity)
        {
            if (newEntity.ResultRows.Count == 0)
            {
                throw new ValidationException("ResultRow count = 0");
            }
        }
    }
}
