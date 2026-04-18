using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        public PatientRepository(ApplicationDbContext context) : base(context) { }

        public Patient? GetByIdMetAfspraken(int id)
            => _context.Patienten
                .Include(p => p.Afspraken)
                .ThenInclude(a => a.Type)
                .FirstOrDefault(p => p.Id == id);

        public IEnumerable<int> GetGekoppeldePatientIds()
            => _context.Users
                .Where(u => u.PatientId != null)
                .Select(u => u.PatientId!.Value)
                .ToHashSet();

        public ApplicationUser? GetUserByEmail(string email)
            => _context.Users.FirstOrDefault(u => u.Email == email);

        public ApplicationUser? GetUserByPatientId(int patientId)
            => _context.Users.FirstOrDefault(u => u.PatientId == patientId);

        public IEnumerable<Patient> GetInactievePatienten()
            => _context.Patienten
                .IgnoreQueryFilters()
                .Where(p => !p.IsActief)
                .OrderBy(p => p.Achternaam)
                .ThenBy(p => p.Voornaam)
                .ToList();

        public Patient? GetByIdInclusiefInactief(int id)
            => _context.Patienten
                .IgnoreQueryFilters()
                .FirstOrDefault(p => p.Id == id);
    }
}

