using BenchmarkLab.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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


    }
}