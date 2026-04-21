using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Helpers
{
    /// <summary>
    /// Helper class voor het bouwen van weekoverzichten en het ophalen van de huidige week.
    /// </summary>
    public class WeekHelper
    {
        public static (DateTime Start, DateTime Einde) GetHuidigeWeek(DateTime datum)
        {
            var start = datum.AddDays(-(int)datum.DayOfWeek + 1); // Maandag
            return (start, start.AddDays(7));
        }

        public static WeekOverzichtViewModel BouwWeekOverzicht(
            DateTime datum, 
            IEnumerable<AfspraakListViewModel> afsprakenLijst,
            bool isPsycholoog)
        {
            var (startWeek, eindeWeek) = GetHuidigeWeek(datum);
            var dagenDict = BouwWeekDagenDict(startWeek, eindeWeek, afsprakenLijst);

            return new WeekOverzichtViewModel
            {
                StartWeek = startWeek,
                EindeWeek = eindeWeek,
                IsPsycholoog = isPsycholoog,
                Dagen = dagenDict
            };
        }

        public static Dictionary<DateTime, List<AfspraakListViewModel>> BouwWeekDagenDict(
            DateTime startWeek, 
            DateTime eindeWeek, 
            IEnumerable<AfspraakListViewModel> afsprakenLijst)
        {
            var dagenDict = new Dictionary<DateTime, List<AfspraakListViewModel>>();
            
            for (var d = startWeek; d < eindeWeek; d = d.AddDays(1))
            {
                dagenDict[d] = afsprakenLijst
                    .Where(a => a.Starttijd.Date == d.Date)
                    .OrderBy(a => a.Starttijd)
                    .ToList();
            }
            
            return dagenDict;
        }
    }
}
