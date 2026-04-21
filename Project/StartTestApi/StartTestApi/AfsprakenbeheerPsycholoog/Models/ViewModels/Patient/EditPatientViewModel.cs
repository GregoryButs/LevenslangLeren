namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Patient
{
    /// <summary>
    /// ViewModel voor het bewerken van een bestaande patiënt, bevat alle velden van CreatePatientViewModel plus een extra veld voor ID.
    /// </summary>
    public class EditPatientViewModel : CreatePatientViewModel
    {
        public int Id { get; set; }
    }
}
