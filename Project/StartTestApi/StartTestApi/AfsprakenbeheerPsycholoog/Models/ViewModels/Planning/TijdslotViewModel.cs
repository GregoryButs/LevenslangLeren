using System;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Planning
{
    /// <summary>
    /// ViewModel voor een tijdslot in het dagoverzicht, dat de start- en eindtijd van het slot bevat, 
    /// evenals informatie over of het slot bezet is en welke afspraak er eventueel in dat slot gepland staat.
    /// </summary>
    public class TijdslotViewModel
    {
        public DateTime Tijd { get; set; }
        public TimeSpan Starttijd { get; set; }
        public TimeSpan Eindtijd { get; set; }
        public bool IsBezet { get; set; }
        public AfspraakListViewModel? Afspraak { get; set; }

        public string StartFormatted => $"{Starttijd:hh\\:mm}";
        public string EindFormatted => $"{Eindtijd:hh\\:mm}";
    }
}
