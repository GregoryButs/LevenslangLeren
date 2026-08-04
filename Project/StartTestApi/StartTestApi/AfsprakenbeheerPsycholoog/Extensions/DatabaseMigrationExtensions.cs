using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AfsprakenbeheerPsycholoog.Extensions
{
    /// <summary>
    /// Extensies voor automatische databasemigratie en seeding bij het opstarten van de webapplicatie.
    /// </summary>
    public static class DatabaseMigrationExtensions
    {
        public static WebApplication SeedAndMigrateDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Als de database bestaat maar geen migratiegeschiedenis heeft (bijv. gemaakt via EnsureCreated),
                // verwijderen we de database om eventuele schema-incompatibiliteiten op te lossen.
                var dbPath = "Afsprakenbeheer.db";
                if (File.Exists(dbPath))
                {
                    bool hasMigrationTable = false;
                    try
                    {
                        using (var conn = context.Database.GetDbConnection())
                        {
                            conn.Open();
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='__EFMigrationsHistory';";
                                var count = Convert.ToInt32(cmd.ExecuteScalar());
                                hasMigrationTable = count > 0;
                            }
                        }
                    }
                    catch { }

                    if (!hasMigrationTable)
                    {
                        context.Database.EnsureDeleted();
                    }
                }

                // Automatic SQLite schema migration for IsHeleDag column
                try
                {
                    using (var conn = context.Database.GetDbConnection())
                    {
                        conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "PRAGMA table_info(Afspraken);";
                            bool hasIsHeleDag = false;
                            bool hasHerinneringWeekVerzonden = false;
                            bool hasGoogleMeetLink = false;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var name = reader["name"]?.ToString();
                                    if (string.Equals(name, "IsHeleDag", StringComparison.OrdinalIgnoreCase))
                                    {
                                        hasIsHeleDag = true;
                                    }
                                    if (string.Equals(name, "HerinneringWeekVerzonden", StringComparison.OrdinalIgnoreCase))
                                    {
                                        hasHerinneringWeekVerzonden = true;
                                    }
                                    if (string.Equals(name, "GoogleMeetLink", StringComparison.OrdinalIgnoreCase))
                                    {
                                        hasGoogleMeetLink = true;
                                    }
                                }
                            }

                            if (!hasIsHeleDag)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Afspraken ADD COLUMN IsHeleDag INTEGER NOT NULL DEFAULT 0;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }

                            if (!hasHerinneringWeekVerzonden)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Afspraken ADD COLUMN HerinneringWeekVerzonden INTEGER NOT NULL DEFAULT 0;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }

                            if (!hasGoogleMeetLink)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Afspraken ADD COLUMN GoogleMeetLink TEXT NULL;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Automatic SQLite schema migration for Patienten table columns
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "PRAGMA table_info(Patienten);";
                            bool hasSecundairEmail = false;
                            bool hasEmotioneleStabiliteit = false;
                            bool hasVerwijderdOp = false;
                            bool hasVerwijderdReden = false;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var name = reader["name"]?.ToString();
                                    if (string.Equals(name, "SecundairEmail", StringComparison.OrdinalIgnoreCase)) hasSecundairEmail = true;
                                    if (string.Equals(name, "EmotioneleStabiliteit", StringComparison.OrdinalIgnoreCase)) hasEmotioneleStabiliteit = true;
                                    if (string.Equals(name, "VerwijderdOp", StringComparison.OrdinalIgnoreCase)) hasVerwijderdOp = true;
                                    if (string.Equals(name, "VerwijderdReden", StringComparison.OrdinalIgnoreCase)) hasVerwijderdReden = true;
                                }
                            }

                            if (!hasSecundairEmail)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Patienten ADD COLUMN SecundairEmail TEXT NULL;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                            if (!hasEmotioneleStabiliteit)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Patienten ADD COLUMN EmotioneleStabiliteit REAL NULL DEFAULT 5.5;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                            if (!hasVerwijderdOp)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Patienten ADD COLUMN VerwijderdOp TEXT NULL;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                            if (!hasVerwijderdReden)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Patienten ADD COLUMN VerwijderdReden TEXT NULL;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }

                try
                {
                    context.Database.Migrate();
                }
                catch (Exception)
                {
                    try
                    {
                        context.Database.EnsureCreated();
                    }
                    catch (Exception) { }
                }

                SeedData.Initialize(scope.ServiceProvider).GetAwaiter().GetResult();

                // Ensure existing Praktijkhuis appointments use 'Therapie Praktijkhuis' (ID 3, orange)
                try
                {
                    var phType = context.AfspraakTypes.FirstOrDefault(t => t.Naam.Contains("Praktijkhuis")) ?? context.AfspraakTypes.FirstOrDefault(t => t.Id == 3);
                    if (phType != null)
                    {
                        var phAppointments = context.Afspraken.Where(a => a.Opmerkingen != null && (a.Opmerkingen.Contains("PH9500") || a.Opmerkingen.Contains("Praktijkhuis"))).ToList();
                        foreach (var item in phAppointments)
                        {
                            if (item.TypeId != phType.Id)
                            {
                                item.TypeId = phType.Id;
                            }
                        }
                        context.SaveChanges();
                    }
                }
                catch (Exception) { }

                // Cleanup dummy 'Blokkering / Melding' events and mark all-day Google events as IsHeleDag = true
                try
                {
                    var dummyAppts = context.Afspraken.Where(a => a.Opmerkingen != null && a.Opmerkingen.Contains("Blokkering / Melding")).ToList();
                    if (dummyAppts.Any())
                    {
                        context.Afspraken.RemoveRange(dummyAppts);
                        context.SaveChanges();
                    }

                    var allDayAppts = context.Afspraken.Where(a => a.GoogleEventId != null && (a.Starttijd == a.Eindtijd || (a.Starttijd.Hour == 2 && a.Eindtijd.Hour == 2 && a.Starttijd.Minute == 0 && a.Eindtijd.Minute == 0))).ToList();
                    foreach (var appt in allDayAppts)
                    {
                        appt.IsHeleDag = true;
                    }
                    context.SaveChanges();
                }
                catch (Exception) { }

                // Dump patients and appointments info if not exists
                try
                {
                    if (!File.Exists("db_dump.txt"))
                    {
                        var patients = context.Patienten.ToList();
                        var pLines = patients.Select(p => $"Patient ID {p.Id}: {p.Voornaam} {p.Achternaam} ({p.Email}, {p.Telefoonnummer})");
                        
                        var appts = context.Afspraken.Include(a => a.Patient).Where(a => a.Starttijd >= new DateTime(2026, 8, 1) && a.Starttijd <= new DateTime(2026, 8, 15)).ToList();
                        var aLines = appts.Select(a => $"Appt ID {a.Id}: Start={a.Starttijd}, PatientId={a.PatientId}, Patient={a.Patient?.Voornaam} {a.Patient?.Achternaam}, Opm={a.Opmerkingen}, GoogleEv={a.GoogleEventId}");

                        File.WriteAllText("db_dump.txt", "--- PATIENTEN ---\n" + string.Join("\n", pLines) + "\n\n--- AFSPRAKEN (1-15 aug) ---\n" + string.Join("\n", aLines));
                    }
                }
                catch (Exception) { }

                // Automatically sync Google Calendar & Praktijkhuis appointments on startup
                try
                {
                    var googleCalendarService = scope.ServiceProvider.GetService<AfsprakenbeheerPsycholoog.Services.IGoogleCalendarService>();
                    if (googleCalendarService != null)
                    {
                        googleCalendarService.SyncIncomingChangesAsync().GetAwaiter().GetResult();
                    }
                }
                catch (Exception) { }

                // Restore patient links from db_dump.txt if any appointments lost PatientId
                RestoreFromDump(context);
            }

            return app;
        }

        public static void RestoreFromDump(ApplicationDbContext context)
        {
            try
            {
                if (!File.Exists("db_dump.txt")) return;

                var lines = File.ReadAllLines("db_dump.txt");
                bool inAppts = false;
                int restoredCount = 0;

                foreach (var line in lines)
                {
                    if (line.Contains("--- AFSPRAKEN"))
                    {
                        inAppts = true;
                        continue;
                    }
                    if (!inAppts || !line.StartsWith("Appt ID ")) continue;

                    var pIdMatch = Regex.Match(line, @"PatientId=(\d+)");
                    var gEvMatch = Regex.Match(line, @"GoogleEv=([^\s,\r\n]+)");
                    var startMatch = Regex.Match(line, @"Start=([^\s,]+ [^\s,]+)");

                    if (!pIdMatch.Success) continue;
                    int patientId = int.Parse(pIdMatch.Groups[1].Value);

                    string? googleEv = gEvMatch.Success ? gEvMatch.Groups[1].Value : null;

                    Afspraak? appt = null;
                    if (!string.IsNullOrEmpty(googleEv))
                    {
                        appt = context.Afspraken.FirstOrDefault(a => a.GoogleEventId == googleEv);
                    }

                    if (appt == null && startMatch.Success && DateTime.TryParse(startMatch.Groups[1].Value, out var startTime))
                    {
                        var utcStart = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                        appt = context.Afspraken.FirstOrDefault(a => (a.Starttijd == utcStart || Math.Abs((a.Starttijd - utcStart).TotalMinutes) < 5) && a.PatientId == null);
                    }

                    if (appt != null && appt.PatientId == null)
                    {
                        var patientExists = context.Patienten.Any(p => p.Id == patientId);
                        if (patientExists)
                        {
                            appt.PatientId = patientId;
                            restoredCount++;
                        }
                    }
                }

                if (restoredCount > 0)
                {
                    context.SaveChanges();
                }
            }
            catch (Exception) { }
        }
    }
}
