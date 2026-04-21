using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Dashboard
{
    /// <summary>
    /// ViewModel voor het dashboard van de psycholoog, dat een overzicht biedt van de afspraken van vandaag, de komende week en algemene statistieken zoals het aantal patiënten en afspraken. 
    /// Het bevat ook een overzicht van de volgende afspraak en een weekoverzicht dat kan worden aangepast op basis van een specifieke datum.
    /// </summary>
    public class DashboardViewModel
    {
        public string? PsycholoogNaam { get; set; }
        public DateTime Vandaag { get; set; } = DateTime.Today;
        public int AantalAfsprakenVandaag { get; set; }
        public int AantalAfsprakenDezeWeek { get; set; }
        public int AantalPatienten { get; set; }
        public AfspraakListViewModel? VolgendeAfspraak { get; set; }
        public List<AfspraakListViewModel> AfsprakenVandaag { get; set; } = new();
        public WeekOverzichtViewModel? WeekOverzicht { get; set; }
    }
}
