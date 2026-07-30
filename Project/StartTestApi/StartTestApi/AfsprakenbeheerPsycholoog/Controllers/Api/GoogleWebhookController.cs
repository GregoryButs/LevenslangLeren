using Microsoft.AspNetCore.Mvc;
using AfsprakenbeheerPsycholoog.Services;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/webhooks/google")]
    public class GoogleWebhookController : ControllerBase
    {
        private readonly IGoogleCalendarService _googleCalendarService;

        public GoogleWebhookController(IGoogleCalendarService googleCalendarService)
        {
            _googleCalendarService = googleCalendarService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            // Google push notifications sturen een POST verzoek naar ons webhook endpoint wanneer er een wijziging optreedt.
            // Omdat de body geen details bevat over de specifieke wijzigingen, starten we een volledige agendasync via de service.
            await _googleCalendarService.SyncIncomingChangesAsync();
            return Ok();
        }
    }
}
