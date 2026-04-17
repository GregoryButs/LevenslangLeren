using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Planning;
using AutoMapper;

namespace AfsprakenbeheerPsycholoog.Helpers
{
    public class TijdslotHelper
    {
        public static List<TijdslotViewModel> BouwTijdsloten(
            DateTime datum,
            IEnumerable<Afspraak> afsprakenDag,
            IMapper mapper)
        {
            // Bouw tijdsloten op basis van praktijkinstellingen en afspraken van de dag
            var tijdsloten = new List<TijdslotViewModel>();
            var start = PraktijkInstellingen.StartWerkdag;
            var einde = PraktijkInstellingen.EindeWerkdag;

            // Loop door de werkdag in stappen van slotduur en controleer of er een afspraak is die overlapt
            while (start < einde)
            {
                // Bereken de start- en eindtijd van het huidige tijdslot
                var slotStart = datum.Date + start;
                var slotEind = datum.Date + start.Add(
                    TimeSpan.FromMinutes(PraktijkInstellingen.SlotDuurMinuten));

                // Controleer of er een afspraak is die overlapt met dit tijdslot
                var bezet = afsprakenDag.FirstOrDefault(a =>
                    a.Starttijd < slotEind && a.Eindtijd > slotStart);

                // Voeg het tijdslot toe aan de lijst, markeer als bezet indien er een afspraak is
                tijdsloten.Add(new TijdslotViewModel
                {
                    Starttijd = start,
                    Eindtijd = start.Add(TimeSpan.FromMinutes(PraktijkInstellingen.SlotDuurMinuten)),
                    IsBezet = bezet != null,
                    Afspraak = bezet != null ? mapper.Map<AfspraakListViewModel>(bezet) : null
                });

                // Ga naar het volgende tijdslot
                start = start.Add(TimeSpan.FromMinutes(PraktijkInstellingen.SlotDuurMinuten));
            }

            // Return de lijst met tijdsloten
            return tijdsloten;
        }
    }
}

