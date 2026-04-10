using Microsoft.EntityFrameworkCore;
using StartTestApi.Data.Entities;

namespace StartTestApi.Data.Repositories
{
    public class InschrijvingRepository : Repository<Inschrijving>, IInschrijvingRepository
    {
        public InschrijvingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Inschrijving? GetByIdWithEvent(int id)
        {
            return _context.Inschrijvingen
                .Include(i => i.Event)
                .FirstOrDefault(i => i.Id == id);
        }

        public void AddInschrijving(Inschrijving inschrijving)
        {
            _context.Inschrijvingen.Add(inschrijving);
        }
    }
}
