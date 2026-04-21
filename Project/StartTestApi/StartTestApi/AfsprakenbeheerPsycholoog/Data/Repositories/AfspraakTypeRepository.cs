using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Data.Repositories
{
    /// <summary>
    /// Repository voor het beheren van afspraaktypes, inclusief methoden voor het ophalen, aanmaken, bewerken en verwijderen van types.
    /// </summary>
    public class AfspraakTypeRepository: Repository<AfspraakType>, IAfspraakTypeRepository
    {
        public AfspraakTypeRepository(ApplicationDbContext context) : base(context) { }
    }
}
