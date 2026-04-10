using AutoMapper;

namespace StartTestApi.Profiles
{
    public class EventProfile : Profile
    {
        public EventProfile()
        {
            CreateMap<Data.Entities.Event, DTO.EventDTO>()
                .ForMember(dest => dest.AantalInschrijvingen, opt => opt.MapFrom(src => src.Inschrijvingen.Count()));
        }
    }
}
