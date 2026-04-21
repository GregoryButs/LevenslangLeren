using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IAfspraakService
    {
        /// <summary>
        /// Haalt alle afspraken op, ongeacht de status of patiënt. Wordt gebruikt voor het overzicht van alle afspraken.
        /// </summary>
        /// <returns>
        /// Een lijst van alle afspraken in de vorm van AfspraakListViewModel.
        /// </returns>
        IEnumerable<AfspraakListViewModel> GetAlleAfspraken();
        IEnumerable<AfspraakListViewModel> GetAfsprakenVanPatient(int patientId);
        IEnumerable<AfspraakListViewModel> GetAfsprakenOpStatus(AfspraakStatus status);
        AfspraakDetailViewModel? GetAfspraakDetail(int id);
        CreateAfspraakViewModel GetCreateViewModel();
        EditAfspraakViewModel? GetEditViewModel(int id);
        bool CreateAfspraak(CreateAfspraakViewModel model);
        bool EditAfspraak(EditAfspraakViewModel model);
        void DeleteAfspraak(int id);
        void DeleteReeks(Guid reeksId);

        /// <summary>
        /// Genereert een overzicht van alle afspraken op een specifieke dag, gegroepeerd op tijdsblokken.
        /// </summary>
        /// <param name="datum">De datum waarvoor het overzicht moet worden gegenereerd.</param>
        /// <returns>Een DagOverzichtViewModel met de afspraken gegroepeerd op tijdsblokken.</returns>
        DagOverzichtViewModel GetDagOverzicht(DateTime datum);
    }
}
