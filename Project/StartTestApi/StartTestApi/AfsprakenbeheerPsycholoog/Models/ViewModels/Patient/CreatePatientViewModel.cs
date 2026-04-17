using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    public class CreatePatientViewModel
    {
        [Required(ErrorMessage = "Voornaam is verplicht")]
        [Display(Name = "Voornaam")]
        public string Voornaam { get; set; }

        [Required(ErrorMessage = "Achternaam is verplicht")]
        [Display(Name = "Achternaam")]
        public string Achternaam { get; set; }

        [Required(ErrorMessage = "Geboortedatum is verplicht")]
        [Display(Name = "Geboortedatum")]
        public DateOnly Geboortedatum { get; set; }

        [Required(ErrorMessage = "E-mail is verplicht")]
        [EmailAddress(ErrorMessage = "Ongeldig e-mailadres")]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefoonnummer is verplicht")]
        [Phone(ErrorMessage = "Ongeldig telefoonnummer")]
        [Display(Name = "Telefoonnummer")]
        public string Telefoonnummer { get; set; }

        [Display(Name = "Dossiernummer")]
        public string? DossierNummer { get; set; }
    }
}
