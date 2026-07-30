using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly bool _useMock;
        private readonly string _senderAddress;
        private readonly string _senderName;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPassword;
        private readonly string _resendApiKey;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _senderAddress = configuration["Email:SenderAddress"] ?? "inge@deverstandhouding.be";
            _senderName = configuration["Email:SenderName"] ?? "De Verstandhouding - Inge Debast";
            _smtpServer = configuration["Email:SmtpServer"] ?? "smtp.mailprotect.be";
            _smtpPort = int.TryParse(configuration["Email:SmtpPort"], out var port) ? port : 465;
            _smtpUser = configuration["Email:SmtpUser"] ?? "";
            _smtpPassword = configuration["Email:SmtpPassword"] ?? "";
            _resendApiKey = configuration["Email:ResendApiKey"] ?? Environment.GetEnvironmentVariable("RESEND_API_KEY") ?? "";

            _useMock = string.Equals(configuration["Email:UseMock"], "true", StringComparison.OrdinalIgnoreCase) ||
                       string.IsNullOrEmpty(_smtpUser) ||
                       string.IsNullOrEmpty(_smtpPassword) ||
                       _smtpPassword == "YOUR_EMAIL_PASSWORD";

            if (_useMock)
            {
                _logger.LogInformation("Email Service is geïnitialiseerd in MOCK/STILLE modus (geen geldige SMTP-sleutel ingesteld).");
            }
            else
            {
                _logger.LogInformation($"Email Service geïnitialiseerd in PRODUCTIE modus (SMTP: {_smtpServer}:{_smtpPort} als {_smtpUser}).");
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, string? attachmentContent = null, string? attachmentFilename = null)
        {
            if (_useMock)
            {
                var attachmentInfo = !string.IsNullOrEmpty(attachmentFilename) ? $"\nBijlage: {attachmentFilename} (ICS inhoud meegestuurd)" : "";
                _logger.LogInformation($"[MOCK EMAIL SENT]\nVan: {_senderName} <{_senderAddress}>\nAan: {toEmail}\nOnderwerp: {subject}\nInhoud: {body}{attachmentInfo}\n");
                return;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_senderName, _senderAddress));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = body;

                if (!string.IsNullOrEmpty(attachmentContent) && !string.IsNullOrEmpty(attachmentFilename))
                {
                    var mediaType = new ContentType("text", "calendar");
                    mediaType.Parameters.Add("method", "REQUEST");
                    mediaType.Parameters.Add("name", attachmentFilename);

                    builder.Attachments.Add(attachmentFilename, System.Text.Encoding.UTF8.GetBytes(attachmentContent), mediaType);
                }

                message.Body = builder.ToMessageBody();

                int[] candidatePorts = new int[] { 2525, _smtpPort, 587, 465, 25 };
                bool connected = false;

                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Timeout = 6000; // 6 seconden per poort-poging

                    string[] candidateServers = new string[] { "smtp-auth.mailprotect.be", _smtpServer, "smtp.mailprotect.be" };

                    foreach (var server in candidateServers.Distinct())
                    {
                        foreach (var port in candidatePorts.Distinct())
                        {
                            try
                            {
                                var options = port == 465 
                                    ? SecureSocketOptions.SslOnConnect 
                                    : SecureSocketOptions.StartTlsWhenAvailable;

                                _logger.LogInformation("SMTP Verbinding proberen op {Server}:{Port}...", server, port);
                                await client.ConnectAsync(server, port, options);
                                connected = true;
                                break;
                            }
                            catch (Exception portEx)
                            {
                                _logger.LogWarning("Poort {Port} op {Server} niet bereikbaar ({Message}).", port, server, portEx.Message);
                            }
                        }

                        if (connected) break;
                    }

                    if (!connected)
                    {
                        _logger.LogWarning("Geen enkele SMTP-poort bereikbaar op de VPS (netwerkblokkade). Proberen te verzonden via HTTPS API...");
                        if (!string.IsNullOrEmpty(_resendApiKey))
                        {
                            await SendViaHttpsApiAsync(toEmail, subject, body);
                            return;
                        }
                        _logger.LogError("Uitgaand SMTP verkeer is geblokkeerd door VPS provider. Gebruik een HTTPS API-key (RESEND_API_KEY) in .env om via poort 443 HTTPS te mailen.");
                        return;
                    }

                    await client.AuthenticateAsync(_smtpUser, _smtpPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation($"Email succesvol verzonden naar {toEmail} met onderwerp: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij versturen e-mail naar {ToEmail}: {ExceptionMessage}", toEmail, ex.ToString());
            }
        }

        private async Task SendViaHttpsApiAsync(string toEmail, string subject, string body)
        {
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _resendApiKey);
                    var payload = new
                    {
                        from = $"{_senderName} <{_senderAddress}>",
                        to = new[] { toEmail },
                        subject = subject,
                        html = body
                    };

                    var jsonContent = new System.Net.Http.StringContent(System.Text.Json.JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("https://api.resend.com/emails", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Email succesvol verzonden via HTTPS API naar {toEmail}");
                    }
                    else
                    {
                        var errorStr = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Fout bij HTTPS API mailverzending naar {ToEmail}: {Error}", toEmail, errorStr);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fout bij verzenden e-mail via HTTPS API naar {ToEmail}", toEmail);
            }
        }

        public string BuildIcsContent(DateTime startUtc, DateTime endUtc, string afspraakType, int afspraakId, string? opmerkingen, string patientNaam = "Patiënt", string toEmail = "")
        {
            var startUtcStr = startUtc.ToString("yyyyMMddTHHmmssZ");
            var eindUtcStr = endUtc.ToString("yyyyMMddTHHmmssZ");
            var nuUtcStr = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

            var cleanType = afspraakType.Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,");
            var cleanNotes = string.IsNullOrEmpty(opmerkingen) ? "" : opmerkingen.Replace("\r\n", " ").Replace("\n", " ").Replace("\\", "\\\\").Replace(";", "\\;").Replace(",", "\\,");
            var description = $"Afspraak bij praktijk De Verstandhouding.\\nType sessie: {cleanType}" + (string.IsNullOrEmpty(cleanNotes) ? "" : $"\\nOpmerkingen: {cleanNotes}");
            var attendeeMail = string.IsNullOrEmpty(toEmail) ? "patient@deverstandhouding.be" : toEmail;
            var cleanPatient = patientNaam.Replace("\"", "'");

            var sb = new System.Text.StringBuilder();
            sb.Append("BEGIN:VCALENDAR\r\n");
            sb.Append("VERSION:2.0\r\n");
            sb.Append("PRODID:-//De Verstandhouding//NONSGML Afsprakenbeheer//NL\r\n");
            sb.Append("CALSCALE:GREGORIAN\r\n");
            sb.Append("METHOD:REQUEST\r\n");
            sb.Append("BEGIN:VEVENT\r\n");
            sb.Append($"UID:afspraak-{afspraakId}@deverstandhouding.be\r\n");
            sb.Append($"DTSTAMP:{nuUtcStr}\r\n");
            sb.Append($"DTSTART:{startUtcStr}\r\n");
            sb.Append($"DTEND:{eindUtcStr}\r\n");
            sb.Append($"SUMMARY:{cleanType} - De Verstandhouding\r\n");
            sb.Append($"DESCRIPTION:{description}\r\n");
            sb.Append("LOCATION:De Verstandhouding\r\n");
            sb.Append("ORGANIZER;CN=\"De Verstandhouding\":mailto:inge@deverstandhouding.be\r\n");
            sb.Append($"ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=ACCEPTED;CN=\"{cleanPatient}\":mailto:{attendeeMail}\r\n");
            sb.Append("STATUS:CONFIRMED\r\n");
            sb.Append("SEQUENCE:0\r\n");
            sb.Append("END:VEVENT\r\n");
            sb.Append("END:VCALENDAR\r\n");

            return sb.ToString();
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string patientNaam, DateTime startUtc, DateTime endUtc, string afspraakType, int afspraakId, string? opmerkingen = null)
        {
            // Timezone handling: In de mail tonen we de Belgische/Nederlandse tijd
            var localTime = TranslatieNaarLokaleTijd(startUtc);
            var datumString = localTime.ToString("dd-MM-yyyy 'om' HH:mm");

            var subject = "Bevestiging van uw afspraak - De Verstandhouding";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                    <h2 style='color: #0f172a;'>Beste {patientNaam},</h2>
                    <p style='color: #475569;'>Hierbij bevestigen wij uw afspraak bij praktijk <strong>De Verstandhouding</strong>.</p>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <table style='width: 100%; text-align: left;'>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Type sessie:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{afspraakType}</td>
                        </tr>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Datum & Tijd:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{datumString}</td>
                        </tr>
                        <tr>
                            <td style='color: #64748b;'>Locatie:</td>
                            <td style='font-weight: bold; color: #0f172a;'>Online of Praktijk (zie afspraakdetails)</td>
                        </tr>
                    </table>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <p style='color: #475569; font-size: 14px;'>Wenst u de afspraak te verzetten of te annuleren? Dit kan tot 24 uur van tevoren via het patiëntenportaal.</p>
                    <p style='color: #94a3b8; font-size: 12px; margin-top: 40px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                </div>";

            var icsContent = BuildIcsContent(startUtc, endUtc, afspraakType, afspraakId, opmerkingen, patientNaam, toEmail);
            var filename = $"afspraak-{afspraakType.ToLower().Replace(" ", "-")}.ics";

            await SendEmailAsync(toEmail, subject, body, icsContent, filename);
        }

        public async Task SendRescheduleEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType)
        {
            var localTime = TranslatieNaarLokaleTijd(startUtc);
            var datumString = localTime.ToString("dd-MM-yyyy 'om' HH:mm");

            var subject = "Uw afspraak is verzet - De Verstandhouding";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                    <h2 style='color: #0f172a;'>Beste {patientNaam},</h2>
                    <p style='color: #475569;'>Uw afspraak bij <strong>De Verstandhouding</strong> is verzet naar een nieuw tijdstip.</p>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <table style='width: 100%; text-align: left;'>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Type sessie:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{afspraakType}</td>
                        </tr>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Nieuwe datum & Tijd:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{datumString}</td>
                        </tr>
                    </table>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <p style='color: #475569; font-size: 14px;'>De details van de wijziging zijn ook beschikbaar in uw patiëntenportaal.</p>
                    <p style='color: #94a3b8; font-size: 12px; margin-top: 40px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendCancellationEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType)
        {
            var localTime = TranslatieNaarLokaleTijd(startUtc);
            var datumString = localTime.ToString("dd-MM-yyyy 'om' HH:mm");

            var subject = "Annulering van uw afspraak - De Verstandhouding";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                    <h2 style='color: #0f172a;'>Beste {patientNaam},</h2>
                    <p style='color: #475569;'>Hierbij bevestigen wij dat uw afspraak van <strong>{datumString}</strong> ({afspraakType}) is geannuleerd.</p>
                    <p style='color: #475569;'>Indien gewenst kunt u via ons online portaal een nieuwe afspraak inplannen.</p>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <p style='color: #94a3b8; font-size: 12px; margin-top: 40px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendReminderEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType, int afspraakId)
        {
            var localTime = TranslatieNaarLokaleTijd(startUtc);
            var datumString = localTime.ToString("dd-MM-yyyy 'om' HH:mm");

            var subject = "Herinnering: Uw afspraak morgen - De Verstandhouding";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                    <h2 style='color: #0f172a;'>Beste {patientNaam},</h2>
                    <p style='color: #475569;'>Dit is een herinnering voor uw afspraak van morgen bij praktijk <strong>De Verstandhouding</strong>.</p>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <table style='width: 100%; text-align: left;'>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Type sessie:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{afspraakType}</td>
                        </tr>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Datum & Tijd:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{datumString}</td>
                        </tr>
                    </table>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <p style='color: #475569; font-size: 14px;'>Mocht u de afspraak onverhoopt moeten annuleren of verzetten, neem dan direct contact met ons op.</p>
                    <p style='color: #94a3b8; font-size: 12px; margin-top: 40px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendWeeklyReminderEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType, int afspraakId)
        {
            var localTime = TranslatieNaarLokaleTijd(startUtc);
            var datumString = localTime.ToString("dd-MM-yyyy 'om' HH:mm");

            var subject = "Herinnering: Uw afspraak volgende week - De Verstandhouding";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #f1f5f9; border-radius: 12px;'>
                    <h2 style='color: #0f172a;'>Beste {patientNaam},</h2>
                    <p style='color: #475569;'>Dit is een herinnering dat u <strong>volgende week</strong> een afspraak heeft bij praktijk <strong>De Verstandhouding</strong>.</p>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <table style='width: 100%; text-align: left;'>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Type sessie:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{afspraakType}</td>
                        </tr>
                        <tr>
                            <td style='color: #64748b; padding-bottom: 8px;'>Datum & Tijd:</td>
                            <td style='font-weight: bold; color: #0f172a; padding-bottom: 8px;'>{datumString}</td>
                        </tr>
                    </table>
                    <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                    <p style='color: #475569; font-size: 14px;'>Mocht u de afspraak onverhoopt moeten annuleren of verzetten, neem dan tijdig contact met ons op via het patiëntenportaal.</p>
                    <p style='color: #94a3b8; font-size: 12px; margin-top: 40px;'>Met vriendelijke groet,<br>Praktijk De Verstandhouding</p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private DateTime TranslatieNaarLokaleTijd(DateTime utcTime)
        {
            // Omzetten van UTC naar de lokale tijdzone van de praktijk (bijv. West-Europa: CET/CEST)
            try
            {
                TimeZoneInfo localZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                return TimeZoneInfo.ConvertTimeFromUtc(utcTime, localZone);
            }
            catch
            {
                // Fallback naar de lokale tijd van de machine als de timezone ID niet bestaat
                return utcTime.ToLocalTime();
            }
        }
    }
}
