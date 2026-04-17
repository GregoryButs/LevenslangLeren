using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AutoMapper;

namespace AfsprakenbeheerPsycholoog.Profiles
{
    public class AfspraakProfile : Profile
    {
        public AfspraakProfile()
        {
            // Afspraak -> AfspraakListViewModel
            CreateMap<Afspraak, AfspraakListViewModel>()
                .ForMember(dest => dest.PatientNaam,
                    opt => opt.MapFrom(src => $"{src.Patient.Voornaam} {src.Patient.Achternaam}"))
                .ForMember(dest => dest.AfspraakTypeNaam,
                    opt => opt.MapFrom(src => src.Type.Naam))
                .ForMember(dest => dest.Kleurcode,
                    opt => opt.MapFrom(src => src.Type.Kleurcode));

            // Afspraak -> AfspraakDetailViewModel
            CreateMap<Afspraak, AfspraakDetailViewModel>()
                .ForMember(dest => dest.PatientVolledigeNaam,
                    opt => opt.MapFrom(src => $"{src.Patient.Voornaam} {src.Patient.Achternaam}"))
                .ForMember(dest => dest.PatientEmail,
                    opt => opt.MapFrom(src => src.Patient.Email))
                .ForMember(dest => dest.PatientTelefoon,
                    opt => opt.MapFrom(src => src.Patient.Telefoonnummer))
                .ForMember(dest => dest.AfspraakTypeNaam,
                    opt => opt.MapFrom(src => src.Type.Naam))
                .ForMember(dest => dest.Kleurcode,
                    opt => opt.MapFrom(src => src.Type.Kleurcode));

            // CreateAfspraakViewModel -> Afspraak
            CreateMap<CreateAfspraakViewModel, Afspraak>()
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => AfspraakStatus.Gepland)) // default
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore());

            // AfspraakEditViewModel → Afspraak
            CreateMap<EditAfspraakViewModel, Afspraak>()
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore());

            // Afspraak -> AfspraakEditViewModel
            CreateMap<Afspraak, EditAfspraakViewModel>()
                .ForMember(dest => dest.PatientenLijst, opt => opt.Ignore())
                .ForMember(dest => dest.TypenLijst, opt => opt.Ignore())
                .ForMember(dest => dest.StatusLijst, opt => opt.Ignore());

            // PatientBoekAfspraakViewModel → Afspraak
            CreateMap<PatientBoekAfspraakViewModel, Afspraak>()
                .ForMember(dest => dest.Starttijd,
                    opt => opt.MapFrom(src => src.GekozeTijdslot))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => AfspraakStatus.Gepland))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())  // manueel na Map()
                .ForMember(dest => dest.Eindtijd, opt => opt.Ignore())  // manueel na Map()
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore());

        }
    }
}
