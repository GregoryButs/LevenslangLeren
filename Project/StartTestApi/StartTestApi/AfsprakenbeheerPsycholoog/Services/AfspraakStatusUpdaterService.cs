using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AfsprakenbeheerPsycholoog.Services
{
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
            // Deze loop blijft draaien op de achtergrond zolang applicatie runt
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WerkStatussenBijAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij het automatisch updaten van de afspraakstatussen.");
                }

                // Wacht een bepaalde tijdstip (bijv. elk kwartier) tot de volgende check
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task WerkStatussenBijAsync()
        {
            // Creëer een nieuwe scope, omdat een BackgroundService (Singleton)
            // anders niet kan praten met de ApplicationDbContext (Scoped).
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Stel de grens in op 'exact 1 uur na de Eindtijd'
            var grensTijd = DateTime.Now.AddHours(-1);

            // Haal afspraken op die de status 'Gepland' hebben
            // EN waarvan het moment (Eindtijd) meer dan 1 uur in het verleden ligt.
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
