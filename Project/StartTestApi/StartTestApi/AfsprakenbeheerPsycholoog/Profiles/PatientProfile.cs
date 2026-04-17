using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;
using AutoMapper;


namespace AfsprakenbeheerPsycholoog.Profiles
{
    public class PatientProfile : Profile
    {
        public PatientProfile()
        {
            // Patient → PatientListViewModel
            CreateMap<Patient, PatientListViewModel>()
                .ForMember(dest => dest.VolledigeNaam,
                    opt => opt.MapFrom(src => $"{src.Voornaam} {src.Achternaam}"))
                .ForMember(dest => dest.AantalAfspraken,
                    opt => opt.MapFrom(src => src.Afspraken != null ? src.Afspraken.Count() : 0))
                .ForMember(dest => dest.IsGekoppeld, opt => opt.Ignore()); // wordt in service gezet

            // Patient → PatientDetailViewModel
            CreateMap<Patient, PatientDetailViewModel>()
                .ForMember(dest => dest.VolledigeNaam,
                    opt => opt.MapFrom(src => $"{src.Voornaam} {src.Achternaam}"))
                .ForMember(dest => dest.IsGekoppeld, opt => opt.Ignore());

            // EditPatientViewModel → Patient
            CreateMap<EditPatientViewModel, Patient>();

            // CreatePatientViewModel → Patient
            CreateMap<CreatePatientViewModel, Patient>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Afspraken, opt => opt.Ignore());
        }
    }
}
