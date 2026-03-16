using test3_model.Data.Entities;
using test3_model.Models.Viewmodels;
using test4_start.Data.Repositories;

namespace test4_start.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepository;

        public EventService(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public IEnumerable<EventViewModel> GetEventList()
        {
            // als het event geannuleerd of in het verleden is, dan moet het niet getoond worden
            var events = _eventRepository.GetEventsWithBegeleidersLocaties()
                .Where(e => !e.IsGeannuleerd && e.StartDatumTijd > DateTime.Now)
                .Select(e => new EventViewModel
                {
                    Id = e.Id,
                    Titel = e.Titel,
                    Beschrijving = e.Beschrijving,
                    StartDatumTijd = e.StartDatumTijd,
                    EindDatumTijd = e.EindDatumTijd,
                    BegeleiderNaam = $"{e.Begeleider.Naam}",
                    BegeleiderFunctie = e.Begeleider.Functie,
                    BegeleiderFotoUrl = e.Begeleider.UrlFoto,
                    LocatieNaam = e.Locatie.Naam
                })
                .ToList();
            return events;
        }

        public EventViewModel GetEventById(int id)
        {
            var e = _eventRepository.GetEventsWithBegeleidersLocaties()
                .FirstOrDefault(e => e.Id == id);
            return new EventViewModel
            {
                Id = e.Id,
                Titel = e.Titel,
                Beschrijving = e.Beschrijving,
                StartDatumTijd = e.StartDatumTijd,
                EindDatumTijd = e.EindDatumTijd,
                BegeleiderNaam = $"{e.Begeleider.Naam}",
                BegeleiderFunctie = e.Begeleider.Functie,
                BegeleiderFotoUrl = e.Begeleider.UrlFoto,
                LocatieNaam = e.Locatie.Naam
            };
        }

        public void CancelEvent(int id)
        {
            var e = _eventRepository.GetById(id);
            e.IsGeannuleerd = true;
            _eventRepository.Update(e);
        }

        public int CreateEvent(CreateEventCommand createEventCommand)
        {
            Event e = createEventCommand.ToEvent();
            _eventRepository.Add(e);
            _eventRepository.SaveChanges();
            return e.Id;
        }
    }
}
