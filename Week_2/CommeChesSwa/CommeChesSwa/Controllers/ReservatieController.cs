using CommeChesSwa.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CommeChesSwa.Controllers
{
    public class ReservatieController : Controller
    {
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Succes()
        {
            return View();
        }

        //POST: Home/Create
        [HttpPost]
        public IActionResult Create(GastViewModel gast)
        {
            //controleren of uw formulier goed is in ingevuld
            if (ModelState.IsValid)
            {
                //andere pagina
                //Opslaan en zo: Hier zou je normaal gezien de gegevens opslaan in een database of zo
                return RedirectToAction("Succes");

            }
            else
            {
                //Dezelfde pagina maar met de gegevens die ingevuld zijn
                return View(gast);
            }

        }

    }
}
