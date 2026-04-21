using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak
{
    /// <summary>
    /// ViewModel voor het bewerken van een bestaande afspraak, bevat alle velden van CreateAfspraakViewModel plus extra velden voor ID en status.
    /// </summary>
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
