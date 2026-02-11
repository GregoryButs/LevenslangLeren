using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Models;

namespace MusicApp.Controllers
{
    public class AlbumController : Controller
    {
        private AlbumRepository _repo;

        public AlbumController()
        {
            _repo = new AlbumRepository();
        }


        public IActionResult Index()
        {
            List<Album> albums = _repo.GetAll();
            return View(albums);
        }

        public IActionResult Details(int id)
        {
            Album album = _repo.FindById(id);

            if (album == null)
            {
                return RedirectToAction("Index");
            }

            return View(album);
        }
    }
}
