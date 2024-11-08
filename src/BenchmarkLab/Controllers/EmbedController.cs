using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Options;
using MeasureThat.Net.Logic.Validation;
using MeasureThat.Net.Logic.Web;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace BenchmarkLab.Controllers
{
    public class EmbedController : Controller
    {
        private readonly SqlServerResultsRepository m_publishResultRepository;
        private readonly IOptions<ResultsConfig> m_resultsConfig;

        public EmbedController([NotNull] SqlServerResultsRepository publishResultRepository, [NotNull] IOptions<ResultsConfig> resultsConfig)
        {
            this.m_publishResultRepository = publishResultRepository;
            this.m_resultsConfig = resultsConfig;
        }

        public async Task<IActionResult> Index(int id)
        {
            this.RemoveFrameOptionsHeader();
            if (!this.m_resultsConfig.Value.BenchmarkEmbeddingEnabled)
            {
                return View("Disabled", id);
            }
            Preconditions.ToBeNonNegative(id);
            ShowResultModel model = await this.m_publishResultRepository.GetResultWithBenchmark(id);
            if (model == null)
            {
                return PartialView("DoesNotExists", id);
            }
            return View(model);
        }

        /*
         * Removes X Frame Options header to allow responses of the controller to be embedded at any website
         */
        private void RemoveFrameOptionsHeader()
        {
            IHeaderDictionary headers = this.HttpContext.Response.Headers;
            if (headers.ContainsKey(FrameOptionsConstants.Header))
            {
                this.HttpContext.Response.Headers.Remove(FrameOptionsConstants.Header);
            }
        }
    }
}