
namespace StartTestApi.Data.Entities
{
    public class Event : BaseEntity
    {
        public string Titel { get; set; }
        public string Beschrijving { get; set; }
        public DateTime StartDatumTijd { get; set; }
        public DateTime EindDatumTijd { get; set; }
        public bool IsGeannuleerd { get; set; }

        public IEnumerable<Inschrijving> Inschrijvingen { get; set; }
    }
}