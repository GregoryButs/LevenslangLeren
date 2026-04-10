using AutoMapper;
using StartTestApi.Data.Entities;
using StartTestApi.Data.Repositories;
using StartTestApi.DTO;

namespace StartTestApi.Services
{
    public class InschrijvingService : IInschrijvingService
    {
        public readonly IInschrijvingRepository _repository;
        private readonly IMapper _mapper;

        public InschrijvingService(IInschrijvingRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public DTO.InschrijvingDTO? GetInschrijvingById(int id)
        {
            var inschrijving = _repository.GetByIdWithEvent(id);
            if (inschrijving == null)
            {
                return null;
            }
            return _mapper.Map<InschrijvingDTO>(inschrijving);
        }

        public void CreateInschrijving(CreateInschrijvingDTO createInschrijvingDto)
        {
            var inschrijvingEntity = _mapper.Map<Inschrijving>(createInschrijvingDto);
            _repository.AddInschrijving(inschrijvingEntity);
            _repository.SaveChanges();
        }

        public void DeleteInschrijving(int id)
        {
            var inschrijving = _repository.GetById(id);
            if (inschrijving is null) return;
            _repository.Delete(inschrijving);
            _repository.SaveChanges();
        }
    }
}
