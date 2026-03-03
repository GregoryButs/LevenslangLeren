using CommeChesSwa.Models;
using CommeChesSwa.Repository;
using CommeChesSwa.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;

namespace CommeChesSwa.Controllers
{
    public class ReservatieController : Controller
    {
        public IActionResult Index()
        {
            // lijst van reservaties ophalen uit de repository en doorsturen naar de view
            List<Reservatie> reservaties = ReservatieRepository.GetAll().ToList();
            return View(reservaties);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Succes()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            Reservatie reservatie = ReservatieRepository.GetById(id);
            if (reservatie == null)
            {
                return NotFound();
            }
            return View(reservatie);
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
                ReservatieRepository.Add(new Reservatie
                {
                    FirstName = gast.FirstName,
                    LastName = gast.LastName,
                    Email = gast.Email,
                    ReservationDate = gast.ReservationDate,
                    Time = gast.Time,
                    NumberOfGuests = gast.NumberOfGuests
                });
                return RedirectToAction("Succes", gast);

            }
            else
            {
                //Dezelfde pagina maar met de gegevens die ingevuld zijn
                return View(gast);
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            Reservatie reservatie = ReservatieRepository.GetById(id);
            if (reservatie != null)
            {
                ReservatieRepository.Delete(reservatie);
            }
            return RedirectToAction("Index");

        }
    }
}
