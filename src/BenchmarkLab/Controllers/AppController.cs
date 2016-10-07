using Microsoft.AspNetCore.Mvc;

namespace MeasureThat.Net.Controllers
{
    public class AppController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
