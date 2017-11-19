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
    using Exceptions;
    using Logic;
    using MeasureThat.Net.Logic.Validation;
    using BenchmarkLab.Models;
    using BenchmarkLab.Logic.Web;

    [Authorize(Policy = "AllowGuests")]
    public class BenchmarksController : Controller
    {
        private readonly CachingBenchmarkRepository m_benchmarkRepository;
        private readonly CachingResultsRepository m_publishResultRepository;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;
        private readonly IOptions<ResultsConfig> m_resultsConfig;
        private const string ErrorMessageKey = "ErrorMessage";
        private const string ErrorActionName = "Error";
        private const int numOfItemsPerPage = 25;

        public BenchmarksController(
            [NotNull] CachingBenchmarkRepository benchmarkRepository,
            [NotNull] UserManager<ApplicationUser> userManager,
            [NotNull] IOptions<ResultsConfig> resultsConfig,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] CachingResultsRepository publishResultRepository)
        {
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_userManager = userManager;
            this.m_resultsConfig = resultsConfig;
            this.m_logger = loggerFactory.CreateLogger<BenchmarksController>();
            this.m_publishResultRepository = publishResultRepository;
        }

        public async Task<IActionResult> Index(int page)
        {
            if (page < 0)
            {
                page = 0;
            }
            IList<BenchmarkDtoForIndex> latestBenchmarks = await m_benchmarkRepository.ListAllForIndex(numOfItemsPerPage, page);
            int totalNumberOfRecords = await m_benchmarkRepository.CountAll();

            return View(new ResultsPaginationHolder<BenchmarkDtoForIndex>(latestBenchmarks, page, totalNumberOfRecords, numOfItemsPerPage));
        }

        [Authorize]
        public async Task<IActionResult> My(int page)
        {
            page = Preconditions.ToValidPage(page);
            ApplicationUser user = await this.GetCurrentUserAsync();
            IList<BenchmarkDto> list = await m_benchmarkRepository.ListByUser(user.Id, page, numOfItemsPerPage);
            return this.View(list);
        }

        public IActionResult Add()
        {
            return this.View(new BenchmarkDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public async Task<IActionResult> Add(BenchmarkDto model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            // Manually parse input
            var testCases = InputDataParser.ReadTestCases(this.HttpContext.Request);

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

            model.TestCases = new List<TestCaseDto>(testCases);

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
            model.OwnerId = user?.Id;

            long id = await this.m_benchmarkRepository.Add(model);

            return this.RedirectToAction("Show",
                new {Id = id, Version=0, name = SeoFriendlyStringConverter.Convert(model.BenchmarkName)});
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Show(int id, int version, string name)
        {
            BenchmarkDto benchmarkToRun = await this.m_benchmarkRepository.FindByIdAndVersion(id, version);
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
            BenchmarkDto benchmark = await this.m_benchmarkRepository.FindById(id);
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
        public async Task<IActionResult> PublishResults(BenchmarkResultDto model)
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
        public async Task<IActionResult> ListResults(int id)
        {
            IList<BenchmarkResultDto> model = await this
                .m_publishResultRepository
                .ListAll(id);

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
        public async Task<IActionResult> Edit(BenchmarkDto model)
        {
            
            ApplicationUser user = await this.GetCurrentUserAsync();
            if (user == null)
            {
                throw new NotLoggedInException("You are not logged in");
            }

            const string ViewName = "Add";
            if (!this.ModelState.IsValid)
            {
                return this.View(ViewName, model);
            }

            // Manually parse input
            var testCases = InputDataParser.ReadTestCases(this.HttpContext.Request);

            // Check if benchmark code was actually entered
            if (testCases.Count() < 2)
            {
                this.ModelState.AddModelError("TestCases", "At least two test cases are required.");
                return this.View(ViewName, model);
            }

            if (testCases.Any(t => string.IsNullOrWhiteSpace(t.BenchmarkCode)))
            {
                this.ModelState.AddModelError("TestCases", "Benchmark code must not be empty.");
                return this.View(ViewName, model);
            }

            model.TestCases = new List<TestCaseDto>(testCases);

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
            
            try
            {
                BenchmarkDto updatedModel = await this.m_benchmarkRepository.Update(model, user.Id);
                return this.RedirectToAction("Show", new { Id = updatedModel.Id, Version = updatedModel.Version, name = SeoFriendlyStringConverter.Convert(model.BenchmarkName) });
            }
            catch (Exception ex)
            {
                m_logger.LogError("Can't update benchmark: " + ex.Message);
                return RedirectToAction(ErrorActionName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public async Task<IActionResult> Delete(long id)
        {
            await this.m_benchmarkRepository.DeleteById(id);
            return RedirectToAction("Index");
        }

        public IActionResult Error()
        {
            return View("Error");
        }
    }
}
