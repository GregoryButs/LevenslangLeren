using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AfsprakenbeheerPsycholoog.Controllers.Api
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class ApiAIController : ControllerBase
    {
        private readonly IAIService _aiService;
        private readonly ApplicationDbContext _dbContext;

        public ApiAIController(IAIService aiService, ApplicationDbContext dbContext)
        {
            _aiService = aiService;
            _dbContext = dbContext;
        }

        [HttpGet("patient-risks")]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult GetPatientRisks()
        {
            try
            {
                var patients = _dbContext.Patienten
                    .Include(p => p.Afspraken)
                    .ThenInclude(a => a.Type)
                    .Where(p => p.IsActief)
                    .ToList();

                var today = DateTime.Today;

                var patientRisks = patients.Select(p =>
                {
                    var completed = p.Afspraken.Where(a => a.Status == AfspraakStatus.Voltooid).ToList();
                    int sessionsCompleted = completed.Count;

                    int lastSessionGap = 7; // Standaard indien geen eerdere sessies
                    if (completed.Any())
                    {
                        var lastSessionDate = completed.Max(a => a.Starttijd);
                        lastSessionGap = (today - lastSessionDate.Date).Days;
                        if (lastSessionGap < 0) lastSessionGap = 0;
                    }

                    // Leeftijdsberekening
                    int age = today.Year - p.Geboortedatum.Year;
                    if (p.Geboortedatum > DateOnly.FromDateTime(today.AddYears(-age))) age--;

                    // Behandelingstype afleiden uit de nieuwste afspraak of de standaard
                    string treatmentType = p.Afspraken.OrderByDescending(a => a.Starttijd).FirstOrDefault()?.Type?.Naam ?? "Therapie";

                    double noShowProb = _aiService.GetNoShowProbability(age, sessionsCompleted, treatmentType, lastSessionGap);

                    string riskCategory = "Low";
                    if (noShowProb > 0.50) riskCategory = "High";
                    else if (noShowProb > 0.25) riskCategory = "Medium";

                    // Bereken Sentiment EMA van de patiënt op basis van voorgaande afspraken
                    double sentimentEma = 0.0;
                    if (completed.Any())
                    {
                        var orderedCompleted = completed.OrderBy(a => a.Starttijd).ToList();
                        double alpha = 0.3;
                        foreach (var afspraak in orderedCompleted)
                        {
                            double score = afspraak.SentimentScore ?? 0.0;
                            sentimentEma = alpha * score + (1.0 - alpha) * sentimentEma;
                        }
                    }

                    // Vraag aanbeveling op van de RL policy
                    string recommendedAction = _aiService.GetRecommendedAction(
                        sessionsCompleted,
                        p.EmotioneleStabiliteit ?? 5.5,
                        lastSessionGap,
                        sentimentEma
                    );

                    // Vraag expert-aanbeveling op
                    string heuristicAction = _aiService.GetHeuristicAction(
                        sessionsCompleted,
                        p.EmotioneleStabiliteit ?? 5.5,
                        lastSessionGap
                    );

                    // Redenen/Kenmerken verklaren
                    var reasons = new List<string>();
                    if (lastSessionGap > 21) reasons.Add("Groot sessie-interval (> 21 dagen)");
                    if (treatmentType.ToLower() == "depressie" || treatmentType.ToLower() == "crisis") reasons.Add("Klinisch risico (diagnose/crisis)");
                    if (age > 60) reasons.Add("Leeftijdscategorie > 60 jaar");
                    if (sessionsCompleted < 3) reasons.Add("Eerste fasen van de behandeling (< 3 sessies)");

                    string reason = reasons.Any() ? string.Join(", ", reasons) : "Standaard cliëntenprofiel (Laag risico)";

                    return new
                    {
                        PatientId = p.Id,
                        VolledigeNaam = p.VolledigeNaam,
                        Age = age,
                        SessionsCompleted = sessionsCompleted,
                        LastSessionGap = lastSessionGap,
                        TreatmentType = treatmentType,
                        EmotioneleStabiliteit = p.EmotioneleStabiliteit ?? 5.5,
                        NoShowProbability = noShowProb,
                        RiskCategory = riskCategory,
                        RecommendedAction = recommendedAction,
                        HeuristicAction = heuristicAction,
                        Reason = reason
                    };
                }).OrderByDescending(r => r.NoShowProbability).ToList();

                return Ok(patientRisks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Fout bij berekenen no-show risico's.", detail = ex.Message });
            }
        }

        [HttpGet("synthetic-patients")]
        [Authorize(Policy = "PsycholoogOnly")]
        public async Task<IActionResult> GetSyntheticPatients([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var (patients, totalCount) = await _aiService.GetSyntheticPatientsAsync(page, pageSize, search);

            return Ok(new
            {
                Patients = patients,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("heatmap")]
        [AllowAnonymous] // Zodat de img tag de afbeelding altijd kan renderen
        public IActionResult GetHeatmap()
        {
            var bytes = _aiService.GetCorrelationHeatmapBytes();
            if (bytes == null || bytes.Length == 0)
            {
                return NotFound(new { message = "Correlatiematrix heatmap niet gevonden. Zorg dat 'generate_synthetic_data.py' is uitgevoerd." });
            }
            return File(bytes, "image/png");
        }

        [HttpPost("simulator/step")]
        [Authorize(Policy = "PsycholoogOnly")]
        public IActionResult SimulateStep([FromBody] SimulatorStepRequest request)
        {
            if (request == null) return BadRequest(new { message = "Ongeldig simulatorverzoek." });

            var result = _aiService.SimulateRLStep(
                request.SessionsCompleted,
                request.Stability,
                request.Gap,
                request.Sentiment,
                request.SentimentEma,
                request.Action
            );

            return Ok(result);
        }
    }

    public class SimulatorStepRequest
    {
        public int SessionsCompleted { get; set; }
        public double Stability { get; set; }
        public int Gap { get; set; }
        public double Sentiment { get; set; }
        public double SentimentEma { get; set; }
        public int Action { get; set; }
    }
}
