using test3_model.Data.Entities;

namespace test4_start.Services
{
    public class BegeleiderService : IBegeleiderService
    {
        private readonly IRepository<Begeleider> _begeleiderRepository;
        public BegeleiderService(IRepository<Begeleider> begeleiderRepository)
        {
            _begeleiderRepository = begeleiderRepository;
        }
        public IEnumerable<Begeleider> GetBegeleiders()
        {
            return _begeleiderRepository.GetAll();
        }
    }
}
