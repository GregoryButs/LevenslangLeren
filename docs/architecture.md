# Boekingswebsite Architectuur

Dit document beschrijft de technische architectuur van het boekingsplatform en dient als referentiepunt voor de Knowledge Base (KB) binnen de Antigravity IDE.

---

## 1. Systeem Overzicht

Het platform bestaat uit een ontkoppelde client-server structuur:

1.  **Frontend (React ClientApp)**:
    *   **Locatie**: `Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/ClientApp`
    *   **Tech Stack**: React 18+, TypeScript, Vite, Tailwind CSS.
    *   **Functionaliteit**: Modulaire, stapsgewijze boekingswizard (service-selectie, datum/tijd-selectie, patiëntgegevens en betaling). Bevat tevens een psychologendashboard voor agendabeheer.

2.  **Backend (ASP.NET Core Web API)**:
    *   **Locatie**: `Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/`
    *   **Tech Stack**: .NET 8, ASP.NET Core, Entity Framework Core (EF Core) met een SQLite database (`Afsprakenbeheer.db`) voor lokale ontwikkeling.
    *   **Functionaliteit**: API-endpoints voor authenticatie, patiëntenbeheer, afspraken en beschikbaarheidsberekening.

---

## 2. Boekingslogica & Concurrency Control

Het voorkomen van dubbele boekingen en het correct berekenen van beschikbaarheid is de belangrijkste backend-taak.

### 2.1 Tijdslot Berekening (Availability Engine)
*   **Dienstroosters**: Medewerkers hebben flexibele werktijden en pauzes per weekdag.
*   **Buffertijden**: Tussen afspraken kan een optionele buffertijd (rusttijd) van bijv. 15 minuten ingesteld worden. Deze buffertijd moet bij de afspraakduur worden opgeteld om te bepalen of een volgend slot beschikbaar is.
*   **Tijdzones**: Alle tijden worden in UTC opgeslagen en gecommuniceerd. De frontend converteert dit naar de lokale tijdzone van de cliënt.

### 2.2 Concurrency- en Lock-strategie
Om race-conditions (waarbij twee cliënten gelijktijdig hetzelfde slot proberen te reserveren) te voorkomen, hanteren we:
1.  **Optimistic Concurrency**:
    *   De `Afspraak` entiteit bevat een `RowVersion` timestamp token.
    *   Als een record tussentijds is gewijzigd door een andere transactie, gooit EF Core een `DbUpdateConcurrencyException`.
2.  **Database Transacties**:
    *   Het schrijven van een afspraak gebeurt binnen een Serializable database-transactie om er zeker van te zijn dat de beschikbaarheidscheck en de schrijfactie atomair zijn.

---

## 3. Externe Integraties

*   **Google & Outlook Calendar**: Synchroniseert gemaakte afspraken tweezijdig. Nieuwe afspraken verschijnen in de agenda van de psycholoog, en blokkades in die agenda maken het tijdstip onbeschikbaar in de web-flow.
*   **Betalingen (Stripe / Mollie)**: Cliënten kunnen optioneel direct betalen tijdens het boeken. De afspraak status blijft 'In afwachting van betaling' totdat de betaling via een webhook is bevestigd.
*   **Notificaties (Twilio & SendGrid)**: Automatische SMS en e-mail reminders worden asynchroon via Hangfire ingepland om de API-responstijd snel te houden.
