using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Planning
{
    /// <summary>
    /// ViewModel voor het tonen van een weekoverzicht, inclusief start- en einddatum van de week, 
    /// of de gebruiker een psycholoog is, en een overzicht van afspraken per dag.
    /// </summary>
    public class WeekOverzichtViewModel
    {
        public DateTime StartWeek { get; set; }
        public DateTime EindeWeek { get; set; }
        public bool IsPsycholoog { get; set; }
        public Dictionary<DateTime, List<AfspraakListViewModel>> Dagen { get; set; } = new();
    }
}