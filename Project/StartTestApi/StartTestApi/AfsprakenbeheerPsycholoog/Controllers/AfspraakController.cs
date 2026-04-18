using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    [Authorize(Policy = "PsycholoogOnly")]
    public class AfspraakController : Controller
    {
        private readonly IAfspraakService _afspraakService;

        public AfspraakController(IAfspraakService afspraakService)
        {
            _afspraakService = afspraakService;
        }

        // GET
        public IActionResult Index()
        {
            var afspraken = _afspraakService.GetAlleAfspraken();
            return View(afspraken);
        }
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var afspraak = _afspraakService.GetAfspraakDetail(id.Value);
            if (afspraak == null)
            {
                return NotFound();
            }

            return View(afspraak);
        }

        // --- DAGPLANNING ---
        public IActionResult Dagplanning(DateTime? datum)
        {
            var dag = datum ?? DateTime.Today;
            return View(_afspraakService.GetDagOverzicht(dag));
        }

        // CREATE
        [HttpGet]
        public IActionResult Create(DateTime? starttijd)
        {
            var vm = _afspraakService.GetCreateViewModel();
            if (starttijd.HasValue) vm.Starttijd = starttijd.Value;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateAfspraakViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var nieuwVm = _afspraakService.GetCreateViewModel();
                // SelectLists opnieuw vullen!
                vm.PatientenLijst = nieuwVm.PatientenLijst;
                vm.TypenLijst = nieuwVm.TypenLijst;
                return View(vm);
            }

            // Controleer of de service erin slaagde zonder conflict op te slaan
            if (!_afspraakService.CreateAfspraak(vm))
            {
                TempData["ErrorMessage"] = "Kies een ander moment: dit tijdstip overlapt met een reeds geplande afspraak.";
                var nieuwVm = _afspraakService.GetCreateViewModel();
                vm.PatientenLijst = nieuwVm.PatientenLijst;
                vm.TypenLijst = nieuwVm.TypenLijst;
                return View(vm);
            }

            TempData["SuccesMessage"] = "Afspraak is ingepland.";
            return RedirectToAction(nameof(Dagplanning), new { datum = vm.Starttijd.ToString("yyyy-MM-dd") });
        }

        // EDIT
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var vm = _afspraakService.GetEditViewModel(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditAfspraakViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Dropdowns opnieuw vullen bij validatiefout!
                var editVm = _afspraakService.GetEditViewModel(vm.Id);
                vm.PatientenLijst = editVm.PatientenLijst;
                vm.TypenLijst = editVm.TypenLijst;
                vm.StatusLijst = editVm.StatusLijst;
                return View(vm);
            }

            // Controleer of de service erin slaagde zonder conflict op te slaan
            if (!_afspraakService.EditAfspraak(vm))
            {
                // Gebruik TempData in plaats van ModelState
                TempData["ErrorMessage"] = "Kies een ander moment: dit tijdstip overlapt met een reeds geplande afspraak.";
                var editVm = _afspraakService.GetEditViewModel(vm.Id);
                vm.PatientenLijst = editVm.PatientenLijst;
                vm.TypenLijst = editVm.TypenLijst;
                vm.StatusLijst = editVm.StatusLijst;
                return View(vm);
            }

            TempData["SuccesMessage"] = "Afspraak is bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _afspraakService.DeleteAfspraak(id);
            TempData["SuccesMessage"] = "Afspraak is verwijderd.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteVanDagplanning(int id, DateTime datum)
        {
            _afspraakService.DeleteAfspraak(id);
            TempData["SuccesMessage"] = "Item is verwijderd.";
            return RedirectToAction(nameof(Dagplanning), new { datum = datum.ToString("yyyy-MM-dd") });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReeks(Guid reeksId)
        {
            if (reeksId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Ongeldige reeks.";
                return RedirectToAction(nameof(Index));
            }

            _afspraakService.DeleteReeks(reeksId);
            TempData["SuccesMessage"] = "Volledige reeks is verwijderd.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReeksVanDagplanning(Guid reeksId, DateTime datum)
        {
            if (reeksId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Ongeldige reeks.";
                return RedirectToAction(nameof(Dagplanning), new { datum = datum.ToString("yyyy-MM-dd") });
            }

            _afspraakService.DeleteReeks(reeksId);
            TempData["SuccesMessage"] = "Volledige reeks is verwijderd.";
            return RedirectToAction(nameof(Dagplanning), new { datum = datum.ToString("yyyy-MM-dd") });
        }
    }
}
