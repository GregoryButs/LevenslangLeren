/**
 * Datum- en tijd-hulpfuncties voor consistente opmaak binnen de client applicatie.
 */

/**
 * Formatteert een uur getal naar 'HH:00' (bijv. 9 -> '09:00')
 */
export function formatHourString(hour: number): string {
  return `${hour.toString().padStart(2, '0')}:00`;
}

/**
 * Converteert een datum en optioneel uur/minuut naar een ISO-achtige datetime-local string (YYYY-MM-DDTHH:mm)
 */
export function formatDateTimeInput(date?: Date, hour?: number, minute: number = 0): string {
  const d = date ? new Date(date) : new Date();
  if (hour !== undefined) {
    const h = Math.floor(hour);
    const m = minute !== undefined ? minute : Math.round((hour % 1) * 60);
    d.setHours(h, m, 0, 0);
  } else if (!date) {
    d.setHours(d.getHours() + 1, 0, 0, 0);
  }
  
  const year = d.getFullYear();
  const month = (d.getMonth() + 1).toString().padStart(2, '0');
  const day = d.getDate().toString().padStart(2, '0');
  const hours = d.getHours().toString().padStart(2, '0');
  const minutes = d.getMinutes().toString().padStart(2, '0');
  
  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

/**
 * Converteert een positionele slot (bijv. 12.5) naar een tijd-string ('12:30')
 */
export function formatSlotTimeString(slot: number): string {
  const h = Math.floor(slot);
  const m = Math.round((slot % 1) * 60);
  return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`;
}

/**
 * Geef een leesbare datumweergave in het Nederlands (bijv. 'ma 15 mei')
 */
export function formatShortDutchDate(date: Date): string {
  return date.toLocaleDateString('nl-NL', { weekday: 'short', day: 'numeric', month: 'short' });
}

