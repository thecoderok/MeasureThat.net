﻿using System.Threading.Tasks;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BenchmarkLab.Data.Dao
{
    public class CachingResultsRepository : SqlServerResultsRepository
    {
        private readonly IMemoryCache m_memoryCache;
        private readonly ILogger<CachingResultsRepository> m_logger;
        private const string CacheKeyPrefix = "result_";

        public CachingResultsRepository([NotNull] ApplicationDbContext db,
            IMemoryCache mMemoryCache,
            ILogger<CachingResultsRepository> mLogger) : base(db)
        {
            m_memoryCache = mMemoryCache;
            m_logger = mLogger;
        }

        public override async Task<PublishResultsModel> FindById(long id)
        {
            string key = CacheKeyPrefix + id;
            PublishResultsModel result = null;
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

        public override async Task<ShowResultModel> GetResultWithBenchmark(long id)
        {
            string key ="result+benchmark_" + id;
            ShowResultModel result = null;
            if (!m_memoryCache.TryGetValue(key, out result))
            {
                m_logger.LogInformation($"Cache miss for key {key}. Doing lookup.");
                result = await base.GetResultWithBenchmark(id);
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
