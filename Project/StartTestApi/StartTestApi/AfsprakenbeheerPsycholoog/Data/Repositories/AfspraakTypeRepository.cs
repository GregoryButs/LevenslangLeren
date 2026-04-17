using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    public class AfspraakTypeRepository: Repository<AfspraakType>, IAfspraakTypeRepository
    {
        public AfspraakTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
