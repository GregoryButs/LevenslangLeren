using Microsoft.AspNetCore.Mvc;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;
using AfsprakenbeheerPsycholoog.Authentication;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    [Authorize(Policy = "PsycholoogOnly")]
    public class PatientController : Controller
    {
        private readonly IPatientService _service;

        public PatientController(IPatientService service)
        {
            _service = service;
        }

        // GET: Patient
        public IActionResult Index()
        {
            var patienten = _service.GetAllePatienten();
            return View(patienten);
        }

        // GET: Patient/Details/
        public IActionResult Details(int id)
        {
            var patient = _service.GetPatientDetail(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: Patient/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patient/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreatePatientViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            _service.CreatePatient(vm);
            TempData["SuccesMessage"] = $"Patiënt '{vm.Voornaam} {vm.Achternaam}' is aangemaakt.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Patient/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var patient = _service.GetPatientForEdit(id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patient/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EditPatientViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            _service.EditPatient(vm);
            TempData["SuccesMessage"] = "Patiënt is bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Patient/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                _service.DeletePatient(id);
                TempData["SuccesMessage"] = "Patiënt is op inactief gezet.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Het was niet mogelijk om de patiënt op inactief te zetten.";
            }
            return RedirectToAction(nameof(Index));
        }
        
        // --- NIEUWE AANMELDINGEN BEHEREN ---

        public async Task<IActionResult> NieuweAanmeldingen()
        {
            var nieuweGebruikers = await _service.GetNieuweAanmeldingenAsync();
            ViewBag.PatientenLijst = new SelectList(_service.GetAllePatienten(), "Id", "VolledigeNaam");
            
            return View(nieuweGebruikers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MaakNieuwePatient(string userId)
        {
            var (succes, naam) = await _service.MaakEnKoppelNieuwePatientAsync(userId);
            
            if (!succes) return NotFound();

            TempData["SuccesMessage"] = $"Nieuwe patiënt '{naam}' aangemaakt en gekoppeld.";
            return RedirectToAction(nameof(NieuweAanmeldingen));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KoppelBestaandePatient(string userEmail, int existingPatientId)
        {
            var succes = _service.KoppelPatientAanUser(existingPatientId, userEmail);
            
            TempData[succes ? "SuccesMessage" : "ErrorMessage"] =
                succes ? $"Account {userEmail} succesvol gekoppeld aan patiënt."
                       : $"Kon account niet koppelen. Bestaat deze niet of is hij al gekoppeld?";
                       
            return RedirectToAction(nameof(NieuweAanmeldingen));
        }

        // --- KOPPELING USER ↔ PATIENT ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Koppel(int patientId, string email)
        {
            var succes = _service.KoppelPatientAanUser(patientId, email);
            TempData[succes ? "SuccesMessage" : "ErrorMessage"] =
                succes ? $"Patiënt is gekoppeld aan {email}."
                       : $"Geen account gevonden voor '{email}', of account is al gekoppeld.";
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Ontkoppel(int patientId)
        {
            _service.OntkoppelPatientVanUser(patientId);
            TempData["SuccesMessage"] = "Account is ontkoppeld van de patiënt.";
            return RedirectToAction(nameof(Details), new { id = patientId });
        }

        // GET: Patient/Archief
        public IActionResult Archief()
        {
            var patienten = _service.GetInactievePatienten();
            return View(patienten);
        }

        // POST: Patient/Heractiveer/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Heractiveer(int id)
        {
            var ok = _service.HeractiveerPatient(id);

            TempData[ok ? "SuccesMessage" : "ErrorMessage"] =
                ok ? "Patiënt is opnieuw actief gezet."
                   : "Patiënt kon niet heractiveerd worden.";

            return RedirectToAction(nameof(Archief));
        }
    }
}
