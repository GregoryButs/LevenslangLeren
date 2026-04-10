using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using StartTestApi.Data.Entities;
using StartTestApi.Data.Repositories;
using StartTestApi.DTO;

namespace StartTestApi.Services
{
    public class EventService : IEventService
    {
        public readonly IEventRepository _repository;
        private readonly IMapper _mapper;

        public EventService(IEventRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public IEnumerable<EventDTO> GetAllEventsWithInschrijvingen()
        {
            // map de events naar eventDTO's, en bereken het aantal inschrijvingen voor elke event
            return _repository.GetAllWithInschrijvingen().Select(ev =>
            {
                EventDTO dto = _mapper.Map<EventDTO>(ev);
                return dto;
            });
        }


        public EventDTO GetEventByIdWithInschrijvingen(int id)
        {
            var ev = _repository.GetByIdWithInschrijvingen(id);
            if (ev == null)
            {
                return null;
            }
            EventDTO dto = _mapper.Map<EventDTO>(ev);
            return dto;
        }
    }
}
