using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IPatientBoekService
    {
        PatientBoekAfspraakViewModel GetBoekViewModel(DateTime datum);
        DagOverzichtViewModel GetDagOverzichtVoorPatient(DateTime datum);
        bool CreatePatientAfspraak(PatientBoekAfspraakViewModel model, int patientId);
        bool AnnuleerPatientAfspraak(int afspraakId, int patientId);
    }
}