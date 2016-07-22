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
    using System.Collections.Generic;

    [Authorize]
    public class BenchmarksController : Controller
    {
        private ApplicationDbContext m_context;
        private readonly  IEntityRepository<NewBenchmarkModel, long> m_benchmarkRepository;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;

        public BenchmarksController(
            [NotNull] ApplicationDbContext context,
            [NotNull] IEntityRepository<NewBenchmarkModel, long> benchmarkRepository,
            [NotNull] UserManager<ApplicationUser> userManager)
        {
            this.m_context = context;
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            IEnumerable<NewBenchmarkModel> list = await this.m_benchmarkRepository.ListAll(20);
            return this.View(list);
        }

        public IActionResult Add()
        {
            return this.View(new NewBenchmarkModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public async Task<IActionResult> Add(/*[Bind(
            "BenchmarkName",
            "Description", 
            "HtmlPreparationCode",
            "ScriptPreparationCode", Prefix = "TestCases")]*/ NewBenchmarkModel model)
        {
            // TODO: bring back bind attribute
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // Check if benchmark code was actually entered
            if (model.TestCases.Count() < 2 || model.TestCases.Any(t=> string.IsNullOrWhiteSpace(t.BenchmarkCode)))
            {
                // TODO: use correct error key
                this.ModelState.AddModelError("TestCases", "At least two test are cases required.");
                return this.View(model);
            }

            ApplicationUser user = await this.GetCurrentUserAsync();
            model.OwnerId = user.Id;

            long id = await this.m_benchmarkRepository.Add(model);

            return this.RedirectToAction("Show", new { Id = id, name = model.BenchmarkName.Unidecode() });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Show(int id, string name)
        {
            NewBenchmarkModel benchmarkToRun = await this.m_benchmarkRepository.FindById(id);
            if (benchmarkToRun == null)
            {
                return this.NotFound();
            }

            return this.View(benchmarkToRun);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] //TODO: should aft be validated?
        public IActionResult Fork(int id)
        {
            return this.View();
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return this.m_userManager.GetUserAsync(this.HttpContext.User);
        }
    }
}
