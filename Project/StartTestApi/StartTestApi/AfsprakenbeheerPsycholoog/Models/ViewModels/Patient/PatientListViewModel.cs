using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    /// <summary>
    /// ViewModel voor het tonen van een lijst van patiënten, inclusief basisinformatie zoals naam, e-mail, telefoonnummer, dossiernummer, 
    /// aantal afspraken en of de patiënt een account heeft.
    /// </summary>
    public class PatientListViewModel
    {
        public int Id { get; set; }
        public string Voornaam { get; set; } = string.Empty;
        public string Achternaam { get; set; } = string.Empty;
        public string VolledigeNaam { get; set; } = string.Empty;
        public string Geboortedatum { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SecundairEmail { get; set; }
        public string? Telefoonnummer { get; set; }
        public string? DossierNummer { get; set; }
        public string? Rijksregisternummer { get; set; }
        public AfsprakenbeheerPsycholoog.Data.Entities.TariefType StandaardTariefType { get; set; } = AfsprakenbeheerPsycholoog.Data.Entities.TariefType.Regulier;
        public double? EmotioneleStabiliteit { get; set; }
        public int AantalAfspraken { get; set; }
        public bool IsGekoppeld { get; set; }  // Heeft deze patiënt een account?
        public IEnumerable<AfspraakListViewModel> Afspraken { get; set; } = new List<AfspraakListViewModel>();
    }
}
