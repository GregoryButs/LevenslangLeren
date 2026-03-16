using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace test3_model.Models.Viewmodels
{
    public class EventViewModel
    {
        public int Id { get; set; }


        [Display(Name = "Titel")]
        public string Titel { get; set; }
        [Display(Name = "Beschrijving")]
        public string Beschrijving { get; set; }
        [Display(Name = "Startmoment")]
        public DateTime StartDatumTijd { get; set; }
        [Display(Name = "Eindmoment")]
        public DateTime EindDatumTijd { get; set; }
        [Display(Name = "Begeleider naam")]
        public string BegeleiderNaam { get; set; }
        [Display(Name = "Begeleider functie")]
        public string BegeleiderFunctie { get; set; }   
        [Display(Name = "Begeleider foto URL")]
        public string BegeleiderFotoUrl { get; set; }
        [Display(Name = "Locatie")]
        public string LocatieNaam { get; set; } 

    }
}
