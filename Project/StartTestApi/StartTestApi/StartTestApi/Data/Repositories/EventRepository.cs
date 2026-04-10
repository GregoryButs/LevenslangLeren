using Microsoft.EntityFrameworkCore;
using StartTestApi.Data.Entities;

namespace StartTestApi.Data.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {

        }

        public IEnumerable<Event> GetAllWithInschrijvingen()
        {
            return _context.Events.Include(e => e.Inschrijvingen).ToList();
        }

        public Event? GetByIdWithInschrijvingen(int id)
        {
            return _context.Events.Include(e => e.Inschrijvingen).FirstOrDefault(e => e.Id == id);
        }

        public bool Cancel(int id)
        {
            var eventToCancel = GetById(id);
            if (eventToCancel == null)
            {
                return false;
            }
            eventToCancel.IsGeannuleerd = true;
            Update(eventToCancel);
            return SaveChanges();
        }
    }
}
