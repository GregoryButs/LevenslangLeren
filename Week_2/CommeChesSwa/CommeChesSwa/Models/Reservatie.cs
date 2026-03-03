using CommeChesSwa.Validation;
using CommeChesSwa.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace CommeChesSwa.Models
{
    public class Reservatie
    {
        public int Id { get; set; }

        [Display(Name = "Voornaam")] // Correct attribute name and syntax
        public string FirstName { get; set; }

        [Display(Name = "Familienaam")]
        public string LastName { get; set; }

        public string Email { get; set; }

        [Display(Name = "Datum")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Display(Name = "Tijdstip")]
        public Time Time
        {
            get; set;
        }

        [Display(Name = "Aantal personen")]
        public int NumberOfGuests { get; set; }

    }
}

