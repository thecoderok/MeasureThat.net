using System.Linq;
using BenchmarkLab.Data;
using BenchmarkLab.Data.Dao;
using BenchmarkLab.Logic.Web;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkLab.Controllers
{
    [Authorize]
    public class BenchmarksController : Controller
    {
        private ApplicationDbContext m_context;
        private IEntityRepository<NewBenchmarkModel, int> m_benchmarkRepository;

        public BenchmarksController([NotNull] ApplicationDbContext context, [NotNull] IEntityRepository<NewBenchmarkModel, int> benchmarkRepository)
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

            m_benchmarkRepository.Add(model);

            return RedirectToAction("Index");
        }
    }
}