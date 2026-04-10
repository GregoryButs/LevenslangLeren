using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    public class Patient : BaseEntity
    {
        [Required]
        [Display(Name = "Voornaam")]
        public string Voornaam { get; set; }

        [Required]
        [Display(Name = "Achternaam")]
        public string Achternaam { get; set; }

        [Required]
        [Display(Name = "Geboortedatum")]
        public DateOnly Geboortedatum { get; set; }

        [Required, EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required, Phone]
        [Display(Name = "Telefoonnummer")]
        public string Telefoonnummer { get; set; }

        [Display(Name = "Dossiernummer")]
        public string? DossierNummer { get; set; }

        // Read-only helper -> nog te zien of ik dit met Automapper en profiles zal doen of hier in de entity zelf laat staan
        public string VolledigeNaam => $"{Voornaam} {Achternaam}";

        // Navigatieproperty
        public ICollection<Afspraak> Afspraken { get; set; }
    }
}
