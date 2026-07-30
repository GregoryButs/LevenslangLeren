using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class ApiDashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public ApiDashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult GetDashboard([FromQuery] DateTime? weekDatum = null)
        {
            try
            {
                var name = "Inge Debast";
                var model = _dashboardService.GetDashboard(name, weekDatum);
                return Ok(model);
            }
            catch (Exception ex)
            {
                System.IO.File.WriteAllText("api_dashboard_error.txt", ex.ToString());
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("weekoverzicht")]
        public IActionResult GetWeekOverzicht([FromQuery] DateTime datum, [FromQuery] int? patientId = null, [FromQuery] bool isPsycholoog = false)
        {
            if (!isPsycholoog)
            {
                var patientIdClaim = User.FindFirst("PatientId")?.Value;
                if (string.IsNullOrEmpty(patientIdClaim) || !int.TryParse(patientIdClaim, out var loggedInPatientId) || loggedInPatientId != patientId)
                {
                    return Forbid("U kunt alleen uw eigen weekoverzicht bekijken.");
                }
            }
            else
            {
                if (!User.HasClaim("IsPsycholoog", "true"))
                {
                    return Forbid("Alleen de psycholoog kan dit overzicht bekijken.");
                }
            }

            var model = _dashboardService.GetWeekOverzicht(datum, patientId, isPsycholoog);
            return Ok(model);
        }
    }
}
