using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak
{
    public class CreateAfspraakViewModel
    {
        [Required(ErrorMessage = "Kies een patiënt")]
        [Display(Name = "Patiënt")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "Kies een type")]
        [Display(Name = "Type afspraak")]
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Starttijd is verplicht")]
        [Display(Name = "Starttijd")]
        public DateTime Starttijd { get; set; }

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }

        // Niet te valideren velden (zoals in modelopdracht)
        [ValidateNever] public SelectList PatientenLijst { get; set; }
        [ValidateNever] public SelectList TypenLijst { get; set; }
    }
}
