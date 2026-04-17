using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Planning
{
    public class TijdslotViewModel
    {
        public TimeSpan Starttijd { get; set; }
        public TimeSpan Eindtijd { get; set; }
        public bool IsBezet { get; set; }
        public AfspraakListViewModel? Afspraak { get; set; }

        public string StartFormatted => $"{Starttijd:hh\\:mm}";
        public string EindFormatted => $"{Eindtijd:hh\\:mm}";
    }
}
