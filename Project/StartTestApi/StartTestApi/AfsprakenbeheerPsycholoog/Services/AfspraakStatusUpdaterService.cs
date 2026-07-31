using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Service voor het automatisch bijwerken van de status van afspraken op basis van hun eindtijd.
    /// Draait op de achtergrond en controleert periodiek of afspraken moeten worden bijgewerkt naar 'Voltooid'.
    /// </summary>
    public class AfspraakStatusUpdaterService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AfspraakStatusUpdaterService> _logger;

        public AfspraakStatusUpdaterService(IServiceProvider serviceProvider, ILogger<AfspraakStatusUpdaterService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WerkStatussenBijAsync();
                    await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij het automatisch updaten van de afspraakstatussen.");
                    try
                    {
                        await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private async Task WerkStatussenBijAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var grensTijd = DateTime.UtcNow.AddHours(-1);

            var teUpdatenAfspraken = await context.Afspraken
                .Where(a => a.Status == AfspraakStatus.Gepland && a.Eindtijd <= grensTijd)
                .ToListAsync();

            if (teUpdatenAfspraken.Any())
            {
                foreach (var afspraak in teUpdatenAfspraken)
                {
                    afspraak.Status = AfspraakStatus.Voltooid;
                }

                await context.SaveChangesAsync();
                _logger.LogInformation($"{teUpdatenAfspraken.Count} afspraak(en) automatisch op 'Voltooid' gezet nadat 1 uur verstreken was na de eindtijd.");
            }
        }
    }
}
