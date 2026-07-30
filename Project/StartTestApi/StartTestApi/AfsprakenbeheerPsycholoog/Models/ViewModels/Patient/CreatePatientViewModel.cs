using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    /// <summary>
    /// ViewModel voor het aanmaken van een nieuwe patiënt, bevat alle benodigde velden en validaties voor het invoeren van patiëntgegevens in een formulier.
    /// </summary>
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

        [Range(1.0, 10.0)]
        [Display(Name = "Emotionele Stabiliteit")]
        public double? EmotioneleStabiliteit { get; set; } = 5.5;
    }
}
