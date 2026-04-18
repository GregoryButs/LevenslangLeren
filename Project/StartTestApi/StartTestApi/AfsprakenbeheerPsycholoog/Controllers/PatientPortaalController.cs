using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    [Authorize]
    public class PatientPortaalController : Controller
    {
        private readonly IAfspraakService _afspraakService;
        private readonly IPatientBoekService _patientBoekService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientPortaalController(
            IAfspraakService afspraakService,
            IPatientBoekService patientBoekService,
            UserManager<ApplicationUser> userManager)
        {
            _afspraakService = afspraakService;
            _patientBoekService = patientBoekService;
            _userManager = userManager;
        }

        public async Task<IActionResult> MijnAfspraken()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.PatientId == null) return View("GeenProfiel");

            var afspraken = _afspraakService.GetAfsprakenVanPatient(user.PatientId.Value);
            return View(afspraken);
        }

        public async Task<IActionResult> Boeken(DateTime? datum)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.PatientId == null) return View("GeenProfiel");

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
        public async Task<IActionResult> Boeken(PatientBoekAfspraakViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.PatientId == null) return View("GeenProfiel");

            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Boeken), new { datum = vm.GekozeTijdslot?.Date ?? DateTime.Today });
            }

            var succes = _patientBoekService.CreatePatientAfspraak(vm, user.PatientId.Value);

            TempData[succes ? "SuccesMessage" : "ErrorMessage"] =
                succes ? "Je afspraak is geboekt!"
                       : "Dit tijdslot is niet meer beschikbaar. Kies een ander moment.";

            return succes
                ? RedirectToAction(nameof(MijnAfspraken))
                : RedirectToAction(nameof(Boeken), new { datum = vm.GekozeTijdslot?.Date ?? DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Annuleren(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user?.PatientId == null) return View("GeenProfiel");

            var succes = _patientBoekService.AnnuleerPatientAfspraak(id, user.PatientId.Value);

            TempData[succes ? "SuccesMessage" : "ErrorMessage"] =
                succes ? "Je afspraak is geannuleerd."
                       : "Deze afspraak kan niet meer geannuleerd worden.";

            return RedirectToAction(nameof(MijnAfspraken));
        }
    }
}
