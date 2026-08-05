using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AutoMapper;

namespace AfsprakenbeheerPsycholoog.Profiles
{
    public class AfspraakProfile : Profile
    {
        /// <summary>
        /// Automapper-profiel voor het mappen van Afspraak-gerelateerde entiteiten en viewmodels, 
        /// inclusief speciale logica voor het tonen van patiëntnamen en afspraaktypen.
        /// </summary>
        public AfspraakProfile()
        {
            // Afspraak -> AfspraakListViewModel
            CreateMap<Afspraak, AfspraakListViewModel>()
                .ForMember(dest => dest.PatientNaam,
                    opt => opt.MapFrom(src =>
                        src.Patient != null
                            ? $"{src.Patient.Voornaam} {src.Patient.Achternaam}".Trim()
                            : (!string.IsNullOrWhiteSpace(src.Opmerkingen) && !src.Opmerkingen.Trim().Equals("[PH9500]") && !src.Opmerkingen.Trim().Equals("Blokkering", StringComparison.OrdinalIgnoreCase))
                                ? src.Opmerkingen.Replace("[PH9500]", "").Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim()
                                : (src.Type != null ? src.Type.Naam : "Praktijkhuis Consultatie")))
                .ForMember(dest => dest.AfspraakTypeNaam,
                    opt => opt.MapFrom(src => src.Type != null ? src.Type.Naam : (src.PatientId.HasValue ? "Consultatie" : "Blokkering")))
                .ForMember(dest => dest.PatientRijksregisternummer,
                    opt => opt.MapFrom(src => src.Patient != null ? src.Patient.Rijksregisternummer : null))
                .ForMember(dest => dest.PatientDossierNummer,
                    opt => opt.MapFrom(src => src.Patient != null ? src.Patient.DossierNummer : null))
                .ForMember(dest => dest.Kleurcode,
                    opt => opt.MapFrom(src => src.Type != null ? src.Type.Kleurcode : (src.PatientId.HasValue ? "#478d96" : "#64748B")));

            // Afspraak -> AfspraakDetailViewModel
            CreateMap<Afspraak, AfspraakDetailViewModel>()
                .ForMember(dest => dest.PatientVolledigeNaam,
                    opt => opt.MapFrom(src =>
                        src.Patient != null
                            ? $"{src.Patient.Voornaam} {src.Patient.Achternaam}".Trim()
                            : (!string.IsNullOrWhiteSpace(src.Opmerkingen) && !src.Opmerkingen.Trim().Equals("[PH9500]") && !src.Opmerkingen.Trim().Equals("Blokkering", StringComparison.OrdinalIgnoreCase))
                                ? src.Opmerkingen.Replace("[PH9500]", "").Trim().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)[0].Trim()
                                : (src.Type != null ? src.Type.Naam : "Praktijkhuis Consultatie")))
                .ForMember(dest => dest.PatientEmail,
                    opt => opt.MapFrom(src => src.Patient != null ? src.Patient.Email : "—"))
                .ForMember(dest => dest.PatientTelefoon,
                    opt => opt.MapFrom(src => src.Patient != null ? src.Patient.Telefoonnummer : "—"))
                .ForMember(dest => dest.AfspraakTypeNaam,
                    opt => opt.MapFrom(src => src.Type != null ? src.Type.Naam : (src.PatientId.HasValue ? "Therapie" : "Blokkering")))
                .ForMember(dest => dest.Kleurcode,
                    opt => opt.MapFrom(src => src.Type != null ? src.Type.Kleurcode : (src.PatientId.HasValue ? "#478d96" : "#64748B")));

            // CreateAfspraakViewModel -> Afspraak
            CreateMap<CreateAfspraakViewModel, Afspraak>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AfspraakStatus.Gepland))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore());

            // AfspraakEditViewModel → Afspraak
            CreateMap<EditAfspraakViewModel, Afspraak>()
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore());

            // Afspraak -> EditAfspraakViewModel
            CreateMap<Afspraak, EditAfspraakViewModel>()
                .ForMember(dest => dest.CustomDuurMinuten, opt => opt.MapFrom(src => (int)Math.Max(15, Math.Round((src.Eindtijd - src.Starttijd).TotalMinutes))))
                .ForMember(dest => dest.LocatieType, opt => opt.MapFrom(src =>
                    !string.IsNullOrEmpty(src.GoogleMeetLink) || (src.Opmerkingen != null && (src.Opmerkingen.Contains("GoogleMeet") || src.Opmerkingen.Contains("Google Meet") || src.Opmerkingen.Contains("[Online via Google Meet]")))
                        ? "GoogleMeet"
                        : (src.Opmerkingen != null && (src.Opmerkingen.Contains("Telefoon") || src.Opmerkingen.Contains("Telefonisch") || src.Opmerkingen.Contains("[Telefonische meeting]")))
                            ? "Telefoon"
                            : "Praktijk"
                ));

            // PatientBoekAfspraakViewModel → Afspraak
            CreateMap<PatientBoekAfspraakViewModel, Afspraak>()
                .ForMember(dest => dest.Starttijd, opt => opt.MapFrom(src => src.GekozeTijdslot))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => AfspraakStatus.Gepland))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PatientId, opt => opt.Ignore())
                .ForMember(dest => dest.Eindtijd, opt => opt.Ignore())
                .ForMember(dest => dest.Patient, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.Ignore());
        }
    }
}
