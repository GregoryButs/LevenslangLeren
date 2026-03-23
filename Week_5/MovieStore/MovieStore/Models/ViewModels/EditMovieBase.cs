using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Movie_Store.Models.ViewModels
{
    public class EditMovieBase
    {
        [Required]
        public string Title { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:0.00}", ApplyFormatInEditMode = true)]
        public double Price { get; set; }
        [Display(Name = "Age Restriction")]
        public AgeRestriction? AgeRestriction { get; set; }

        [Required]
        [Display(Name = "Genre")]
        public int? SelectedGenreId { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
    }
}
