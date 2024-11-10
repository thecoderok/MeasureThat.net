using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Options;
using MeasureThat.Net.Logic.Web;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using UAParser;

namespace MeasureThat.Net.Controllers
{
    using BenchmarkLab.Logic.Web;
    using BenchmarkLab.Models;
    using Exceptions;
    using Logic;
    using MeasureThat.Net.Logic.Validation;
    using MeasureThat.Net.Logic.Web.Security;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using System;
    using System.Collections.Generic;
    using Wangkanai.Detection.Services;

    [Authorize(Policy = "AllowGuests")]
    public class BenchmarksController : Controller
    {
        private readonly SqlServerBenchmarkRepository m_benchmarkRepository;
        private readonly SqlServerResultsRepository m_publishResultRepository;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;
        private readonly IOptions<ResultsConfig> m_resultsConfig;
        private readonly IDetectionService detection;
        private const string ErrorActionName = "Error";
        private const int numOfItemsPerPage = 25;

        public BenchmarksController(
            [NotNull] SqlServerBenchmarkRepository benchmarkRepository,
            [NotNull] UserManager<ApplicationUser> userManager,
            [NotNull] IOptions<ResultsConfig> resultsConfig,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] SqlServerResultsRepository publishResultRepository,
            IDetectionService detection)
        {
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_userManager = userManager;
            this.m_resultsConfig = resultsConfig;
            this.m_logger = loggerFactory.CreateLogger<BenchmarksController>();
            this.m_publishResultRepository = publishResultRepository;
            this.detection = detection;
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
            int count = await m_benchmarkRepository.CountUserBenchmarks(user.Id);
            IList<BenchmarkDtoForIndex> list = await m_benchmarkRepository.ListByUser(user.Id, page, numOfItemsPerPage);
            return this.View(new ResultsPaginationHolder<BenchmarkDtoForIndex>(list, page, count, numOfItemsPerPage));
        }

