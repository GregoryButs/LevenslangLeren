using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;
using System;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Service verantwoordelijk voor het beheren van het boekingsproces voor patiënten (async variant).
    /// </summary>
    public interface IPatientBookingService
    {
        PatientBoekAfspraakViewModel GetBoekViewModel(DateTime datum);
        Task<DagOverzichtViewModel> GetDagOverzichtVoorPatientAsync(DateTime datum);
        Task<bool> CreatePatientAfspraakAsync(PatientBoekAfspraakViewModel model, int patientId);
        Task<bool> AnnuleerPatientAfspraakAsync(int afspraakId, int patientId);
    }
}
