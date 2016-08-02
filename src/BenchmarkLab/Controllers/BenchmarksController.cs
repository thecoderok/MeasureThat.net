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
            [NotNull] IOptions<ResultsConfig> resultsConfig)
        {
            this.m_context = context;
            this.m_benchmarkRepository = benchmarkRepository;
            this.m_userManager = userManager;
            m_resultsConfig = resultsConfig;
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

/*
 http://stackoverflow.com/questions/26792362/benchmark-js-how-to-display-read-results-ops-sec

{
    "0": {
        "name": "test 1_1",
        "options": {
            "async": false,
            "defer": false,
            "delay": 0.005,
            "initCount": 1,
            "maxTime": 5,
            "minSamples": 5,
            "minTime": 0.075
        },
        "async": false,
        "defer": false,
        "delay": 0.005,
        "initCount": 1,
        "maxTime": 5,
        "minSamples": 5,
        "minTime": 0.075,
        "id": 1,
        "stats": {
            "moe": 0.000023307161775252783,
            "rme": 5.302515126491075,
            "sem": 0.000010865809685432532,
            "deviation": 0.00004208309995473945,
            "mean": 0.0004395491803278689,
            "sample": [0.0003165983606557377, 0.00038831967213114754, 0.0004057377049180328, 0.00041803278688524586, 0.00044979508196721314, 0.00046926229508196724, 0.00046618852459016393, 0.00046004098360655736, 0.0004559426229508197, 0.0004559426229508197, 0.00045491803278688525, 0.00045081967213114754, 0.0004610655737704918, 0.0004764344262295082, 0.00046413934426229507],
            "variance": 1.7709873018005915e-9
        },
        "times": {
            "cycle": 0.42900000000000005,
            "elapsed": 6.804,
            "period": 0.0004395491803278689,
            "timeStamp": 1469739241782
        },
        "_timerId": 19,
        "events": {
            "complete": []
        },
        "running": false,
        "count": 976,
        "cycles": 2,
        "hz": 2275.058275058275
    },
    "1": {
        "name": "test 1_2",
        "options": {
            "async": false,
            "defer": false,
            "delay": 0.005,
            "initCount": 1,
            "maxTime": 5,
            "minSamples": 5,
            "minTime": 0.075
        },
        "async": false,
        "defer": false,
        "delay": 0.005,
        "initCount": 1,
        "maxTime": 5,
        "minSamples": 5,
        "minTime": 0.075,
        "id": 2,
        "stats": {
            "moe": 0.000008990961551438582,
            "rme": 1.970634155726584,
            "sem": 0.000004587225281346215,
            "deviation": 0.00003077204267158721,
            "mean": 0.0004562471184877826,
            "sample": [0.0003195020746887967, 0.0003651452282157676, 0.0003858921161825726, 0.0004190871369294606, 0.00045643153526970956, 0.00046473029045643153, 0.00046473029045643153, 0.00046058091286307055, 0.00046473029045643153, 0.00046473029045643153, 0.00046058091286307055, 0.00048132780082987554, 0.00048132780082987554, 0.00047302904564315356, 0.00046058091286307055, 0.0005103734439834025, 0.0004688796680497925, 0.0004688796680497925, 0.0004688796680497925, 0.0004688796680497925, 0.0004688796680497925, 0.00047717842323651455, 0.0004688796680497925, 0.00047717842323651455, 0.00046473029045643153, 0.00047302904564315356, 0.00044813278008298753, 0.00046058091286307055, 0.00045228215767634857, 0.00048132780082987554, 0.00045228215767634857, 0.00045643153526970956, 0.00045228215767634857, 0.00045643153526970956, 0.00046058091286307055, 0.00046058091286307055, 0.00045643153526970956, 0.00044813278008298753, 0.00045643153526970956, 0.00047717842323651455, 0.00045643153526970956, 0.00045643153526970956, 0.00045228215767634857, 0.00045228215767634857, 0.00045643153526970956],
            "variance": 9.469186101819838e-10
        },
        "times": {
            "cycle": 0.1099555555555556,
            "elapsed": 6.189,
            "period": 0.0004562471184877826,
            "timeStamp": 1469739252973
        },
        "_timerId": 56,
        "events": {
            "complete": []
        },
        "running": false,
        "count": 241,
        "cycles": 7,
        "hz": 2191.7946645109128
    },
    "options": {},
    "length": 2,
    "events": {
        "start": [null],
        "cycle": [null],
        "abort": [null],
        "error": [null],
        "reset": [null],
        "complete": [null]
    },
    "running": false
} 
*/
