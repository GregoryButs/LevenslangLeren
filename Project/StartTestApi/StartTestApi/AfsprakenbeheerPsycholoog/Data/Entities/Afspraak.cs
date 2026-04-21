using System.ComponentModel.DataAnnotations;

namespace AfsprakenbeheerPsycholoog.Data.Entities
{
    /// <summary>
    /// Entiteit die een afspraak voorstelt in het systeem, met relaties naar patiënt en type, en eigenschappen zoals start- en eindtijd, status en opmerkingen.
    /// </summary>
    public class Afspraak : BaseEntity
    {
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? TypeId { get; set; }
        public AfspraakType? Type { get; set; }

        // Giud om afspraken in een reeks te groeperen, bijvoorbeeld voor terugkerende afspraken.
        // Alle afspraken in dezelfde reeks hebben dezelfde ReeksId, wat het makkelijker maakt om ze samen te beheren (zoals annuleren of verplaatsen van de hele reeks).
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

