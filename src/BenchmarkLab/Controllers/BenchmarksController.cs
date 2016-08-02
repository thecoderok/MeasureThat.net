using System.Linq;
using System.Threading.Tasks;
using BenchmarkLab.Data;
using BenchmarkLab.Data.Dao;
using BenchmarkLab.Logic.Options;
using BenchmarkLab.Logic.Web;
using BenchmarkLab.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UAParser;

namespace BenchmarkLab.Controllers
{
    using System;
    using System.Collections.Generic;
    using Unidecode.NET;

    [Authorize]
    public class BenchmarksController : Controller
    {
        private ApplicationDbContext m_context;
        private readonly  IEntityRepository<NewBenchmarkModel, long> m_benchmarkRepository;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;
        private readonly IOptions<ResultsConfig> m_resultsConfig;

        public BenchmarksController(
            [NotNull] ApplicationDbContext context,
            [NotNull] IEntityRepository<NewBenchmarkModel, long> benchmarkRepository,
            [NotNull] UserManager<ApplicationUser> userManager,
            [NotNull] IOptions<ResultsConfig> resultsConfig,
            [NotNull] ILoggerFactory loggerFactory)
        {
            this.m_context = context;
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_userManager = userManager;
            this.m_resultsConfig = resultsConfig;
            this.m_logger = loggerFactory.CreateLogger<BenchmarksController>();
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
            throw new NotImplementedException();
            return this.View();
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

            // TODO: store it in database
            if (this.m_resultsConfig.Value.UploadResultsToDb)
            {
                //await UploadResultsToDb(model, user);
            }

            return this.PartialView(model);
        }

        /*private Task UploadResultsToDb(PublishResultsModel model, ApplicationUser user)
        {
            if (user != null || this.m_resultsConfig.Value.UploadGuestUserResultsToDb)
            {
                
            }
        }*/

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return this.m_userManager.GetUserAsync(this.HttpContext.User);
        }
    }
}