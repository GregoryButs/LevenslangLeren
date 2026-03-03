using System.ComponentModel.DataAnnotations;
using CommeChesSwa.Validation;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.Extensions.Options;

namespace CommeChesSwa.ViewModel
{
    public class GastViewModel
    {
        [Required(ErrorMessage = "Voornaam is vereist")]
        [Display(Name = "Voornaam")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Achternaam is vereist")]
        [Display(Name = "Achternaam")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Emailadres is vereist")]
        [Display(Name = "Emailadres")]
        [EmailAddress(ErrorMessage = "Ongeldig emailadres")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Bevestig emailadres is vereist")]
        [Display(Name = "Bevestig emailadres")]
        [Compare("Email", ErrorMessage = "Emailadressen komen niet overeen")]
        public string ConfirmEmail { get; set; }


        [Required(ErrorMessage = "Reservatiedatum is vereist")]
        [Display(Name = "Reservatiedatum")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Tijdstip is vereist")]
        [Display(Name = "Tijdstip")]
        public Time Time
        {
            get; set;
        }

        [Required(ErrorMessage = "Aantal gasten is vereist")]
        [Display(Name = "Aantal gasten")]
        [Range(1, 10, ErrorMessage = "Aantal gasten moet tussen 1 en 10 liggen")]
        public int NumberOfGuests { get; set; }

        [Display(Name = "Accepteer de voorwaarden")]
        [ExpectedValue(true, ErrorMessage = "Je moet de voorwaarden accepteren.")]
        public bool AcceptTerms { get; set; }

    }

    public enum Time
    {
        [Display(Name = "Ochtend (8u30 - 11u)")]
        Ochtend,

        [Display(Name = "Middag (11u - 14u)")]
        Middag,

        [Display(Name = "Avond (17u - 20u)")]
        Avond
    }
}

