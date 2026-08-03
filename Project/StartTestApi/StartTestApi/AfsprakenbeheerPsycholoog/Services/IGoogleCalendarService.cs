using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    /// <summary>
    /// Service voor de integratie met Google Calendar API.
    /// </summary>
    public interface IGoogleCalendarService
    {
        /// <summary>
        /// Maakt een nieuw event aan in Google Calendar en geeft het unieke Google Event ID en optionele Meet link terug.
        /// </summary>
        Task<(string EventId, string? MeetLink)> CreateEventAsync(DateTime startUtc, DateTime endUtc, int afspraakId, bool createMeetLink = false, string locationDetails = "", string patientNaam = "");

        /// <summary>
        /// Past een bestaand event aan in Google Calendar.
        /// </summary>
        Task UpdateEventAsync(string googleEventId, DateTime startUtc, DateTime endUtc, int afspraakId, string patientNaam = "");

        /// <summary>
        /// Verwijdert een event uit Google Calendar.
        /// </summary>
        Task DeleteEventAsync(string googleEventId);

        /// <summary>
        /// Synchroniseert inkomende wijzigingen (wijzigingen in Google Calendar worden lokaal bijgewerkt).
        /// </summary>
        Task SyncIncomingChangesAsync();

        /// <summary>
        /// Haalt bezette tijdsloten op uit Google Calendar via FreeBusy.
        /// </summary>
        Task<List<(DateTime Start, DateTime End)>> GetBusySlotsAsync(DateTime startUtc, DateTime endUtc);
    }
}
