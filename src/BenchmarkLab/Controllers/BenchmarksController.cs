using System.Linq;
using BenchmarkLab.Data;
using BenchmarkLab.Data.Dao;
using BenchmarkLab.Logic.Web;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BenchmarkLab.Controllers
{
    [Authorize]
    public class BenchmarksController : Controller
    {
        private ApplicationDbContext m_context;
        private IBenchmarksRepository m_benchmarkRepository;
        private readonly ILogger m_logger;

        public BenchmarksController([NotNull] ApplicationDbContext context, [NotNull] IBenchmarksRepository benchmarkRepository)
        {
            this.m_context = context;
            this.m_benchmarkRepository = benchmarkRepository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var list = m_benchmarkRepository.ListAll();
            return View(list);
        }

        public IActionResult Add()
        {
            return View(new NewBenchmarkModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public IActionResult Add(NewBenchmarkModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if benchmark code was actually entered
            if (model.BenchmarkCode.Count() < 2 || model.BenchmarkCode.Any(string.IsNullOrWhiteSpace))
            {
                // TODO: use correct error key
                ModelState.AddModelError("BenchmarkCode", "Enter the benchmark code");
                return View(model);
            }

            // This is brand new benchmark
            model.BenchmarkVersion = 0;

            m_benchmarkRepository.Add(model);

            return RedirectToAction("Run", new { benchmarkId = model.Id, benchmarkVersion = model.BenchmarkVersion });
        }

        public IActionResult Run(int benchmarkId, int benchmarkVersion)
        {
            NewBenchmarkModel benchmarkToRun = m_benchmarkRepository.FindBenchmark(benchmarkId, benchmarkVersion);
            if (benchmarkToRun == null)
            {
                return View("Error", "Can't find benchmark to run.");
            }

            return View(new NewBenchmarkModel());
        }
    }
}