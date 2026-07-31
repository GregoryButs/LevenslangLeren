/**
 * Formulier- en gegevenstype validatiehulpmiddelen.
 */

export function isValidEmail(email: string): boolean {
  if (!email || !email.trim()) return false;
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email.trim());
}

export function isValidPhone(phone: string): boolean {
  if (!phone || !phone.trim()) return true; // optioneel
  const phoneRegex = /^[\d\s+\-/().]{8,20}$/;
  return phoneRegex.test(phone.trim());
}

export function isValidRijksregisternummer(rrn: string): boolean {
  if (!rrn) return true; // optioneel
  const cleaned = rrn.replace(/[^\d]/g, '');
  return cleaned.length === 11;
}
