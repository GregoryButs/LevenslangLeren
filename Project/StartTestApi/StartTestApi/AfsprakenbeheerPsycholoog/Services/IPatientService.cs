using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IPatientService
    {
        public IEnumerable<PatientListViewModel> GetAllePatienten();
        public PatientDetailViewModel? GetPatientDetail(int id);
        public void CreatePatient(CreatePatientViewModel model);
        public EditPatientViewModel? GetPatientForEdit(int id);
        public void EditPatient(EditPatientViewModel model);
        public void DeletePatient(int id);
        bool KoppelPatientAanUser(int patientId, string userId);
        bool OntkoppelPatientVanUser(int patientId);
    }
}
