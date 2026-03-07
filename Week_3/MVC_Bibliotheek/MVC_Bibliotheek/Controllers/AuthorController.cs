using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MVC_Bibliotheek.Data;
using MVC_Bibliotheek.Data.Entities;
using MVC_Bibliotheek.Models;

namespace MVC_Bibliotheek.Controllers
{
    public class AuthorController : Controller
    {
        private BibService _service = new (new BibDbContext());

        private readonly ILogger<AuthorController> _logger;

        public AuthorController(ILogger<AuthorController> logger)
        {
            _logger = logger;
        }

        // GET: AuthorController
        public ActionResult Index()
        {
            IEnumerable<Author> authors = _service.GetAuthors();
            return View(authors);
        }

        // GET: AuthorController/Details/5
        public ActionResult DetailsAuthor(int id)
        {
            Author author = _service.GetAuthorById(id);
            return View(author);
        }

        // GET: AuthorController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AuthorController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AuthorViewModel authorVm)
        {
            if (ModelState.IsValid)
            {
                _service.AddAuthor(new Author
                {
                    FirstName = authorVm.FirstName,
                    LastName = authorVm.LastName,
                    Birthdate = authorVm.Birthdate
                });
                return RedirectToAction("Index", authorVm);

            }
            return View(authorVm);
        }

        // GET: AuthorController/Edit/5
        public ActionResult Edit(int id)
        {
            Author author = _service.GetAuthorById(id);
            AuthorViewModel authorVM = new AuthorViewModel
            {
                AuthorId = author.AuthorId,
                FirstName = author.FirstName,
                LastName = author.LastName,
                Birthdate = author.Birthdate
            };
            return View(authorVM);
        }

        // POST: AuthorController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AuthorViewModel autorVm)
        {
            if (ModelState.IsValid)
            {
                Author author = _service.GetAuthorById(autorVm.AuthorId);
                author.FirstName = autorVm.FirstName;
                author.LastName = autorVm.LastName;
                author.Birthdate = autorVm.Birthdate;

                _service.UpdateAuthor(author);
                return RedirectToAction("Index");
            }
            else
            {
                return View(autorVm);
            }
        }

        // POST: AuthorController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _service.DeleteAuthor(id);
            return RedirectToAction("Index");
        }
    }
}
