using AfsprakenbeheerPsycholoog.Models.ViewModels.Afspraak;
using AfsprakenbeheerPsycholoog.Services;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/afspraak")]
    [Authorize(Policy = "PsycholoogOnly")]
    public class ApiAfspraakController : ControllerBase
    {
        private readonly IAfspraakService _afspraakService;
        private readonly IPatientRepository _patientRepo;
        private readonly IAfspraakTypeRepository _typeRepo;
        private readonly IGoogleCalendarService _googleCalendarService;

        public ApiAfspraakController(
            IAfspraakService afspraakService,
            IPatientRepository patientRepo,
            IAfspraakTypeRepository typeRepo,
            IGoogleCalendarService googleCalendarService)
        {
            _afspraakService = afspraakService;
            _patientRepo = patientRepo;
            _typeRepo = typeRepo;
            _googleCalendarService = googleCalendarService;
        }

        private static DateTime _lastSyncTime = DateTime.MinValue;
        private static readonly object _syncLock = new object();

        [HttpGet]
        public IActionResult GetAlleAfspraken([FromQuery] bool forceSync = false)
        {
            bool shouldSync = forceSync;
            if (!shouldSync)
            {
                lock (_syncLock)
                {
                    if ((DateTime.UtcNow - _lastSyncTime).TotalSeconds > 60)
                    {
                        _lastSyncTime = DateTime.UtcNow;
                        shouldSync = true;
                    }
                }
            }

            if (shouldSync)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = HttpContext.RequestServices.CreateScope();
                        var calendarService = scope.ServiceProvider.GetRequiredService<IGoogleCalendarService>();
                        await calendarService.SyncIncomingChangesAsync();
                    }
                    catch { }
                });
            }

            var afspraken = _afspraakService.GetAlleAfspraken();
            return Ok(afspraken);
        }

        [HttpGet("dagplanning")]
        public IActionResult GetDagplanning([FromQuery] DateTime? datum)
        {
            var dag = datum ?? DateTime.Today;
            var planning = _afspraakService.GetDagOverzicht(dag);
            return Ok(planning);
        }

        [HttpGet("{id}")]
        public IActionResult GetDetails(int id)
        {
            var afspraak = _afspraakService.GetAfspraakDetail(id);
            if (afspraak == null) return NotFound(new { message = "Afspraak niet gevonden." });
            return Ok(afspraak);
        }

        [HttpGet("create-data")]
        public IActionResult GetCreateData()
        {
            var patienten = _patientRepo.GetAllByCondition(p => p.IsActief).Select(p => new { p.Id, Naam = $"{p.Voornaam} {p.Achternaam}" }).OrderBy(p => p.Naam);
            var types = _typeRepo.GetAll().Select(t => new { t.Id, t.Naam, t.StandaardDuurMinuten, t.Kleurcode, t.VereistPatient });
            return Ok(new
            {
                Patienten = patienten,
                Types = types
            });
        }

        [HttpPost]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("booking-policy")]
        public async Task<IActionResult> Create([FromBody] CreateAfspraakViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var succes = await _afspraakService.CreateAfspraakAsync(vm);
            if (!succes)
            {
                return BadRequest(new { message = "Kies een ander moment: dit tijdstip overlapt met een reeds geplande afspraak of de invoer is ongeldig." });
            }

            return Ok(new { message = "Afspraak is ingepland." });
        }

        [HttpGet("{id}/edit-data")]
        public IActionResult GetEditData(int id)
        {
            var vm = _afspraakService.GetEditViewModel(id);
            if (vm == null) return NotFound(new { message = "Afspraak niet gevonden." });

            var patienten = _patientRepo.GetAllByCondition(p => p.IsActief).Select(p => new { p.Id, Naam = $"{p.Voornaam} {p.Achternaam}" }).OrderBy(p => p.Naam);
            var types = _typeRepo.GetAll().Select(t => new { t.Id, t.Naam, t.StandaardDuurMinuten, t.Kleurcode, t.VereistPatient });
            return Ok(new
            {
                ViewModel = vm,
                Patienten = patienten,
                Types = types
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] EditAfspraakViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != vm.Id) return BadRequest(new { message = "ID in URL komt niet overeen met ID in body." });

            var succes = await _afspraakService.EditAfspraakAsync(vm);
            if (!succes)
            {
                return BadRequest(new { message = "Kies een ander moment: dit tijdstip overlapt met een reeds geplande afspraak of de invoer is ongeldig." });
            }

            return Ok(new { message = "Afspraak is bijgewerkt." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _afspraakService.DeleteAfspraakAsync(id);
            return Ok(new { message = "Afspraak is verwijderd." });
        }

        [HttpDelete("reeks/{reeksId}")]
        public async Task<IActionResult> DeleteReeks(Guid reeksId)
        {
            if (reeksId == Guid.Empty) return BadRequest(new { message = "Ongeldige reeks." });
            await _afspraakService.DeleteReeksAsync(reeksId);
            return Ok(new { message = "Volledige reeks is verwijderd." });
        }
    }
}
