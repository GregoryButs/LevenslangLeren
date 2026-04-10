
using StartTestApi.DTO;

namespace StartTestApi.Services
{
    public interface IEventService
    {
            IEnumerable<EventDTO> GetAllEventsWithInschrijvingen();
            EventDTO? GetEventByIdWithInschrijvingen(int id);
    }
}
