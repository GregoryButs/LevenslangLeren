using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    /// <summary>
    /// Repository-interface voor het beheren van patiënten, inclusief methoden voor het ophalen van patiënten met hun afspraken,
    /// het beheren van gekoppelde accounts, en het ophalen van inactieve patiënten.
    /// </summary>
    public interface IPatientRepository : IRepository<Patient>
    {
        Patient? GetByIdMetAfspraken(int id);
        IEnumerable<int> GetGekoppeldePatientIds(); // Geeft IDs van patiënten die een account hebben
        ApplicationUser? GetUserByEmail(string email);
        ApplicationUser? GetUserByPatientId(int patientId);
        int? GetPatientIdByUserId(string userId);

        IEnumerable<Patient> GetAlleActievePatientenWithAfspraken();
        IEnumerable<Patient> GetInactievePatienten();
        Patient? GetByIdInclusiefInactief(int id);
        bool MergePatients(int targetPatientId, int sourcePatientId, Patient targetUpdatedData);
    }
}
