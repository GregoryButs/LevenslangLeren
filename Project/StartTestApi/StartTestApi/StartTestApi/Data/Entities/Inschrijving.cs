using System.Text.Json.Serialization;

namespace StartTestApi.Data.Entities
{
    public class Inschrijving : BaseEntity
    {
        public string DeelnemerNaam { get; set; }

        public string DeelnemerEmail { get; set; }

        public DateTime InschrijfDatumTijd { get; set; }
        public int EventId { get; set; }

        [JsonIgnore]
        public Event Event { get; set; }
    }
}
