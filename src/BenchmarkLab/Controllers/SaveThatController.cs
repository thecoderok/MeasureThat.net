using MeasureThat.Net.Logic.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkLab.Controllers
{
    public class SaveThatController : Controller
    {
        // GET: SaveThat
        public ActionResult Index()
        {
            return View();
        }

        // GET: SaveThat/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SaveThat/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SaveThat/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SaveThat/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: SaveThat/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ValidateReCaptchaAttribute))]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: SaveThat/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SaveThat/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}