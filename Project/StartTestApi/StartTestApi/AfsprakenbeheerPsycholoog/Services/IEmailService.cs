using System;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Service voor het versturen van e-mails vanuit de praktijk.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Verstuurt een algemene e-mail met optionele kalenderbijlage.
        /// </summary>
        Task SendEmailAsync(string toEmail, string subject, string body, string? attachmentContent = null, string? attachmentFilename = null);

        /// <summary>
        /// Verstuurt een bevestiging van een nieuwe afspraak inclusief .ics kalenderbijlage.
        /// </summary>
        Task SendConfirmationEmailAsync(string toEmail, string patientNaam, DateTime startUtc, DateTime endUtc, string afspraakType, int afspraakId, string? opmerkingen = null, string? googleMeetLink = null);

        /// <summary>
        /// Verstuurt een e-mail naar de patiënt wanneer de psycholoog het account heeft goedgekeurd/gekoppeld.
        /// </summary>
        Task SendAccountApprovalEmailAsync(string toEmail, string patientNaam);

        /// <summary>
        /// Verstuurt een welkomst e-mail naar de patiënt nadat de psycholoog een nieuw dossier heeft aangemaakt.
        /// </summary>
        Task SendPatientWelcomeEmailAsync(string toEmail, string patientNaam);

        /// <summary>
        /// Genereert de string-inhoud van een ICS kalenderbestand volgens RFC 5545 (CRLF).
        /// </summary>
        string BuildIcsContent(DateTime startUtc, DateTime endUtc, string afspraakType, int afspraakId, string? opmerkingen, string patientNaam = "Patiënt", string toEmail = "", string? googleMeetLink = null);

        /// <summary>
        /// Verstuurt een bericht dat een afspraak is verzet.
        /// </summary>
        Task SendRescheduleEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType);

        /// <summary>
        /// Verstuurt een bericht dat een afspraak is geannuleerd.
        /// </summary>
        Task SendCancellationEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType);

        /// <summary>
        /// Verstuurt een herinneringsmail voor een aankomende afspraak (24u op voorhand).
        /// </summary>
        Task SendReminderEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType, int afspraakId);

        /// <summary>
        /// Verstuurt een herinneringsmail 1 week voor een aankomende afspraak.
        /// </summary>
        Task SendWeeklyReminderEmailAsync(string toEmail, string patientNaam, DateTime startUtc, string afspraakType, int afspraakId);
    }
}