        public IActionResult Add(bool dummyTest)
        {
            BenchmarkDto model = null;
            if (!dummyTest)
            {
                model = new BenchmarkDto();
            }
            else
            {
                model = new BenchmarkDto
                {
                    BenchmarkName = "Test name for add",
                    HtmlPreparationCode = "<div id='myid'>yoyo</div><script src = 'https://cdnjs.cloudflare.com/ajax/libs/jquery/3.3.1/jquery.js'></script> ",
                    TestCases = new List<TestCaseDto>()
                {
                    new()
                    {
                        TestCaseName = "Test case name 1",
                        BenchmarkCode = "var t = document.getElementById('myid');",
                    },
                    new ()
                    {
                        TestCaseName = "Test case name 2",
                        BenchmarkCode = "var z = $('myid');",
                    },
                }
                };
            }

            return this.View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public async Task<IActionResult> Add(BenchmarkDto model)
        {
            await this.ValidateBenchmarkForAdd(model);

            if (this.ModelState.ErrorCount > 0)
            {
                return View("Add", model);
            }

            ApplicationUser user = await this.GetCurrentUserAsync();
            model.OwnerId = user?.Id;

            long id = await this.m_benchmarkRepository.Add(model);

            return this.RedirectToAction("Show",
                new
                {
                    Id = id,
                    Version = 0,
                    name = SeoFriendlyStringConverter.Convert(model.BenchmarkName)
                });
        }

        private async Task ValidateBenchmarkForAdd(BenchmarkDto model)
        {
            this.ValidateInputModel(model);

            if (model != null && !string.IsNullOrWhiteSpace(model.BenchmarkName))
            {
                var titles = await m_benchmarkRepository.GetTitles();
                // Check if the title is unique, but account for edits to avoid showing error on the same benchmark
                if (titles.TryGetValue(model.BenchmarkName.ToLower(), out long existing_id))
                {
                    if (existing_id != model.Id)
                    {
                        this.ModelState.AddModelError("BenchmarkName", "Benchmark with such name already exists.");
                    }
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Show(int id, int version, string name)
        {
            BenchmarkDto benchmarkToRun = await this.m_benchmarkRepository.FindById(id);
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

            ClientInfo clientInfo;
            if (HttpContext.Request.Headers.ContainsKey("User-Agent"))
            {
                string userAgent = HttpContext.Request.Headers["User-Agent"];
                model.RawUserAgenString = userAgent;

                try
                {
                    clientInfo = UAParser.Parser.GetDefault().Parse(userAgent);

                    if (clientInfo != null)
                    {
                        string browser = clientInfo.UserAgent.Family + " " + clientInfo.UserAgent.Major;
                        model.Browser = browser;
                        model.DevicePlatform = detection.Device?.Type.ToString();
                        model.OS = clientInfo.OS.ToString();
                    }
                }
                catch (Exception ex)
                {
                    m_logger.LogError("Error while parsing UA String: " + ex.Message);
                }
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

            return View(new Tuple<IList<BenchmarkResultDto>, int>(model, id));
        }

        public async Task<IActionResult> Edit(int id)
        {
            BenchmarkDto benchmark = await this.ValidateOwner(id);
            return View("Add", benchmark);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            BenchmarkDto benchmark = await this.ValidateOwner(id);

            ApplicationUser user = await this.GetCurrentUserAsync();

            await this.m_benchmarkRepository.DeleteById(id, user);

            return RedirectToAction("My");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public async Task<IActionResult> Edit(BenchmarkDto model)
        {
            BenchmarkDto benchmark = await this.ValidateOwner(model.Id);

            ApplicationUser user = await this.GetCurrentUserAsync();

            this.ValidateInputModel(model);
            if (this.ModelState.ErrorCount > 0)
            {
                return View("Add", model);
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

        private async Task<BenchmarkDto> ValidateOwner(long id)
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

            return benchmark;
        }

        private void ValidateInputModel(BenchmarkDto model)
        {
            if (!this.ModelState.IsValid)
            {
                return;
            }

            // Manually parse input
            // TODO: do I really need to do this?
            var testCases = InputDataParser.ReadTestCases(this.HttpContext.Request);

            // Check if benchmark code was actually entered
            if (testCases.Count() < 2)
            {
                this.ModelState.AddModelError("TestCases", "At least two test cases are required.");
            }

            if (testCases.Any(t => string.IsNullOrWhiteSpace(t.BenchmarkCode)))
            {
                this.ModelState.AddModelError("TestCases", "Benchmark code must not be empty.");
            }

            model.TestCases = new List<TestCaseDto>(testCases);

            // Check that there are no test cases with the same name
            var set = new HashSet<string>();
            foreach (var testCase in model.TestCases)
            {
                if (testCase == null)
                {
                    continue;
                }
                if (string.IsNullOrWhiteSpace(testCase.TestCaseName))
                {
                    this.ModelState.AddModelError("TestCases", "Test name must not be empty");
                    continue;
                }
                if (testCase.TestCaseName.Contains('\''))
                {
                    this.ModelState.AddModelError("TestCases", "Test name can't contain ' character (apostrophe)");
                    continue;
                }
                if (!set.Add(testCase.TestCaseName.ToLowerInvariant().Trim()))
                {
                    this.ModelState.AddModelError("TestCases", "Test cases must have unique names");
                }
            }
        }

        public IActionResult Error()
        {
            return View("Error");
        }

        public async Task<IActionResult> TestFrame(long id)
        {
            BenchmarkDto benchmarkToRun = await m_benchmarkRepository.FindById(id);
            if (benchmarkToRun == null)
            {
                return this.NotFound();
            }

            return this.View(benchmarkToRun);
        }

        [HttpPost]
        [Produces("application/json")]
        public async Task<IActionResult> ValidateBenchmark(BenchmarkDto model)
        {
            try
            {
                return await ValidateBenchmarkInternal(model);
            }
            catch (Exception e)
            {
                return new JsonResult(new { error = e.Message });
            }
        }

        private async Task<IActionResult> ValidateBenchmarkInternal(BenchmarkDto model)
        {
            await this.ValidateBenchmarkForAdd(model);
            if (this.ModelState.ErrorCount == 0)
            {
                return new JsonResult(new { valid = true });
            }
            else
            {
                var errors = new List<string>();
                foreach (ModelStateEntry modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }

                return new JsonResult(new { valid = false, errors = errors });
            }
        }

        public IActionResult TestFrameForValidation()
        {
            return this.View("TestFrame", new BenchmarkDto());
        }

        public IActionResult TestFrameForValidationWithHtmlPrepCode(string htmlPrepCode, bool autostart)
        {
            if (this.HttpContext.Response.Headers.ContainsKey(XssProtectionConstants.Header))
            {
                // HTML Preparation code can contain text that browsers will consider as XSS attack.
                this.HttpContext.Response.Headers.Remove(XssProtectionConstants.Header);
                this.HttpContext.Response.Headers.Append(XssProtectionConstants.Header, XssProtectionConstants.Disabled);
            }
            return this.View("TestFrame", new BenchmarkDto() { HtmlPreparationCode = htmlPrepCode });
        }
    }
}
