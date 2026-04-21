using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    /// <summary>
    /// Controller voor het beheren van het patiëntenportaal, inclusief het bekijken, boeken en annuleren van afspraken.
    /// </summary>
    [Authorize]
    public class PatientPortaalController : Controller
    {
        private readonly IAfspraakService _afspraakService;
        private readonly IPatientBoekService _patientBoekService;

        public PatientPortaalController(
            IAfspraakService afspraakService,
            IPatientBoekService patientBoekService)
        {
            _afspraakService = afspraakService;
            _patientBoekService = patientBoekService;
        }

        public IActionResult MijnAfspraken()
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue) return View("GeenProfiel");

            var afspraken = _afspraakService.GetAfsprakenVanPatient(patientId.Value);
            return View(afspraken);
        }

        public IActionResult Boeken(DateTime? datum)
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue) return View("GeenProfiel");

            var minimumBoekdatum = PraktijkInstellingen.EerstVolgendeWerkdag(DateTime.Today.AddDays(1));
            var gevraagdeDatum = datum ?? minimumBoekdatum;

            if (gevraagdeDatum.Date < minimumBoekdatum.Date)
            {
                gevraagdeDatum = minimumBoekdatum;
            }

            var dag = PraktijkInstellingen.EerstVolgendeWerkdag(gevraagdeDatum);

            var overzicht = _patientBoekService.GetDagOverzichtVoorPatient(dag);
            overzicht.MinimumNavigatieDatum = minimumBoekdatum;

            ViewBag.DagOverzicht = overzicht;
            var vm = _patientBoekService.GetBoekViewModel(dag);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Boeken(PatientBoekAfspraakViewModel vm)
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue) return View("GeenProfiel");

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Boeken), new { datum = vm.GekozeTijdslot?.Date ?? DateTime.Today });
            }

            var succes = _patientBoekService.CreatePatientAfspraak(vm, patientId.Value);

            TempData[succes ? "SuccesMessage" : "ErrorMessage"] =
                succes ? "Je afspraak is geboekt!"
                       : "Dit tijdslot is niet meer beschikbaar. Kies een ander moment.";

            return succes
                ? RedirectToAction(nameof(MijnAfspraken))
                : RedirectToAction(nameof(Boeken), new { datum = vm.GekozeTijdslot?.Date ?? DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Annuleren(int id)
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue) return View("GeenProfiel");

            var succes = _patientBoekService.AnnuleerPatientAfspraak(id, patientId.Value);

            TempData[succes ? "SuccesMessage" : "ErrorMessage"] =
                succes ? "Je afspraak is geannuleerd."
                       : "Deze afspraak kan niet meer geannuleerd worden.";

            return RedirectToAction(nameof(MijnAfspraken));
        }

        // Deze methode haalt het PatientId op uit de claims van de ingelogde gebruiker.
        // Het gaat ervan uit dat er een claim met de naam "PatientId" aanwezig is,
        // wat meestal het geval is als je een aangepaste gebruikersidentiteit hebt ingesteld bij het inloggen van patiënten.
        // Als de claim niet aanwezig is of niet kan worden omgezet naar een int, retourneert deze methode null.
        private int? GetPatientId()
        {
            var patientIdClaim = User.FindFirstValue("PatientId");
            if (string.IsNullOrWhiteSpace(patientIdClaim))
            {
                return null;
            }

            // tryParse gebruiken om te voorkomen dat er een uitzondering wordt gegooid als de claim niet correct is geformatteerd
            return int.TryParse(patientIdClaim, out var patientId)
                ? patientId
                : null;
        }
    }
}
