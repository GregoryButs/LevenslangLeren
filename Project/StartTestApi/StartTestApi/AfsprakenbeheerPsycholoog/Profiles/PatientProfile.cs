using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;
using AfsprakenbeheerPsycholoog.Authentication;
using AutoMapper;


namespace AfsprakenbeheerPsycholoog.Profiles
{
    public class PatientProfile : Profile
    {
        /// <summary>
        /// Automapper-profiel voor het mappen van Patient-gerelateerde entiteiten en viewmodels,
        /// inclusief speciale logica voor het tonen van volledige namen en aantal afspraken.
        /// </summary>
        public PatientProfile()
        {
            // Patient → PatientListViewModel
            CreateMap<Patient, PatientListViewModel>()
                .ForMember(dest => dest.VolledigeNaam,
                    opt => opt.MapFrom(src => $"{src.Voornaam} {src.Achternaam}"))
                .ForMember(dest => dest.Geboortedatum,
                    opt => opt.MapFrom(src => src.Geboortedatum.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.AantalAfspraken,
                    opt => opt.MapFrom(src => src.Afspraken != null ? src.Afspraken.Count() : 0))
                .ForMember(dest => dest.IsGekoppeld, opt => opt.Ignore()); // wordt in service gezet

            // Patient → PatientDetailViewModel
            CreateMap<Patient, PatientDetailViewModel>()
                .ForMember(dest => dest.VolledigeNaam,
                    opt => opt.MapFrom(src => $"{src.Voornaam} {src.Achternaam}"))
                .ForMember(dest => dest.IsGekoppeld, opt => opt.Ignore());

            // EditPatientViewModel ↔ Patient
            CreateMap<EditPatientViewModel, Patient>().ReverseMap();

            // CreatePatientViewModel → Patient
            CreateMap<CreatePatientViewModel, Patient>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Afspraken, opt => opt.Ignore());

            // ApplicationUser → CreatePatientViewModel
            CreateMap<ApplicationUser, CreatePatientViewModel>()
                .ForMember(dest => dest.Voornaam, opt => opt.MapFrom(src => src.Voornaam ?? "Nieuw"))
                .ForMember(dest => dest.Achternaam, opt => opt.MapFrom(src => src.Achternaam ?? "Account"))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Telefoonnummer, opt => opt.MapFrom(src => src.PhoneNumber ?? "000000000"))
                .ForMember(dest => dest.Geboortedatum, opt => opt.MapFrom(src => new DateOnly(2000, 1, 1)));
        }
    }
}
