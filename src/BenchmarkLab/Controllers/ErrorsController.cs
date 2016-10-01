using Microsoft.AspNetCore.Mvc;

namespace MeasureThat.Net.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult Code(int id)
        {
            if (id == 404)
            {
                return View("404");
            }

            return View("Error");
        }
    }
}
