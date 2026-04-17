using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    public interface IPatientRepository: IRepository<Patient>
    {
        Patient? GetByIdMetAfspraken(int id);
        IEnumerable<int> GetGekoppeldePatientIds();  // Geeft IDs van patiënten die een account hebben
        ApplicationUser? GetUserByEmail(string email);
        ApplicationUser? GetUserByPatientId(int patientId);
    }
}
