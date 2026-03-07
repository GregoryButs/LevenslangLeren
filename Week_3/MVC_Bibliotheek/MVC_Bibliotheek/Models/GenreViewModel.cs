using System.ComponentModel.DataAnnotations;

namespace MVC_Bibliotheek.Models
{
    public class GenreViewModel
    {
        public int GenreId { get; set; }

        [Required(ErrorMessage = "GenreNaam is verreist.")]
        [Display(Name = "Genrenaam")]
        [StringLength(100, ErrorMessage = "Genrenaam mag niet langer zijn dan 100 tekens.")]
        public string Name { get; set; }
    }
}
