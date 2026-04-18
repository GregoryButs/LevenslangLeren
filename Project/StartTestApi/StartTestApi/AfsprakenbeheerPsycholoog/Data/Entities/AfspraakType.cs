using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    public class AfspraakType : BaseEntity
    {
        [Required]
        [Display(Name = "Naam")]
        public string Naam { get; set; }          

        [Required]
        [Display(Name = "Standaard duur (minuten)")]
        public int StandaardDuurMinuten { get; set; }

        [Display(Name = "Kleurcode")]   
        public string Kleurcode { get; set; } = "#6c757d";  // Bootstrap grijs als default om zeker geen null-waarde te hebben

        [ValidateNever]
        public IEnumerable<Afspraak> Afspraken { get; set; }

        public bool VereistPatient { get; set; } = true;
    }
}
