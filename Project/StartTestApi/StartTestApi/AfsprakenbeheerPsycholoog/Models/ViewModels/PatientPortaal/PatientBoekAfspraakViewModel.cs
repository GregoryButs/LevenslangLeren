using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal
{
    /// <summary>
    /// ViewModel voor het boeken van een afspraak door een patiënt, bevat velden voor het kiezen van een tijdslot, het toevoegen van opmerkingen en de datum van de afspraak.
    /// </summary>
    public class PatientBoekAfspraakViewModel
    {
        [Required(ErrorMessage = "Kies een tijdslot")]
        [Display(Name = "Gekozen tijdslot")]
        public DateTime? GekozeTijdslot { get; set; }

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }

        [Display(Name = "Locatie Type")]
        public string? LocatieType { get; set; } // "Praktijk", "GoogleMeet", "Telefoon"

        public DateTime Datum { get; set; }

        [Display(Name = "Afspraak Type ID")]
        public int? AfspraakTypeId { get; set; }
    }
}
