using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    public class Afspraak : BaseEntity
    {
        [Required]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [Required]
        public int TypeId { get; set; }
        public AfspraakType Type { get; set; }

        [Required]
        [Display(Name = "Starttijd")]
        public DateTime Starttijd { get; set; }
        [Required]
        [Display(Name = "Eindtijd")]
        public DateTime Eindtijd { get; set; }

        [Required]
        [Display(Name = "Status")]
        public Status Status { get; set; } = Status.Gepland; // Gepland | Voltooid | Geannuleerd - standaard op gepland

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }
    }

    public enum Status
    {
        Gepland,
        Voltooid,
        Geannuleerd
    }
}

