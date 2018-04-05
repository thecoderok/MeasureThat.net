using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using Microsoft.Extensions.Logging;

namespace BenchmarkLab.Controllers
{
    [Produces("application/json")]
    public class ApiController : Controller
    {
        private readonly SqlServerBenchmarkRepository m_benchmarkRepository;
        private readonly ILogger m_logger;

        public ApiController([NotNull] SqlServerBenchmarkRepository benchmarkRepository,
            [NotNull] ILoggerFactory loggerFactory)
        {
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_logger = loggerFactory.CreateLogger<ApiController>();
        }

        // GET: api/Api
        public async Task<bool> HasTitle(string title)
        {
            Dictionary<string, long> titles = await m_benchmarkRepository.GetTitles();
            if (titles.ContainsKey(title.ToLower()))
            {
                return true;
            }

            return false;
        }
    }
}
