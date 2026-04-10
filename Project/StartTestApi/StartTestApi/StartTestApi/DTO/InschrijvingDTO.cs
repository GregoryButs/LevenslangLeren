using StartTestApi.Data.Entities;
using System.Text.Json.Serialization;

namespace StartTestApi.DTO
{
    public class InschrijvingDTO
    {
        public string DeelnemerNaam { get; set; }

        public string DeelnemerEmail { get; set; }

        public DateTime InschrijfDatumTijd { get; set; }
        public string EventTitel { get; set; }

        public DateTime EventStartDatumTijd { get; set; }
    }
}
