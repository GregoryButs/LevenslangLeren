using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;

namespace AfsprakenbeheerPsycholoog.Helpers
{
    /// <summary>
    /// Helper class voor het bouwen van tijdsloten op basis van de afspraken van een specifieke dag.
    /// </summary>
    public class TijdslotHelper
    {
        public static List<TijdslotViewModel> BouwTijdsloten(
            DateTime datum,
            IEnumerable<AfspraakListViewModel> afsprakenDag,
            int? slotDuurMinuten = null)
        {

            // Gebruik de slotduur uit de instellingen als er geen specifieke duur is opgegeven
            var duur = slotDuurMinuten ?? PraktijkInstellingen.SlotDuurMinuten;
            var tijdsloten = new List<TijdslotViewModel>();
            var start = PraktijkInstellingen.StartWerkdag;
            var einde = PraktijkInstellingen.EindeWerkdag;

            // Loop door de werkdag in stappen van de slotduur en controleer of er een afspraak is die overlapt met het huidige tijdslot
            while (start < einde)
            {
                var slotStart = datum.Date + start;
                var slotEind = datum.Date + start.Add(TimeSpan.FromMinutes(duur));

                // Controleer of er een afspraak is die overlapt met dit tijdslot
                var bezet = afsprakenDag.FirstOrDefault(a =>
                    a.Starttijd < slotEind && a.Eindtijd > slotStart);

                // Voeg het tijdslot toe aan de lijst, markeer het als bezet als er een afspraak is
                tijdsloten.Add(new TijdslotViewModel
                {
                    Starttijd = start,
                    Eindtijd = start.Add(TimeSpan.FromMinutes(duur)),
                    IsBezet = bezet != null,
                    Afspraak = bezet
                });

                // Ga door naar het volgende tijdslot
                start = start.Add(TimeSpan.FromMinutes(duur));
            }

            return tijdsloten;
        }
    }
}

