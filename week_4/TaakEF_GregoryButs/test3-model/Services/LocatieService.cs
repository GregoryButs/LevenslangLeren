using test3_model.Data.Entities;

namespace test4_start.Services
{
    public class LocatieService : ILocatieService
    {
        private readonly IRepository<Locatie> _locatieRepository;

        public LocatieService(IRepository<Locatie> locatieRepository)
        {
            _locatieRepository = locatieRepository;
        }

        public IEnumerable<Locatie> GetLocaties()
        {
            return _locatieRepository.GetAll();
        }
    }
}
