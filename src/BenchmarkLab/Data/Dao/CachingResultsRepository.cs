using System.Threading.Tasks;
using MeasureThat.Net.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Net.Data.Dao
{
    using System;
    using System.Collections.Generic;

    public class CachingResultsRepository : SqlServerResultsRepository
    {
        private readonly IMemoryCache m_memoryCache;
        private readonly ILogger<CachingResultsRepository> m_logger;

        public CachingResultsRepository([NotNull] ApplicationDbContext db,
            IMemoryCache mMemoryCache,
            ILogger<CachingResultsRepository> mLogger) : base(db)
        {
            m_memoryCache = mMemoryCache;
            m_logger = mLogger;
        }

        public override async Task<PublishResultsModel> FindById(long id)
        {
            string key = "result_" + id;
            Func<Task<PublishResultsModel>> dbLookup = async () => await base.FindById(id).ConfigureAwait(false);
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache).ConfigureAwait(false);
        }

        public override async Task<ShowResultModel> GetResultWithBenchmark(long id)
        {
            string key ="result+benchmark_" + id;
            Func<Task<ShowResultModel>> dbLookup = async () => await base.GetResultWithBenchmark(id).ConfigureAwait(false);
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache).ConfigureAwait(false);
        }

        public override async Task<IEnumerable<PublishResultsModel>> ListBenchmarkResults(int maxEntities, int benchmarkId)
        {
            string key = "result+list_" + benchmarkId + "_" + maxEntities;
            Func<Task<IEnumerable<PublishResultsModel>>> dbLookup = async () => await base.ListBenchmarkResults(maxEntities, benchmarkId).ConfigureAwait(false);
            var expirationOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache, expirationOptions).ConfigureAwait(false);
        }
    }
}
