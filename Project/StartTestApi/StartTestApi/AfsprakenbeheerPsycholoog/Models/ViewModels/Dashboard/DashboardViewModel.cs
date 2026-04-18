using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Dashboard
{
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
