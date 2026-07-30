using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly ILogger<GoogleCalendarService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly bool _useMock;
        private readonly CalendarService? _calendarService;
        private readonly string _calendarId;

        public GoogleCalendarService(
            IConfiguration configuration,
            ILogger<GoogleCalendarService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _calendarId = configuration["GoogleCalendar:CalendarId"] ?? "primary";

            // Bepaal of we in lokale testmodus (mock) draaien
            _useMock = string.Equals(configuration["GoogleCalendar:UseMock"], "true", StringComparison.OrdinalIgnoreCase);

            if (_useMock)
            {
                _logger.LogInformation("Google Calendar Service is geinitialiseerd in MOCK modus.");
                return;
            }

            try
            {
                string credentialsPath = configuration["GoogleCalendar:CredentialsJsonPath"] ?? "google-credentials.json";

                if (!File.Exists(credentialsPath))
                {
                    _logger.LogWarning($"Google credentials bestand niet gevonden op {credentialsPath}. Service schakelt automatisch over naar MOCK modus.");
                    _useMock = true;
                    return;
                }

                GoogleCredential credential;
                using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(CalendarService.Scope.Calendar);
                }

                _calendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "De Verstandhouding Agendabeheer",
                });

                _logger.LogInformation("Google Calendar Service succesvol geinitialiseerd met Service Account.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij initialiseren Google Calendar Service. Schakelt over naar MOCK modus.");
                _useMock = true;
            }
        }

        public async Task<string> CreateEventAsync(DateTime startUtc, DateTime endUtc, int afspraakId, bool createMeetLink = false, string locationDetails = "", string patientNaam = "")
        {
            if (_useMock)
            {
                var mockEventId = $"mock_event_{Guid.NewGuid().ToString("N")}";
                _logger.LogInformation($"[MOCK GOOGLE CALENDAR] Afspraak #{afspraakId} aangemaakt van {startUtc} tot {endUtc} UTC. Locatie: {locationDetails}. MockEventID: {mockEventId}");
                return mockEventId;
            }

            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geinitialiseerd.");

            var summaryText = string.IsNullOrWhiteSpace(patientNaam)
                ? $"Afspraak #{afspraakId} - patiëntenportaal"
                : $"{patientNaam} - patiëntenportaal";

            var calendarEvent = new Event
            {
                Summary = summaryText,
                Description = "Gereserveerd via online patientenportaal - De Verstandhouding.",
                Start = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(startUtc, TimeSpan.Zero) },
                End = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(endUtc, TimeSpan.Zero) },
                Location = locationDetails
            };

            if (createMeetLink)
            {
                calendarEvent.ConferenceData = new ConferenceData
                {
                    CreateRequest = new CreateConferenceRequest
                    {
                        RequestId = Guid.NewGuid().ToString(),
                        ConferenceSolutionKey = new ConferenceSolutionKey { Type = "hangoutsMeet" }
                    }
                };
            }

            var request = _calendarService.Events.Insert(calendarEvent, _calendarId);
            if (createMeetLink)
            {
                request.ConferenceDataVersion = 1;
            }

            var createdEvent = await request.ExecuteAsync();
            
            _logger.LogInformation($"Google Calendar Event aangemaakt voor afspraak #{afspraakId} ({summaryText}) met ID: {createdEvent.Id}");
            return createdEvent.Id;
        }

        public async Task UpdateEventAsync(string googleEventId, DateTime startUtc, DateTime endUtc, int afspraakId, string patientNaam = "")
        {
            if (_useMock)
            {
                _logger.LogInformation($"[MOCK GOOGLE CALENDAR] Afspraak #{afspraakId} (GoogleEventID: {googleEventId}) bijgewerkt naar {startUtc} tot {endUtc} UTC.");
                return;
            }

            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geinitialiseerd.");

            var summaryText = string.IsNullOrWhiteSpace(patientNaam)
                ? $"Afspraak #{afspraakId} - patiëntenportaal"
                : $"{patientNaam} - patiëntenportaal";

            var calendarEvent = new Event
            {
                Summary = summaryText,
                Start = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(startUtc, TimeSpan.Zero) },
                End = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(endUtc, TimeSpan.Zero) }
            };

            var request = _calendarService.Events.Update(calendarEvent, _calendarId, googleEventId);
            await request.ExecuteAsync();
            
            _logger.LogInformation($"Google Calendar Event {googleEventId} ({summaryText}) bijgewerkt voor afspraak #{afspraakId}");
        }

        public async Task DeleteEventAsync(string googleEventId)
        {
            if (_useMock)
            {
                _logger.LogInformation($"[MOCK GOOGLE CALENDAR] Event {googleEventId} verwijderd.");
                return;
            }

            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geinitialiseerd.");

            try
            {
                var request = _calendarService.Events.Delete(_calendarId, googleEventId);
                await request.ExecuteAsync();
                _logger.LogInformation($"Google Calendar Event {googleEventId} succesvol verwijderd.");
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Google Calendar Event {googleEventId} kon niet worden verwijderd omdat het niet gevonden is (reeds verwijderd).");
            }
        }

        public async Task SyncIncomingChangesAsync()
        {
            if (_useMock)
            {
                _logger.LogInformation("[MOCK GOOGLE CALENDAR] SyncIncomingChangesAsync aangeroepen.");
                return;
            }

            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geinitialiseerd.");

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                
                var request = _calendarService.Events.List(_calendarId);
                // Haal wijzigingen op van de afgelopen 60 dagen tot de komende 90 dagen
                request.TimeMinDateTimeOffset = DateTimeOffset.UtcNow.AddDays(-60);
                request.TimeMaxDateTimeOffset = DateTimeOffset.UtcNow.AddDays(90);
                
                var events = await request.ExecuteAsync();

                if (events.Items == null) return;

                foreach (var ev in events.Items)
                {
                    // Negeer Google Systeem-events, werklocaties of events zonder geldige starttijd/datum
                    if (ev == null || 
                        string.Equals(ev.EventType, "workingLocation", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(ev.EventType, "focusTime", StringComparison.OrdinalIgnoreCase) ||
                        (ev.Start?.DateTimeDateTimeOffset == null && string.IsNullOrEmpty(ev.Start?.Date)))
                    {
                        continue;
                    }

                    // Als het event geen titel én geen deelnemers/attendees heeft, is het een achtergrond-event -> negeer
                    if (string.IsNullOrWhiteSpace(ev.Summary) && (ev.Attendees == null || !ev.Attendees.Any()))
                    {
                        continue;
                    }

                    // Zoek lokale afspraak gekoppeld aan dit Google Event
                    var localAppointment = dbContext.Afspraken.FirstOrDefault(a => a.GoogleEventId == ev.Id);
                    if (localAppointment != null)
                    {
                        bool isDeclined = ev.Status == "cancelled" || 
                            (ev.Attendees != null && ev.Attendees.Any(a => 
                                (a.Self == true || (a.Email != null && (a.Email.Contains("ingedebast", StringComparison.OrdinalIgnoreCase) || a.Email.Contains("deverstandhouding", StringComparison.OrdinalIgnoreCase)))) && 
                                string.Equals(a.ResponseStatus, "declined", StringComparison.OrdinalIgnoreCase)));

                        if (isDeclined)
                        {
                            if (localAppointment.Status != Data.Entities.AfspraakStatus.Geannuleerd)
                            {
                                localAppointment.Status = Data.Entities.AfspraakStatus.Geannuleerd;
                                _logger.LogInformation($"Lokale afspraak #{localAppointment.Id} geannuleerd na 'Nee' / annulering in Google Calendar.");
                            }
                        }
                        else
                        {
                            var (newStart, newEnd) = GetEventStartAndEnd(ev);

                            if (localAppointment.Starttijd != newStart || localAppointment.Eindtijd != newEnd)
                            {
                                localAppointment.Starttijd = newStart;
                                localAppointment.Eindtijd = newEnd;
                                _logger.LogInformation($"Lokale afspraak #{localAppointment.Id} verplaatst naar {newStart} - {newEnd} UTC na wijziging in Google Calendar.");
                            }

                            bool isPraktijkhuis = CheckIsPraktijkhuis(ev);
                            bool isAllDay = (ev.Start?.Date != null && ev.Start?.DateTimeDateTimeOffset == null);
                            bool isTransparent = string.Equals(ev.Transparency, "transparent", StringComparison.OrdinalIgnoreCase);
                            bool isExplicitBlocker = IsExplicitBlocker(ev.Summary);

                            localAppointment.IsHeleDag = isAllDay || isTransparent;
                            var attendee = ev.Attendees?.FirstOrDefault(a => a.Self != true && !string.Equals(a.Email, _calendarId, StringComparison.OrdinalIgnoreCase));
                            var displayNaam = attendee?.DisplayName ?? ev.Summary ?? "Onbekende Patient";

                            string voornaam, achternaam, telefoonnummer, cleanOpmerkingen;
                            DateOnly geboortedatum;

                            if (isPraktijkhuis)
                            {
                                var parsed = ParsePraktijkhuisEventInfo(ev, displayNaam);
                                voornaam = parsed.Voornaam;
                                achternaam = parsed.Achternaam;
                                geboortedatum = parsed.Geboortedatum;
                                telefoonnummer = parsed.Telefoonnummer;
                                cleanOpmerkingen = parsed.CleanOpmerkingen;
                            }
                            else
                            {
                                var splitNaam = displayNaam.Split(' ', 2);
                                voornaam = splitNaam.Length > 0 ? splitNaam[0] : "Patient";
                                achternaam = splitNaam.Length > 1 ? splitNaam[1] : "van Google";
                                geboortedatum = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));
                                telefoonnummer = "";
                                cleanOpmerkingen = ev.Description ?? "";
                            }

                            var opmerkingTag = isPraktijkhuis ? "[PH9500]\n" : "";
                            localAppointment.Opmerkingen = opmerkingTag + cleanOpmerkingen;

                            var currentPatient = localAppointment.PatientId.HasValue ? dbContext.Patienten.FirstOrDefault(p => p.Id == localAppointment.PatientId.Value) : null;
                            
                            bool isPlaceholderPatient = currentPatient == null ||
                                (!string.IsNullOrEmpty(currentPatient.Achternaam) && currentPatient.Achternaam.Contains("Crombrugge", StringComparison.OrdinalIgnoreCase)) ||
                                (!string.IsNullOrEmpty(currentPatient.Email) && currentPatient.Email.Contains("praktijkhuis", StringComparison.OrdinalIgnoreCase));

                            var afspraakTypesList = dbContext.AfspraakTypes.ToList();
                            var praktijkhuisType = afspraakTypesList.FirstOrDefault(t => t.Naam.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase))
                                                   ?? afspraakTypesList.FirstOrDefault(t => t.Id == 3);
                            var therapieType = afspraakTypesList.FirstOrDefault(t => t.Id == 2) ?? afspraakTypesList.FirstOrDefault();

                            if (isPraktijkhuis && praktijkhuisType != null)
                            {
                                localAppointment.TypeId = praktijkhuisType.Id;
                            }

                            bool isAnonymizedTitle = displayNaam.StartsWith("Sessie #", StringComparison.OrdinalIgnoreCase) 
                                || voornaam.Equals("Sessie", StringComparison.OrdinalIgnoreCase)
                                || displayNaam.Contains("patientenportaal", StringComparison.OrdinalIgnoreCase)
                                || displayNaam.Contains("patiëntenportaal", StringComparison.OrdinalIgnoreCase);

                            if (!isExplicitBlocker && isPlaceholderPatient && !isAnonymizedTitle)
                            {
                                var matchedPatient = dbContext.Patienten.FirstOrDefault(p => p.Voornaam == voornaam && p.Achternaam == achternaam);
                                if (matchedPatient == null)
                                {
                                    var safeName = System.Text.RegularExpressions.Regex.Replace(displayNaam.ToLower(), @"[^a-z0-9]", ".");
                                    var newEmail = !string.IsNullOrEmpty(attendee?.Email) ? attendee.Email : $"{safeName}@googlecalendar.local";
                                    matchedPatient = new Data.Entities.Patient
                                    {
                                        Voornaam = voornaam,
                                        Achternaam = achternaam,
                                        Email = newEmail,
                                        Geboortedatum = geboortedatum,
                                        IsActief = true,
                                        Telefoonnummer = telefoonnummer
                                    };
                                    dbContext.Patienten.Add(matchedPatient);
                                    await dbContext.SaveChangesAsync();
                                }

                                localAppointment.PatientId = matchedPatient.Id;
                                if (!localAppointment.TypeId.HasValue)
                                {
                                    localAppointment.TypeId = isPraktijkhuis && praktijkhuisType != null ? praktijkhuisType.Id : (therapieType?.Id ?? 2);
                                }
                            }
                        }
                    }
                    else
                    {
                        bool isDeclined = ev.Status == "cancelled" || 
                            (ev.Attendees != null && ev.Attendees.Any(a => 
                                (a.Self == true || (a.Email != null && (a.Email.Contains("ingedebast", StringComparison.OrdinalIgnoreCase) || a.Email.Contains("deverstandhouding", StringComparison.OrdinalIgnoreCase)))) && 
                                string.Equals(a.ResponseStatus, "declined", StringComparison.OrdinalIgnoreCase)));

                        bool isPraktijkhuis = CheckIsPraktijkhuis(ev);

                        var attendee = ev.Attendees?.FirstOrDefault(a => a.Self != true && !string.Equals(a.Email, _calendarId, StringComparison.OrdinalIgnoreCase));
                        var email = attendee?.Email;
                        if (string.IsNullOrEmpty(email) && ev.Creator?.Email != null && !string.Equals(ev.Creator.Email, _calendarId, StringComparison.OrdinalIgnoreCase))
                        {
                            email = ev.Creator.Email;
                        }
                        var displayNaam = attendee?.DisplayName ?? ev.Summary ?? "Onbekende Patient";

                        string voornaam, achternaam, telefoonnummer, cleanOpmerkingen;
                        DateOnly geboortedatum;

                        if (isPraktijkhuis)
                        {
                            var parsed = ParsePraktijkhuisEventInfo(ev, displayNaam);
                            voornaam = parsed.Voornaam;
                            achternaam = parsed.Achternaam;
                            geboortedatum = parsed.Geboortedatum;
                            telefoonnummer = parsed.Telefoonnummer;
                            cleanOpmerkingen = parsed.CleanOpmerkingen;
                        }
                        else
                        {
                            var splitNaam = displayNaam.Split(' ', 2);
                            voornaam = splitNaam.Length > 0 ? splitNaam[0] : "Patient";
                            achternaam = splitNaam.Length > 1 ? splitNaam[1] : "van Google";
                            geboortedatum = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));
                            telefoonnummer = "";
                            cleanOpmerkingen = ev.Description ?? "";
                        }

                        var opmerkingTag = isPraktijkhuis ? "[PH9500]\n" : "";
                        var opmerking = opmerkingTag + cleanOpmerkingen;

                        bool isExplicitBlocker = IsExplicitBlocker(ev.Summary);
                        bool isAnonymizedTitleNew = displayNaam.StartsWith("Sessie #", StringComparison.OrdinalIgnoreCase) 
                            || voornaam.Equals("Sessie", StringComparison.OrdinalIgnoreCase)
                            || displayNaam.Contains("patientenportaal", StringComparison.OrdinalIgnoreCase)
                            || displayNaam.Contains("patiëntenportaal", StringComparison.OrdinalIgnoreCase);
                        bool isPatientAppointment = !isExplicitBlocker && !isAnonymizedTitleNew && (!string.IsNullOrEmpty(email) || !string.IsNullOrWhiteSpace(ev.Summary));

                        if (isPatientAppointment)
                        {
                            if (string.IsNullOrEmpty(email))
                            {
                                var safeName = System.Text.RegularExpressions.Regex.Replace(displayNaam.ToLower(), @"[^a-z0-9]", ".");
                                email = $"{safeName}@googlecalendar.local";
                            }

                            // Zoek patient op naam-combinatie om te voorkomen dat verschillende patienten van een gedeelde praktijkmail samengevoegd worden
                            var patient = dbContext.Patienten.FirstOrDefault(p => 
                                (p.Voornaam == voornaam && p.Achternaam == achternaam) ||
                                (p.Email == email && p.Voornaam == voornaam)
                            );

                            if (patient == null)
                            {
                                var patientEmail = email;
                                var eMailAlInGebruik = dbContext.Patienten.Any(p => p.Email == email);
                                if (eMailAlInGebruik)
                                {
                                    var safeName = System.Text.RegularExpressions.Regex.Replace(displayNaam.ToLower(), @"[^a-z0-9]", ".");
                                    patientEmail = $"{safeName}@praktijkhuis9500.be";
                                }

                                patient = new Data.Entities.Patient
                                {
                                    Voornaam = voornaam,
                                    Achternaam = achternaam,
                                    Email = patientEmail,
                                    Geboortedatum = geboortedatum,
                                    IsActief = true,
                                    Telefoonnummer = telefoonnummer
                                };
                                dbContext.Patienten.Add(patient);
                                await dbContext.SaveChangesAsync();
                                _logger.LogInformation($"Nieuwe patient {patient.VolledigeNaam} ({telefoonnummer}) aangemaakt na boeking via Google Calendar.");
                            }
                            else
                            {
                                // Update eventuele ontbrekende telefoonnummer of geboortedatum
                                if (string.IsNullOrEmpty(patient.Telefoonnummer) && !string.IsNullOrEmpty(telefoonnummer))
                                {
                                    patient.Telefoonnummer = telefoonnummer;
                                }
                                if (geboortedatum != DateOnly.FromDateTime(DateTime.Today.AddYears(-30)))
                                {
                                    patient.Geboortedatum = geboortedatum;
                                }
                            }

                            // Haal het afspraaktype op
                            var allTypes = dbContext.AfspraakTypes.ToList();
                            var praktijkhuisType = allTypes.FirstOrDefault(t => t.Naam.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase))
                                                   ?? allTypes.FirstOrDefault(t => t.Id == 3);
                            var therapieType = allTypes.FirstOrDefault(t => t.Id == 2) ?? allTypes.FirstOrDefault();

                            var type = isPraktijkhuis && praktijkhuisType != null ? praktijkhuisType : (therapieType ?? allTypes.FirstOrDefault());

                            var (newStart, newEnd) = GetEventStartAndEnd(ev);

                            bool isAllDay = (ev.Start?.Date != null && ev.Start?.DateTimeDateTimeOffset == null);
                            bool isTransparent = string.Equals(ev.Transparency, "transparent", StringComparison.OrdinalIgnoreCase);

                            var nieuweAfspraak = new Data.Entities.Afspraak
                            {
                                PatientId = patient.Id,
                                TypeId = type?.Id,
                                Starttijd = newStart,
                                Eindtijd = newEnd,
                                Status = isDeclined ? Data.Entities.AfspraakStatus.Geannuleerd : Data.Entities.AfspraakStatus.Gepland,
                                GoogleEventId = ev.Id,
                                Opmerkingen = opmerking,
                                IsHeleDag = isAllDay || isTransparent
                            };

                            dbContext.Afspraken.Add(nieuweAfspraak);
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"Nieuwe afspraak #{nieuweAfspraak.Id} lokaal aangemaakt voor {patient.VolledigeNaam} via Google sync.");

                            // Geen e-mails versturen tijdens Google Calendar synchronisatie (alleen bij nieuw online boekingen via het portaal)
                        }
                        else
                        {
                            // Dit is een algemeen blocker event / persoonlijke afspraak / melding!
                            var (newStart, newEnd) = GetEventStartAndEnd(ev);

                            bool isAllDay = (ev.Start?.Date != null && ev.Start?.DateTimeDateTimeOffset == null);
                            bool isTransparent = string.Equals(ev.Transparency, "transparent", StringComparison.OrdinalIgnoreCase);

                            var nieuweAfspraak = new Data.Entities.Afspraak
                            {
                                PatientId = null,
                                TypeId = null,
                                Starttijd = newStart,
                                Eindtijd = newEnd,
                                Status = isDeclined ? Data.Entities.AfspraakStatus.Geannuleerd : Data.Entities.AfspraakStatus.Gepland,
                                GoogleEventId = ev.Id,
                                Opmerkingen = ev.Summary ?? "Blokkering / Melding",
                                IsHeleDag = isAllDay || isTransparent
                            };

                            dbContext.Afspraken.Add(nieuweAfspraak);
                            await dbContext.SaveChangesAsync();
                            _logger.LogInformation($"Nieuwe afspraak/melding '{nieuweAfspraak.Opmerkingen}' (IsHeleDag: {nieuweAfspraak.IsHeleDag}) toegevoegd via Google sync.");
                        }
                    }
                }
                await dbContext.SaveChangesAsync();
            }
        }

        private static (DateTime StartUtc, DateTime EndUtc) GetEventStartAndEnd(Event ev)
        {
            DateTime startUtc = DateTime.UtcNow;
            DateTime endUtc = startUtc.AddHours(1);

            if (ev?.Start != null)
            {
                if (ev.Start.DateTimeDateTimeOffset.HasValue)
                {
                    startUtc = ev.Start.DateTimeDateTimeOffset.Value.UtcDateTime;
                }
                else if (!string.IsNullOrEmpty(ev.Start.Date))
                {
                    if (DateTime.TryParse(ev.Start.Date, out var parsedDate))
                    {
                        startUtc = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                    }
                }
            }

            if (ev?.End != null)
            {
                if (ev.End.DateTimeDateTimeOffset.HasValue)
                {
                    endUtc = ev.End.DateTimeDateTimeOffset.Value.UtcDateTime;
                }
                else if (!string.IsNullOrEmpty(ev.End.Date))
                {
                    if (DateTime.TryParse(ev.End.Date, out var parsedDateEnd))
                    {
                        endUtc = DateTime.SpecifyKind(parsedDateEnd, DateTimeKind.Utc);
                    }
                }
            }

            if (endUtc <= startUtc)
            {
                endUtc = startUtc.AddHours(1);
            }

            return (startUtc, endUtc);
        }

        private static bool IsExplicitBlocker(string? summary)
        {
            if (string.IsNullOrWhiteSpace(summary)) return true;
            var lower = summary.Trim().ToLower();
            return lower == "verlof" || lower == "vakantie" || lower == "afwezig" || 
                   lower == "pauze" || lower == "lunch" || lower == "blokkering" || 
                   lower == "vrij nemen?" || lower.StartsWith("verlof ") || lower.StartsWith("vakantie ") ||
                   lower.StartsWith("blokkering") || lower.Contains("blokkering");
        }

        private static bool CheckIsPraktijkhuis(Event ev)
        {
            if (ev == null) return false;
            
            if (ev.Creator?.DisplayName != null && ev.Creator.DisplayName.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase)) return true;
            if (ev.Creator?.Email != null && ev.Creator.Email.Contains("praktijkhuis", StringComparison.OrdinalIgnoreCase)) return true;
            if (ev.Organizer?.DisplayName != null && ev.Organizer.DisplayName.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase)) return true;
            if (ev.Organizer?.Email != null && ev.Organizer.Email.Contains("praktijkhuis", StringComparison.OrdinalIgnoreCase)) return true;

            if (ev.Attendees != null && ev.Attendees.Any(a => 
                (a.DisplayName != null && a.DisplayName.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase)) ||
                (a.Email != null && a.Email.Contains("praktijkhuis", StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }

            if (ev.Location != null && ev.Location.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase)) return true;
            if (ev.Summary != null && (ev.Summary.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase) || 
                                      (ev.Summary.Contains("Psycholoog", StringComparison.OrdinalIgnoreCase) && ev.Summary.Contains("(")))) return true;

            return false;
        }

        private static (string Voornaam, string Achternaam, DateOnly Geboortedatum, string Telefoonnummer, string CleanOpmerkingen) ParsePraktijkhuisEventInfo(Event ev, string defaultDisplayNaam)
        {
            var summary = ev.Summary ?? defaultDisplayNaam ?? "";
            var description = ev.Description ?? "";

            // 1. Parse Geboortedatum uit Summary, bijv: "1ste Ricour Dirk ( 22/09/1953) Psycholoog"
            DateOnly geboortedatum = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));
            var dobMatch = System.Text.RegularExpressions.Regex.Match(summary, @"\(\s*(\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{4})\s*\)");
            if (dobMatch.Success)
            {
                var dobStr = dobMatch.Groups[1].Value.Replace('-', '/').Replace('.', '/');
                if (DateOnly.TryParseExact(dobStr, "dd/MM/yyyy", out var parsedDob))
                {
                    geboortedatum = parsedDob;
                }
            }

            // 2. Parse Telefoonnummer & Clean Opmerkingen uit Description
            string telefoonnummer = "";
            var descLines = description.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var cleanDescList = new System.Collections.Generic.List<string>();

            foreach (var line in descLines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(telefoonnummer) && System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^[\d\s\.\-\+]{8,16}$") && trimmed.Any(char.IsDigit))
                {
                    telefoonnummer = trimmed;
                }
                else if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    cleanDescList.Add(trimmed);
                }
            }

            // 3. Clean Name van Summary
            var cleanName = summary;
            if (dobMatch.Success)
            {
                cleanName = cleanName.Replace(dobMatch.Value, "");
            }
            cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"\b(1ste|2de|3de|Psycholoog|consult|intake)\b", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
            
            var nameParts = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string voornaam = "Patient";
            string achternaam = "van Google";

            if (nameParts.Length == 1)
            {
                voornaam = nameParts[0];
                achternaam = "";
            }
            else if (nameParts.Length >= 2)
            {
                achternaam = nameParts[0];
                voornaam = string.Join(" ", nameParts.Skip(1));
            }

            string cleanOpmerkingen = string.Join("\n", cleanDescList);

            return (voornaam, achternaam, geboortedatum, telefoonnummer, cleanOpmerkingen);
        }

        public async Task<List<(DateTime Start, DateTime End)>> GetBusySlotsAsync(DateTime startUtc, DateTime endUtc)
        {
            var busySlots = new List<(DateTime Start, DateTime End)>();

            // Always check local DB for scheduled appointments that aren't cancelled
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                var appointments = dbContext.Afspraken
                    .Where(a => a.Starttijd < endUtc && a.Eindtijd > startUtc && a.Status != Data.Entities.AfspraakStatus.Geannuleerd)
                    .ToList();

                foreach (var app in appointments)
                {
                    busySlots.Add((app.Starttijd, app.Eindtijd));
                }
            }

            if (_useMock)
            {
                return busySlots;
            }

            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geinitialiseerd.");

            try
            {
                var request = new FreeBusyRequest
                {
                    TimeMinDateTimeOffset = startUtc,
                    TimeMaxDateTimeOffset = endUtc,
                    Items = new List<FreeBusyRequestItem> { new FreeBusyRequestItem { Id = _calendarId } }
                };

                var query = _calendarService.Freebusy.Query(request);
                var response = await query.ExecuteAsync();

                if (response.Calendars != null && response.Calendars.TryGetValue(_calendarId, out var calendarFreeBusy))
                {
                    if (calendarFreeBusy.Busy != null)
                    {
                        foreach (var busy in calendarFreeBusy.Busy)
                        {
                            if (busy.StartDateTimeOffset.HasValue && busy.EndDateTimeOffset.HasValue)
                            {
                                busySlots.Add((busy.StartDateTimeOffset.Value.UtcDateTime, busy.EndDateTimeOffset.Value.UtcDateTime));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij ophalen FreeBusy van Google Calendar. Terugvallen op lege lijst.");
            }

            return busySlots;
        }
    }
}
