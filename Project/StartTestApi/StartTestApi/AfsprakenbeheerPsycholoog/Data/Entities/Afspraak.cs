using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    public class Afspraak : BaseEntity
    {
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? TypeId { get; set; }
        public AfspraakType? Type { get; set; }

        public Guid? ReeksId { get; set; } // zelfde waarde voor alle afspraken in één reeks

        [Required]
        [Display(Name = "Starttijd")]
        public DateTime Starttijd { get; set; }

        [Required]
        [Display(Name = "Eindtijd")]
        public DateTime Eindtijd { get; set; }

        [Required]
        [Display(Name = "Status")]
        public AfspraakStatus Status { get; set; } = AfspraakStatus.Gepland;

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }
    }

    public enum AfspraakStatus
    {
        Gepland,
        Voltooid,
        Geannuleerd
    }
}

