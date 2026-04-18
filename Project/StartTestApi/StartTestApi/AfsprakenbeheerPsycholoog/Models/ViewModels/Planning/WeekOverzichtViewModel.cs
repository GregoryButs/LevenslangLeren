using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Planning
{
    public class WeekOverzichtViewModel
    {
        public DateTime StartWeek { get; set; }
        public DateTime EindeWeek { get; set; }
        public bool IsPsycholoog { get; set; }
        public Dictionary<DateTime, List<AfspraakListViewModel>> Dagen { get; set; } = new();
    }
}