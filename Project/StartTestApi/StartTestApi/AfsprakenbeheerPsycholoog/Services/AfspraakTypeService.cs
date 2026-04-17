using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class AfspraakTypeService : IAfspraakTypeService
    {
        private readonly IAfspraakTypeRepository _repo;

        public AfspraakTypeService(IAfspraakTypeRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<AfspraakType> GetAlleTypes()
        {
            return _repo.GetAll();
        }

        public AfspraakType? GetTypeById(int id)
        {
            return _repo.GetById(id);
        }

        public void CreateType(AfspraakType type)
        {
            _repo.Add(type);
            _repo.SaveChanges();
        }

        public void EditType(AfspraakType type)
        {
            _repo.Update(type);
            _repo.SaveChanges();
        }

        public void DeleteType(int id)
        {
            var type = _repo.GetById(id);
            if (type != null)
            {
                _repo.Delete(type);
                _repo.SaveChanges();
            }
        }
    }
}
