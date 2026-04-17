namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    public class PatientListViewModel
    {
        public int Id { get; set; }
        public string VolledigeNaam { get; set; }
        public string Email { get; set; }
        public string Telefoonnummer { get; set; }
        public string? DossierNummer { get; set; }
        public int AantalAfspraken { get; set; }
        public bool IsGekoppeld { get; set; }  // Heeft deze patiënt een account?
    }
}
