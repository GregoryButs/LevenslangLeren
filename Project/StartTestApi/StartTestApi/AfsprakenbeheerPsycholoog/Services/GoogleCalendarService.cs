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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AfsprakenbeheerPsycholoog.Data;
using AfsprakenbeheerPsycholoog.Data.Entities;

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

            _useMock = string.Equals(configuration["GoogleCalendar:UseMock"], "true", StringComparison.OrdinalIgnoreCase);

            if (_useMock)
            {
                _logger.LogInformation("Google Calendar Service is geïnitialiseerd in MOCK modus.");
                return;
            }

            try
            {
                string envJson = configuration["GoogleCalendar:CredentialsJsonContent"] 
                    ?? Environment.GetEnvironmentVariable("GOOGLE_CREDENTIALS_JSON") 
                    ?? "";

                GoogleCredential? credential = null;

                if (!string.IsNullOrWhiteSpace(envJson))
                {
                    using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(envJson)))
                    {
                        credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.Calendar);
                    }
                }
                else
                {
                    string credentialsPath = configuration["GoogleCalendar:CredentialsJsonPath"] ?? "google-credentials.json";
                    if (!File.Exists(credentialsPath))
                    {
                        var baseCandidate = Path.Combine(AppContext.BaseDirectory, credentialsPath);
                        var currentCandidate = Path.Combine(Directory.GetCurrentDirectory(), credentialsPath);
                        if (File.Exists(baseCandidate)) credentialsPath = baseCandidate;
                        else if (File.Exists(currentCandidate)) credentialsPath = currentCandidate;
                    }

                    if (File.Exists(credentialsPath))
                    {
                        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
                        {
                            credential = GoogleCredential.FromStream(stream).CreateScoped(CalendarService.Scope.Calendar);
                        }
                        _logger.LogInformation($"Google Calendar credentials succesvol geladen van: {credentialsPath}");
                    }
                    else
                    {
                        _logger.LogWarning($"Google credentials niet gevonden (noch in GOOGLE_CREDENTIALS_JSON noch op bestandspad {credentialsPath}). Service schakelt over naar MOCK modus.");
                        _useMock = true;
                        return;
                    }
                }

                _calendarService = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "De Verstandhouding Agendabeheer",
                });

                _logger.LogInformation($"Google Calendar Service succesvol geïnitialiseerd op agenda ID: {_calendarId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij initialiseren Google Calendar Service. Schakelt over naar MOCK modus.");
                _useMock = true;
            }
        }

        private static string GenerateValidMeetUrl(int afspraakId)
        {
            return "https://meet.google.com/new";
        }

        public async Task<(string EventId, string? MeetLink)> CreateEventAsync(DateTime startUtc, DateTime endUtc, int afspraakId, bool createMeetLink = false, string locationDetails = "", string patientNaam = "")
        {
            string? validMeetLink = createMeetLink ? GenerateValidMeetUrl(afspraakId) : null;

            if (_useMock)
            {
                var mockEventId = $"mock_event_{Guid.NewGuid():N}";
                _logger.LogInformation($"[MOCK] Afspraak #{afspraakId} aangemaakt ({startUtc} tot {endUtc} UTC). MockID: {mockEventId}, MeetLink: {validMeetLink}");
                return (mockEventId, validMeetLink);
            }

            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geïnitialiseerd.");

            var summaryText = string.IsNullOrWhiteSpace(patientNaam)
                ? $"Afspraak #{afspraakId}"
                : patientNaam;

            var startOffset = new DateTimeOffset(startUtc, TimeSpan.Zero);
            var endOffset = new DateTimeOffset(endUtc, TimeSpan.Zero);

            var calendarEvent = new Event
            {
                Summary = summaryText,
                Description = createMeetLink 
                    ? $"Online Google Meet Videoconsultatie\nLink: {validMeetLink}\nGereserveerd via online patiëntenportaal - De Verstandhouding."
                    : "Gereserveerd via online patiëntenportaal - De Verstandhouding.",
                Start = new EventDateTime { DateTimeDateTimeOffset = startOffset, TimeZone = "Europe/Brussels" },
                End = new EventDateTime { DateTimeDateTimeOffset = endOffset, TimeZone = "Europe/Brussels" },
                Location = createMeetLink ? validMeetLink : locationDetails
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

            try
            {
                var request = _calendarService.Events.Insert(calendarEvent, _calendarId);
                if (createMeetLink) request.ConferenceDataVersion = 1;

                var createdEvent = await request.ExecuteAsync();
                string? meetLink = createdEvent.HangoutLink 
                    ?? createdEvent.ConferenceData?.EntryPoints?.FirstOrDefault(e => e.EntryPointType == "video")?.Uri
                    ?? validMeetLink;

                _logger.LogInformation($"Google Calendar Event succesvol aangemaakt voor afspraak #{afspraakId} met ID: {createdEvent.Id}, MeetLink: {meetLink}");
                return (createdEvent.Id, meetLink);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Google Calendar API insert met ConferenceData mislukt op agenda '{_calendarId}'. Probeert opnieuw zonder ConferenceData...");
                
                try
                {
                    calendarEvent.ConferenceData = null;
                    var retryRequest = _calendarService.Events.Insert(calendarEvent, _calendarId);
                    var createdEvent = await retryRequest.ExecuteAsync();
                    _logger.LogInformation($"Google Calendar Event (zonder ConferenceData) succesvol aangemaakt op agenda '{_calendarId}' met ID: {createdEvent.Id}");
                    return (createdEvent.Id, validMeetLink);
                }
                catch (Exception retryEx)
                {
                    _logger.LogError(retryEx, $"Google Calendar API insert mislukt op agenda '{_calendarId}' voor afspraak #{afspraakId}. Controleer of agenda '{_calendarId}' is gedeeld met de service account.");
                    return ($"local_event_{afspraakId}", validMeetLink);
                }
            }
        }

        public async Task UpdateEventAsync(string googleEventId, DateTime startUtc, DateTime endUtc, int afspraakId, string patientNaam = "")
        {
            if (_useMock) return;
            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geïnitialiseerd.");

            var summaryText = string.IsNullOrWhiteSpace(patientNaam) ? $"Afspraak #{afspraakId}" : $"{patientNaam}";

            var calendarEvent = new Event
            {
                Summary = summaryText,
                Start = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(startUtc, TimeSpan.Zero) },
                End = new EventDateTime { DateTimeDateTimeOffset = new DateTimeOffset(endUtc, TimeSpan.Zero) }
            };

            var request = _calendarService.Events.Update(calendarEvent, _calendarId, googleEventId);
            await request.ExecuteAsync();
        }

        public async Task DeleteEventAsync(string googleEventId)
        {
            if (_useMock) return;
            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geïnitialiseerd.");

            try
            {
                await _calendarService.Events.Delete(_calendarId, googleEventId).ExecuteAsync();
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Google Event {googleEventId} reeds verwijderd.");
            }
        }

        public async Task SyncIncomingChangesAsync()
        {
            if (_useMock) return;
            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geïnitialiseerd.");

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var request = _calendarService.Events.List(_calendarId);
            request.TimeMinDateTimeOffset = DateTimeOffset.UtcNow.AddDays(-180);
            request.TimeMaxDateTimeOffset = DateTimeOffset.UtcNow.AddDays(365);
            request.SingleEvents = true;
            request.MaxResults = 2500;

            string? pageToken = null;
            int totalProcessed = 0;

            do
            {
                request.PageToken = pageToken;
                var events = await request.ExecuteAsync();
                if (events.Items == null) break;

                foreach (var ev in events.Items)
                {
                    if (ShouldSkipEvent(ev)) continue;

                    try
                    {
                        await ProcessSingleEventAsync(ev, dbContext);
                        totalProcessed++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Fout bij verwerken van Google Calendar event {EventId} ({Summary})", ev.Id, ev.Summary);
                    }
                }

                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fout bij opslaan van EF Core wijzigingen tijdens Google Calendar synchronisatie");
                }
                pageToken = events.NextPageToken;
            } while (!string.IsNullOrEmpty(pageToken));

            _logger.LogInformation($"Google Calendar synchronisatie voltooid. Totaal geanalyseerd en verwerkt: {totalProcessed} events.");
        }

        private async Task ProcessSingleEventAsync(Event ev, ApplicationDbContext dbContext)
        {
            var (startUtc, endUtc) = GetEventStartAndEnd(ev);
            bool isDeclined = IsDeclinedOrCancelled(ev);
            bool isAllDay = IsAllDayEvent(ev);
            bool isTransparent = string.Equals(ev.Transparency, "transparent", StringComparison.OrdinalIgnoreCase);

            bool isPraktijkhuis = CheckIsPraktijkhuis(ev);
            bool isExplicitBlocker = IsExplicitBlocker(ev.Summary);

            var patientInfo = ExtractPatientDetails(ev, isPraktijkhuis);
            bool hasRealPatientName = !string.IsNullOrWhiteSpace(patientInfo.Voornaam) 
                && !patientInfo.Voornaam.Equals("Praktijkhuis", StringComparison.OrdinalIgnoreCase) 
                && !patientInfo.Voornaam.Equals("Patient", StringComparison.OrdinalIgnoreCase)
                && !patientInfo.Voornaam.Equals("Onbekende", StringComparison.OrdinalIgnoreCase);

            bool isPatientAppointment = !isExplicitBlocker && !patientInfo.IsAnonymized && hasRealPatientName && (!string.IsNullOrEmpty(patientInfo.Email) || !string.IsNullOrWhiteSpace(ev.Summary));
            string? googleMeetLink = ExtractGoogleMeetLink(ev);

            var localAppointment = dbContext.Afspraken.Local.FirstOrDefault(a => a.GoogleEventId == ev.Id)
                ?? dbContext.Afspraken.FirstOrDefault(a => a.GoogleEventId == ev.Id);

            if (localAppointment != null)
            {
                localAppointment.Starttijd = startUtc;
                localAppointment.Eindtijd = endUtc;
                localAppointment.IsHeleDag = isAllDay || isTransparent;
                localAppointment.Status = isDeclined ? AfspraakStatus.Geannuleerd : AfspraakStatus.Gepland;
                localAppointment.GoogleMeetLink = googleMeetLink;

                var afspraakType = ResolveAfspraakType(dbContext, isPraktijkhuis, ev?.Summary, ev?.Description, googleMeetLink);
                if (afspraakType != null)
                {
                    localAppointment.TypeId = afspraakType.Id;
                }

                if (isPatientAppointment)
                {
                    var patient = await GetOrCreatePatientAsync(dbContext, patientInfo);
                    if (patient != null && patient.Id > 0)
                    {
                        localAppointment.PatientId = patient.Id;
                    }
                    if (!string.IsNullOrWhiteSpace(patientInfo.Opmerkingen))
                    {
                        localAppointment.Opmerkingen = patientInfo.Opmerkingen;
                    }
                }
                else if (isExplicitBlocker)
                {
                    localAppointment.PatientId = null;
                }
                return;
            }

            if (isPatientAppointment)
            {
                var patient = await GetOrCreatePatientAsync(dbContext, patientInfo);
                var afspraakType = ResolveAfspraakType(dbContext, isPraktijkhuis, ev?.Summary, ev?.Description, googleMeetLink);
                int? assignedPatientId = (patient != null && patient.Id > 0) ? patient.Id : null;

                var nieuweAfspraak = new Afspraak
                {
                    PatientId = assignedPatientId,
                    TypeId = afspraakType?.Id,
                    Starttijd = startUtc,
                    Eindtijd = endUtc,
                    Status = isDeclined ? AfspraakStatus.Geannuleerd : AfspraakStatus.Gepland,
                    GoogleEventId = ev.Id,
                    GoogleMeetLink = googleMeetLink,
                    Opmerkingen = patientInfo.Opmerkingen,
                    IsHeleDag = isAllDay || isTransparent
                };

                dbContext.Afspraken.Add(nieuweAfspraak);
            }
            else
            {
                var afspraakType = ResolveAfspraakType(dbContext, isPraktijkhuis, ev?.Summary, ev?.Description, googleMeetLink);
                var blockerAfspraak = new Afspraak
                {
                    PatientId = null,
                    TypeId = afspraakType?.Id,
                    Starttijd = startUtc,
                    Eindtijd = endUtc,
                    Status = isDeclined ? AfspraakStatus.Geannuleerd : AfspraakStatus.Gepland,
                    GoogleEventId = ev.Id,
                    GoogleMeetLink = googleMeetLink,
                    Opmerkingen = ev.Summary ?? "Blokkering / Melding",
                    IsHeleDag = isAllDay || isTransparent
                };

                dbContext.Afspraken.Add(blockerAfspraak);
            }
        }

        private static bool ShouldSkipEvent(Event ev)
        {
            if (ev == null) return true;
            if (string.Equals(ev.EventType, "workingLocation", StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(ev.EventType, "focusTime", StringComparison.OrdinalIgnoreCase)) return true;

            bool hasValidStart = ev.Start != null && (
                ev.Start.DateTimeDateTimeOffset.HasValue ||
                !string.IsNullOrEmpty(ev.Start.DateTimeRaw) ||
                !string.IsNullOrEmpty(ev.Start.Date)
            );

            if (!hasValidStart) return true;

            if (string.IsNullOrWhiteSpace(ev.Summary) && (ev.Attendees == null || !ev.Attendees.Any()))
            {
                return true;
            }

            return false;
        }

        private bool IsDeclinedOrCancelled(Event ev)
        {
            if (ev.Status == "cancelled") return true;

            if (ev.Attendees != null)
            {
                return ev.Attendees.Any(a =>
                    (a.Self == true || (a.Email != null && (a.Email.Contains("ingedebast", StringComparison.OrdinalIgnoreCase) || a.Email.Contains(_calendarId, StringComparison.OrdinalIgnoreCase)))) &&
                    string.Equals(a.ResponseStatus, "declined", StringComparison.OrdinalIgnoreCase)
                );
            }

            return false;
        }

        private static (DateTime StartUtc, DateTime EndUtc) GetEventStartAndEnd(Event ev)
        {
            DateTime startUtc = DateTime.UtcNow;
            DateTime endUtc = startUtc.AddHours(1);

            if (ev?.Start != null)
            {
                if (ev.Start.DateTimeDateTimeOffset.HasValue)
                    startUtc = ev.Start.DateTimeDateTimeOffset.Value.UtcDateTime;
                else if (!string.IsNullOrEmpty(ev.Start.DateTimeRaw) && DateTimeOffset.TryParse(ev.Start.DateTimeRaw, out var dtoStart))
                    startUtc = dtoStart.UtcDateTime;
                else if (!string.IsNullOrEmpty(ev.Start.Date) && DateTime.TryParse(ev.Start.Date, out var parsedDate))
                    startUtc = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            }

            if (ev?.End != null)
            {
                if (ev.End.DateTimeDateTimeOffset.HasValue)
                    endUtc = ev.End.DateTimeDateTimeOffset.Value.UtcDateTime;
                else if (!string.IsNullOrEmpty(ev.End.DateTimeRaw) && DateTimeOffset.TryParse(ev.End.DateTimeRaw, out var dtoEnd))
                    endUtc = dtoEnd.UtcDateTime;
                else if (!string.IsNullOrEmpty(ev.End.Date) && DateTime.TryParse(ev.End.Date, out var parsedDateEnd))
                    endUtc = DateTime.SpecifyKind(parsedDateEnd, DateTimeKind.Utc);
            }

            if (endUtc <= startUtc) endUtc = startUtc.AddHours(1);

            return (startUtc, endUtc);
        }

        private static bool IsAllDayEvent(Event ev)
        {
            if (ev?.Start == null) return false;
            bool hasTime = ev.Start.DateTimeDateTimeOffset.HasValue || !string.IsNullOrEmpty(ev.Start.DateTimeRaw);
            return !string.IsNullOrEmpty(ev.Start.Date) && !hasTime;
        }

        private record PatientInfoDTO(
            string Voornaam,
            string Achternaam,
            string Email,
            string Telefoonnummer,
            DateOnly Geboortedatum,
            string Opmerkingen,
            bool IsAnonymized
        );

        private PatientInfoDTO ExtractPatientDetails(Event ev, bool isPraktijkhuis)
        {
            var attendee = ev.Attendees?.FirstOrDefault(a => a.Self != true && !string.Equals(a.Email, _calendarId, StringComparison.OrdinalIgnoreCase));
            var email = attendee?.Email;
            if (string.IsNullOrEmpty(email) && ev.Creator?.Email != null && !string.Equals(ev.Creator.Email, _calendarId, StringComparison.OrdinalIgnoreCase))
            {
                email = ev.Creator.Email;
            }

            var displayNaam = !string.IsNullOrWhiteSpace(ev.Summary) ? ev.Summary : (attendee?.DisplayName ?? "Onbekende Patient");
            string voornaam, achternaam, telefoonnummer, cleanOpmerkingen;
            DateOnly geboortedatum;

            if (isPraktijkhuis)
            {
                var parsed = ParsePraktijkhuisEventInfo(ev, displayNaam);
                voornaam = parsed.Voornaam;
                achternaam = parsed.Achternaam;
                geboortedatum = parsed.Geboortedatum;
                telefoonnummer = parsed.Telefoonnummer;
                cleanOpmerkingen = "[PH9500]\n" + parsed.CleanOpmerkingen;

                var safeV = Regex.Replace(voornaam.ToLower(), @"[^a-z0-9]", "");
                var safeA = Regex.Replace(achternaam.ToLower(), @"[^a-z0-9]", "");
                if (string.IsNullOrEmpty(safeV)) safeV = "patient";
                email = $"{safeV}.{safeA}@praktijkhuis.local";
            }
            else
            {
                var splitNaam = displayNaam.Split(' ', 2);
                voornaam = splitNaam.Length > 0 ? splitNaam[0] : "Patient";
                achternaam = splitNaam.Length > 1 ? splitNaam[1] : "";
                geboortedatum = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));
                telefoonnummer = "";
                cleanOpmerkingen = ev.Description ?? "";
            }

            if (string.IsNullOrEmpty(email))
            {
                var safeName = Regex.Replace(displayNaam.ToLower(), @"[^a-z0-9]", ".");
                email = $"{safeName}@googlecalendar.local";
            }

            bool isAnonymized = displayNaam.StartsWith("Sessie #", StringComparison.OrdinalIgnoreCase)
                || voornaam.Equals("Sessie", StringComparison.OrdinalIgnoreCase)
                || displayNaam.Contains("patientenportaal", StringComparison.OrdinalIgnoreCase)
                || displayNaam.Contains("patiëntenportaal", StringComparison.OrdinalIgnoreCase);

            return new PatientInfoDTO(voornaam, achternaam, email, telefoonnummer, geboortedatum, cleanOpmerkingen, isAnonymized);
        }

        private async Task<Patient> GetOrCreatePatientAsync(ApplicationDbContext dbContext, PatientInfoDTO info)
        {
            var vNorm = (info.Voornaam ?? "").Trim().ToLower();
            var aNorm = (info.Achternaam ?? "").Trim().ToLower();
            var eNorm = (info.Email ?? "").Trim().ToLower();
            var fullNorm = $"{vNorm} {aNorm}".Trim();

            bool isSharedEmail = !string.IsNullOrEmpty(eNorm) && 
                (eNorm.Contains("praktijkhuis@") || eNorm.Contains("googlecalendar.local") || eNorm.Contains("ingedebast@gmail.com"));

            var patient = dbContext.Patienten.Local.FirstOrDefault(p =>
                (!isSharedEmail && !string.IsNullOrEmpty(eNorm) && p.Email.ToLower() == eNorm) ||
                (p.Voornaam.ToLower() == vNorm && p.Achternaam.ToLower() == aNorm && !string.IsNullOrEmpty(vNorm)) ||
                (p.Voornaam.ToLower() == aNorm && p.Achternaam.ToLower() == vNorm && !string.IsNullOrEmpty(vNorm)) ||
                ((p.Voornaam + " " + p.Achternaam).Trim().ToLower() == fullNorm && !string.IsNullOrEmpty(fullNorm)) ||
                ((p.Achternaam + " " + p.Voornaam).Trim().ToLower() == fullNorm && !string.IsNullOrEmpty(fullNorm))
            ) ?? dbContext.Patienten.FirstOrDefault(p =>
                (!isSharedEmail && !string.IsNullOrEmpty(eNorm) && p.Email.ToLower() == eNorm) ||
                (p.Voornaam.ToLower() == vNorm && p.Achternaam.ToLower() == aNorm && !string.IsNullOrEmpty(vNorm)) ||
                (p.Voornaam.ToLower() == aNorm && p.Achternaam.ToLower() == vNorm && !string.IsNullOrEmpty(vNorm)) ||
                ((p.Voornaam + " " + p.Achternaam).Trim().ToLower() == fullNorm && !string.IsNullOrEmpty(fullNorm)) ||
                ((p.Achternaam + " " + p.Voornaam).Trim().ToLower() == fullNorm && !string.IsNullOrEmpty(fullNorm))
            );

            if (patient == null)
            {
                var patientEmail = info.Email;
                var safeName = Regex.Replace(((info.Voornaam ?? "") + (info.Achternaam ?? "")).ToLower(), @"[^a-z0-9]", ".");
                if (string.IsNullOrEmpty(safeName)) safeName = "patient";

                if (!string.IsNullOrEmpty(info.Email) && (dbContext.Patienten.Local.Any(p => p.Email == info.Email) || dbContext.Patienten.Any(p => p.Email == info.Email)))
                {
                    patientEmail = $"{safeName}.{Guid.NewGuid().ToString().Substring(0, 6)}@praktijkhuis9500.be";
                }

                patient = new Patient
                {
                    Voornaam = info.Voornaam,
                    Achternaam = info.Achternaam,
                    Email = patientEmail,
                    Geboortedatum = info.Geboortedatum,
                    IsActief = true,
                    Telefoonnummer = string.IsNullOrWhiteSpace(info.Telefoonnummer) ? "" : info.Telefoonnummer
                };

                dbContext.Patienten.Add(patient);
                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    // Fallback to unique email if first save failed due to email constraint
                    patient.Email = $"{safeName}.{Guid.NewGuid().ToString().Substring(0, 8)}@praktijkhuis9500.be";
                    try
                    {
                        await dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Fout bij opslaan van nieuwe patiënt {Naam}", patient.VolledigeNaam);
                    }
                }
                _logger.LogInformation($"Nieuwe patiënt {patient.VolledigeNaam} aangemaakt na synchronisatie uit Google Calendar.");
            }
            else
            {
                if (string.IsNullOrEmpty(patient.Telefoonnummer) && !string.IsNullOrEmpty(info.Telefoonnummer))
                {
                    patient.Telefoonnummer = info.Telefoonnummer;
                }
            }

            return patient;
        }

        private static string? ExtractGoogleMeetLink(Event ev)
        {
            if (ev == null) return null;

            var textToSearch = $"{ev.Summary ?? ""} {ev.Description ?? ""} {ev.Location ?? ""}".ToLower();

            // Only extract Google Meet link for explicitly online appointments
            bool isExplicitOnline = textToSearch.Contains("google meet") || textToSearch.Contains("online") ||
                                    textToSearch.Contains("videoconsult") || textToSearch.Contains("webinar") ||
                                    textToSearch.Contains("meet.google.com");

            if (!isExplicitOnline)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(ev.HangoutLink)) return ev.HangoutLink;

            if (ev.ConferenceData?.EntryPoints != null)
            {
                var videoEntryPoint = ev.ConferenceData.EntryPoints.FirstOrDefault(e => e.EntryPointType == "video" || (!string.IsNullOrEmpty(e.Uri) && e.Uri.Contains("meet.google.com")));
                if (videoEntryPoint != null && !string.IsNullOrWhiteSpace(videoEntryPoint.Uri))
                {
                    return videoEntryPoint.Uri;
                }
            }

            if (!string.IsNullOrWhiteSpace(ev.Location) && ev.Location.Contains("meet.google.com"))
            {
                var match = Regex.Match(ev.Location, @"https?://meet\.google\.com/[a-z0-9\-]+", RegexOptions.IgnoreCase);
                if (match.Success) return match.Value;
            }

            if (!string.IsNullOrWhiteSpace(ev.Description) && ev.Description.Contains("meet.google.com"))
            {
                var match = Regex.Match(ev.Description, @"https?://meet\.google\.com/[a-z0-9\-]+", RegexOptions.IgnoreCase);
                if (match.Success) return match.Value;
            }

            return "https://meet.google.com/";
        }

        private static AfspraakType? ResolveAfspraakType(ApplicationDbContext dbContext, bool isPraktijkhuis, string? summary = null, string? description = null, string? meetLink = null)
        {
            var allTypes = dbContext.AfspraakTypes.ToList();
            if (!allTypes.Any()) return null;

            var textToSearch = $"{summary ?? ""} {description ?? ""}".Trim();
            var lowerText = textToSearch.ToLower();

            // 0. Google Meet / Online meeting detection (only for explicit online keywords)
            bool isExplicitOnline = lowerText.Contains("google meet") || lowerText.Contains("online") || lowerText.Contains("videoconsult") || lowerText.Contains("webinar");
            if (isExplicitOnline)
            {
                var onlineType = allTypes.FirstOrDefault(t => t.Naam.Contains("Online", StringComparison.OrdinalIgnoreCase) || t.Naam.Contains("Video", StringComparison.OrdinalIgnoreCase));
                if (onlineType != null) return onlineType;
            }

            // 1. Direct name match against any existing AfspraakType in the database
            if (!string.IsNullOrWhiteSpace(textToSearch))
            {
                foreach (var type in allTypes)
                {
                    if (!string.IsNullOrWhiteSpace(type.Naam) && textToSearch.Contains(type.Naam, StringComparison.OrdinalIgnoreCase))
                    {
                        return type;
                    }
                }
            }

            // 2. Keyword fallback matching
            if (lowerText.Contains("pauze") || lowerText.Contains("lunch"))
            {
                var pauzeType = allTypes.FirstOrDefault(t => t.Naam.Contains("Pauze", StringComparison.OrdinalIgnoreCase) || t.Naam.Contains("Lunch", StringComparison.OrdinalIgnoreCase));
                if (pauzeType != null) return pauzeType;
            }

            if (lowerText.Contains("verlof") || lowerText.Contains("vakantie") || lowerText.Contains("afwezig") || lowerText.Contains("vrij"))
            {
                var verlofType = allTypes.FirstOrDefault(t => t.Naam.Contains("Verlof", StringComparison.OrdinalIgnoreCase) || t.Naam.Contains("Vakantie", StringComparison.OrdinalIgnoreCase) || t.Naam.Contains("Afwezig", StringComparison.OrdinalIgnoreCase));
                if (verlofType != null) return verlofType;
            }

            if (lowerText.Contains("intake") || lowerText.Contains("1ste") || lowerText.Contains("eerste") || lowerText.Contains("kennismaking"))
            {
                var intakeType = allTypes.FirstOrDefault(t => t.Naam.Contains("Intake", StringComparison.OrdinalIgnoreCase));
                if (intakeType != null) return intakeType;
            }

            if (isPraktijkhuis)
            {
                var praktijkhuisType = allTypes.FirstOrDefault(t => t.Naam.Contains("Praktijkhuis", StringComparison.OrdinalIgnoreCase));
                if (praktijkhuisType != null) return praktijkhuisType;
            }

            // 3. Default fallback to Consultatie
            var consultatieType = allTypes.FirstOrDefault(t => string.Equals(t.Naam.Trim(), "Consultatie", StringComparison.OrdinalIgnoreCase))
                                 ?? allTypes.FirstOrDefault(t => t.Naam.Contains("Consult", StringComparison.OrdinalIgnoreCase))
                                 ?? allTypes.FirstOrDefault(t => !t.Naam.Contains("Intake", StringComparison.OrdinalIgnoreCase))
                                 ?? allTypes.FirstOrDefault();

            return consultatieType;
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
            var summary = ev.Summary ?? "";
            var description = ev.Description ?? "";

            DateOnly geboortedatum = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));
            var dobMatch = Regex.Match(summary + " " + description, @"\(\s*(\d{1,2}[\/\-\.]\d{1,2}[\/\-\.]\d{4})\s*\)");
            if (dobMatch.Success)
            {
                var dobStr = dobMatch.Groups[1].Value.Replace('-', '/').Replace('.', '/');
                if (DateOnly.TryParseExact(dobStr, "dd/MM/yyyy", out var parsedDob))
                {
                    geboortedatum = parsedDob;
                }
            }

            string telefoonnummer = "";
            var descLines = description.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var cleanDescList = new List<string>();
            string patientNameFromDesc = "";

            foreach (var line in descLines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(telefoonnummer) && Regex.IsMatch(trimmed, @"^[\d\s\.\-\+]{8,16}$") && trimmed.Any(char.IsDigit))
                {
                    telefoonnummer = trimmed;
                }
                else if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    if (trimmed.StartsWith("Naam:", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("Patiënt:", StringComparison.OrdinalIgnoreCase) || trimmed.StartsWith("Patient:", StringComparison.OrdinalIgnoreCase))
                    {
                        patientNameFromDesc = Regex.Replace(trimmed, @"^(Naam|Patiënt|Patient):\s*", "", RegexOptions.IgnoreCase).Trim();
                    }
                    cleanDescList.Add(trimmed);
                }
            }

            string candidateName = summary;
            string cleanSummaryCheck = Regex.Replace(summary, @"\b(Praktijkhuis|Consultatie|Consult|Intake|Psycholoog|1ste|2de|3de)\b", "", RegexOptions.IgnoreCase).Trim();

            if (string.IsNullOrWhiteSpace(cleanSummaryCheck))
            {
                if (!string.IsNullOrWhiteSpace(patientNameFromDesc))
                {
                    candidateName = patientNameFromDesc;
                }
                else if (!string.IsNullOrWhiteSpace(defaultDisplayNaam) && !defaultDisplayNaam.Contains("jockman", StringComparison.OrdinalIgnoreCase) && !defaultDisplayNaam.Contains("praktijkhuis", StringComparison.OrdinalIgnoreCase))
                {
                    candidateName = defaultDisplayNaam;
                }
            }

            var cleanName = candidateName;
            if (dobMatch.Success) cleanName = cleanName.Replace(dobMatch.Value, "");
            cleanName = Regex.Replace(cleanName, @"\b(Praktijkhuis|Psycholoog|consultatie|consult|intake|1ste|2de|3de)\b", "", RegexOptions.IgnoreCase).Trim();

            var nameParts = cleanName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string voornaam = "Praktijkhuis";
            string achternaam = "Consultatie";

            if (nameParts.Length == 1)
            {
                voornaam = nameParts[0];
                achternaam = "";
            }
            else if (nameParts.Length >= 2)
            {
                voornaam = nameParts[0];
                achternaam = string.Join(" ", nameParts.Skip(1));
            }

            return (voornaam, achternaam, geboortedatum, telefoonnummer, string.Join("\n", cleanDescList));
        }

        public async Task<List<(DateTime Start, DateTime End)>> GetBusySlotsAsync(DateTime startUtc, DateTime endUtc)
        {
            var busySlots = new List<(DateTime Start, DateTime End)>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var appointments = dbContext.Afspraken
                    .Where(a => a.Starttijd < endUtc && a.Eindtijd > startUtc && a.Status != AfspraakStatus.Geannuleerd)
                    .ToList();

                foreach (var app in appointments)
                {
                    busySlots.Add((app.Starttijd, app.Eindtijd));
                }
            }

            if (_useMock) return busySlots;
            if (_calendarService == null) throw new InvalidOperationException("Google Calendar Service is niet geïnitialiseerd.");

            try
            {
                var request = new FreeBusyRequest
                {
                    TimeMinDateTimeOffset = startUtc,
                    TimeMaxDateTimeOffset = endUtc,
                    Items = new List<FreeBusyRequestItem> { new FreeBusyRequestItem { Id = _calendarId } }
                };

                var response = await _calendarService.Freebusy.Query(request).ExecuteAsync();

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
                _logger.LogError(ex, "Fout bij ophalen FreeBusy van Google Calendar.");
            }

            return busySlots;
        }
    }
}
