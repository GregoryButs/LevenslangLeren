using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Dashboard;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IAfspraakService
    {
        public IEnumerable<AfspraakListViewModel> GetAlleAfspraken();
        public IEnumerable<AfspraakListViewModel> GetAfsprakenVanPatient(int patientId);
        public IEnumerable<AfspraakListViewModel> GetAfsprakenOpStatus(AfspraakStatus status);
        public AfspraakDetailViewModel? GetAfspraakDetail(int id);
        public CreateAfspraakViewModel GetCreateViewModel();
        public EditAfspraakViewModel? GetEditViewModel(int id);
        public bool CreateAfspraak(CreateAfspraakViewModel model);
        public bool EditAfspraak(EditAfspraakViewModel model);
        public void DeleteAfspraak(int id);

        // Dagplanning
        DagOverzichtViewModel GetDagOverzicht(DateTime datum);

        // Patient boeken
        PatientBoekAfspraakViewModel GetBoekViewModel(DateTime datum);
        bool CreatePatientAfspraak(PatientBoekAfspraakViewModel model, int patientId);
        bool AnnuleerPatientAfspraak(int afspraakId, int patientId);

        // Dashboard
        DashboardViewModel GetDashboard(string psycholoogNaam);
    }
}
