using StartTestApi.Data.Entities;

namespace StartTestApi.DTO
{
    public class EventDTO
    {
        public string Titel { get; set; }
        public string Beschrijving { get; set; }
        public DateTime StartDatumTijd { get; set; }
        public DateTime EindDatumTijd { get; set; }
        public bool IsGeannuleerd { get; set; }

        // tel het aantal inschrijvingen en zet het in de AantalInschrijvingen property
        public int AantalInschrijvingen { get; set; }
    }
}
