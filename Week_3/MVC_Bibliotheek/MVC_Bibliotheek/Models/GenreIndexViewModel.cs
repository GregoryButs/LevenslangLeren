using MVC_Bibliotheek.Data.Entities;

namespace MVC_Bibliotheek.Models
{
    public class GenreIndexViewModel
    {
        public List<Genre> Genres { get; set; }
        public int? EditingGenreId { get; set; }
        public Genre NewGenre { get; set; } // <-- Add this property

        // Constructor to initialize NewGenre
        public GenreIndexViewModel()
        {
            Genres = new List<Genre>();
            NewGenre = new Genre();
        }
    }
}