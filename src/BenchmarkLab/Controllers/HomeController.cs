using System.Text;
using System.Threading.Tasks;
using MeasureThat.Logic.Web.Sitemap;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Net.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SitemapGenerator sitemapGenerator;

        public HomeController(ILogger<HomeController> logger, SitemapGenerator sitemapGenerator)
        {
            this._logger = logger;
            this.sitemapGenerator = sitemapGenerator;
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

        [Route("sitemap.xml")]
        public async Task<IActionResult> Sitemap()
        {
            var sitemap = await this.sitemapGenerator.Generate();
            return this.Content(sitemap, "application/xml", Encoding.UTF8);
        }

        [Route("Faq")]
        public IActionResult Faq()
        {
            return View();
        }

        [Route("Pyodide")]
        public IActionResult Pyodide()
        {
            return View();
        }

        [Route("VersionHistory")]
        public IActionResult VersionHistory()
        {
            return View();
        }
    }
}
