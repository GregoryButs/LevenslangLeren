using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IAfspraakTypeService
    {
        /// <summary>
        /// Haalt alle afspraaktypes op uit de database.
        /// </summary>
        /// <returns>Een lijst van alle afspraaktypes.</returns>
        public IEnumerable<AfspraakType> GetAlleTypes();
        AfspraakType? GetTypeById(int id);
        void CreateType(AfspraakType type);
        void EditType(AfspraakType type);
        void DeleteType(int id);

    }
}
