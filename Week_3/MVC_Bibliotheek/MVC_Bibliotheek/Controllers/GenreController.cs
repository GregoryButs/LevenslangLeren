using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVC_Bibliotheek.Data;
using MVC_Bibliotheek.Data.Entities;
using MVC_Bibliotheek.Models;

namespace MVC_Bibliotheek.Controllers
{
    public class GenreController : Controller
    {
        private BibService _service = new(new BibDbContext());

        private readonly ILogger<GenreController> _logger;

        public GenreController(ILogger<GenreController> logger)
        {
            _logger = logger;
        }

        // GET: GenreController
        public ActionResult Index()
        {
            IEnumerable<Genre> genres = _service.GetGenres();
            var viewModel = new GenreIndexViewModel
            {
                Genres = genres.Select(g => new GenreViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                })
            };
            return View(viewModel);
        }

        // GET: GenreController/Details/5
        public ActionResult Details(int id)
        {
            Genre genre = _service.GetGenreById(id);
            return View(genre);
        }

        // GET: GenreController/Create
        public ActionResult Create()
        {
            var genreVm = new GenreViewModel();
            return View(genreVm);
        }

        // POST: GenreController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(GenreViewModel genreVm)
        {
            if (ModelState.IsValid)
            {
                _service.AddGenre(new Genre
                {
                    Name = genreVm.Name
                });
                return RedirectToAction("Index");
            }
            return View(genreVm);
        }

        // GET: GenreController/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            IEnumerable<Genre> genres = _service.GetGenres();
            var viewModel = new GenreIndexViewModel
            {
                Genres = genres.Select(g => new GenreViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                }),
                EditingGenreId = id
            };
            return View("Index", viewModel);
        }

        // POST: GenreController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(GenreViewModel genreVm)
        {
            if (ModelState.IsValid)
            {
                _service.UpdateGenre(new Genre
                {
                    GenreId = genreVm.GenreId,
                    Name = genreVm.Name
                });
                return RedirectToAction("Index");
            }
            
            // Als validatie faalt, toon de index pagina opnieuw met de edit mode actief
            IEnumerable<Genre> genres = _service.GetGenres();
            var viewModel = new GenreIndexViewModel
            {
                Genres = genres.Select(g => new GenreViewModel
                {
                    GenreId = g.GenreId,
                    Name = g.Name
                }),
                EditingGenreId = genreVm.GenreId
            };
            return View("Index", viewModel);
        }


        // POST: GenreController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _service.DeleteGenre(id);
            return RedirectToAction("Index");
        }
    }
}
