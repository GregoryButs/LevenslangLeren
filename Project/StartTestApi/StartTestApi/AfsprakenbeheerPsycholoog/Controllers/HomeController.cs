using AfsprakenbeheerPsycholoog.Models;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    /// <summary>
    /// Controller voor het beheren van de startpagina en het dashboard, inclusief het weergeven van weekoverzichten en foutpagina's.
    /// </summary>
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public HomeController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }


        // De indexactie bepaalt op basis van de gebruikersrol of de gebruiker naar het dashboard of het weekoverzicht wordt geleid.
        [AllowAnonymous]
        public IActionResult Index(DateTime? weekDatum = null)
        {
            if (User.HasClaim("IsPsycholoog", "true"))
            {
                return RedirectToAction(nameof(Dashboard), new { weekDatum });
            }

            return RedirectToAction(nameof(WeekOverzicht), new { weekDatum });
        }

        // De dashboardactie is alleen toegankelijk voor psychologen en toont een overzicht van afspraken, statistieken en een weekoverzicht.
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult Dashboard(DateTime? weekDatum = null)
        {
            var peilDatum = weekDatum ?? DateTime.Today;
            var naam = User.Identity?.Name ?? "Psycholoog";
            var dashboard = _dashboardService.GetDashboard(naam, peilDatum);

            return View("Dashboard", dashboard);
        }

        // De weekoverzichtactie is toegankelijk voor alle gebruikers en toont een overzicht van afspraken voor een specifieke week, afhankelijk van de datum en de patiënt.
        [AllowAnonymous]
        public IActionResult WeekOverzicht(DateTime? weekDatum = null)
        {
            var peilDatum = weekDatum ?? DateTime.Today;

            int? patientId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var patientIdClaim = User.FindFirstValue("PatientId");
                if (!string.IsNullOrWhiteSpace(patientIdClaim)
                    && int.TryParse(patientIdClaim, out var parsedPatientId))
                {
                    patientId = parsedPatientId;
                }
            }

            var weekCal = _dashboardService.GetWeekOverzicht(peilDatum, patientId, false);
            return View("Index", weekCal);
        }

        // De erroractie toont een foutpagina met details over de fout, inclusief een unieke request-id voor debuggingdoeleinden.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // De statuscodeactie toont een foutpagina op basis van de opgegeven statuscode, met een bijbehorende foutmelding.
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
