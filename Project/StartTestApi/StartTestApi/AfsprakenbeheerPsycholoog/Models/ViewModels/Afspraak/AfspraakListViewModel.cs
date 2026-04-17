using AfsprakenbeheerPsycholoog.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak
{
    public class AfspraakListViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Naam")]
        public string PatientNaam { get; set; }

        [Display(Name = "Type")]
        public string AfspraakTypeNaam { get; set; }
        public DateTime Starttijd { get; set; }
        public DateTime Eindtijd { get; set; }
        public AfspraakStatus Status { get; set; }
        public string Kleurcode { get; set; }

        public string StatusLabel => Status switch
        {
            AfspraakStatus.Gepland => "bg-primary",
            AfspraakStatus.Voltooid => "bg-success",
            AfspraakStatus.Geannuleerd => "bg-danger",
            _ => "bg-secondary"
        };
    }
}
