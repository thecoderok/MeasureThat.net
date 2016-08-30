using System.Collections.Generic;
using System.Linq;
using MeasureThat.Net.Models;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MeasureThat.Net.Controllers
{
    [Produces("application/json")]
    [Route("api/AdminApi")]
    [Authorize(Roles = "Admin")]
    public class AdminApiController : Controller
    {
        private readonly UserManager<ApplicationUser> m_userManager;
        private readonly SignInManager<ApplicationUser> m_signInManager;

        public AdminApiController(
            [NotNull] UserManager<ApplicationUser> mUserManager,
            [NotNull] SignInManager<ApplicationUser> mSignInManager)
        {
            m_userManager = mUserManager;
            m_signInManager = mSignInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IEnumerable<ApplicationUser> GetUsers()
        {
            return m_userManager.Users.Take(50).ToList();
        }
    }
}
