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
                var defaultType = context.AfspraakTypes.FirstOrDefault();
                int defaultTypeId = defaultType?.Id ?? 1;
                int restoredCount = 0;

                // Embedded hardcoded backup snapshot from db_dump.txt
                var embeddedDump = new (int PatientId, string StartIso, string GoogleEv, string Opm)[]
                {
                    (39, "2026-08-13T08:00:00Z", "4d9f0oukpcjrr0118fnb61guvj", ""),
                    (44, "2026-08-13T11:00:00Z", "67hgs8ebnqrr4nq7f7s9pfs59t", ""),
                    (53, "2026-08-14T12:00:00Z", "447gbjgpnt5g5srh12esfug9e3", ""),
                    (63, "2026-08-04T09:00:00Z", "2rsabitt18cl4pu62qmo6238vq", ""),
                    (64, "2026-08-14T08:00:00Z", "3lka6pcknjit3gbr38u3lh8s7c", ""),
                    (68, "2026-08-14T09:00:00Z", "7rpt0d9r4a2i89pbuemu1q0665", ""),
                    (69, "2026-08-07T08:30:00Z", "6l3t88gs1h3vcqbo0f0qsjamq8", ""),
                    (72, "2026-08-07T12:00:00Z", "3nmg101ggk1e56294moii6iphn", ""),
                    (74, "2026-08-03T13:15:00Z", "70gqapug8alihad334aeoff7vo", ""),
                    (75, "2026-08-10T13:00:00Z", "68qj0d1occs68bb2c8q6ab9k64rjab9o71i6ab9j75j3gc1g6pj68ohgcc", ""),
                    (78, "2026-08-04T08:00:00Z", "48m7fpmsh4e2qamgff78vjj139", ""),
                    (80, "2026-08-12T08:00:00Z", "c4p62opncgs62bb5clhj6b9kckp66b9p60o3gb9gc4qm2p9o70pj8cr1co", ""),
                    (81, "2026-08-03T12:00:00Z", "c5j34opg6sq38bb66cq32b9k6dgm4b9p74om8bb2clh30d9p6hi62oph64", ""),
                    (85, "2026-08-03T10:00:00Z", "2c3lj5i6trm4s6v40dqno0nh22", ""),
                    (91, "2026-08-07T09:30:00Z", "6aa4rsthalv60i98qroh77q05l", ""),
                    (244, "2026-08-05T08:00:00Z", "5jgbqj9dno7ghn8okmjie90olg", ""),
                    (94, "2026-08-10T09:30:00Z", "3ujanih7g1o2bpj3355dkslm6l", ""),
                    (95, "2026-08-07T15:30:00Z", "3fbuoinv1goeauogduff9bgg2d", ""),
                    (204, "2026-08-06T09:00:00Z", "6tj6aor4c4rj0b9ic9i66b9k6gojibb16phj0b9kcdgj0ohoc4s68pb56k", "[PH9500]"),
                    (206, "2026-08-06T07:00:00Z", "5lgeu2tvsv2sg3kr0v6h3st2kh", "[PH9500]"),
                    (213, "2026-08-11T16:00:00Z", "4rcq0k18n7rinn4gjvj7lnta8c", "[PH9500]"),
                    (214, "2026-08-11T12:00:00Z", "5rhlc5kn65cd6hcdjeejq4hvt6", "[PH9500]"),
                    (215, "2026-08-11T11:00:00Z", "53ecqiuq9osof606dntm66nnhp", "[PH9500]"),
                    (229, "2026-08-11T17:00:00Z", "coqjgoj665h68b9occrj0b9k6dimabb1c8rm8b9l6opjep1j6orm6db3c4", "[PH9500]"),
                    (232, "2026-08-11T15:00:00Z", "39k6cqfoqukkmg81mbt2fd2n8n", "[PH9500]\nconsult\nsluit zich sociaal meer en meer af\ngeen depr of suic gedachten\nelke ochtend spijt van wakker te worden\nsinds overlijden vrouw tien jaar terug\nwens nog geen medic\nooit al naar psy geweest maar werd er niet gelukkig van\nKathleen"),
                    (231, "2026-08-06T08:00:00Z", "19cgvguqqv9tnu5q7sl63f9b1q_20260806T080000Z", "[PH9500]\nstemmings- en concentratieproblemen - relationele problematiek is etiologie")
                };

                foreach (var item in embeddedDump)
                {
                    int patientId = item.PatientId;
                    string googleEv = item.GoogleEv;
                    string opm = item.Opm;
                    DateTime utcStart = DateTime.Parse(item.StartIso).ToUniversalTime();
                    DateTime utcEnd = utcStart.AddMinutes(50);

                    var patient = context.Patienten.FirstOrDefault(p => p.Id == patientId) ?? context.Patienten.FirstOrDefault();
                    int? resolvedPatientId = patient?.Id;

                    Afspraak? appt = context.Afspraken.FirstOrDefault(a => a.GoogleEventId == googleEv);

                    if (appt == null)
                    {
                        appt = context.Afspraken.FirstOrDefault(a => Math.Abs((a.Starttijd - utcStart).TotalMinutes) < 5);
                    }

                    if (appt == null)
                    {
                        var newAppt = new Afspraak
                        {
                            PatientId = resolvedPatientId,
                            TypeId = defaultTypeId,
                            Starttijd = utcStart,
                            Eindtijd = utcEnd,
                            Status = AfspraakStatus.Gepland,
                            GoogleEventId = googleEv,
                            Opmerkingen = opm
                        };
                        context.Afspraken.Add(newAppt);
                        restoredCount++;
                    }
                    else if (appt.PatientId == null && resolvedPatientId.HasValue)
                    {
                        appt.PatientId = resolvedPatientId.Value;
                        if (!string.IsNullOrWhiteSpace(opm) && string.IsNullOrWhiteSpace(appt.Opmerkingen))
                        {
                            appt.Opmerkingen = opm;
                        }
                        restoredCount++;
                    }
                }

                // Also attempt file parse if db_dump.txt happens to exist
                if (File.Exists("db_dump.txt"))
                {
                    var lines = File.ReadAllLines("db_dump.txt");
                    bool inAppts = false;

                    foreach (var line in lines)
                    {
                        if (line.Contains("--- AFSPRAKEN")) { inAppts = true; continue; }
                        if (!inAppts || !line.StartsWith("Appt ID ")) continue;

                        var pIdMatch = Regex.Match(line, @"PatientId=(\d+)");
                        var gEvMatch = Regex.Match(line, @"GoogleEv=([^\s,\r\n]+)");
                        var startMatch = Regex.Match(line, @"Start=([^\s,]+ [^\s,]+)");

                        if (!pIdMatch.Success || !startMatch.Success) continue;

                        int patientId = int.Parse(pIdMatch.Groups[1].Value);
                        string? googleEv = gEvMatch.Success ? gEvMatch.Groups[1].Value : null;

                        if (!DateTime.TryParse(startMatch.Groups[1].Value, out var startTime)) continue;
                        var utcStart = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
                        var utcEnd = utcStart.AddMinutes(50);

                        var patient = context.Patienten.FirstOrDefault(p => p.Id == patientId) ?? context.Patienten.FirstOrDefault();
                        int? resolvedPatientId = patient?.Id;

                        Afspraak? appt = !string.IsNullOrEmpty(googleEv) ? context.Afspraken.FirstOrDefault(a => a.GoogleEventId == googleEv) : null;
                        if (appt == null) appt = context.Afspraken.FirstOrDefault(a => Math.Abs((a.Starttijd - utcStart).TotalMinutes) < 5);

                        if (appt == null)
                        {
                            var newAppt = new Afspraak
                            {
                                PatientId = resolvedPatientId,
                                TypeId = defaultTypeId,
                                Starttijd = utcStart,
                                Eindtijd = utcEnd,
                                Status = AfspraakStatus.Gepland,
                                GoogleEventId = googleEv
                            };
                            context.Afspraken.Add(newAppt);
                            restoredCount++;
                        }
                        else if (appt.PatientId == null && resolvedPatientId.HasValue)
                        {
                            appt.PatientId = resolvedPatientId.Value;
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
