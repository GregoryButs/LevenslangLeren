namespace MVC_Bibliotheek.Models
{
    public class GenreIndexViewModel
    {
        public IEnumerable<GenreViewModel> Genres { get; set; } = new List<GenreViewModel>();
        public int? EditingGenreId { get; set; }
    }
}