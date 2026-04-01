using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movie_Store.Models;
using Movie_Store.Models.ViewModels;
using Movie_Store.Services;

namespace MovieStore_StartHier_OK.Controllers
{
    public class MovieController : Controller
    {
        private readonly IMovieService _service;

        public MovieController(IMovieService movieService)
        {
            _service = movieService;
        }

        public IActionResult Index()
        {
            return View(_service.GetAllMovies());
        }

        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Movie movie = _service.GetMovieById(id.Value);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        [Authorize(Policy = "ProductManagerOnly")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var updateCommand = _service.GetMovieForUpdate(id.Value);

            if (updateCommand == null)
            {
                return NotFound();
            }

            PopulateViewModel(updateCommand);

            return View(updateCommand);
        }

        [Authorize(Policy = "ProductManagerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, UpdateMovieCommand cmd)
        {
            if (id != cmd.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _service.UpdateMovie(cmd);
                return RedirectToAction("Details", new { id });
            }

            PopulateViewModel(cmd);

            return View(cmd);
        }

        [Authorize(Policy = "ProductManagerOnly")]
        public IActionResult Create()
        {
            CreateMovieCommand cmd = new CreateMovieCommand();
            PopulateViewModel(cmd);

            return View(cmd);
        }

        [Authorize(Policy = "ProductManagerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CreateMovieCommand cmd)
        {
            if (ModelState.IsValid)
            {
                int id = _service.CreateMovie(cmd);
                return RedirectToAction("Details", new { id });
            }

            PopulateViewModel(cmd);
            return View(cmd);
        }

        [Authorize(Policy = "ProductManagerOnly")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            _service.DeleteMovie(id.Value);

            return RedirectToAction("Index");
        }


        private void PopulateViewModel(EditMovieBase editMovie)
        {
            editMovie.Genres = _service.GetAllGenres();
        }
    }
}
