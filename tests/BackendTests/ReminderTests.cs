using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Services;

namespace BackendTests
{
    public class ReminderTests
    {
        [Fact]
        public async Task SendScheduledRemindersAsync_ShouldOnlySendForUpcomingUnsentAppointments()
        {
            // 1. Setup in-memory SQLite database
            var dbName = $"ReminderTestDb_{Guid.NewGuid()}";
            var connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";

            using var masterConnection = new SqliteConnection(connectionString);
            await masterConnection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(masterConnection)
                .Options;

            // Initialize database schema and auto-seeded model data
            using (var context = new ApplicationDbContext(options))
            {
                await context.Database.EnsureCreatedAsync();

                // Clear any existing seeded appointments to have a clean slate
                context.Afspraken.RemoveRange(context.Afspraken);
                await context.SaveChangesAsync();

                // Seed specific test appointments
                var now = DateTime.UtcNow;

                // A: Starts in 10 hours, status Gepland, HerinneringVerzonden = false -> Should send
                context.Afspraken.Add(new Afspraak
                {
                    Id = 10,
                    PatientId = 1,
                    TypeId = 2,
                    Starttijd = now.AddHours(10),
                    Eindtijd = now.AddHours(11),
                    Status = AfspraakStatus.Gepland,
                    HerinneringVerzonden = false
                });

                // B: Starts in 10 hours, status Gepland, HerinneringVerzonden = true -> Should NOT send
                context.Afspraken.Add(new Afspraak
                {
                    Id = 11,
                    PatientId = 1,
                    TypeId = 2,
                    Starttijd = now.AddHours(10),
                    Eindtijd = now.AddHours(11),
                    Status = AfspraakStatus.Gepland,
                    HerinneringVerzonden = true
                });

                // C: Starts in 30 hours (outside 24h), status Gepland, HerinneringVerzonden = false -> Should NOT send
                context.Afspraken.Add(new Afspraak
                {
                    Id = 12,
                    PatientId = 1,
                    TypeId = 2,
                    Starttijd = now.AddHours(30),
                    Eindtijd = now.AddHours(31),
                    Status = AfspraakStatus.Gepland,
                    HerinneringVerzonden = false
                });

                // D: Starts in 10 hours, status Geannuleerd, HerinneringVerzonden = false -> Should NOT send
                context.Afspraken.Add(new Afspraak
                {
                    Id = 13,
                    PatientId = 1,
                    TypeId = 2,
                    Starttijd = now.AddHours(10),
                    Eindtijd = now.AddHours(11),
                    Status = AfspraakStatus.Geannuleerd,
                    HerinneringVerzonden = false
                });

                await context.SaveChangesAsync();
            }

            // 2. Setup Dependency Injection container for the BackgroundService scope
            var mockEmail = new Mock<IEmailService>();
            mockEmail.Setup(e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.IsAny<int>()))
                     .Returns(Task.CompletedTask);

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlite(connectionString));
            services.AddSingleton(mockEmail.Object);

            var serviceProvider = services.BuildServiceProvider();
            var mockLogger = new Mock<ILogger<ReminderBackgroundService>>();

            // 3. Instantiate and run the worker method
            var backgroundService = new ReminderBackgroundService(serviceProvider, mockLogger.Object);
            await backgroundService.SendScheduledRemindersAsync(CancellationToken.None);

            // 4. Verify assertions
            // IEmailService should only be called once for Appointment ID 10 (A)
            mockEmail.Verify(
                e => e.SendReminderEmailAsync("jan@test.be", "Jan Janssens", It.IsAny<DateTime>(), It.IsAny<string>(), 10),
                Times.Once
            );

            // Check other appointments were not sent reminders
            mockEmail.Verify(
                e => e.SendReminderEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<string>(), It.Is<int>(id => id != 10)),
                Times.Never
            );

            // Verify database states
            using (var verifyContext = new ApplicationDbContext(options))
            {
                var appA = await verifyContext.Afspraken.FindAsync(10);
                var appB = await verifyContext.Afspraken.FindAsync(11);
                var appC = await verifyContext.Afspraken.FindAsync(12);
                var appD = await verifyContext.Afspraken.FindAsync(13);

                Assert.NotNull(appA);
                Assert.NotNull(appB);
                Assert.NotNull(appC);
                Assert.NotNull(appD);
                
                Assert.True(appA.HerinneringVerzonden);
                Assert.True(appB.HerinneringVerzonden);
                Assert.False(appC.HerinneringVerzonden);
                Assert.False(appD.HerinneringVerzonden);
            }
        }
    }
}
