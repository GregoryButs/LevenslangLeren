using Microsoft.AspNetCore.Components.Web;
using System.ComponentModel.DataAnnotations;
using test3_model.Data.Entities;

namespace test3_model.Models.Viewmodels
{
    public class CreateEventCommand 
    {

        [Display(Name = "Titel Evenement")]
        [Required(ErrorMessage = "Gelieve een titel in te vullen")]
        public string Titel { get; set; }

        [Display(Name = "Beschrijving Evenement")]
        [Required(ErrorMessage = "Gelieve een beschrijving in te vullen")]
        public string Beschrijving { get; set; }

        [Display(Name = "Startdatum en tijd")]
        [Required(ErrorMessage = "Gelieve een startdatum en tijd in te vullen")]
        [DataType(DataType.DateTime)]
        public DateTime StartDatumTijd { get; set; }



        [Display(Name = "Einddatum en tijd")]
        [Required(ErrorMessage = "Gelieve een einddatum en tijd in te vullen")]
        [DataType(DataType.DateTime)]
        public DateTime EindDatumTijd { get; set; }

        [Display(Name = "Locatie")]
        [Required(ErrorMessage = "Gelieve een locatie in te vullen")]
        public int LocatieId { get; set; }

        [Display(Name = "Begeleider")]
        [Required(ErrorMessage = "Gelieve een begeleider in te vullen")]
        public int BegeleiderId { get; set; }



        public IEnumerable<Locatie>? AllLocaties { get; set; } 
        public IEnumerable<Begeleider>? AllBegeleiders { get; set; } 
        public Event ToEvent()
        {
            return new Event
            {
                Titel = this.Titel,
                Beschrijving = this.Beschrijving,
                StartDatumTijd = this.StartDatumTijd,
                EindDatumTijd = this.EindDatumTijd,
                BegeleiderId = this.BegeleiderId,
                LocatieId = this.LocatieId,
                IsGeannuleerd = false
            };  
        }

    }
}
