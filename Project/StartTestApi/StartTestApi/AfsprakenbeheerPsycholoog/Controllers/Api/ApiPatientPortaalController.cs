using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Helpers;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/patientportaal")]
    [Authorize]
    public class ApiPatientPortaalController : ControllerBase
    {
        private readonly IAfspraakService _afspraakService;
        private readonly IPatientBookingService _patientBookingService;
        private readonly Data.ApplicationDbContext _dbContext;
        private readonly IEmailService _emailService;

        public ApiPatientPortaalController(
            IAfspraakService afspraakService,
            IPatientBookingService patientBookingService,
            Data.ApplicationDbContext dbContext,
            IEmailService emailService)
        {
            _afspraakService = afspraakService;
            _patientBookingService = patientBookingService;
            _dbContext = dbContext;
            _emailService = emailService;
        }

        [HttpGet("mijnafspraken")]
        public IActionResult MijnAfspraken()
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue)
            {
                return BadRequest(new { message = "Uw account is nog niet gekoppeld aan een patiëntendossier." });
            }

            var afspraken = _afspraakService.GetAfsprakenVanPatient(patientId.Value);
            return Ok(afspraken);
        }

        [HttpGet("boeken")]
        public async Task<IActionResult> Boeken([FromQuery] DateTime? datum)
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue)
            {
                return BadRequest(new { message = "Uw account is nog niet gekoppeld aan een patiëntendossier." });
            }

            var minimumBoekdatum = DateTime.Today.AddDays(1);
            var gevraagdeDatum = datum ?? minimumBoekdatum;

            if (gevraagdeDatum.Date < minimumBoekdatum.Date)
            {
                gevraagdeDatum = minimumBoekdatum;
            }

            var dag = gevraagdeDatum;
            var overzicht = await _patientBookingService.GetDagOverzichtVoorPatientAsync(dag);
            overzicht.MinimumNavigatieDatum = minimumBoekdatum;

            var vm = _patientBookingService.GetBoekViewModel(dag);

            return Ok(new
            {
                DagOverzicht = overzicht,
                ViewModel = vm
            });
        }

        [HttpPost("boeken")]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("booking-policy")]
        public async Task<IActionResult> Boeken([FromBody] PatientBoekAfspraakViewModel vm)
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue)
            {
                // Geen patient gekoppeld
                return BadRequest(new { message = "Uw account is nog niet gekoppeld aan een patiëntendossier." });
            }

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var nuUtc = System.DateTime.UtcNow;
                var aantalGepland = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(
                    _dbContext.Afspraken,
                    a => a.PatientId == patientId.Value && a.Status == AfspraakStatus.Gepland && a.Eindtijd > nuUtc
                );

                if (aantalGepland >= 2)
                {
                    return BadRequest(new { message = "U kunt maximaal 2 geplande afspraken tegelijk hebben staan. Annuleer of voltooi eerst een afspraak om een nieuwe in te plannen." });
                }

                var succes = await _patientBookingService.CreatePatientAfspraakAsync(vm, patientId.Value);
                if (!succes)
                {
                    return Conflict(new { message = "Dit tijdslot is niet meer beschikbaar of u heeft het maximale aantal geplande afspraken bereikt." });
                }

                return Ok(new { message = "Je afspraak is succesvol geboekt!" });
            }
            catch (DbUpdateException)
            {
                // Concurrency of database conflict
                return Conflict(new { message = "Er is een boekingsconflict opgetreden (mogelijk is dit slot zojuist door iemand anders gereserveerd). Kies een ander moment." });
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { message = "Er is een interne fout opgetreden bij het verwerken van uw boeking." });
            }
        }

        [HttpPost("annuleren/{id}")]
        public async Task<IActionResult> Annuleren(int id)
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue)
            {
                return BadRequest(new { message = "Uw account is nog niet gekoppeld aan een patiëntendossier." });
            }

            var succes = await _patientBookingService.AnnuleerPatientAfspraakAsync(id, patientId.Value);
            if (!succes)
            {
                return BadRequest(new { message = "Deze afspraak kan niet meer geannuleerd worden (bijvoorbeeld omdat de termijn verstreken is)." });
            }

            return Ok(new { message = "Je afspraak is succesvol geannuleerd." });
        }

        [HttpGet("afspraak/{id}/ics")]
        public IActionResult GetIcsFile(int id)
        {
            var patientId = GetPatientId();
            if (!patientId.HasValue)
            {
                return BadRequest(new { message = "Uw account is nog niet gekoppeld aan een patiëntendossier." });
            }

            var afspraak = _dbContext.Afspraken
                .Include(a => a.Type)
                .FirstOrDefault(a => a.Id == id && a.PatientId == patientId.Value);

            if (afspraak == null)
            {
                return NotFound(new { message = "Afspraak niet gevonden." });
            }

            var afspraakTypeNaam = afspraak.Type?.Naam ?? "Sessie";
            var icsContent = _emailService.BuildIcsContent(
                afspraak.Starttijd,
                afspraak.Eindtijd,
                afspraakTypeNaam,
                afspraak.Id,
                afspraak.Opmerkingen
            );

            var filename = $"afspraak-{afspraakTypeNaam.ToLower().Replace(" ", "-")}.ics";
            var bytes = System.Text.Encoding.UTF8.GetBytes(icsContent);

            return File(bytes, "text/calendar", filename);
        }

        private int? GetPatientId()
        {
            var patientIdClaim = User.FindFirstValue("PatientId");
            if (string.IsNullOrWhiteSpace(patientIdClaim))
            {
                return null;
            }

            return int.TryParse(patientIdClaim, out var patientId) ? patientId : null;
        }
    }
}
