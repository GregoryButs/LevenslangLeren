using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Service voor het beheren van afspraaktypes, inclusief aanmaken, bewerken, verwijderen en ophalen van types.
    /// </summary>
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
            var existing = _repo.GetById(type.Id);
            if (existing != null)
            {
                existing.Naam = type.Naam;
                existing.StandaardDuurMinuten = type.StandaardDuurMinuten;
                existing.Kleurcode = type.Kleurcode;
                existing.VereistPatient = type.VereistPatient;
                _repo.Update(existing);
                _repo.SaveChanges();
            }
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
