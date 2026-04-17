using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    public class AfspraakRepository : Repository<Afspraak>, IAfspraakRepository
    {
        public AfspraakRepository(ApplicationDbContext context) : base(context) { }

        public IEnumerable<Afspraak> GetAllMetDetails()
            => _context.Afspraken
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .OrderBy(a => a.Starttijd)
                .ToList();

        public Afspraak? GetByIdMetDetails(int id)
            => _context.Afspraken
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .FirstOrDefault(a => a.Id == id);

        public IEnumerable<Afspraak> GetByPatientId(int patientId)
            => _context.Afspraken
                .Include(a => a.Type)
                .Where(a => a.PatientId == patientId)
                .OrderBy(a => a.Starttijd)
                .ToList();

        public IEnumerable<Afspraak> GetByStatus(AfspraakStatus status)
            => _context.Afspraken
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Status == status)
                .OrderBy(a => a.Starttijd)
                .ToList();

        public IEnumerable<Afspraak> GetByDatum(DateTime datum)
        => _context.Afspraken
            .Include(a => a.Patient)
            .Include(a => a.Type)
            .Where(a => a.Starttijd.Date == datum.Date
                     && a.Status != AfspraakStatus.Geannuleerd)
            .ToList();

        public bool HeeftConflict(DateTime starttijd, DateTime eindtijd, int? teNegerenAfspraakId = null)
             => _context.Afspraken.Any(a =>
                 (teNegerenAfspraakId == null || a.Id != teNegerenAfspraakId) // Negeer de huidige afspraak (bij Edit)
                 && a.Starttijd < eindtijd && a.Eindtijd > starttijd
                 && a.Status != AfspraakStatus.Geannuleerd);

        public int CountByWeek(DateTime startWeek, DateTime eindeWeek)
            => _context.Afspraken
                .Count(a => a.Starttijd >= startWeek && a.Starttijd < eindeWeek
                         && a.Status == AfspraakStatus.Gepland);

        public int CountPatienten()
            => _context.Patienten.Count();

        public Afspraak? GetVolgende()
            => _context.Afspraken
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Starttijd > DateTime.Now
                         && a.Status == AfspraakStatus.Gepland)
                .OrderBy(a => a.Starttijd)
                .FirstOrDefault();

        public Afspraak? GetByIdEnPatient(int afspraakId, int patientId)
            => _context.Afspraken
                .FirstOrDefault(a => a.Id == afspraakId && a.PatientId == patientId);
    }
}
