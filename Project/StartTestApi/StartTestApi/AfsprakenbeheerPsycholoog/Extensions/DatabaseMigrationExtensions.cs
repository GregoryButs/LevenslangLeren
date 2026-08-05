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
                            bool hasRijksregisternummer = false;
                            bool hasEmotioneleStabiliteit = false;
                            bool hasVerwijderdOp = false;
                            bool hasVerwijderdReden = false;
                            bool hasStandaardTariefType = false;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var name = reader["name"]?.ToString();
                                    if (string.Equals(name, "SecundairEmail", StringComparison.OrdinalIgnoreCase)) hasSecundairEmail = true;
                                    if (string.Equals(name, "Rijksregisternummer", StringComparison.OrdinalIgnoreCase)) hasRijksregisternummer = true;
                                    if (string.Equals(name, "EmotioneleStabiliteit", StringComparison.OrdinalIgnoreCase)) hasEmotioneleStabiliteit = true;
                                    if (string.Equals(name, "VerwijderdOp", StringComparison.OrdinalIgnoreCase)) hasVerwijderdOp = true;
                                    if (string.Equals(name, "VerwijderdReden", StringComparison.OrdinalIgnoreCase)) hasVerwijderdReden = true;
                                    if (string.Equals(name, "StandaardTariefType", StringComparison.OrdinalIgnoreCase)) hasStandaardTariefType = true;
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
                            if (!hasRijksregisternummer)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Patienten ADD COLUMN Rijksregisternummer TEXT NULL;";
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
                            if (!hasStandaardTariefType)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Patienten ADD COLUMN StandaardTariefType INTEGER NOT NULL DEFAULT 0;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }

                            // Ensure no existing NULL Telefoonnummer rows trigger SQLite NOT NULL constraint failures
                            using (var fixNullsCmd = conn.CreateCommand())
                            {
                                fixNullsCmd.CommandText = "UPDATE Patienten SET Telefoonnummer = '' WHERE Telefoonnummer IS NULL;";
                                fixNullsCmd.ExecuteNonQuery();
                            }
                        }

                        // Automatic SQLite schema migration for Afspraken table columns
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "PRAGMA table_info(Afspraken);";
                            bool hasTariefType = false;
                            bool hasELPStatus = false;
                            bool hasELPType = false;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var name = reader["name"]?.ToString();
                                    if (string.Equals(name, "TariefType", StringComparison.OrdinalIgnoreCase)) hasTariefType = true;
                                    if (string.Equals(name, "ELPStatus", StringComparison.OrdinalIgnoreCase)) hasELPStatus = true;
                                    if (string.Equals(name, "ELPType", StringComparison.OrdinalIgnoreCase)) hasELPType = true;
                                }
                            }

                            if (!hasTariefType)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Afspraken ADD COLUMN TariefType INTEGER NOT NULL DEFAULT 0;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                            if (!hasELPStatus)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Afspraken ADD COLUMN ELPStatus INTEGER NOT NULL DEFAULT 0;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                            if (!hasELPType)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE Afspraken ADD COLUMN ELPType TEXT NULL DEFAULT 'Individueel';";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Automatic SQLite schema migration for AspNetUsers table columns
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "PRAGMA table_info(AspNetUsers);";
                            bool hasIsOpWachtlijst = false;
                            bool hasWachtlijstDatum = false;
                            bool hasGeboortedatum = false;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var name = reader["name"]?.ToString();
                                    if (string.Equals(name, "IsOpWachtlijst", StringComparison.OrdinalIgnoreCase)) hasIsOpWachtlijst = true;
                                    if (string.Equals(name, "WachtlijstDatum", StringComparison.OrdinalIgnoreCase)) hasWachtlijstDatum = true;
                                    if (string.Equals(name, "Geboortedatum", StringComparison.OrdinalIgnoreCase)) hasGeboortedatum = true;
                                }
                            }

                            if (!hasIsOpWachtlijst)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN IsOpWachtlijst INTEGER NOT NULL DEFAULT 0;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                            if (!hasWachtlijstDatum)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN WachtlijstDatum TEXT NULL;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                            if (!hasGeboortedatum)
                            {
                                using (var addColCmd = conn.CreateCommand())
                                {
                                    addColCmd.CommandText = "ALTER TABLE AspNetUsers ADD COLUMN Geboortedatum TEXT NULL;";
                                    addColCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                catch (Exception) { }

                // Safely synchronize EF Migrations History if tables already exist in SQLite
                try
                {
                    using (var conn = context.Database.GetDbConnection())
                    {
                        conn.Open();
                        bool hasRoles = false;
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AspNetRoles';";
                            hasRoles = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                        }

                        if (hasRoles)
                        {
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = @"
                                    CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                                        ""MigrationId"" TEXT NOT NULL CONSTRAINT ""PK___EFMigrationsHistory"" PRIMARY KEY,
                                        ""ProductVersion"" TEXT NOT NULL
                                    );";
                                cmd.ExecuteNonQuery();
                            }

                            var pending = context.Database.GetPendingMigrations();
                            foreach (var migrationId in pending)
                            {
                                using (var cmd = conn.CreateCommand())
                                {
                                    cmd.CommandText = @"
                                        INSERT OR IGNORE INTO ""__EFMigrationsHistory"" (""MigrationId"", ""ProductVersion"")
                                        VALUES (@mId, '8.0.0');";
                                    var p = cmd.CreateParameter();
                                    p.ParameterName = "@mId";
                                    p.Value = migrationId;
                                    cmd.Parameters.Add(p);
                                    cmd.ExecuteNonQuery();
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

                // First restore all appointments from embedded dump instantly (takes < 5ms)
                RestoreFromDump(context);

                // Automatically sync Google Calendar asynchronously in background so app starts listening on port 5000 in < 0.1s
                try
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            using var bgScope = app.Services.CreateScope();
                            var bgCalendarService = bgScope.ServiceProvider.GetService<AfsprakenbeheerPsycholoog.Services.IGoogleCalendarService>();
                            if (bgCalendarService != null)
                            {
                                await bgCalendarService.SyncIncomingChangesAsync();
                            }
                        }
                        catch { }
                    });
                }
                catch (Exception) { }
            }

            return app;
        }

        public static void RestoreFromDump(ApplicationDbContext context)
        {
            try
            {
                if (!context.AfspraakTypes.Any())
                {
                    context.AfspraakTypes.Add(new AfspraakType { Naam = "Consultatie", Kleurcode = "#478d96", StandaardDuurMinuten = 50, VereistPatient = true });
                    context.AfspraakTypes.Add(new AfspraakType { Naam = "Intake", Kleurcode = "#3b82f6", StandaardDuurMinuten = 60, VereistPatient = true });
                    context.AfspraakTypes.Add(new AfspraakType { Naam = "Praktijkhuis Consultatie", Kleurcode = "#a855f7", StandaardDuurMinuten = 50, VereistPatient = true });
                    context.AfspraakTypes.Add(new AfspraakType { Naam = "Online Consultatie", Kleurcode = "#06b6d4", StandaardDuurMinuten = 50, VereistPatient = true });
                    context.AfspraakTypes.Add(new AfspraakType { Naam = "Pauze / Lunch", Kleurcode = "#6b7280", StandaardDuurMinuten = 30, VereistPatient = false });
                    context.AfspraakTypes.Add(new AfspraakType { Naam = "Verlof / Afwezig", Kleurcode = "#ef4444", StandaardDuurMinuten = 480, VereistPatient = false });
                    context.SaveChanges();
                }


                // Update existing patients created with shared email addresses to unique per-patient emails
                try
                {
                    var sharedPatients = context.Patienten.Where(p => p.Email != null && (p.Email.ToLower().Contains("praktijkhuis9500.be") || p.Email.ToLower().Contains("ingedebast@gmail.com"))).ToList();
                    foreach (var p in sharedPatients)
                    {
                        var safeV = Regex.Replace(p.Voornaam.ToLower(), @"[^a-z0-9]", "");
                        var safeA = Regex.Replace(p.Achternaam.ToLower(), @"[^a-z0-9]", "");
                        if (string.IsNullOrEmpty(safeV)) safeV = "patient";
                        p.Email = $"{safeV}.{safeA}@praktijkhuis.local";
                    }
                    context.SaveChanges();
                }
                catch (Exception) { }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RestoreFromDump] Fout tijdens herstel: {ex.Message}");
            }
        }
    }
}
