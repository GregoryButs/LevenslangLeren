using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    /// <summary>
    /// Entiteit die een patiënt vertegenwoordigt in het systeem, met eigenschappen voor persoonlijke gegevens, contactinformatie en navigatie naar afspraken.
    /// </summary>
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

        [Phone]
        [Display(Name = "Telefoonnummer")]
        public string? Telefoonnummer { get; set; }

        [Display(Name = "Dossiernummer")]
        public string? DossierNummer { get; set; }

        // Read-only helper -> nog te zien of ik dit met Automapper en profiles zal doen of hier in de entity zelf laat staan
        public string VolledigeNaam => $"{Voornaam} {Achternaam}";

        // Navigatieproperty
        public ICollection<Afspraak> Afspraken { get; set; } = new List<Afspraak>();

        [Display(Name = "Actief")]
        public bool IsActief { get; set; } = true;

        [Range(1.0, 10.0)]
        [Display(Name = "Emotionele Stabiliteit")]
        public double? EmotioneleStabiliteit { get; set; } = 5.5;

        public DateTime? VerwijderdOp { get; set; }
        public string? VerwijderdReden { get; set; }
    }
}
