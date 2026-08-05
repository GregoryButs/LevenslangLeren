using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Interface voor het beheren van patiënten, inclusief koppeling aan gebruikersaccounts en verwerking van nieuwe aanmeldingen.
    /// </summary>
    public interface IPatientService
    {
        IEnumerable<PatientListViewModel> GetAllePatienten();
        PatientDetailViewModel? GetPatientDetail(int id);
        int CreatePatient(CreatePatientViewModel model);
        EditPatientViewModel? GetPatientForEdit(int id);
        void EditPatient(EditPatientViewModel model);
        void DeletePatient(int id);
        bool KoppelPatientAanUser(int patientId, string userEmail, bool setAsPrimary = true);
        (bool Success, string ErrorMessage) KoppelPatientAanUserDetailed(int patientId, string userEmail, bool setAsPrimary = true);
        bool OntkoppelPatientVanUser(int patientId);

        IEnumerable<PatientListViewModel> GetInactievePatienten();
        bool HeractiveerPatient(int id);

        Task<IEnumerable<ApplicationUser>> GetNieuweAanmeldingenAsync();
        Task<IEnumerable<ApplicationUser>> GetWachtlijstAanmeldingenAsync();
        Task<bool> PlaatsOpWachtlijstAsync(string userId);
        Task<bool> HerstelVanWachtlijstAsync(string userId);
        Task<(bool succes, string naam)> MaakEnKoppelNieuwePatientAsync(string userId);
        (bool Success, string ErrorMessage) MergePatients(MergePatientViewModel model);
    }
}
