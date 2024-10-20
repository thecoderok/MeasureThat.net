using System.Collections.Generic;
using System.Threading.Tasks;
using MeasureThat.Net.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using MeasureThat.Net.Data.Models;
using System.Linq;
using MeasureThat.Net.Exceptions;
using System;
using BenchmarkLab.Data.Models;

namespace MeasureThat.Net.Data.Dao
{
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
                ResultRow = new List<ResultRow>(),
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
                newEntity.ResultRow.Add(dbrow);
            }

            this.Validate(newEntity);

            this.m_db.Result.Add(newEntity);
            await this.m_db.SaveChangesAsync().ConfigureAwait(false);

            return newEntity.Id;
        }

        public virtual async Task<long> DeleteById(long id)
        {
            var entity = await this.m_db.Result.SingleOrDefaultAsync(m => m.Id == id).ConfigureAwait(false);
            if (entity != null)
            {
                this.m_db.Result.Remove(entity);
                await this.m_db.SaveChangesAsync().ConfigureAwait(false);
            }

            return id;
        }

        public virtual async Task<BenchmarkResultDto> FindById(long id)
        {
            var entity = await this.m_db.Result
                .Include(b => b.ResultRow)
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
            var entity = await this.m_db.Result
                .Include(b => b.ResultRow)
                .Include(b => b.Benchmark)
                .Include(b => b.Benchmark.BenchmarkTest)
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
            var entity = await this.m_db.Result
                .OrderByDescending(m => m.Created)
                .Include(b => b.ResultRow)
                .FirstOrDefaultAsync(m => m.BenchmarkId == benchmarkId)
                .ConfigureAwait(false);
            if (entity == null)
            {
                return null;
            }

            BenchmarkResultDto result = DbEntityToModel(entity);

            return result;
        }

        public virtual async Task<GenAidescription> GetGenAISummary(long benchmarkId)
        {
            const string defaultModel = "llama3.1:latest";
            var entity = await this.m_db.GenAidescription
                .FirstOrDefaultAsync(m => m.BenchmarkId == benchmarkId && m.Model == defaultModel)
                .ConfigureAwait(false);
            return entity;
        }

        public virtual async Task<List<Benchmark>> GetBenchmarksByIds(HashSet<long> benchmarksIds)
        {
            var list = await this.m_db.Benchmark
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
            var list = await this.m_db.Result
                .Where(t => t.BenchmarkId == benchmarkId)
                .OrderByDescending(t => t.Created)
                .ToListAsync()
                .ConfigureAwait(false);

            return ProcessQueryResult(list);
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

            foreach(var row in entity.ResultRow)
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
            if (newEntity.ResultRow.Count == 0)
            {
                throw new ValidationException("ResultRow count = 0");
            }
        }
    }
}
