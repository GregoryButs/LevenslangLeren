using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Extensions;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/settings")]
    [Authorize]
    public class ApiSettingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoogleCalendarService _googleCalendarService;

        public ApiSettingsController(ApplicationDbContext context, IGoogleCalendarService googleCalendarService)
        {
            _context = context;
            _googleCalendarService = googleCalendarService;
        }

        [HttpGet]
        public IActionResult GetSettings()
        {
            var settings = _context.PraktijkInstellingen.FirstOrDefault(s => s.Id == 1);
            if (settings == null)
            {
                settings = new PraktijkInstelling { Id = 1 };
                _context.PraktijkInstellingen.Add(settings);
                _context.SaveChanges();
            }
            return Ok(settings);
        }

        [HttpPost]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult UpdateSettings([FromBody] PraktijkInstelling model)
        {
            try
            {
                var settings = _context.PraktijkInstellingen.FirstOrDefault();
                if (settings == null)
                {
                    settings = new PraktijkInstelling();
                    _context.PraktijkInstellingen.Add(settings);
                }

                settings.GoogleCalendarId = string.IsNullOrWhiteSpace(model.GoogleCalendarId) ? "primary" : model.GoogleCalendarId;
                
                settings.MaandagActief = model.MaandagActief;
                settings.MaandagStart = model.MaandagStart ?? "09:00";
                settings.MaandagEinde = model.MaandagEinde ?? "12:00";
                settings.Maandag2Actief = model.Maandag2Actief;
                settings.MaandagStart2 = model.MaandagStart2 ?? "13:00";
                settings.MaandagEinde2 = model.MaandagEinde2 ?? "17:00";
                
                settings.DinsdagActief = model.DinsdagActief;
                settings.DinsdagStart = model.DinsdagStart ?? "09:00";
                settings.DinsdagEinde = model.DinsdagEinde ?? "12:00";
                settings.Dinsdag2Actief = model.Dinsdag2Actief;
                settings.DinsdagStart2 = model.DinsdagStart2 ?? "13:00";
                settings.DinsdagEinde2 = model.DinsdagEinde2 ?? "17:00";

                settings.WoensdagActief = model.WoensdagActief;
                settings.WoensdagStart = model.WoensdagStart ?? "09:00";
                settings.WoensdagEinde = model.WoensdagEinde ?? "12:00";
                settings.Woensdag2Actief = model.Woensdag2Actief;
                settings.WoensdagStart2 = model.WoensdagStart2 ?? "13:00";
                settings.WoensdagEinde2 = model.WoensdagEinde2 ?? "17:00";

                settings.DonderdagActief = model.DonderdagActief;
                settings.DonderdagStart = model.DonderdagStart ?? "09:00";
                settings.DonderdagEinde = model.DonderdagEinde ?? "12:00";
                settings.Donderdag2Actief = model.Donderdag2Actief;
                settings.DonderdagStart2 = model.DonderdagStart2 ?? "13:00";
                settings.DonderdagEinde2 = model.DonderdagEinde2 ?? "17:00";

                settings.VrijdagActief = model.VrijdagActief;
                settings.VrijdagStart = model.VrijdagStart ?? "09:00";
                settings.VrijdagEinde = model.VrijdagEinde ?? "12:00";
                settings.Vrijdag2Actief = model.Vrijdag2Actief;
                settings.VrijdagStart2 = model.VrijdagStart2 ?? "13:00";
                settings.VrijdagEinde2 = model.VrijdagEinde2 ?? "17:00";

                settings.ZaterdagActief = model.ZaterdagActief;
                settings.ZaterdagStart = model.ZaterdagStart ?? "10:00";
                settings.ZaterdagEinde = model.ZaterdagEinde ?? "12:00";
                settings.Zaterdag2Actief = model.Zaterdag2Actief;
                settings.ZaterdagStart2 = model.ZaterdagStart2 ?? "13:00";
                settings.ZaterdagEinde2 = model.ZaterdagEinde2 ?? "17:00";

                settings.ZondagActief = model.ZondagActief;
                settings.ZondagStart = model.ZondagStart ?? "10:00";
                settings.ZondagEinde = model.ZondagEinde ?? "12:00";
                settings.Zondag2Actief = model.Zondag2Actief;
                settings.ZondagStart2 = model.ZondagStart2 ?? "13:00";
                settings.ZondagEinde2 = model.ZondagEinde2 ?? "17:00";

                settings.SlotDuurMinuten = model.SlotDuurMinuten <= 0 ? 60 : model.SlotDuurMinuten;
                settings.BufferMinuten = model.BufferMinuten < 0 ? 0 : model.BufferMinuten;

                settings.LocatiePraktijk = model.LocatiePraktijk;
                settings.LocatieGoogleMeet = model.LocatieGoogleMeet;
                settings.LocatieTelefoon = model.LocatieTelefoon;

                settings.MinimaalVoorafUren = model.MinimaalVoorafUren;
                settings.MaximaleToekomstDagen = model.MaximaleToekomstDagen;

                _context.SaveChanges();
                return Ok(settings);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = $"Fout bij opslaan instellingen: {ex.InnerException?.Message ?? ex.Message}" });
            }
        }

        [HttpPost("sync-calendar")]
        [Authorize(Policy = "PsycholoogOnly")]
        public async Task<IActionResult> SyncCalendar()
        {
            try
            {
                await _googleCalendarService.SyncIncomingChangesAsync();
                DatabaseMigrationExtensions.RestoreFromDump(_context);
                return Ok(new { message = "Agenda en alle patiëntafspraken zijn succesvol hersteld en gesynchroniseerd!" });
            }
            catch (System.Exception ex)
            {
                var errorDetails = ex.InnerException != null ? $"{ex.Message} -> {ex.InnerException.Message}" : ex.Message;
                return StatusCode(500, new { error = errorDetails });
            }
        }

        [HttpPost("clean-resync")]
        [Authorize(Policy = "PsycholoogOnly")]
        public async Task<IActionResult> CleanResync()
        {
            try
            {
                // Alleen externe blokkeringen / Google-events ZONDER patiënt verwijderen!
                // Afspraken van patiënten (PatientId != null) moeten bewaard blijven voor patiënt- en boekingshistoriek.
                var externalBlockers = _context.Afspraken.Where(a => a.GoogleEventId != null && a.PatientId == null);
                _context.Afspraken.RemoveRange(externalBlockers);
                await _context.SaveChangesAsync();

                await _googleCalendarService.SyncIncomingChangesAsync();
                DatabaseMigrationExtensions.RestoreFromDump(_context);
                return Ok(new { message = "Agenda is opgeschoond voor externe blokkeringen en opnieuw gesynchroniseerd vanuit Google Calendar. Patiëntafspraken zijn behouden." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("restore-dump")]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult RestoreDump()
        {
            try
            {
                DatabaseMigrationExtensions.RestoreFromDump(_context);
                return Ok(new { message = "Herstelprocedure succesvol uitgevoerd. Patiëntkoppelingen zijn opnieuw opgebouwd." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
