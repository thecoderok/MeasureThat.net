using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Validation;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MeasureThat.Net.Controllers.ViewComponents
{
    public class LatestBenchmarkResultViewComponent : ViewComponent
    {
        private readonly SqlServerResultsRepository m_publishResultRepository;

        public LatestBenchmarkResultViewComponent([NotNull] SqlServerResultsRepository publishResultRepository)
        {
            this.m_publishResultRepository = publishResultRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync(
        long benchmarkId)
        {
            Preconditions.ToBeNonNegative(benchmarkId);
            BenchmarkResultDto latestResult = await this.m_publishResultRepository.GetLatestResultForBenchmark(benchmarkId);
            if (latestResult == null)
            {
                return View("NoResults");
            }

            return View(latestResult);
        }
    }
}