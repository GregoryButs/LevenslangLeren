using System.Diagnostics;
using DemoKlasAuthenticatie.Authenticatie;
using DemoKlasAuthenticatie.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DemoKlasAuthenticatie.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [Authorize(Roles = "AdminOnly")]
        public IActionResult Admin()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            Response.Cookies.Append("Lang", "NL");

            // stel een sessie op om de taal van de gebruiker op te slaan
            HttpContext.Session.SetString("Lang", "NL");

            // object van persoon maken en serialiseren naar json
            Person person = new Person { Id = 1, FirstName = "John", LastName = "Doe" };
            HttpContext.Session.SetString("Person", System.Text.Json.JsonSerializer.Serialize(person));

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                var userName = _userManager.GetUserName(User);
                ApplicationUser user = _userManager.GetUserAsync(User).Result;
                ViewBag.UserInfo = $"{user.FirstName} {user.LastName}";
            }
            return View();
        }

        public IActionResult Privacy()
        {
            // cookie uitlezen
            Request.Cookies.TryGetValue("Lang", out string lang);
            if (lang == null)
            {
                ViewBag.Lang = "EN";
            }
            else
            {
                ViewBag.Lang = lang;
            }

            // sessie uitlezen
            ViewBag.SessionLang = HttpContext.Session.GetString("Lang") ?? "EN";
            Person p = HttpContext.Session.GetString("Person") != null ? System.Text.Json.JsonSerializer.Deserialize<Person>(HttpContext.Session.GetString("Person")) : null;
            ViewBag.Person = $"{p.FirstName} {p.LastName}";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
