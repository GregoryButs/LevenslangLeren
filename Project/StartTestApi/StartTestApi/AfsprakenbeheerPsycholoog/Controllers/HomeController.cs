using AfsprakenbeheerPsycholoog.Models;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IAfspraakService _afspraakService;

        public HomeController(IAfspraakService afspraakService)
        {
            _afspraakService = afspraakService;
        }

        public IActionResult Index()
        {
            // Psycholoog ziet dashboard, anderen zien welkomstpagina
            if (User.HasClaim("IsPsycholoog", "true"))
            {
                var naam = User.Identity?.Name ?? "Psycholoog";
                var dashboard = _afspraakService.GetDashboard(naam);
                return View("Dashboard", dashboard);
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult StatusCode(int code)
        {
            ViewBag.ErrorCode = code;
            ViewBag.ErrorMessage = code switch
            {
                404 => "Pagina niet gevonden",
                403 => "Geen toegang",
                _ => "Er ging iets mis"
            };
            return View();
        }
    }
}
