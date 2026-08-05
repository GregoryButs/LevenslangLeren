using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [Range(-1.0, 1.0)]
        [Display(Name = "Sentiment Score")]
        public double? SentimentScore { get; set; } = 0.0;

        [Display(Name = "Opmerkingen")]
        public string? Opmerkingen { get; set; }

        [Display(Name = "Google Event ID")]
        public string? GoogleEventId { get; set; }

        [Display(Name = "Google Meet Link")]
        public string? GoogleMeetLink { get; set; }

        [Display(Name = "Herinnering 24u Verzonden")]
        public bool HerinneringVerzonden { get; set; } = false;

        [Display(Name = "Herinnering 1 Week Verzonden")]
        public bool HerinneringWeekVerzonden { get; set; } = false;

        [Display(Name = "Is Hele Dag / Melding")]
        public bool IsHeleDag { get; set; } = false;

        [Display(Name = "Tarieftype")]
        public TariefType TariefType { get; set; } = TariefType.Regulier;

        [Display(Name = "ELP Status")]
        public ELPStatus ELPStatus { get; set; } = ELPStatus.TeVerwerken;

        [Display(Name = "ELP Type")]
        public string? ELPType { get; set; } = "Individueel";
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AfspraakStatus
    {
        Gepland,
        Voltooid,
        Geannuleerd
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TariefType
    {
        Regulier = 0,
        ELP = 1
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ELPStatus
    {
        TeVerwerken = 0,
        Verwerkt = 1
    }
}

