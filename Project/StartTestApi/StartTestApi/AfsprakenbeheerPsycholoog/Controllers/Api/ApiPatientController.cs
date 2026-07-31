using AfsprakenbeheerPsycholoog.Authentication;
using AfsprakenbeheerPsycholoog.Models.ViewModels.Patient;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/patient")]
    [Authorize(Policy = "PsycholoogOnly")]
    public class ApiPatientController : ControllerBase
    {
        private readonly IPatientService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ApiPatientController> _logger;

        public ApiPatientController(
            IPatientService service,
            UserManager<ApplicationUser> userManager,
            IServiceScopeFactory scopeFactory,
            ILogger<ApiPatientController> logger)
        {
            _service = service;
            _userManager = userManager;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAllePatienten()
        {
            var patienten = _service.GetAllePatienten();
            return Ok(patienten);
        }

        [HttpGet("archief")]
        public IActionResult GetInactievePatienten()
        {
            var patienten = _service.GetInactievePatienten();
            return Ok(patienten);
        }

        [HttpGet("{id}")]
        public IActionResult GetPatientDetail(int id)
        {
            var patient = _service.GetPatientDetail(id);
            if (patient == null) return NotFound(new { message = "Patiënt niet gevonden." });
            return Ok(patient);
        }

        [HttpPost]
        public IActionResult CreatePatient([FromBody] CreatePatientViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var id = _service.CreatePatient(vm);

            if (!string.IsNullOrWhiteSpace(vm.Email))
            {
                var patientEmail = vm.Email.Trim();
                var patientNaam = $"{vm.Voornaam} {vm.Achternaam}".Trim();

                // Probeer eventueel al een bestaand account te koppelen
                _service.KoppelPatientAanUser(id, patientEmail);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendPatientWelcomeEmailAsync(patientEmail, patientNaam);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fout bij verzenden van welkomstmail naar {Email}", patientEmail);
                    }
                });
            }

            return CreatedAtAction(nameof(GetPatientDetail), new { id = id }, new { id = id, message = "Patiënt succesvol aangemaakt en welkomstmail verzonden." });
        }

        [HttpPut("{id}")]
        public IActionResult EditPatient(int id, [FromBody] EditPatientViewModel vm)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != vm.Id) return BadRequest(new { message = "ID in URL komt niet overeen met ID in body." });

            var patient = _service.GetPatientDetail(id);
            if (patient == null) return NotFound(new { message = "Patiënt niet gevonden." });

            _service.EditPatient(vm);
            return Ok(new { message = "Patiënt succesvol bijgewerkt." });
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePatient(int id)
        {
            var patient = _service.GetPatientDetail(id);
            if (patient == null) return NotFound(new { message = "Patiënt niet gevonden." });

            _service.DeletePatient(id);
            return Ok(new { message = "Patiënt succesvol op inactief gezet." });
        }

        [HttpPost("{id}/heractiveer")]
        public IActionResult HeractiveerPatient(int id)
        {
            var succes = _service.HeractiveerPatient(id);
            if (!succes) return NotFound(new { message = "Patiënt kon niet worden heractiveerd." });
            return Ok(new { message = "Patiënt succesvol heractiveerd." });
        }

        [HttpGet("aanmeldingen")]
        public async Task<IActionResult> GetNieuweAanmeldingen()
        {
            var aanmeldingen = await _service.GetNieuweAanmeldingenAsync();
            return Ok(aanmeldingen);
        }

        [HttpPost("aanmeldingen/{userId}/goedkeuren")]
        public async Task<IActionResult> MaakNieuwePatient(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return BadRequest(new { message = "Gebruiker niet gevonden." });

            var (succes, naam) = await _service.MaakEnKoppelNieuwePatientAsync(userId);
            if (!succes) return BadRequest(new { message = "Kon de nieuwe patiënt niet aanmaken of koppelen." });

            if (!string.IsNullOrEmpty(user.Email))
            {
                var userEmail = user.Email;
                var patientNaam = string.IsNullOrWhiteSpace(naam) ? user.UserName : naam;

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendAccountApprovalEmailAsync(userEmail, patientNaam);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fout bij verzenden van account-goedkeuringsmail naar {Email}", userEmail);
                    }
                });
            }

            return Ok(new { message = $"Nieuwe patiënt '{naam}' succesvol aangemaakt en gekoppeld. Er is een e-mail ter bevestiging naar de patiënt verzonden." });
        }

        [HttpPost("koppel")]
        public IActionResult Koppel([FromBody] KoppelRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var succes = _service.KoppelPatientAanUser(request.PatientId, request.Email, request.SetAsPrimaryEmail);
            if (!succes) return BadRequest(new { message = "Kon patiënt niet koppelen. Bestaat deze account niet of is deze al gekoppeld?" });

            var patient = _service.GetPatientDetail(request.PatientId);
            var patientNaam = patient != null ? $"{patient.Voornaam} {patient.Achternaam}" : "Patiënt";
            var userEmail = request.Email;

            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                    await emailService.SendAccountApprovalEmailAsync(userEmail, patientNaam);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij verzenden van account-koppelingsmail naar {Email}", userEmail);
                }
            });

            return Ok(new { message = $"Patiënt succesvol gekoppeld aan {request.Email}. Er is een e-mail ter bevestiging naar de patiënt verzonden." });
        }

        [HttpPost("{patientId}/ontkoppel")]
        public IActionResult Ontkoppel(int patientId)
        {
            var succes = _service.OntkoppelPatientVanUser(patientId);
            if (!succes) return BadRequest(new { message = "Er was geen gekoppeld account gevonden voor deze patiënt." });
            return Ok(new { message = "Account succesvol ontkoppeld." });
        }
    }

    public class KoppelRequest
    {
        public int PatientId { get; set; }
        public string Email { get; set; } = null!;
        public bool SetAsPrimaryEmail { get; set; } = true;
    }
}
