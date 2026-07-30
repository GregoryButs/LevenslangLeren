using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using AutoMapper;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using AfsprakenbeheerPsycholoog.Data.Repositories;
using AfsprakenbeheerPsycholoog.Services;
using AfsprakenbeheerPsycholoog.Models.ViewModels.PatientPortaal;

namespace BackendTests
{
    public class BookingConcurrencyTests
    {
        [Fact]
        public async Task CreatePatientAfspraakAsync_ShouldPreventDoubleBookings_UnderHighConcurrency()
        {
            // 1. Setup in-memory SQLite connection with shared cache to support concurrent connections
            var dbName = $"BookingConcurrencyTestDb_{Guid.NewGuid()}";
            var connectionString = $"Data Source={dbName};Mode=Memory;Cache=Shared";

            using var masterConnection = new SqliteConnection(connectionString);
            await masterConnection.OpenAsync();

            var masterOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(masterConnection)
                .Options;

            // 2. Initialize database schema and auto-seeded model data (via OnModelCreating HasData)
            using (var context = new ApplicationDbContext(masterOptions))
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 3. Setup Mocks for external dependencies
            var mockMapper = new Mock<IMapper>();
            var mockEmail = new Mock<IEmailService>();
            
            // Mock Calendar to return empty busy slots (slot is free)
            var mockCalendar = new Mock<IGoogleCalendarService>();
            mockCalendar.Setup(c => c.GetBusySlotsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                        .ReturnsAsync(new List<(DateTime Start, DateTime End)>());

            var syncQueue = new CalendarSyncQueue();

            // We will test booking for next Wednesday at 10:00 AM (local time)
            // Wednesday is active in seeded PraktijkInstellingen interval 1 (09:00 - 12:00)
            var nextWednesday = DateTime.Today.AddDays(1);
            while (nextWednesday.DayOfWeek != DayOfWeek.Wednesday)
            {
                nextWednesday = nextWednesday.AddDays(1);
            }
            var targetTime = nextWednesday.Date.AddHours(10); // 10:00:00 AM

            // 4. Start 10 parallel threads attempting to book the exact same slot concurrently
            int concurrencyFactor = 10;
            var tasks = new List<Task<bool>>();

            for (int i = 0; i < concurrencyFactor; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    // Create a separate SQLite connection per thread to simulate scoped connection behavior in shared cache
                    using var threadConnection = new SqliteConnection(connectionString);
                    await threadConnection.OpenAsync();

                    var threadOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseSqlite(threadConnection)
                        .Options;

                    using var threadContext = new ApplicationDbContext(threadOptions);
                    var afspraakRepo = new AfspraakRepository(threadContext);
                    var typeRepo = new AfspraakTypeRepository(threadContext);
                    var patientRepo = new PatientRepository(threadContext);

                    var bookingService = new PatientBookingService(
                        afspraakRepo,
                        typeRepo,
                        patientRepo,
                        mockMapper.Object,
                        mockCalendar.Object,
                        mockEmail.Object,
                        threadContext,
                        syncQueue
                    );

                    var vm = new PatientBoekAfspraakViewModel
                    {
                        GekozeTijdslot = targetTime,
                        LocatieType = "Praktijk",
                        Opmerkingen = "Concurrency E2E Test"
                    };

                    try
                    {
                        // Patient ID = 1 (seeded Jan Janssens)
                        return await bookingService.CreatePatientAfspraakAsync(vm, 1);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[THREAD EXCEPTION]: {ex.Message} {ex.StackTrace}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"[INNER EXCEPTION]: {ex.InnerException.Message} {ex.InnerException.StackTrace}");
                        }
                        // Catch concurrency or lock exceptions
                        return false;
                    }
                }));
            }

            // 5. Gather all booking request outcomes
            var results = await Task.WhenAll(tasks);

            // 6. Verify assertions
            int successfulBookingsCount = results.Count(r => r);
            int failedBookingsCount = results.Count(r => !r);

            using (var verifyContext = new ApplicationDbContext(masterOptions))
            {
                var appointmentsInDb = await verifyContext.Afspraken
                    .Where(a => a.Status != AfspraakStatus.Geannuleerd)
                    .ToListAsync();

                // Assert that only exactly ONE booking is registered in the database
                Assert.Equal(1, successfulBookingsCount);
                Assert.Equal(9, failedBookingsCount);
                
                // 3 seeded active appointments + 1 new concurrent booking = 4 total active appointments
                Assert.Equal(4, appointmentsInDb.Count);
                
                // Assert that the newly created booking at targetTime exists in the database
                TimeZoneInfo tz;
                try
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Amsterdam");
                }
                catch
                {
                    tz = TimeZoneInfo.Local;
                }
                var targetUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(targetTime, DateTimeKind.Unspecified), tz);
                var newBooking = appointmentsInDb.SingleOrDefault(a => a.Starttijd == targetUtc);
                Assert.NotNull(newBooking);
            }
        }
    }
}
