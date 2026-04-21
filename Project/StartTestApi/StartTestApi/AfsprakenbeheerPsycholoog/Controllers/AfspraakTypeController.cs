using Microsoft.AspNetCore.Mvc;

namespace AfsprakenbeheerPsycholoog.Controllers
{
    using AfsprakenbeheerPsycholoog.Data.Entities;
    using AfsprakenbeheerPsycholoog.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Controller voor het beheren van afspraaktypes, inclusief aanmaken, bewerken, verwijderen en weergeven van alle types.
    /// </summary>
    
    [Authorize(Policy = "PsycholoogOnly")]
    public class AfspraakTypeController : Controller
    {
        private readonly IAfspraakTypeService _service;

        public AfspraakTypeController(IAfspraakTypeService service)
        {
            _service = service;
        }

        // GET: AfspraakType
        public IActionResult Index()
        {
            var types = _service.GetAlleTypes();
            return View(types);
        }

        // GET: AfspraakType/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: AfspraakType/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AfspraakType model)
        {
            if (!ModelState.IsValid) return View(model);

            _service.CreateType(model);
            TempData["SuccesMessage"] = $"Type '{model.Naam}' is aangemaakt.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AfspraakType/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var type = _service.GetTypeById(id);
            if (type == null) return NotFound();

            return View(type);
        }

        // POST: AfspraakType/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AfspraakType model)
        {
            if (!ModelState.IsValid) return View(model);

            _service.EditType(model);
            TempData["SuccesMessage"] = $"Type '{model.Naam}' is bijgewerkt.";
            return RedirectToAction(nameof(Index));
        }

        // POST: AfspraakType/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _service.DeleteType(id);
            TempData["SuccesMessage"] = "Type is verwijderd.";
            return RedirectToAction(nameof(Index));
        }
    }

}
