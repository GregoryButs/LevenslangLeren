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
            return View(genres);
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
        public ActionResult Edit(int id)
        {
            Genre genre = _service.GetGenreById(id);
            GenreViewModel genreVm = new GenreViewModel
            {
                GenreId = genre.GenreId,
                Name = genre.Name
            };
            return View(genreVm);
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
            return View(genreVm);
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
