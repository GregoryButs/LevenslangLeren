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
        public string Kleurcode { get; set; }

        public ICollection<Afspraak> Afspraken { get; set; }
    }
}
