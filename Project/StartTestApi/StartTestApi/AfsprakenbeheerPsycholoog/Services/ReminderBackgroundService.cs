using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Background service die periodiek controleert op geplande afspraken die binnen de komende 24 uur plaatsvinden
    /// en stuurt automatisch een herinneringsmail naar de patiënt.
    /// </summary>
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;

        public ReminderBackgroundService(IServiceProvider serviceProvider, ILogger<ReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Background Service is gestart.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendScheduledRemindersAsync(stoppingToken);

                    // Controleer elke 30 minuten
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Service is gestopt
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout opgetreden in de Reminder Background Service.");
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        public async Task SendScheduledRemindersAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var nuUtc = DateTime.UtcNow;

            // 1. HERINNERINGEN VOOR OVER 1 WEEK (tussen 6 en 7 dagen vooraf)
            var weekGrensMax = nuUtc.AddDays(7);
            var weekGrensMin = nuUtc.AddDays(6);

            var weeklyReminders = await context.Afspraken
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Status == AfspraakStatus.Gepland
                         && a.Starttijd <= weekGrensMax
                         && a.Starttijd > weekGrensMin
                         && !a.HerinneringWeekVerzonden)
                .ToListAsync(stoppingToken);

            foreach (var afspraak in weeklyReminders)
            {
                if (afspraak.Patient == null || string.IsNullOrEmpty(afspraak.Patient.Email))
                {
                    afspraak.HerinneringWeekVerzonden = true;
                    continue;
                }

                try
                {
                    _logger.LogInformation("Versturen van 1-week herinnering voor afspraak {AfspraakId} naar {Email}...", afspraak.Id, afspraak.Patient.Email);
                    await emailService.SendWeeklyReminderEmailAsync(
                        afspraak.Patient.Email,
                        afspraak.Patient.VolledigeNaam,
                        afspraak.Starttijd,
                        afspraak.Type?.Naam ?? "Consult",
                        afspraak.Id
                    );

                    afspraak.HerinneringWeekVerzonden = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij verzenden van 1-week herinneringsmail voor afspraak {AfspraakId}.", afspraak.Id);
                }
            }

            // 2. HERINNERINGEN VOOR MORGEN / BINNEN 24 UUR
            var grens24u = nuUtc.AddHours(24);

            var reminders24h = await context.Afspraken
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Status == AfspraakStatus.Gepland
                         && a.Starttijd <= grens24u
                         && a.Starttijd > nuUtc
                         && !a.HerinneringVerzonden)
                .ToListAsync(stoppingToken);

            foreach (var afspraak in reminders24h)
            {
                if (afspraak.Patient == null || string.IsNullOrEmpty(afspraak.Patient.Email))
                {
                    afspraak.HerinneringVerzonden = true;
                    continue;
                }

                try
                {
                    _logger.LogInformation("Versturen van 24u-herinnering voor afspraak {AfspraakId} naar {Email}...", afspraak.Id, afspraak.Patient.Email);
                    await emailService.SendReminderEmailAsync(
                        afspraak.Patient.Email,
                        afspraak.Patient.VolledigeNaam,
                        afspraak.Starttijd,
                        afspraak.Type?.Naam ?? "Consult",
                        afspraak.Id
                    );

                    afspraak.HerinneringVerzonden = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij verzenden van 24u-herinneringsmail voor afspraak {AfspraakId}.", afspraak.Id);
                }
            }

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Herinneringen-verzendcyclus (1 week & 24u) succesvol voltooid.");
        }
    }
}
