using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    public class PatientDetailViewModel
    {
        public int Id { get; set; }
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public string VolledigeNaam { get; set; }
        public DateOnly Geboortedatum { get; set; }
        public string Email { get; set; }
        public string Telefoonnummer { get; set; }
        public string? DossierNummer { get; set; }
        public bool IsGekoppeld { get; set; }
        public IEnumerable<AfspraakListViewModel> Afspraken { get; set; }
    }
}
