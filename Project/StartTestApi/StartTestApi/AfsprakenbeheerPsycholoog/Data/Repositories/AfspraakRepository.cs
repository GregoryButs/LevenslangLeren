using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    /// <summary>
    /// Repository voor het beheren van afspraken, inclusief methoden voor het ophalen van afspraken met gerelateerde gegevens (zoals patiënt en type),
    /// het controleren op conflicten, en het tellen van afspraken binnen bepaalde periodes.
    /// </summary>
    public class AfspraakRepository : Repository<Afspraak>, IAfspraakRepository
    {
        public AfspraakRepository(ApplicationDbContext context) : base(context) { }

        public IEnumerable<Afspraak> GetAllMetDetails()
            => _context.Afspraken
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .OrderBy(a => a.Starttijd)
                .ToList();

        public Afspraak? GetByIdMetDetails(int id)
            => _context.Afspraken
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .FirstOrDefault(a => a.Id == id);

        public IEnumerable<Afspraak> GetByPatientId(int patientId)
            => _context.Afspraken
                .AsNoTracking()
                .Include(a => a.Type)
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.Starttijd)
                .ToList();

        public IEnumerable<Afspraak> GetByStatus(AfspraakStatus status)
            => _context.Afspraken
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Status == status)
                .OrderBy(a => a.Starttijd)
                .ToList();

        // Haal alle afspraken op voor een specifieke datum, exclusief geannuleerde afspraken
        public IEnumerable<Afspraak> GetByDatum(DateTime datum)
        {
            var startVanDag = datum.Date;
            var eindeVanDag = startVanDag.AddDays(1);

            return _context.Afspraken
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Starttijd >= startVanDag
                         && a.Starttijd < eindeVanDag
                         && a.Status != AfspraakStatus.Geannuleerd)
                .ToList();
        }

        // Haal alle afspraken op binnen een specifieke periode, met de mogelijkheid om te filteren op patiënt, exclusief geannuleerde afspraken
        public IEnumerable<Afspraak> GetInPeriodeMetDetails(DateTime start, DateTime einde, int? patientId = null)
        {
            var query = _context.Afspraken
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Starttijd >= start
                         && a.Starttijd < einde
                         && a.Status != AfspraakStatus.Geannuleerd);

            if (patientId.HasValue)
            {
                query = query.Where(a => a.PatientId == patientId.Value);
            }

            return query
                .OrderBy(a => a.Starttijd)
                .ToList();
        }

        // Controleer of er een conflict is met bestaande afspraken binnen een gegeven tijdsperiode,
        // met opties om blokkeringen te negeren en een specifieke afspraak te negeren (bijvoorbeeld bij het bewerken van een afspraak)
        public bool HeeftConflict(DateTime starttijd, DateTime eindtijd, bool negeerBlokkeringen = false, int? teNegerenAfspraakId = null)
            => _context.Afspraken.Any(a =>
                (teNegerenAfspraakId == null || a.Id != teNegerenAfspraakId)
                && a.Starttijd < eindtijd
                && a.Eindtijd > starttijd
                && a.Status != AfspraakStatus.Geannuleerd
                && (!negeerBlokkeringen || a.PatientId.HasValue));

        // Tel het aantal geplande afspraken binnen een specifieke week, met de optie om blokkeringen te negeren
        public int CountByWeek(DateTime startWeek, DateTime eindeWeek, bool zonderBlokkeringen = false)
            => _context.Afspraken.Count(a =>
                a.Starttijd >= startWeek
                && a.Starttijd < eindeWeek
                && a.Status == AfspraakStatus.Gepland
                && (!zonderBlokkeringen || a.PatientId.HasValue));

        // Tel het totale aantal patiënten in de database
        public int CountPatienten()
            => _context.Patienten.Count();
        
        // Haal de volgende geplande afspraak op, met de optie om blokkeringen te negeren
        public Afspraak? GetVolgende(bool zonderBlokkeringen = false)
            => _context.Afspraken
                .AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Starttijd > DateTime.Now
                         && a.Status == AfspraakStatus.Gepland
                         && (!zonderBlokkeringen || a.PatientId.HasValue))
                .OrderBy(a => a.Starttijd)
                .FirstOrDefault();
        
        // Haal een specifieke afspraak op voor een specifieke patiënt
        public Afspraak? GetByIdEnPatient(int afspraakId, int patientId)
            => _context.Afspraken.FirstOrDefault(a => a.Id == afspraakId && a.PatientId == patientId);
    }
}
