using AfsprakenbeheerPsycholoog.Helpers;

namespace AfsprakenbeheerPsycholoog.Models.ViewModels.Planning
{
    public class DagOverzichtViewModel
    {
        public DateTime Datum { get; set; }
        public List<TijdslotViewModel> Tijdsloten { get; set; }
        public DateTime VorigeDag => Datum.AddDays(-1);
        public DateTime VolgendeDag => Datum.AddDays(1);
        public bool IsWerkdag => PraktijkInstellingen.IsWerkdag(Datum);
    }
}
