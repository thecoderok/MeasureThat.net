using BenchmarkLab.Data;
using BenchmarkLab.Logic.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BenchmarkLab.Controllers
{
    [Authorize]
    public class BenchmarksController : Controller
    {
        private ApplicationDbContext m_context;

        public BenchmarksController(ApplicationDbContext context)
        {
            this.m_context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            var bencmmarks = this.m_context.Benchmark;
            return View();
        }

        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public IActionResult Dummy()
        {
            if (ModelState.IsValid)
            {
                @ViewData["Message"] = "Model valid";
            }
            else
            {
                @ViewData["Message"] = "Invalid!!!";
            }
            return View();
        }
    }
}