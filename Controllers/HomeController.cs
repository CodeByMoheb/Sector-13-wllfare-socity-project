using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using System.Linq;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [OutputCache(PolicyName = "Public300")]
        public IActionResult Index()
        {
            var latestNotice = _context.Notices.Where(n => n.IsApproved).OrderByDescending(n => n.ApprovedAt).FirstOrDefault();
            ViewBag.LatestNotice = latestNotice;
            return View();
        }
        

// In Controllers/HomeController.cs
        public IActionResult Gallery()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult MessageOfChairman()
        {
            return View();
        }
          public IActionResult MessageOfSecretary()
        {
            return View();
        }

        public IActionResult HowDoWeWork()
        {
            return View();
        }

        public IActionResult memberDirectory()
        {
            return View();
        }

        public IActionResult ElectedCandidates()
        {
            return View();
        }
         public IActionResult PreviousElectedCandidate()
        {
            return View();
        }

        public IActionResult SubComitys()
        {
            return View();
        }
        public IActionResult CONTRTRIBUTORS()
        {
            return View();
        }


        public IActionResult donate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (!string.IsNullOrEmpty(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                );
            }
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = Url.Action("Index", "Home");
            }
            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
      
    }
}
