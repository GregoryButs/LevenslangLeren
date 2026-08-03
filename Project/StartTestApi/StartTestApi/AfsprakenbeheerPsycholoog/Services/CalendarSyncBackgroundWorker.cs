using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfsprakenbeheerPsycholoog.Services
{
    public enum SyncAction
    {
        Create,
        Cancel
    }

    public class CalendarSyncTask
    {
        public int AfspraakId { get; set; }
        public int PatientId { get; set; }
        public SyncAction Action { get; set; }
        public string? LocationText { get; set; }
        public bool CreateMeetLink { get; set; }
        public int RetryCount { get; set; } = 0;
    }

    public class CalendarSyncQueue
    {
        private readonly Channel<CalendarSyncTask> _queue;

        public CalendarSyncQueue()
        {
            _queue = Channel.CreateUnbounded<CalendarSyncTask>(new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });
        }

        public ValueTask QueueBackgroundWorkItemAsync(CalendarSyncTask task)
        {
            return _queue.Writer.WriteAsync(task);
        }

        public ValueTask<CalendarSyncTask> DequeueAsync(CancellationToken cancellationToken)
        {
            return _queue.Reader.ReadAsync(cancellationToken);
        }
    }

    public class CalendarSyncBackgroundWorker : BackgroundService
    {
        private readonly CalendarSyncQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CalendarSyncBackgroundWorker> _logger;

        public CalendarSyncBackgroundWorker(
            CalendarSyncQueue queue,
            IServiceProvider serviceProvider,
            ILogger<CalendarSyncBackgroundWorker> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Calendar Sync Background Worker is gestart.");

            while (!stoppingToken.IsCancellationRequested)
            {
                CalendarSyncTask task;
                try
                {
                    task = await _queue.DequeueAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Service is stopping
                }

                try
                {
                    await ProcessSyncTaskAsync(task);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij verwerken van synchronisatie-taak voor afspraak {AfspraakId}", task.AfspraakId);

                    // Retry mechanism with exponential backoff (e.g. 5 retries: 3s, 9s, 27s, 81s, 243s)
                    if (task.RetryCount < 5)
                    {
                        task.RetryCount++;
                        int delaySeconds = (int)Math.Pow(3, task.RetryCount);
                        _logger.LogWarning("Synchronisatie voor afspraak {AfspraakId} mislukt. Retry {Attempt}/5 in {Delay} seconden...", task.AfspraakId, task.RetryCount, delaySeconds);

                        // Run delay and re-queue asynchronously
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), stoppingToken);
                            await _queue.QueueBackgroundWorkItemAsync(task);
                        }, stoppingToken);
                    }
                    else
                    {
                        _logger.LogError("Maximum aantal retries bereikt voor afspraak {AfspraakId}. Synchronisatie is definitief mislukt.", task.AfspraakId);
                    }
                }
            }
        }

        private async Task ProcessSyncTaskAsync(CalendarSyncTask task)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var calendarService = scope.ServiceProvider.GetRequiredService<IGoogleCalendarService>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Fetch appointment and patient from database
            var afspraak = dbContext.Afspraken
                .Include(a => a.Type)
                .FirstOrDefault(a => a.Id == task.AfspraakId);

            var patient = dbContext.Patienten.FirstOrDefault(p => p.Id == task.PatientId);

            if (afspraak == null || patient == null)
            {
                _logger.LogWarning("Afspraak {AfspraakId} of patiënt {PatientId} niet gevonden in database. Synchronisatietaak genegeerd.", task.AfspraakId, task.PatientId);
                return;
            }

            if (task.Action == SyncAction.Create)
            {
                _logger.LogInformation("Asynchroon synchroniseren en bevestiging verzenden voor nieuwe afspraak {AfspraakId}...", afspraak.Id);

                // 1. Google Calendar Sync
                string? googleEventId = null;
                string? meetLink = null;
                try
                {
                    var (eventId, googleMeetLink) = await calendarService.CreateEventAsync(
                        afspraak.Starttijd,
                        afspraak.Eindtijd,
                        afspraak.Id,
                        task.CreateMeetLink,
                        task.LocationText ?? "",
                        patient.VolledigeNaam
                    );
                    googleEventId = eventId;
                    meetLink = googleMeetLink;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Google Calendar sync mislukt voor afspraak {AfspraakId}", afspraak.Id);
                    throw; // Re-throw to trigger retry loop
                }

                // Update Google Event ID and Meet Link in database
                if (!string.IsNullOrEmpty(googleEventId) || !string.IsNullOrEmpty(meetLink))
                {
                    afspraak.GoogleEventId = googleEventId;
                    afspraak.GoogleMeetLink = meetLink;
                    dbContext.Update(afspraak);
                    await dbContext.SaveChangesAsync();
                }

                // 2. Send confirmation email
                if (!string.IsNullOrEmpty(patient.Email))
                {
                    try
                    {
                        await emailService.SendConfirmationEmailAsync(
                            patient.Email,
                            patient.VolledigeNaam,
                            afspraak.Starttijd,
                            afspraak.Eindtijd,
                            afspraak.Type?.Naam ?? "Consult",
                            afspraak.Id,
                            afspraak.Opmerkingen,
                            meetLink
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Bevestigingsmail verzenden mislukt voor afspraak {AfspraakId} naar {Email}", afspraak.Id, patient.Email);
                        throw; // Re-throw to trigger retry loop
                    }
                }
            }
            else if (task.Action == SyncAction.Cancel)
            {
                _logger.LogInformation("Asynchroon verwijderen en annulering verzenden voor afspraak {AfspraakId}...", afspraak.Id);

                // 1. Google Calendar Delete
                if (!string.IsNullOrEmpty(afspraak.GoogleEventId))
                {
                    try
                    {
                        await calendarService.DeleteEventAsync(afspraak.GoogleEventId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Google Calendar delete mislukt voor event {EventId}", afspraak.GoogleEventId);
                        throw; // Re-throw to trigger retry loop
                    }
                }

                // 2. Send cancellation email
                if (!string.IsNullOrEmpty(patient.Email))
                {
                    try
                    {
                        await emailService.SendCancellationEmailAsync(
                            patient.Email,
                            patient.VolledigeNaam,
                            afspraak.Starttijd,
                            afspraak.Type?.Naam ?? "Consult"
                        );
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Annuleringsmail verzenden mislukt voor afspraak {AfspraakId} naar {Email}", afspraak.Id, patient.Email);
                        throw; // Re-throw to trigger retry loop
                    }
                }
            }
        }
    }
}
