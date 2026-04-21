namespace AfsprakenbeheerPsycholoog.Helpers
{
    /// <summary>
    /// Helper class voor het beheren van praktijkinstellingen, zoals werkdaguren en boekingsregels.
    /// </summary>
    public class PraktijkInstellingen
    {
        public static TimeSpan StartWerkdag = new TimeSpan(8, 0, 0);
        public static TimeSpan EindeWerkdag = new TimeSpan(20, 0, 0);
        public static int SlotDuurMinuten = 30;

        // Weekend blokkeren
        public static bool IsWerkdag(DateTime datum)
        {
            return datum.DayOfWeek != DayOfWeek.Saturday
                && datum.DayOfWeek != DayOfWeek.Sunday;
        }

        // Patiënt mag alleen toekomstige werkdagen boeken
        public static bool MagBoeken(DateTime datum)
        {
            return datum.Date > DateTime.Today && IsWerkdag(datum);
        }

        public static DateTime EerstVolgendeWerkdag(DateTime datum)
        {
            while (!IsWerkdag(datum))
                datum = datum.AddDays(1);
            return datum;
        }

    }
}
