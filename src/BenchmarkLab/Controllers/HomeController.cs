using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Net.Controllers
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

        public IActionResult Error()
        {
            return View();
        }

        public IActionResult Discussions()
        {
            return View();
        }
    }
}
