/**
 * Patient utilities en hulpfuncties.
 */

export interface PatientNameSource {
  id: number;
  volledigeNaam?: string;
  VolledigeNaam?: string;
  voornaam?: string;
  achternaam?: string;
}

/**
 * Bepaalt de weergavenaam van een patiënt consistent.
 */
export function getPatientDisplayName(patient: PatientNameSource): string {
  if (patient.volledigeNaam && patient.volledigeNaam.trim() !== '') {
    return patient.volledigeNaam;
  }
  if (patient.VolledigeNaam && patient.VolledigeNaam.trim() !== '') {
    return patient.VolledigeNaam;
  }
  if (patient.voornaam) {
    const full = `${patient.voornaam} ${patient.achternaam || ''}`.trim();
    if (full) return full;
  }
  return `Patiënt #${patient.id}`;
}
