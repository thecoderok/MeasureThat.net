using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkLab.Controllers
{
    [Produces("application/json")]
    public class ApiController : Controller
    {
        const int TitleLengthToCheckForSimilarBenchmarks = 15;
        const int SimilarityPercentThreshold = 85;

        private readonly SqlServerBenchmarkRepository m_benchmarkRepository;

        public ApiController([NotNull] SqlServerBenchmarkRepository benchmarkRepository)
        {
            this.m_benchmarkRepository = benchmarkRepository;
        }

        // GET: api/Api. Returns true if such title exists for the different benchmarklidation
        public async Task<bool> CheckBenchmarkTitle(long benchmarkId, string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                // Empty result
                return false;
            }
            Dictionary<string, long> titles = await m_benchmarkRepository.GetTitles();
            if (titles.TryGetValue(title.ToLower(), out long existing_id))
            {
                if (existing_id != benchmarkId)
                {
                    // Contains the same title that belongs to the different benchmark
                    return true;
                }
            }
            return false;
        }
    }
}
