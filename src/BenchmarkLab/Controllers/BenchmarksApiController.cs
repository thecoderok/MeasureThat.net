using System.Collections.Generic;
using System.Threading.Tasks;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Mvc;

namespace MeasureThat.Net.Controllers
{
    using Logic.Web;

    [Produces("application/json")]
    [Route("api/benchmarks")]
    public class BenchmarksApiController : Controller
    {
        private readonly CachingBenchmarkRepository m_benchmarkRepository;

        public BenchmarksApiController(CachingBenchmarkRepository mBenchmarkRepository)
        {
            m_benchmarkRepository = mBenchmarkRepository;
        }

        public async Task<IEnumerable<BenchmarkDto>> Get(int page, string query)
        {
            var list = await this.m_benchmarkRepository.ListAll(20, page);
            
            return list;
        }

        public async Task<IActionResult> Delete(int id)
        {
            await this.m_benchmarkRepository.DeleteById(id);
            return new NoContentResult();
        }

        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public Task<BenchmarkDto> Post(BenchmarkDto model)
        {
            return null;
        }
    }
}
