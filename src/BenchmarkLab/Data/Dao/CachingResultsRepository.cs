using System.Threading.Tasks;
using MeasureThat.Net.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Net.Data.Dao
{
    using System;
    using System.Collections.Generic;
    using Controllers;

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

        public override async Task<BenchmarkResultDto> FindById(long id)
        {
            string key = $"result_{id}";
            Func<Task<BenchmarkResultDto>> dbLookup = async () => await base.FindById(id).ConfigureAwait(false);
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache).ConfigureAwait(false);
        }

        public override async Task<ShowResultModel> GetResultWithBenchmark(long id)
        {
            string key = $"result+bmk_{id}";
            Func<Task<ShowResultModel>> dbLookup = async () => await base.GetResultWithBenchmark(id).ConfigureAwait(false);
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache).ConfigureAwait(false);
        }

        public override async Task<IList<BenchmarkResultDto>> ListAll(int benchmarkId)
        {
            string key = $"result_{benchmarkId}";
            Func<Task<IList<BenchmarkResultDto>>> dbLookup = async () => await base.ListAll(benchmarkId).ConfigureAwait(false);
            var expirationOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(30));
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache, expirationOptions).ConfigureAwait(false);
        }
    }
}
