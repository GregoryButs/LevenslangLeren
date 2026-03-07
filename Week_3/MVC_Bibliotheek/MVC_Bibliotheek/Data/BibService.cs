using Microsoft.EntityFrameworkCore;
using MVC_Bibliotheek.Data.Entities;

namespace MVC_Bibliotheek.Data
{
    public class BibService
    {
        private BibDbContext _context;

        public BibService(BibDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Author> GetAuthors()
        {
            var authors = _context.Authors
                .Include(a => a.Books)
                .ToList();
            return authors;
        }

        public IEnumerable<Book> GetBooks()
        {
            var books = _context.Books.
                Where(b => !b.IsDeleted)
                .Include(b => b.Author)
                .Include(b => b.Genre)
                 .ToList();
            return books;
        }

        public IEnumerable<Genre> GetGenres()
        {
            var genres = _context.Genres
                .Include(g => g.Books)
                .ToList();
            return genres;
        }

        public Author GetAuthorById(int id)
        {
            var author = _context.Authors
                .Include(a => a.Books)
                .FirstOrDefault(a => a.AuthorId == id);
            return author;
        }

        public Book GetBookById(int id)
        {
            var book = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .FirstOrDefault(b => b.BookId == id);
            return book;
        }

        public Genre GetGenreById(int id)
        {
            var genre = _context.Genres
                .Include(g => g.Books)
                .FirstOrDefault(g => g.GenreId == id);
            return genre;
        }

        public void AddAuthor(Author author)
        {
            _context.Authors.Add(author);
            _context.SaveChanges();
        }

        public void AddBook(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
        }

        public void AddGenre(Genre genre)
        {
            _context.Genres.Add(genre);
            _context.SaveChanges();
        }
         public void UpdateAuthor(Author author)
        {
            _context.Authors.Update(author);
            _context.SaveChanges();
        }

        public void UpdateBook(Book book)
        {
            _context.Books.Update(book);
            _context.SaveChanges();
        }

        public void UpdateGenre(Genre genre)
        {
            _context.Genres.Update(genre);
            _context.SaveChanges();
        }

        public void DeleteAuthor(int id)
        {
            var author = _context.Authors.Find(id);
            if (author != null)
            {
                _context.Authors.Remove(author);
                _context.SaveChanges();
            }
        }

        public void DeleteBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book != null)
            {
                book.IsDeleted = true;
                _context.Books.Update(book);
                _context.SaveChanges();
            }
        }

        public void DeleteGenre(int id)
        {
            var genre = _context.Genres.Find(id);
            if (genre != null)
            {
                _context.Genres.Remove(genre);
                _context.SaveChanges();
            }
        }
    }
}
