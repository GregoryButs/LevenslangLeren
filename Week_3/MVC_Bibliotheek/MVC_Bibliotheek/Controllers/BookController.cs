using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVC_Bibliotheek.Data;
using MVC_Bibliotheek.Data.Entities;
using MVC_Bibliotheek.Models;

namespace MVC_Bibliotheek.Controllers
{
    public class BookController : Controller
    {
        private BibService _service = new(new BibDbContext());
        private readonly ILogger<BookController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public BookController(ILogger<BookController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: BookController
        public ActionResult BookList()
        {
            IEnumerable<Book> books = _service.GetBooks().Where(b => !b.IsDeleted);
            return View(books);
        }

        // GET: BookController/Details/5
        public ActionResult Details(int id)
        {
            Book book = _service.GetBookById(id);
            return View(book);
        }

        // GET: BookController/Create
        public ActionResult Create()
        {
            var bookVm = new BookViewModel();
            PopulateDropdowns(bookVm);
            return View(bookVm);
        }

        // POST: BookController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BookViewModel bookVm)
        {
            if (ModelState.IsValid)
            {
                string? coverImagePath = null;

                // Upload cover image indien aanwezig
                if (bookVm.CoverImage != null)
                {
                    coverImagePath = await UploadCoverImage(bookVm.CoverImage);
                }

                _service.AddBook(new Book
                {
                    Title = bookVm.Title,
                    ISBN13 = bookVm.ISBN13,
                    Publisher = bookVm.Publisher,
                    PublicationDate = bookVm.PublicationDate,
                    Pages = bookVm.Pages,
                    AuthorId = bookVm.AuthorId,
                    GenreId = bookVm.GenreId,
                    CoverImagePath = coverImagePath
                });
                return RedirectToAction("BookList");
            }

            // Hervul de dropdowns bij validatiefouten
            PopulateDropdowns(bookVm);
            return View(bookVm);
        }

        // GET: BookController/Edit/5
        public ActionResult Edit(int id)
        {
            Book book = _service.GetBookById(id);
            BookViewModel bookVm = new BookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                ISBN13 = book.ISBN13,
                Publisher = book.Publisher,
                PublicationDate = book.PublicationDate,
                Pages = book.Pages,
                AuthorId = book.AuthorId,
                GenreId = book.GenreId,
                ExistingCoverImagePath = book.CoverImagePath
            };
            PopulateDropdowns(bookVm);
            return View(bookVm);
        }

        // POST: BookController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(BookViewModel bookVm)
        {
            if (ModelState.IsValid)
            {
                Book book = _service.GetBookById(bookVm.BookId);
                book.Title = bookVm.Title;
                book.ISBN13 = bookVm.ISBN13;
                book.Publisher = bookVm.Publisher;
                book.PublicationDate = bookVm.PublicationDate;
                book.Pages = bookVm.Pages;
                book.AuthorId = bookVm.AuthorId;
                book.GenreId = bookVm.GenreId;

                // Upload nieuwe cover image indien aanwezig
                if (bookVm.CoverImage != null)
                {
                    // Verwijder oude cover image indien aanwezig
                    if (!string.IsNullOrEmpty(book.CoverImagePath))
                    {
                        DeleteCoverImage(book.CoverImagePath);
                    }

                    book.CoverImagePath = await UploadCoverImage(bookVm.CoverImage);
                }

                _service.UpdateBook(book);
                return RedirectToAction("BookList");
            }

            // Hervul de dropdowns bij validatiefouten
            PopulateDropdowns(bookVm);
            return View(bookVm);
        }

        // POST: BookController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            _service.DeleteBook(id);
            return RedirectToAction("BookList");
        }

        private void PopulateDropdowns(BookViewModel bookVm)
        {
            bookVm.Authors = _service.GetAuthors()
                .Select(a => new SelectListItem
                {
                    Value = a.AuthorId.ToString(),
                    Text = $"{a.FirstName} {a.LastName}"
                });
            bookVm.Genres = _service.GetGenres()
                .Select(g => new SelectListItem
                {
                    Value = g.GenreId.ToString(),
                    Text = g.Name
                });
        }

        private async Task<string> UploadCoverImage(IFormFile coverImage)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "covers");

            // Maak de folder aan indien deze niet bestaat
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Genereer een unieke bestandsnaam
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + coverImage.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Upload het bestand
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await coverImage.CopyToAsync(fileStream);
            }

            // Return het relatieve pad
            return "/images/covers/" + uniqueFileName;
        }

        private void DeleteCoverImage(string coverImagePath)
        {
            if (!string.IsNullOrEmpty(coverImagePath))
            {
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, coverImagePath.TrimStart('/'));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
        }
    }
}


