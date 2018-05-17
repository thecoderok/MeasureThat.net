using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BenchmarkLab.Controllers
{
    [Produces("application/json")]
    public class ApiController : Controller
    {
        const int TitleLengthToCheckForSimilarBenchmarks = 15;
        const int SimilarityPercentThreshold = 85;

        private readonly SqlServerBenchmarkRepository m_benchmarkRepository;
        private readonly ILogger m_logger;

        public ApiController([NotNull] SqlServerBenchmarkRepository benchmarkRepository,
            [NotNull] ILoggerFactory loggerFactory)
        {
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_logger = loggerFactory.CreateLogger<ApiController>();
        }

        // GET: api/Api
        public async Task<bool> CheckBenchmarkTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                // Empty result
                return false;
            }
            Dictionary<string, long> titles = await m_benchmarkRepository.GetTitles();
            return titles.ContainsKey(title.ToLower());
        }
    }
}