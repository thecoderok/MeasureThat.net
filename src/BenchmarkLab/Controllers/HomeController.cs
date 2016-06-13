using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BenchmarkLab.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this._logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult RunTest()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
