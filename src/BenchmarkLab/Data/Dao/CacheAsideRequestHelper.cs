namespace MeasureThat.Net.Data.Dao
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;

    public class CacheAsideRequestHelper
    {
        public static async Task<T> CacheAsideRequest<T>(Func<Task<T>> lookupWhenNotFound, 
            string key,
            IMemoryCache memoryCache,
            MemoryCacheEntryOptions cacheOptions = null)
        {
            T result;
            if (!memoryCache.TryGetValue(key, out result))
            {
                result = await lookupWhenNotFound().ConfigureAwait(false);
                CacheEntryIfNeeded(key, memoryCache, cacheOptions, result);
            }

            return result;
        }

        private static void CacheEntryIfNeeded<T>(string key, IMemoryCache memoryCache, MemoryCacheEntryOptions cacheOptions,
            T result)
        {
            if (result == null)
            {
                return;
            }

            if (cacheOptions == null)
            {
                memoryCache.Set(key, result);
            }
            else
            {
                memoryCache.Set(key, result, cacheOptions);
            }
        }
    }
}
