namespace AfsprakenbeheerPsycholoog.Helpers
{
    public class WeekHelper
    {
        public static (DateTime Start, DateTime Einde) GetHuidigeWeek(DateTime datum)
        {
            var start = datum.AddDays(-(int)datum.DayOfWeek + 1); // Maandag
            return (start, start.AddDays(7));
        }
    }
}
