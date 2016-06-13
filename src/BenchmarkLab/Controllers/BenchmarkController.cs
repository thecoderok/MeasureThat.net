using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkLab.Controllers
{
    [Authorize]
    public class BenchmarkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}