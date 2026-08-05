using System;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    /// <summary>
    /// ViewModel voor het samenvoegen (mergen) van twee patiëntendossiers.
    /// TargetPatientId is het dossier dat behouden blijft; SourcePatientId wordt samengevoegd en gedeactiveerd.
    /// </summary>
    public class MergePatientViewModel
    {
        [Required]
        public int TargetPatientId { get; set; }

        [Required]
        public int SourcePatientId { get; set; }

        [Required]
        public string Voornaam { get; set; } = string.Empty;

        [Required]
        public string Achternaam { get; set; } = string.Empty;

        [Required]
        public string Geboortedatum { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? SecundairEmail { get; set; }

        public string? Telefoonnummer { get; set; }

        public string? DossierNummer { get; set; }

        public string? Rijksregisternummer { get; set; }

        public double? EmotioneleStabiliteit { get; set; } = 5.5;
    }
}
