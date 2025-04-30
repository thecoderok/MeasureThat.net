using System.Threading.Tasks;
using JetBrains.Annotations;
using MeasureThat.Net.Data.Dao;
using MeasureThat.Net.Logic.Web;
using MeasureThat.Net.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MeasureThat.Controllers
{
    public class SaveThatController : Controller
    {
        private readonly SqlServerSaveThatBlobReporitory m_saveThatBlobReporitory;
        private readonly ILogger m_logger;
        private readonly UserManager<ApplicationUser> m_userManager;

        public SaveThatController(
            [NotNull] SqlServerSaveThatBlobReporitory saveThatBlobReporitory,
            [NotNull] UserManager<ApplicationUser> userManager,
            [NotNull] ILoggerFactory loggerFactory)
        {
            this.m_saveThatBlobReporitory = saveThatBlobReporitory;
            this.m_userManager = userManager;
            this.m_logger = loggerFactory.CreateLogger<SaveThatController>();
        }

        // GET: SaveThat
        public async Task<IActionResult> Index()
        {
            var user = await this.m_userManager.GetUserAsync(this.HttpContext.User);
            await m_saveThatBlobReporitory.Add(user.Id);
            var entities = await m_saveThatBlobReporitory.ListAll(25, 0);
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