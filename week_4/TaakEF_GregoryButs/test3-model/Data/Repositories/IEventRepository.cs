using test3_model.Data.Entities;

namespace test4_start.Data.Repositories
{
    public interface IEventRepository : IRepository<Event>
    {
        IEnumerable<Event> GetEventsWithBegeleidersLocaties();
    }
}
