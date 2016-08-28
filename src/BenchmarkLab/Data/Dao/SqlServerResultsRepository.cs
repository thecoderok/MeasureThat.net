using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using BenchmarkLab.Data.Models;
using System.Linq;
using BenchmarkLab.Logic.Exceptions;

namespace BenchmarkLab.Data.Dao
{
    public class SqlServerResultsRepository : IResultsRepository
    {
        private readonly ApplicationDbContext m_db;

        public SqlServerResultsRepository([NotNull] ApplicationDbContext db)
        {
            this.m_db = db;
        }

        public virtual async Task<long> Add(PublishResultsModel entity)
        {
            var newEntity = new Result()
            {
                BenchmarkId = entity.BenchmarkId,
                Browser = entity.Browser,
                DevicePlatform = entity.DevicePlatform,
                OperatingSystem = entity.OS,
                RawUastring = entity.RawUserAgenString,
                UserId = entity.UserId,
                ResultRow = new List<ResultRow>()
            };

            foreach(var row in entity.ResultRows)
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
            await this.m_db.SaveChangesAsync();

            return newEntity.Id;
        }

        public virtual async Task<long> DeleteById(long id)
        {
            var entity = await this.m_db.Result.SingleOrDefaultAsync(m => m.Id == id);
            if (entity != null)
            {
                this.m_db.Result.Remove(entity);
                await this.m_db.SaveChangesAsync();
            }

            return id;
        }

        public virtual async Task<PublishResultsModel> FindById(long id)
        {
            var entity = await this.m_db.Result.Include(b => b.ResultRow).FirstOrDefaultAsync(m => m.Id == id);
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null)
            {
                return null;
            }

            PublishResultsModel result = DbEntityToModel(entity);
            NewBenchmarkModel benchmark = SqlServerBenchmarkRepository.DbEntityToModel(entity.Benchmark);

            return new ShowResultModel(result, benchmark);
        }

        public virtual async Task<IEnumerable<PublishResultsModel>> ListAll(uint maxEntities)
        {
            var entities = await this.m_db.Result.Include(b => b.ResultRow).Take((int)maxEntities).ToListAsync();
            var result = new List<PublishResultsModel>();
            foreach (var benchmark in entities)
            {
                PublishResultsModel model = DbEntityToModel(benchmark);
                result.Add(model);
            }

            return result;
        }

        public Task<IEnumerable<PublishResultsModel>> ListByUser(uint maxEntities, string userId)
        {
            throw new NotImplementedException();
        }

        private PublishResultsModel DbEntityToModel([NotNull] Result entity)
        {
            var result = new PublishResultsModel()
            {
                Id = entity.Id,
                BenchmarkId = entity.BenchmarkId,
                Browser = entity.Browser,
                DevicePlatform = entity.DevicePlatform,
                OS = entity.OperatingSystem,
                RawUserAgenString = entity.RawUastring,
                UserId = entity.UserId,
                ResultRows = new List<ResultsRowModel>()
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
