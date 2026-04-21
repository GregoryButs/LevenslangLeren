using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    /// <summary>
    /// Repository interface voor het beheren van afspraken, inclusief methoden voor het ophalen van afspraken met details,
    /// het controleren op conflicten, en het tellen van afspraken binnen bepaalde periodes.
    /// </summary>
    public interface IAfspraakRepository : IRepository<Afspraak>
    {
        IEnumerable<Afspraak> GetAllMetDetails();
        Afspraak? GetByIdMetDetails(int id);
        IEnumerable<Afspraak> GetByPatientId(int patientId);
        IEnumerable<Afspraak> GetByStatus(AfspraakStatus status);
        IEnumerable<Afspraak> GetByDatum(DateTime datum);
        IEnumerable<Afspraak> GetInPeriodeMetDetails(DateTime start, DateTime einde, int? patientId = null);
        bool HeeftConflict(DateTime starttijd, DateTime eindtijd, bool negeerBlokkeringen = false, int? teNegerenAfspraakId = null);
        int CountByWeek(DateTime startWeek, DateTime eindeWeek, bool zonderBlokkeringen = false);
        int CountPatienten();
        Afspraak? GetVolgende(bool zonderBlokkeringen = false);
        Afspraak? GetByIdEnPatient(int afspraakId, int patientId);
    }
}
