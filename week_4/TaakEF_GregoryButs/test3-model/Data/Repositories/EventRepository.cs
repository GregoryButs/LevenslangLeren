using Microsoft.EntityFrameworkCore;
using test3_model.Data;
using test3_model.Data.Entities;

namespace test4_start.Data.Repositories
{
    public class EventRepository : Repository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }
        public IEnumerable<Event> GetEventsWithBegeleidersLocaties()
        {
            return _context.Events
                .Include(e => e.Begeleider)
                .Include(e => e.Locatie)
                .ToList();
        }
    }
}
