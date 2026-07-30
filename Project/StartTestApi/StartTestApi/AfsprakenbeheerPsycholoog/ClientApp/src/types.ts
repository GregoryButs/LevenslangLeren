export interface User {
  email: string;
  voornaam: string;
  achternaam: string;
  isPsycholoog: boolean;
  patientId: number | null;
}

export interface Patient {
  id: number;
  voornaam: string;
  achternaam: string;
  geboortedatum: string;
  email: string;
  telefoonnummer: string | null;
  dossierNummer: string | null;
  volledigeNaam: string;
  isActief: boolean;
  isGekoppeld?: boolean;
  emotioneleStabiliteit?: number | null;
  afspraken?: Afspraak[];
}

export type AfspraakStatus = 'Gepland' | 'Voltooid' | 'Geannuleerd';

export interface Afspraak {
  id: number;
  patientId: number | null;
  patientNaam: string;
  patientVolledigeNaam?: string;
  patientEmail?: string;
  patientTelefoon?: string;
  typeId: number;
  afspraakTypeNaam: string;
  starttijd: string;
  eindtijd: string;
  status: AfspraakStatus;
  kleurcode: string;
  opmerkingen: string | null;
  reeksId: string | null;
  googleEventId: string | null;
  isHeleDag?: boolean;
}

export interface AfspraakType {
  id: number;
  naam: string;
  standaardDuurMinuten: number;
  kleurcode: string;
  vereistPatient: boolean;
}

export interface Tijdslot {
  tijd: string;
  isBezet: boolean;
  afspraak: Afspraak | null;
  starttijd?: string;
  eindtijd?: string;
}

export interface DagOverzicht {
  datum: string;
  tijdsloten: Tijdslot[];
  minimumNavigatieDatum?: string;
  eerstVolgendeVrijeSlotDatumStr?: string;
  eerstVolgendeVrijeSlotTijd?: string;
}

export interface DagPlanning {
  datum: string;
  weergaveNaam: string;
  afspraken: Afspraak[];
}

export interface WeekOverzicht {
  peilDatum: string;
  startWeek: string;
  eindeWeek: string;
  dagen: { [key: string]: Afspraak[] };
  isPsycholoog: boolean;
}

export interface DashboardData {
  psycholoogNaam: string;
  aantalAfsprakenVandaag: number;
  aantalAfsprakenDezeWeek: number;
  aantalPatienten: number;
  afsprakenVandaag: Afspraak[];
  volgendeAfspraak: Afspraak | null;
  weekOverzicht: WeekOverzicht;
}
