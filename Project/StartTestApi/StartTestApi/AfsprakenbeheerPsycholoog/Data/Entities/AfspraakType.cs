using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    /// <summary>
    /// Entiteit die een type afspraak vertegenwoordigt, zoals "Intake", "Therapie", "Evaluatie", etc.
    /// </summary>
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

        public ICollection<Afspraak> Afspraken { get; set; } = new List<Afspraak>();

        public bool VereistPatient { get; set; } = true;
    }
}
