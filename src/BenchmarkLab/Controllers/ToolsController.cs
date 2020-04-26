using Microsoft.AspNetCore.Mvc;

namespace BenchmarkLab.Controllers
{
    public class ToolsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult JSONBeautify()
        {
            return View();
        }

        public IActionResult JSONMinify()
        {
            return View();
        }

        public IActionResult JavaScriptBeautify()
        {
            return View();
        }

        public IActionResult HTMLBeautify()
        {
            return View();
        }

        public IActionResult CSSBeautify()
        {
            return View();
        }
        
        public IActionResult FormatSQL()
        {
            return View();
        }

        public IActionResult SortLines()
        {
            return View();
        }

        public IActionResult ComputeFileHash()
        {
            return View();
        }

        // User Agent String
        // Remote IP
        // Browser features
        // Base64 Encode/Decode
    }
}