using AfsprakenbeheerPsycholoog.Helpers;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Planning
{
    public class DagOverzichtViewModel
    {
        public DateTime Datum { get; set; }
        public List<TijdslotViewModel> Tijdsloten { get; set; }

        public DateTime? MinimumNavigatieDatum { get; set; }

        public DateTime VorigeDag => Datum.AddDays(-1);
        public DateTime VolgendeDag => Datum.AddDays(1);

        public DateTime VorigeWerkdag => BerekenVorigeWerkdag(Datum);
        public DateTime VolgendeWerkdag => BerekenVolgendeWerkdag(Datum);

        public bool KanNaarVorigeWerkdag =>
            !MinimumNavigatieDatum.HasValue || VorigeWerkdag.Date >= MinimumNavigatieDatum.Value.Date;

        public bool IsWerkdag => PraktijkInstellingen.IsWerkdag(Datum);

        private static DateTime BerekenVorigeWerkdag(DateTime datum)
        {
            var vorige = datum.AddDays(-1);
            while (!PraktijkInstellingen.IsWerkdag(vorige))
            {
                vorige = vorige.AddDays(-1);
            }

            return vorige;
        }

        private static DateTime BerekenVolgendeWerkdag(DateTime datum)
        {
            var volgende = datum.AddDays(1);
            while (!PraktijkInstellingen.IsWerkdag(volgende))
            {
                volgende = volgende.AddDays(1);
            }

            return volgende;
        }
    }
}
