using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
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
            var settings = _context.PraktijkInstellingen.FirstOrDefault(s => s.Id == 1);
            if (settings == null)
            {
                model.Id = 1;
                _context.PraktijkInstellingen.Add(model);
            }
            else
            {
                settings.GoogleCalendarId = model.GoogleCalendarId;
                
                settings.MaandagActief = model.MaandagActief;
                settings.MaandagStart = model.MaandagStart;
                settings.MaandagEinde = model.MaandagEinde;
                settings.Maandag2Actief = model.Maandag2Actief;
                settings.MaandagStart2 = model.MaandagStart2;
                settings.MaandagEinde2 = model.MaandagEinde2;
                
                settings.DinsdagActief = model.DinsdagActief;
                settings.DinsdagStart = model.DinsdagStart;
                settings.DinsdagEinde = model.DinsdagEinde;
                settings.Dinsdag2Actief = model.Dinsdag2Actief;
                settings.DinsdagStart2 = model.DinsdagStart2;
                settings.DinsdagEinde2 = model.DinsdagEinde2;

                settings.WoensdagActief = model.WoensdagActief;
                settings.WoensdagStart = model.WoensdagStart;
                settings.WoensdagEinde = model.WoensdagEinde;
                settings.Woensdag2Actief = model.Woensdag2Actief;
                settings.WoensdagStart2 = model.WoensdagStart2;
                settings.WoensdagEinde2 = model.WoensdagEinde2;

                settings.DonderdagActief = model.DonderdagActief;
                settings.DonderdagStart = model.DonderdagStart;
                settings.DonderdagEinde = model.DonderdagEinde;
                settings.Donderdag2Actief = model.Donderdag2Actief;
                settings.DonderdagStart2 = model.DonderdagStart2;
                settings.DonderdagEinde2 = model.DonderdagEinde2;

                settings.VrijdagActief = model.VrijdagActief;
                settings.VrijdagStart = model.VrijdagStart;
                settings.VrijdagEinde = model.VrijdagEinde;
                settings.Vrijdag2Actief = model.Vrijdag2Actief;
                settings.VrijdagStart2 = model.VrijdagStart2;
                settings.VrijdagEinde2 = model.VrijdagEinde2;

                settings.ZaterdagActief = model.ZaterdagActief;
                settings.ZaterdagStart = model.ZaterdagStart;
                settings.ZaterdagEinde = model.ZaterdagEinde;
                settings.Zaterdag2Actief = model.Zaterdag2Actief;
                settings.ZaterdagStart2 = model.ZaterdagStart2;
                settings.ZaterdagEinde2 = model.ZaterdagEinde2;

                settings.ZondagActief = model.ZondagActief;
                settings.ZondagStart = model.ZondagStart;
                settings.ZondagEinde = model.ZondagEinde;
                settings.Zondag2Actief = model.Zondag2Actief;
                settings.ZondagStart2 = model.ZondagStart2;
                settings.ZondagEinde2 = model.ZondagEinde2;

                settings.SlotDuurMinuten = model.SlotDuurMinuten;
                settings.BufferMinuten = model.BufferMinuten;

                settings.LocatiePraktijk = model.LocatiePraktijk;
                settings.LocatieGoogleMeet = model.LocatieGoogleMeet;
                settings.LocatieTelefoon = model.LocatieTelefoon;

                settings.MinimaalVoorafUren = model.MinimaalVoorafUren;
                settings.MaximaleToekomstDagen = model.MaximaleToekomstDagen;

                _context.PraktijkInstellingen.Update(settings);
            }
            _context.SaveChanges();
            return Ok(settings);
        }

        [HttpPost("sync-calendar")]
        [Authorize(Policy = "PsycholoogOnly")]
        public async Task<IActionResult> SyncCalendar()
        {
            try
            {
                await _googleCalendarService.SyncIncomingChangesAsync();
                return Ok(new { message = "Synchronisatie succesvol afgerond." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
