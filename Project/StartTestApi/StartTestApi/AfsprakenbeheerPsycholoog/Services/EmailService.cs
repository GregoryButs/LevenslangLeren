using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

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

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _senderAddress = configuration["Email:SenderAddress"] ?? "inge@deverstandhouding.be";
            _senderName = configuration["Email:SenderName"] ?? "De Verstandhouding - Inge Debast";
            _smtpServer = configuration["Email:SmtpServer"] ?? "smtp-auth.mailprotect.be";
            _smtpPort = int.TryParse(configuration["Email:SmtpPort"], out var port) ? port : 587;
            _smtpUser = configuration["Email:SmtpUser"] ?? "";
            _smtpPassword = configuration["Email:SmtpPassword"] ?? "";

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
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(_senderAddress, _senderName);
                    mail.To.Add(new MailAddress(toEmail));
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(attachmentContent) && !string.IsNullOrEmpty(attachmentFilename))
                    {
                        var mediaType = new System.Net.Mime.ContentType("text/calendar");
                        mediaType.Parameters.Add("method", "REQUEST");
                        mediaType.Parameters.Add("name", attachmentFilename);

                        var attachment = Attachment.CreateAttachmentFromString(attachmentContent, mediaType);
                        attachment.ContentDisposition.FileName = attachmentFilename;
                        mail.Attachments.Add(attachment);
                    }

                    using (var smtp = new SmtpClient(_smtpServer, _smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(_smtpUser, _smtpPassword);
                        smtp.EnableSsl = true;
                        smtp.Timeout = 4000;
                        await smtp.SendMailAsync(mail);
                    }
                }
                _logger.LogInformation($"Email succesvol verzonden naar {toEmail} met onderwerp: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fout bij versturen e-mail naar {toEmail}.");
                // In testomgevingen willen we niet dat de app crasht als mailen faalt, dus we loggen enkel de fout.
            }
        }

        public string BuildIcsContent(DateTime startUtc, DateTime endUtc, string afspraakType, int afspraakId, string? opmerkingen)
        {
            var startUtcStr = startUtc.ToString("yyyyMMddTHHmmssZ");
            var eindUtcStr = endUtc.ToString("yyyyMMddTHHmmssZ");
            var nuUtcStr = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");

            var icsBuilder = new System.Text.StringBuilder();
            icsBuilder.AppendLine("BEGIN:VCALENDAR");
            icsBuilder.AppendLine("VERSION:2.0");
            icsBuilder.AppendLine("PRODID:-//De Verstandhouding//Afsprakenbeheer//NL");
            icsBuilder.AppendLine("BEGIN:VEVENT");
            icsBuilder.AppendLine($"UID:afspraak-{afspraakId}@deverstandhouding.be");
            icsBuilder.AppendLine($"DTSTAMP:{nuUtcStr}");
            icsBuilder.AppendLine($"DTSTART:{startUtcStr}");
            icsBuilder.AppendLine($"DTEND:{eindUtcStr}");
            icsBuilder.AppendLine($"SUMMARY:{afspraakType} - De Verstandhouding");
            
            var description = $"Afspraak bij praktijk De Verstandhouding.\\n\\nType sessie: {afspraakType}";
            if (!string.IsNullOrEmpty(opmerkingen))
            {
                description += $"\\nOpmerkingen: {opmerkingen.Replace("\r\n", "\\n").Replace("\n", "\\n")}";
            }
            icsBuilder.AppendLine($"DESCRIPTION:{description}");
            icsBuilder.AppendLine("LOCATION:De Verstandhouding");
            icsBuilder.AppendLine("END:VEVENT");
            icsBuilder.AppendLine("END:VCALENDAR");

            return icsBuilder.ToString();
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

            var icsContent = BuildIcsContent(startUtc, endUtc, afspraakType, afspraakId, opmerkingen);
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
