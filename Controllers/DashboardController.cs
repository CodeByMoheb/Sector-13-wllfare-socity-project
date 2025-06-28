using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var roles = await _userManager.GetRolesAsync(user);
            
            // Redirect to role-specific dashboard
            if (roles.Contains("Admin"))
                return RedirectToAction("Admin");
            else if (roles.Contains("President"))
                return RedirectToAction("President");
            else if (roles.Contains("Secretary"))
                return RedirectToAction("Secretary");
            else if (roles.Contains("Manager"))
                return RedirectToAction("Manager");
            else if (roles.Contains("Member"))
                return RedirectToAction("Member");
            else
                return RedirectToAction("Member"); // Default fallback
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.Role = "Admin";
            return View();
        }

        [Authorize(Roles = "President")]
        public IActionResult President()
        {
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.Role = "President";
            return View();
        }

        [Authorize(Roles = "Secretary")]
        public IActionResult Secretary()
        {
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.Role = "Secretary";
            return View();
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Manager()
        {
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.Role = "Manager";
            return View();
        }

        [Authorize(Roles = "Member")]
        public IActionResult Member()
        {
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.Role = "Member";
            return View();
        }
    }
} 