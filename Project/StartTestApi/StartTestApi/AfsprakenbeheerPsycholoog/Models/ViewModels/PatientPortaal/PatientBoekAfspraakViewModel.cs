using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal
{
    public class PatientBoekAfspraakViewModel
    {
        [Required(ErrorMessage = "Kies een tijdslot")]
        [Display(Name = "Gekozen tijdslot")]
        public DateTime? GekozeTijdslot { get; set; }

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }

        public DateTime Datum { get; set; }
    }
}
