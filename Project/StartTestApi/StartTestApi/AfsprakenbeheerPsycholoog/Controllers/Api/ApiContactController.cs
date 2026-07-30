using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using AfsprakenbeheerPsycholoog.Services;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/contact")]
    public class ApiContactController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ApiContactController> _logger;

        public ApiContactController(IEmailService emailService, ILogger<ApiContactController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public class ContactFormDto
        {
            public string Name { get; set; } = string.Empty;
            public string Surname { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<IActionResult> SendContactMessage([FromBody] ContactFormDto model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest(new { message = "Naam en e-mailadres zijn verplicht." });
            }

            try
            {
                var subject = $"Nieuw contactbericht van {model.Name} {model.Surname}";
                var body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                        <h3 style='color: #1a2c30;'>Nieuw contactbericht via de website</h3>
                        <p><strong>Van:</strong> {model.Name} {model.Surname} ({model.Email})</p>
                        <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 15px 0;'>
                        <p style='white-space: pre-wrap; color: #334155;'>{model.Message}</p>
                        <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 15px 0;'>
                        <p style='font-size: 12px; color: #64748b;'>U kunt direct antwoorden op deze e-mail of contact opnemen via {model.Email}.</p>
                    </div>";

                // Send email to practice inbox
                await _emailService.SendEmailAsync("inge@deverstandhouding.be", subject, body);

                _logger.LogInformation($"Contactbericht ontvangen van {model.Name} ({model.Email}) en verzonden naar inge@deverstandhouding.be");

                return Ok(new { success = true, message = "Bericht succesvol verzonden." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verwerken van contactformulier.");
                return StatusCode(500, new { message = "Er is een fout opgetreden bij het versturen van uw bericht." });
            }
        }
    }
}
