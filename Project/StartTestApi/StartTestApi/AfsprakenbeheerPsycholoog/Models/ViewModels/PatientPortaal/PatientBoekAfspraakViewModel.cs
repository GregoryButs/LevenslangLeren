using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal
{
    public class PatientBoekAfspraakViewModel
    {
        [Required(ErrorMessage = "Kies een type afspraak")]
        [Display(Name = "Type afspraak")]
        public int TypeId { get; set; }

        [Required(ErrorMessage = "Kies een tijdslot")]
        [Display(Name = "Gekozen tijdslot")]
        public DateTime GekozeTijdslot { get; set; }    

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }

        [ValidateNever] 
        public SelectList TypenLijst { get; set; }

        [ValidateNever] 
        public DateTime Datum { get; set; }
    }
}
