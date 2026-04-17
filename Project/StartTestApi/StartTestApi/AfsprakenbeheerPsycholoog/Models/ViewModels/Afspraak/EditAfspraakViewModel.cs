using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak
{
    public class EditAfspraakViewModel : CreateAfspraakViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status")]
        public AfspraakStatus Status { get; set; }

        [ValidateNever]
        public SelectList StatusLijst { get; set; }
    }
}
