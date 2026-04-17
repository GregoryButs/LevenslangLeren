using AfsprakenbeheerPsycholoog.Data.Entities;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IAfspraakTypeService
    {
        public IEnumerable<AfspraakType> GetAlleTypes();
        AfspraakType? GetTypeById(int id);
        void CreateType(AfspraakType type);
        void EditType(AfspraakType type);
        void DeleteType(int id);

    }
}
