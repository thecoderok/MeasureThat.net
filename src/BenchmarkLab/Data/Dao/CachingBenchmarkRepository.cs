using System.Threading.Tasks;
using MeasureThat.Net.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Net.Data.Dao
{
    using System;
    using System.Collections.Generic;

    public class CachingBenchmarkRepository : SqlServerBenchmarkRepository
    {
        private readonly IMemoryCache m_memoryCache;
        private readonly ILogger<CachingBenchmarkRepository> m_logger;
        private const string CacheKeyPrefix = "benchmark_";

        public CachingBenchmarkRepository(IMemoryCache mMemoryCache,
            [NotNull] ApplicationDbContext db, ILogger<CachingBenchmarkRepository> mLogger): base(db)
        {
            m_memoryCache = mMemoryCache;
            m_logger = mLogger;
        }

        public override async Task<NewBenchmarkModel> FindById(long id)
        {
            string key = CacheKeyPrefix + id;
            Func<Task<NewBenchmarkModel>> dbLookup = async () => await base.FindById(id).ConfigureAwait(false);
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache).ConfigureAwait(false);
        }

        public override async Task<IEnumerable<NewBenchmarkModel>> GetLatest(int numOfItems)
        {
            const string key = "latest_benchmarks";
            Func<Task<IEnumerable<NewBenchmarkModel>>> dbLookup = async () => await base.GetLatest(numOfItems).ConfigureAwait(false);
            var expirationOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache, expirationOptions).ConfigureAwait(false);
        }
    }
}
