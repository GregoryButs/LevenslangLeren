using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Models;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IDashboardService dashboardService, UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? weekDatum = null)
        {
            var peilDatum = weekDatum ?? DateTime.Today;

            if (User.HasClaim("IsPsycholoog", "true"))
            {
                var naam = User.Identity?.Name ?? "Psycholoog";
                var dashboard = _dashboardService.GetDashboard(naam, peilDatum);
                return View("Dashboard", dashboard);
            }

            int? patientId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var appUser = await _userManager.GetUserAsync(User);
                if (appUser?.PatientId.HasValue == true)
                {
                    patientId = appUser.PatientId.Value;
                }
            }

            var weekCal = _dashboardService.GetWeekOverzicht(peilDatum, patientId, false);
            return View(weekCal);
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
