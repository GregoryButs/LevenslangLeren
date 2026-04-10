using StartTestApi.Data.Entities;

namespace StartTestApi.Data.Repositories
{
    public interface IEventRepository: IRepository<Event>
    {
        IEnumerable<Event> GetAllWithInschrijvingen();
        Event? GetByIdWithInschrijvingen(int id);
        bool Cancel(int id); 
    }
}
