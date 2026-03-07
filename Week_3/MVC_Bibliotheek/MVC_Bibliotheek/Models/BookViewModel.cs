using Microsoft.AspNetCore.Mvc.Rendering;
using MVC_Bibliotheek.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace MVC_Bibliotheek.Models
{
    public class BookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Title is vereist.")]
        [Display(Name = "Titel")]
        public string Title { get; set; }

        [Required(ErrorMessage = "ISBN-13 is vereist.")]
        [Display(Name = "ISBN-nummer")]
        [Range(1000000000000, 9999999999999, ErrorMessage = "ISBN-13 moet een 13-cijferig nummer zijn.")]
        public long ISBN13 { get; set; }

        [Required(ErrorMessage = "Publisher is vereist.")]
        [Display(Name = "Uitgever")]
        public string Publisher { get; set; }

        [Required(ErrorMessage = "Publicatiedatum is vereist.")]
        [Display(Name = "Publicatiedatum")]
        [DataType(DataType.Date)]
        public DateTime PublicationDate { get; set; }

        [Required(ErrorMessage = "Aantal pagina's is vereist.")]
        [Display(Name = "Aantal pagina's")]
        [Range(1, int.MaxValue, ErrorMessage = "Aantal pagina's moet een positief getal zijn.")]
        public int Pages { get; set; }

        [Required(ErrorMessage = "Auteur is vereist.")]
        [Display(Name = "Auteur")]
        public int AuthorId { get; set; }

        [Required(ErrorMessage = "Genre is vereist.")]
        [Display(Name = "Genre")]
        public int GenreId { get; set; }

        [Display(Name = "Cover afbeelding")]
        public IFormFile? CoverImage { get; set; }

        public string? ExistingCoverImagePath { get; set; }


        // Voor dropdowns (in plaats van Author/Genre objecten):
        public IEnumerable<SelectListItem>? Authors { get; set; }
        public IEnumerable<SelectListItem>? Genres { get; set; }
    }
}
