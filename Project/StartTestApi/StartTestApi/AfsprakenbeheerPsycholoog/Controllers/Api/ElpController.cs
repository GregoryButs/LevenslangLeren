using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/elp")]
    [Authorize(Policy = "PsycholoogOnly")]
    public class ElpController : ControllerBase
    {
        private readonly IAfspraakRepository _afspraakRepo;
        private readonly IPatientRepository _patientRepo;

        public ElpController(IAfspraakRepository afspraakRepo, IPatientRepository patientRepo)
        {
            _afspraakRepo = afspraakRepo;
            _patientRepo = patientRepo;
        }

        [HttpGet("maandoverzicht")]
        public IActionResult GetMaandoverzicht([FromQuery] int? jaar, [FromQuery] int? maand)
        {
            int targetJaar = jaar ?? DateTime.Today.Year;
            int targetMaand = maand ?? DateTime.Today.Month;

            // Haal alle ELP afspraken op van het hele jaar om de sessieteller (Sessie X van 8) nauwkeurig te berekenen per patiënt
            var startVanJaar = new DateTime(targetJaar, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var eindVanJaar = new DateTime(targetJaar, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            var alleElpAfsprakenVanJaar = _afspraakRepo.GetAllByCondition(a => 
                a.TariefType == TariefType.ELP && 
                a.Starttijd >= startVanJaar && 
                a.Starttijd <= eindVanJaar &&
                a.Status != AfspraakStatus.Geannuleerd
            ).OrderBy(a => a.Starttijd).ToList();

            // Bereken per patiënt de volgorde van de sessies in dit jaar
            var sessieIndexenPerAfspraakId = new Dictionary<int, int>();
            var tellerPerPatient = new Dictionary<int, int>();

            foreach (var a in alleElpAfsprakenVanJaar)
            {
                if (a.PatientId.HasValue)
                {
                    int pId = a.PatientId.Value;
                    if (!tellerPerPatient.ContainsKey(pId))
                    {
                        tellerPerPatient[pId] = 0;
                    }
                    tellerPerPatient[pId]++;
                    sessieIndexenPerAfspraakId[a.Id] = tellerPerPatient[pId];
                }
            }

            // Filter op de geselecteerde maand voor het dashboard
            var maandAfspraken = alleElpAfsprakenVanJaar.Where(a => 
                a.Starttijd.Year == targetJaar && 
                a.Starttijd.Month == targetMaand
            ).OrderBy(a => a.Starttijd).ToList();

            var resultaat = maandAfspraken.Select(a =>
            {
                var patient = a.PatientId.HasValue ? _patientRepo.GetById(a.PatientId.Value) : null;
                int sessieNr = sessieIndexenPerAfspraakId.ContainsKey(a.Id) ? sessieIndexenPerAfspraakId[a.Id] : 1;

                return new
                {
                    id = a.Id,
                    starttijd = a.Starttijd,
                    eindtijd = a.Eindtijd,
                    patientId = a.PatientId,
                    patientNaam = patient != null ? $"{patient.Voornaam} {patient.Achternaam}".Trim() : (a.Opmerkingen ?? "Onbekende Patiënt"),
                    dossierNummer = patient?.DossierNummer ?? "DOS-N/A",
                    rijksregisternummer = patient?.Rijksregisternummer ?? null,
                    heeftRijksregisternummer = !string.IsNullOrWhiteSpace(patient?.Rijksregisternummer),
                    elpType = string.IsNullOrWhiteSpace(a.ELPType) ? "Individueel" : a.ELPType,
                    elpStatus = a.ELPStatus.ToString(),
                    elpSessieTeller = $"Sessie {sessieNr} van 8",
                    status = a.Status.ToString()
                };
            });

            return Ok(new
            {
                jaar = targetJaar,
                maand = targetMaand,
                totaalElpSessies = maandAfspraken.Count,
                totaalVerwerkt = maandAfspraken.Count(a => a.ELPStatus == ELPStatus.Verwerkt),
                totaalTeVerwerken = maandAfspraken.Count(a => a.ELPStatus == ELPStatus.TeVerwerken),
                ontbrekendeRijksregisternummers = maandAfspraken.Count(a => a.PatientId.HasValue && string.IsNullOrWhiteSpace(_patientRepo.GetById(a.PatientId.Value)?.Rijksregisternummer)),
                afspraken = resultaat
            });
        }

        [HttpPost("markeer-verwerkt")]
        public IActionResult MarkeerVerwerkt([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest(new { message = "Geen afspraken geselecteerd." });

            var afspraken = _afspraakRepo.GetAllByCondition(a => ids.Contains(a.Id)).ToList();
            foreach (var a in afspraken)
            {
                a.ELPStatus = ELPStatus.Verwerkt;
                _afspraakRepo.Update(a);
            }
            _afspraakRepo.SaveChanges();

            return Ok(new { message = $"{afspraken.Count} ELP-afspraken gemarkeerd als verwerkt." });
        }

        [HttpPost("markeer-te-verwerken")]
        public IActionResult MarkeerTeVerwerken([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest(new { message = "Geen afspraken geselecteerd." });

            var afspraken = _afspraakRepo.GetAllByCondition(a => ids.Contains(a.Id)).ToList();
            foreach (var a in afspraken)
            {
                a.ELPStatus = ELPStatus.TeVerwerken;
                _afspraakRepo.Update(a);
            }
            _afspraakRepo.SaveChanges();

            return Ok(new { message = $"{afspraken.Count} ELP-afspraken gemarkeerd als te verwerken." });
        }

        [HttpPost("toggle-status/{id}")]
        public IActionResult ToggleStatus(int id)
        {
            var afspraak = _afspraakRepo.GetById(id);
            if (afspraak == null) return NotFound(new { message = "Afspraak niet gevonden." });

            afspraak.ELPStatus = afspraak.ELPStatus == ELPStatus.Verwerkt ? ELPStatus.TeVerwerken : ELPStatus.Verwerkt;
            _afspraakRepo.Update(afspraak);
            _afspraakRepo.SaveChanges();

            return Ok(new
            {
                id = afspraak.Id,
                elpStatus = afspraak.ELPStatus.ToString(),
                message = $"Status gewijzigd naar {afspraak.ELPStatus}."
            });
        }
    }
}
