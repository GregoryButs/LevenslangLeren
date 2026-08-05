using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    /// <summary>
    /// ViewModel voor het tonen van gedetailleerde informatie over een patiënt, inclusief persoonlijke gegevens en een lijst van afspraken.
    /// </summary>
    public class PatientDetailViewModel
    {
        public int Id { get; set; }
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public string VolledigeNaam { get; set; }
        public DateOnly Geboortedatum { get; set; }
        public string Email { get; set; }
        public string? SecundairEmail { get; set; }
        public string Telefoonnummer { get; set; }
        public string? DossierNummer { get; set; }
        public string? Rijksregisternummer { get; set; }
        public AfsprakenbeheerPsycholoog.Data.Entities.TariefType StandaardTariefType { get; set; } = AfsprakenbeheerPsycholoog.Data.Entities.TariefType.Regulier;
        public bool IsGekoppeld { get; set; }
        public double? EmotioneleStabiliteit { get; set; }
        public IEnumerable<AfspraakListViewModel> Afspraken { get; set; }
    }
}
