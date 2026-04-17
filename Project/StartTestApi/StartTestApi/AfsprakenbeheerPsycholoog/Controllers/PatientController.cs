using Microsoft.AspNetCore.Mvc;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;

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
            _service.DeletePatient(id);
            TempData["SuccesMessage"] = "Patiënt is verwijderd.";
            return RedirectToAction(nameof(Index));
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
    }
}
