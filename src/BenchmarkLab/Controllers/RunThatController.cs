
using System.Threading.Tasks;
using MeasureThat.Net.Logic.Web;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkLab.Controllers
{   
    // Run JS with HTML & CSS from the browser
    public class RunThatController : Controller
    {
        public IActionResult Index()
        {
            // View with explanation of the feature
            // Show user's workspaces
            // Should workspace only be visible to creator?
            // Or they all should be public?
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public IActionResult Add()
        {
            return View();
        }

        public IActionResult Show(long id)
        {
            // Show js/html/css and iframe with results
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            await Task.Yield();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public async Task<IActionResult> Edit(long id)
        {
            await Task.Yield();
            return RedirectToAction("Index");
        }
    }
}