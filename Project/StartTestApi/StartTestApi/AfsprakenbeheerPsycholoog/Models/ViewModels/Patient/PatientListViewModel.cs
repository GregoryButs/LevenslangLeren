namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    /// <summary>
    /// ViewModel voor het tonen van een lijst van patiënten, inclusief basisinformatie zoals naam, e-mail, telefoonnummer, dossiernummer, 
    /// aantal afspraken en of de patiënt een account heeft.
    /// </summary>
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
