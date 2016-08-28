using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BenchmarkLab.Data.Dao
{
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
            NewBenchmarkModel result = null;
            if (!m_memoryCache.TryGetValue(key, out result))
            {
                m_logger.LogInformation($"Cache miss for key {key}. Doing lookup.");

                result = await base.FindById(id);
                if (result != null)
                {
                    m_memoryCache.Set(key, result);
                    m_logger.LogInformation($"Item with key {key} was added to the cache");
                }
            }

            return result;
        }
    }
}
