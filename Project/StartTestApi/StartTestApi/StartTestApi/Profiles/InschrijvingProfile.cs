using AutoMapper;
using StartTestApi.Data.Entities;
using StartTestApi.DTO;

namespace StartTestApi.Profiles
{
    public class InschrijvingProfile : Profile
    {
        public InschrijvingProfile()
        {
            CreateMap<Inschrijving, InschrijvingDTO>()
               .ForMember(d => d.EventTitel, o => o.MapFrom(s => s.Event.Titel))
               .ForMember(d => d.EventStartDatumTijd, o => o.MapFrom(s => s.Event.StartDatumTijd));

            CreateMap<CreateInschrijvingDTO, Inschrijving>()
                // InschrijfDatumTijd wordt ingesteld op DateTime.Now op het moment van aanmaken
                .ForMember(d => d.InschrijfDatumTijd, o => o.MapFrom(s => DateTime.Now));
        }
    }
}
