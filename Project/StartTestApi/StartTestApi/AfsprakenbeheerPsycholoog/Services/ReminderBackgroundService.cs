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
            var grensTijd = nuUtc.AddHours(24);

            // Haal afspraken op die:
            // 1. De status 'Gepland' hebben
            // 2. Starten binnen de komende 24 uur
            // 3. Nog niet gestart zijn
            // 4. Waarvoor nog geen herinnering is verzonden
            var appointmentsToRemind = await context.Afspraken
                .Include(a => a.Patient)
                .Include(a => a.Type)
                .Where(a => a.Status == AfspraakStatus.Gepland
                         && a.Starttijd <= grensTijd
                         && a.Starttijd > nuUtc
                         && !a.HerinneringVerzonden)
                .ToListAsync(stoppingToken);

            if (!appointmentsToRemind.Any())
            {
                return;
            }

            _logger.LogInformation("Reminder Background Service heeft {Count} afspraak(en) gevonden voor herinnering.", appointmentsToRemind.Count);

            foreach (var afspraak in appointmentsToRemind)
            {
                if (afspraak.Patient == null || string.IsNullOrEmpty(afspraak.Patient.Email))
                {
                    _logger.LogWarning("Afspraak {AfspraakId} heeft geen gekoppelde patiënt of e-mailadres. Markeren als verzonden.", afspraak.Id);
                    afspraak.HerinneringVerzonden = true;
                    continue;
                }

                try
                {
                    _logger.LogInformation("Versturen van herinnering voor afspraak {AfspraakId} naar {Email}...", afspraak.Id, afspraak.Patient.Email);
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
                    _logger.LogError(ex, "Fout bij verzenden van herinneringsmail voor afspraak {AfspraakId}.", afspraak.Id);
                }
            }

            await context.SaveChangesAsync(stoppingToken);
            _logger.LogInformation("Herinneringen-verzendcyclus succesvol voltooid.");
        }
    }
}
