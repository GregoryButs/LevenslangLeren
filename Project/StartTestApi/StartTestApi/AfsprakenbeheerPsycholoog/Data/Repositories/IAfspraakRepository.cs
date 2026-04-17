using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    public interface IAfspraakRepository: IRepository<Afspraak>
    {
        IEnumerable<Afspraak> GetAllMetDetails();
        Afspraak? GetByIdMetDetails(int id);
        IEnumerable<Afspraak> GetByPatientId(int patientId);
        IEnumerable<Afspraak> GetByStatus(AfspraakStatus status);
        IEnumerable<Afspraak> GetByDatum(DateTime datum);
        bool HeeftConflict(DateTime starttijd, DateTime eindtijd, int? teNegerenAfspraakId = null);
        int CountByWeek(DateTime startWeek, DateTime eindeWeek);
        int CountPatienten();
        Afspraak? GetVolgende();
        Afspraak? GetByIdEnPatient(int afspraakId, int patientId);
    }
}
