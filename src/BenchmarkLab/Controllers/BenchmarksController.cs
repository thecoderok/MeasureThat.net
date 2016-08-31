using System.Linq;
using System.Threading.Tasks;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Options;
using MeasureThat.Net.Logic.Web;
using MeasureThat.Net.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UAParser;

namespace MeasureThat.Net.Controllers
{
    using System;
    using System.Collections.Generic;
    using Unidecode.NET;

    [Authorize]
    public class BenchmarksController : Controller
    {
        private readonly IEntityRepository<NewBenchmarkModel, long> m_benchmarkRepository;
        private readonly IResultsRepository m_publishResultRepository;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;
        private readonly IOptions<ResultsConfig> m_resultsConfig;

        public BenchmarksController(
            [NotNull] IEntityRepository<NewBenchmarkModel, long> benchmarkRepository,
            [NotNull] UserManager<ApplicationUser> userManager,
            [NotNull] IOptions<ResultsConfig> resultsConfig,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IResultsRepository publishResultRepository)
        {
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_userManager = userManager;
            this.m_resultsConfig = resultsConfig;
            this.m_logger = loggerFactory.CreateLogger<BenchmarksController>();
            this.m_publishResultRepository = publishResultRepository;
        }

        public async Task<IActionResult> Index()
        {
            ApplicationUser user = await this.GetCurrentUserAsync();
            IEnumerable<NewBenchmarkModel> list = await this.m_benchmarkRepository.ListByUser(20, user.Id);
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
                this.ModelState.AddModelError("TestCases", "At least two test cases are required.");
                return this.View(model);
            }

            // Check that there are no test cases with the same name
            var set = new HashSet<string>();
            foreach (var testCase in model.TestCases)
            {
                if (!set.Add(testCase.TestCaseName.ToLowerInvariant().Trim()))
                {
                    this.ModelState.AddModelError("TestCases", "Test cases must have unique names");
                    return this.View(model);
                }
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fork(int id)
        {
            NewBenchmarkModel benchmark = await this.m_benchmarkRepository.FindById(id);
            if (benchmark == null)
            {
                return this.NotFound();
            }

            var user = await GetCurrentUserAsync();
            benchmark.OwnerId = user.Id;
            benchmark.Id = 0;

            return View("Add", benchmark);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> PublishResults(PublishResultsModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user != null)
            {
                model.UserId = user.Id;
            }

            ClientInfo clientInfo = null;
            if (HttpContext.Request.Headers.ContainsKey("User-Agent"))
            {
                string userAgent = HttpContext.Request.Headers["User-Agent"];
                model.RawUserAgenString = userAgent;
                
                try
                {
                    clientInfo = UAParser.Parser.GetDefault().Parse(userAgent);
                }
                catch (Exception ex)
                {
                    m_logger.LogError("Error while parsing UA String: " + ex.Message);
                }
            }            

            if (clientInfo != null)
            {
                model.Browser = clientInfo.UserAgent.Family + " " + clientInfo.UserAgent.Major;
                model.DevicePlatform = clientInfo.Device.ToString();
                model.OS = clientInfo.OS.ToString();
            }

            if (this.m_resultsConfig.Value.UploadResultsToDb)
            {
                if (user != null || this.m_resultsConfig.Value.UploadGuestUserResultsToDb)
                {
                    long id = await this.m_publishResultRepository.Add(model);
                    model.Id = id;
                }
            }

            return this.PartialView(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ShowResult(long id)
        {
            ShowResultModel model = await this.m_publishResultRepository.GetResultWithBenchmark(id);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestBeforeSubmit(NewBenchmarkModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View("ValidationErrors");
            }

            // Check if benchmark code was actually entered
            if (model.TestCases.Count() < 2 || model.TestCases.Any(t => string.IsNullOrWhiteSpace(t.BenchmarkCode)))
            {
                // TODO: use correct error key
                this.ModelState.AddModelError("TestCases", "At least two test are cases required.");
                return this.View("ValidationErrors");
            }

            return this.View("Show", model);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return this.m_userManager.GetUserAsync(this.HttpContext.User);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Latest()
        {
            const int numOfItems = 25;
            IEnumerable<NewBenchmarkModel> latestBenchmarks = await m_benchmarkRepository.GetLatest(numOfItems);

            return View(latestBenchmarks);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ListResults(int id)
        {
            const int numOfItems = 25;
            IEnumerable<PublishResultsModel> model = await this.m_publishResultRepository.ListBenchmarkResults(numOfItems, id);
            return View(model);
        }
    }
}
