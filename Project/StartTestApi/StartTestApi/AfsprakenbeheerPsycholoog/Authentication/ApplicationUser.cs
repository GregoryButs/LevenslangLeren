using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string Voornaam { get; set; }

        [PersonalData]
        public string Achternaam { get; set; }

        // Optionele koppeling aan patiënt-record
        [PersonalData]
        public int? PatientId { get; set; }

        // Read-only helper property (zoals in modelopdracht)
        public bool HeeftPatientProfiel => PatientId.HasValue;
    }
}
