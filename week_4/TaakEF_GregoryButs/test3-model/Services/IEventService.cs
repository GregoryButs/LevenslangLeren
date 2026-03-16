using test3_model.Models.Viewmodels;

namespace test4_start.Services
{
    public interface IEventService
    {
        IEnumerable<EventViewModel> GetEventList();
        EventViewModel GetEventById(int id);
        void CancelEvent(int id);
        int CreateEvent(CreateEventCommand createEventCommand);
    }
}
