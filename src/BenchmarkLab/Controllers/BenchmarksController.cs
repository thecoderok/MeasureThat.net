using System.Linq;
using System.Threading.Tasks;
using BenchmarkLab.Data;
using BenchmarkLab.Data.Dao;
using BenchmarkLab.Logic.Web;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BenchmarkLab.Logic.Text.Unidecode;

namespace BenchmarkLab.Controllers
{
    [Authorize]
    public class BenchmarksController : Controller
    {
        private ApplicationDbContext m_context;
        private readonly IBenchmarksRepository m_benchmarkRepository;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;

        public BenchmarksController(
            [NotNull] ApplicationDbContext context,
            [NotNull] IBenchmarksRepository benchmarkRepository,
            [NotNull] UserManager<ApplicationUser> userManager)
        {
            this.m_context = context;
            this.m_benchmarkRepository = benchmarkRepository;
            m_userManager = userManager;
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
        public async Task<IActionResult> Add([Bind(
            "BenchmarkName",
            "Description", 
            "HtmlPreparationCode",
            "ScriptPreparationCode",
            "BenchmarkCode")] NewBenchmarkModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if benchmark code was actually entered
            if (model.BenchmarkCode.Count() < 2 || model.BenchmarkCode.Any(string.IsNullOrWhiteSpace))
            {
                // TODO: use correct error key
                ModelState.AddModelError("BenchmarkCode", "At least two test are cases required.");
                return View(model);
            }

            // This is brand new benchmark
            model.BenchmarkVersion = 0;

            ApplicationUser user = await this.GetCurrentUserAsync();
            model.OwnerId = user.Id;

            m_benchmarkRepository.Add(model);

            return RedirectToAction("Show", new { Id = model.Id, ver = model.BenchmarkVersion, name = model.BenchmarkName.Unidecode() });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Show(int id, int ver, string name)
        {
            NewBenchmarkModel benchmarkToRun = m_benchmarkRepository.FindBenchmark(id, ver);
            if (benchmarkToRun == null)
            {
                return NotFound();
            }

            return View(benchmarkToRun);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //TODO: should aft be validated?
        public IActionResult Fork(int id, int ver)
        {
            return View();
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return m_userManager.GetUserAsync(HttpContext.User);
        }
    }
}
