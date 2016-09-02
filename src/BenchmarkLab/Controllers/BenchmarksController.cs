using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Options;
using MeasureThat.Net.Logic.Web;
using MeasureThat.Net.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    [Authorize(Policy = "AllowGuests")]
    public class BenchmarksController : Controller
    {
        private static readonly Regex TestCaseKeyRegex = new Regex("TestCases\\[(\\d+)\\]",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly CachingBenchmarkRepository m_benchmarkRepository;
        private readonly IResultsRepository m_publishResultRepository;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;
        private readonly IOptions<ResultsConfig> m_resultsConfig;
        private const string ErrorMessageKey = "ErrorMessage";
        private const string ErrorActionName = "Error";

        public BenchmarksController(
            [NotNull] CachingBenchmarkRepository benchmarkRepository,
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

        [Authorize]
        public async Task<IActionResult> Index()
        {
            return await ShowLatestBenchmarks();
        }

        [Authorize]
        public async Task<IActionResult> My()
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
        public async Task<IActionResult> Add(NewBenchmarkModel model)
        {
            // TODO: bring back bind attribute
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // Manually parse input
            var testCases = ReadTestCases();

            // Check if benchmark code was actually entered
            if (testCases.Count() < 2)
            {
                // TODO: use correct error key
                this.ModelState.AddModelError("TestCases", "At least two test cases are required.");
                return this.View(model);
            }

            if (testCases.Any(t => string.IsNullOrWhiteSpace(t.BenchmarkCode)))
            {
                this.ModelState.AddModelError("TestCases", "Benchmark code must not be empty.");
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

            model.TestCases = new List<TestCase>(testCases);

            ApplicationUser user = await this.GetCurrentUserAsync();
            model.OwnerId = user?.Id;

            long id = await this.m_benchmarkRepository.Add(model);

            return this.RedirectToAction("Show",
                new {Id = id, Version=0, name = SeoFriendlyStringConverter.Convert(model.BenchmarkName)});
        }

        // TODO: Unit tests
        private List<TestCase> ReadTestCases()
        {
            var testCases = new List<TestCase>();
            if (!this.HttpContext.Request.HasFormContentType)
            {
                return testCases;
            }

            var indexes = new HashSet<int>(); // list of test case indexes
            IFormCollection form = this.HttpContext.Request.Form;
            foreach (var key in form.Keys)
            {
                if (key.StartsWith("TestCases["))
                {
                    var match = TestCaseKeyRegex.Match(key);
                    if (!match.Success || match.Groups.Count != 2)
                    {
                        continue;
                    }

                    int index = 0;
                    if (int.TryParse(match.Groups[1].Value, out index))
                    {
                        indexes.Add(index);
                    }
                }
            }

            foreach (var idx in indexes)
            {
                string nameKey = $"TestCases[{idx}].TestCaseName";
                string codeKey = $"TestCases[{idx}].BenchmarkCode";

                if (form.ContainsKey(nameKey) && form.ContainsKey(codeKey))
                {
                    var name = form[nameKey];
                    var code = form[codeKey];
                    var testCase = new TestCase()
                    {
                        BenchmarkCode = code,
                        TestCaseName = name
                    };
                    testCases.Add(testCase);
                }
            }

            return testCases;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Show(int id, int version, string name)
        {
            NewBenchmarkModel benchmarkToRun = await this.m_benchmarkRepository.FindByIdAndVersion(id, version);
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
            benchmark.OwnerId = user?.Id;
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
            model.UserId = user?.Id;

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

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return this.m_userManager.GetUserAsync(this.HttpContext.User);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Latest()
        {
            return await ShowLatestBenchmarks();
        }

        private async Task<IActionResult> ShowLatestBenchmarks()
        {
            const int numOfItems = 25;
            IEnumerable<NewBenchmarkModel> latestBenchmarks = await m_benchmarkRepository.GetLatest(numOfItems);

            return View("Index", latestBenchmarks);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ListResults(int id)
        {
            const int numOfItems = 25;
            IEnumerable<PublishResultsModel> model = await this
                .m_publishResultRepository
                .ListBenchmarkResults(numOfItems, id);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                throw new NotLoggedInException("You are not logged in");
            }

            var benchmark = await this.m_benchmarkRepository.FindById(id);
            if (benchmark == null)
            {
                throw new Exception("Can't find benchmark");
            }

            if (benchmark.OwnerId != user.Id)
            {
                throw new Exception("Only owner can edit benchmark.");
            }

            return View("Add", benchmark);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public async Task<IActionResult> Edit(NewBenchmarkModel model)
        {
            // TODO: duplicated code, DRY
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                throw new NotLoggedInException("You are not logged in");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View("Add", model);
            }

            // Manually parse input
            var testCases = ReadTestCases();

            // Check if benchmark code was actually entered
            if (testCases.Count() < 2)
            {
                // TODO: use correct error key
                this.ModelState.AddModelError("TestCases", "At least two test cases are required.");
                return this.View("Add", model);
            }

            if (testCases.Any(t => string.IsNullOrWhiteSpace(t.BenchmarkCode)))
            {
                this.ModelState.AddModelError("TestCases", "Benchmark code must not be empty.");
                return this.View("Add", model);
            }

            // Check that there are no test cases with the same name
            var set = new HashSet<string>();
            foreach (var testCase in model.TestCases)
            {
                if (!set.Add(testCase.TestCaseName.ToLowerInvariant().Trim()))
                {
                    this.ModelState.AddModelError("TestCases", "Test cases must have unique names");
                    return this.View("Add", model);
                }
            }

            model.TestCases = new List<TestCase>(testCases);
            
            try
            {
                NewBenchmarkModel updatedModel = await this.m_benchmarkRepository.Update(model, user.Id);
                return this.RedirectToAction("Show", new { Id = updatedModel.Id, Version = updatedModel.Version, name = SeoFriendlyStringConverter.Convert(model.BenchmarkName) });
            }
            catch (Exception ex)
            {
                m_logger.LogError("Can't update benchmark: " + ex.Message);
                return RedirectToAction(ErrorActionName);
            }
        }

        public IActionResult Error()
        {
            return View("Error");
        }
    }

    public class NotLoggedInException : Exception
    {
        public NotLoggedInException(string youAreNotLoggedIn)
        {
        }
    }
}
