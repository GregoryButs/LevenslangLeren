using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;

namespace AfsprakenbeheerPsycholoog.Helpers
{
    public static class HerhalingHelper
    {
        public static List<DateTime> BouwStartmomenten(DateTime starttijd, HerhaalPatroon herhaling, DateTime? herhaalTot)
        {
            var momenten = new List<DateTime> { starttijd };

            if (herhaling == HerhaalPatroon.Geen || !herhaalTot.HasValue)
            {
                return momenten;
            }

            var stapDagen = herhaling == HerhaalPatroon.Dagelijks ? 1 : 7;
            var volgende = starttijd.AddDays(stapDagen);
            var grens = herhaalTot.Value.Date.AddDays(1).AddTicks(-1);

            while (volgende <= grens)
            {
                momenten.Add(volgende);
                volgende = volgende.AddDays(stapDagen);
            }

            return momenten;
        }
    }
}