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
  secundairEmail?: string | null;
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

export interface SettingsData {
  googleCalendarId?: string;
  maandagActief?: boolean;
  maandagStart?: string;
  maandagEinde?: string;
  maandag2Actief?: boolean;
  maandagStart2?: string;
  maandagEinde2?: string;

  dinsdagActief?: boolean;
  dinsdagStart?: string;
  dinsdagEinde?: string;
  dinsdag2Actief?: boolean;
  dinsdagStart2?: string;
  dinsdagEinde2?: string;

  woensdagActief?: boolean;
  woensdagStart?: string;
  woensdagEinde?: string;
  woensdag2Actief?: boolean;
  woensdagStart2?: string;
  woensdagEinde2?: string;

  donderdagActief?: boolean;
  donderdagStart?: string;
  donderdagEinde?: string;
  donderdag2Actief?: boolean;
  donderdagStart2?: string;
  donderdagEinde2?: string;

  vrijdagActief?: boolean;
  vrijdagStart?: string;
  vrijdagEinde?: string;
  vrijdag2Actief?: boolean;
  vrijdagStart2?: string;
  vrijdagEinde2?: string;

  zaterdagActief?: boolean;
  zaterdagStart?: string;
  zaterdagEinde?: string;
  zaterdag2Actief?: boolean;
  zaterdagStart2?: string;
  zaterdagEinde2?: string;

  zondagActief?: boolean;
  zondagStart?: string;
  zondagEinde?: string;
  zondag2Actief?: boolean;
  zondagStart2?: string;
  zondagEinde2?: string;

  slotDuurMinuten?: number;
  bufferMinuten?: number;
  locatiePraktijk?: boolean;
  locatieGoogleMeet?: boolean;
  locatieTelefoon?: boolean;
  minimaalVoorafUren?: number;
  maximaleToekomstDagen?: number;
}
