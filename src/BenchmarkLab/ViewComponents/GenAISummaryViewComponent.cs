using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Validation;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MeasureThat.Net.Controllers.ViewComponents
{
    public class GenAISummaryViewComponent : ViewComponent
    {
        private readonly SqlServerResultsRepository m_db;

        public GenAISummaryViewComponent([NotNull] SqlServerResultsRepository db)
        {
            this.m_db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(long benchmarkId)
        {
            Preconditions.ToBeNonNegative(benchmarkId);
            var latestResult = await this.m_db.GetGenAISummary(benchmarkId);
            if (latestResult == null)
            {
                return View("NoResults");
            }

            return View(latestResult);
        }
    }
}
