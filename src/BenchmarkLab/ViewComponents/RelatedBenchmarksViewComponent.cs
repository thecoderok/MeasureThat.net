using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Validation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MeasureThat.Net.Controllers.ViewComponents
{
    public class RelatedBenchmarksViewComponent : ViewComponent
    {
        private readonly SqlServerResultsRepository m_db;

        public RelatedBenchmarksViewComponent([NotNull] SqlServerResultsRepository db)
        {
            this.m_db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync(string relatedIds)
        {
            var benchmarkIds = ParseBenchmarkIds(relatedIds);
            if (benchmarkIds.Count == 0)
            {
                return View("NoResults");
            }
            var relatedBenchmarks = await this.m_db.GetBenchmarksByIds(benchmarkIds);
            if (relatedBenchmarks == null || relatedBenchmarks.Count == 0)
            {
                return View("NoResults");
            }
            

            return View(relatedBenchmarks);
        }

        private HashSet<long> ParseBenchmarkIds(string relatedBenchmarks)
        {
            if (string.IsNullOrEmpty(relatedBenchmarks))
            {
                return new HashSet<long>();
            }

            return new HashSet<long>(
                relatedBenchmarks
                    .Split(',')
                    .Select(id => long.TryParse(id, out var parsedId) ? parsedId : (long?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
            );
        }
    }
}
