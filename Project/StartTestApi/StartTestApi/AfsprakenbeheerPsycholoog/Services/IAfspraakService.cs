using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IAfspraakService
    {
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

        DagOverzichtViewModel GetDagOverzicht(DateTime datum);
    }
}
