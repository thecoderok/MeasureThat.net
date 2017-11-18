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

    public class CachingBenchmarkRepository : SqlServerBenchmarkRepository
    {
        private readonly IMemoryCache m_memoryCache;
        private readonly ILogger<CachingBenchmarkRepository> m_logger;
        private const string CacheKeyPrefix = "benchmark_";

        public CachingBenchmarkRepository(IMemoryCache mMemoryCache,
            [NotNull] ApplicationDbContext db,
            ILogger<CachingBenchmarkRepository> mLogger): base(db)
        {
            m_memoryCache = mMemoryCache;
            m_logger = mLogger;
        }
        
        public override async Task<IList<BenchmarkDto>> ListAll(int numOfItems, int page)
        {
            string key = $"latest_benchmarks_{numOfItems}_{page}";
            Func<Task<IList<BenchmarkDto>>> dbLookup = async () => await base.ListAll(numOfItems, page).ConfigureAwait(false);
            var expirationOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache, expirationOptions).ConfigureAwait(false);
        }

        public async Task<BenchmarkDto> FindByIdAndVersion(int id, int version)
        {
            string key = $"{CacheKeyPrefix}_{id}_{version}";
            Func<Task<BenchmarkDto>> dbLookup = async () => await base.FindById(id).ConfigureAwait(false);
            var expirationOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(1));
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache, expirationOptions).ConfigureAwait(false);
        }

        public override async Task<IList<BenchmarkDto>> ListByUser(string userId, int page, int numOfItems)
        {
            string key = $"my_{userId}_{numOfItems}_{page}";
            Func<Task<IList<BenchmarkDto>>> dbLookup = async () => await base.ListByUser(userId, page, numOfItems).ConfigureAwait(false);
            var expirationOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(20));
            return await CacheAsideRequestHelper.CacheAsideRequest(dbLookup, key, this.m_memoryCache, expirationOptions).ConfigureAwait(false);
        }
    }
}
