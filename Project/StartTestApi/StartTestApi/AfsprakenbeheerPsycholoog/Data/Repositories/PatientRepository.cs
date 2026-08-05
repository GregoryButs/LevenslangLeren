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
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var cleanEmail = email.Trim();
            var normalizedEmail = cleanEmail.ToUpperInvariant();
            return _context.Users.FirstOrDefault(u => u.NormalizedEmail == normalizedEmail || (u.Email != null && u.Email.ToLower() == cleanEmail.ToLower()));
        }

        public ApplicationUser? GetUserByPatientId(int patientId)
            => _context.Users.FirstOrDefault(u => u.PatientId == patientId);

        public int? GetPatientIdByUserId(string userId)
            => _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PatientId)
                .FirstOrDefault();

        public IEnumerable<Patient> GetAlleActievePatientenWithAfspraken()
            => _context.Patienten
                .Include(p => p.Afspraken)
                .ThenInclude(a => a.Type)
                .Where(p => p.IsActief)
                .OrderBy(p => p.Achternaam)
                .ThenBy(p => p.Voornaam)
                .ToList();

        public IEnumerable<Patient> GetInactievePatienten()
            => _context.Patienten
                .IgnoreQueryFilters()
                .Include(p => p.Afspraken)
                .ThenInclude(a => a.Type)
                .Where(p => !p.IsActief)
                .OrderBy(p => p.Achternaam)
                .ThenBy(p => p.Voornaam)
                .ToList();

        public Patient? GetByIdInclusiefInactief(int id)
            => _context.Patienten
                .IgnoreQueryFilters()
                .FirstOrDefault(p => p.Id == id);

        public bool MergePatients(int targetPatientId, int sourcePatientId, Patient targetUpdatedData)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var target = _context.Patienten.FirstOrDefault(p => p.Id == targetPatientId);
                var source = _context.Patienten.FirstOrDefault(p => p.Id == sourcePatientId);

                if (target == null || source == null) return false;

                // 1. Reassign all Afspraken from source to target
                var sourceAfspraken = _context.Afspraken.Where(a => a.PatientId == sourcePatientId).ToList();
                foreach (var appt in sourceAfspraken)
                {
                    appt.PatientId = targetPatientId;
                }

                // 2. Reassign or handle Identity Users
                var sourceUser = _context.Users.FirstOrDefault(u => u.PatientId == sourcePatientId);
                var targetUser = _context.Users.FirstOrDefault(u => u.PatientId == targetPatientId);

                if (sourceUser != null)
                {
                    if (targetUser == null)
                    {
                        sourceUser.PatientId = targetPatientId;
                    }
                    else
                    {
                        // Unlink source user if target user is already linked to target patient
                        sourceUser.PatientId = null;
                    }
                }

                // 3. Update target patient details with chosen values
                target.Voornaam = targetUpdatedData.Voornaam;
                target.Achternaam = targetUpdatedData.Achternaam;
                target.Geboortedatum = targetUpdatedData.Geboortedatum;
                target.Email = targetUpdatedData.Email;
                target.SecundairEmail = targetUpdatedData.SecundairEmail;
                target.Telefoonnummer = targetUpdatedData.Telefoonnummer ?? string.Empty;
                target.DossierNummer = targetUpdatedData.DossierNummer;
                target.Rijksregisternummer = targetUpdatedData.Rijksregisternummer;
                target.EmotioneleStabiliteit = targetUpdatedData.EmotioneleStabiliteit;

                // 4. Soft-delete and archive source patient
                source.IsActief = false;
                source.VerwijderdOp = DateTime.UtcNow;
                source.VerwijderdReden = $"Samengevoegd met patiënt #{targetPatientId}";
                if (source.Telefoonnummer == null)
                {
                    source.Telefoonnummer = string.Empty;
                }

                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}

