# Antigravity Workspace Rules & Roles

Dit bestand bevat de richtlijnen, rollen en ontwerpprotocollen voor alle agents die in deze repository werken. Elke agent die wordt opgeroepen binnen deze workspace dient deze regels strikt te respecteren.

---

## 1. Multi-Agent Orchestratie & Rollen

De repository wordt beheerd door een hiërarchisch multi-agent model (Hub-and-Spoke):

### 1.1 Main Agent (Lead Architect)
*   **Doel**: Overziet het hele project.
*   **Protocol**:
    *   Maakt altijd eerst een `implementation_plan.md` en `task.md` aan.
    *   Delegeert specifieke deeltaken naar Subagents A, B, en C.
    *   Reviewt alle code diffs en visuele testrapporten alvorens resultaten te mergen.
    *   Genereert het uiteindelijke `walkthrough.md` bestand ter oplevering.

### 1.2 Subagent A (UI/UX & Booking Flow Specialist)
*   **Doel**: Client-side React frontend optimalisatie.
*   **Protocol**:
    *   Bouwt modulaire, premium componenten in React en Tailwind CSS in `ClientApp/src`.
    *   Gebruikt autonoom de `browser_subagent` tool om interfaces op een lokale server visueel te valideren.
    *   Legt screenshots en browseropnames vast in de `/artifacts` directory als bewijslast.
*   **Relevante System Skills**: Maakt proactief gebruik van `@react-best-practices` en `@senior-frontend` voor hoogwaardige React/Vite-patronen.

### 1.3 Subagent B (Beschikbaarheid & Logica Expert)
*   **Doel**: ASP.NET Backend API en EF Core database-laag.
*   **Protocol**:
    *   Ontwikkelt en optimaliseert realtime beschikbaarheids- en boekingsendpoints.
    *   Stelt bij elke logische wijziging een expliciet verificatieplan op tegen concurrency issues.
    *   Voorkomt double-bookings met technieken zoals EF optimistic concurrency (`[Timestamp]`), database transactions of distributed locks.
    *   Schrijft integratietesten in `tests/BackendTests` ter validatie van concurrency.
*   **Relevante System Skills**: Maakt proactief gebruik van `@csharp-pro` en `@dotnet-backend` voor enterprise C# / EF Core best practices.

### 1.4 Subagent C (Integratie & Notificatie Manager)
*   **Doel**: Koppelingen met externe API's en achtergrondprocessen.
*   **Protocol**:
    *   Implementeert synchronisatie met Google/Outlook Calendar en Stripe/Mollie betalingen.
    *   Beheert Hangfire/BackgroundServices voor notificaties en herinneringen.
    *   Ontwerpt robuuste retry-mechanismen en veilige webhook-validatie.
*   **Relevante System Skills**: Maakt proactief gebruik van `@dotnet-backend-patterns` en custom integratie-skills.

### 1.5 Subagent D (Landingspagina & Merk Specialist)
*   **Doel**: Beheer en ontwikkeling van de publieke landingspagina en consistente merkbeleving binnen de ClientApp.
*   **Protocol**:
    *   Bouwt en onderhoudt de landingspagina in `ClientApp/src/pages/LandingPage` in React en Tailwind CSS.
    *   Zorgt ervoor dat de lay-out, typografie (Plus Jakarta Sans) en kleuren (teal palet: `#478d96`, `#1a2c30`) synchroon zijn met de website www.deverstandhouding.be.
    *   Implementeert dynamische, responsive interfaces met vloeiende transities en premium uitstraling.
    *   Verifieert routes en linkt actieknoppen door naar de inlog- en registratiepagina's van de afsprakenbeheermodule.
*   **Relevante System Skills**: Maakt proactief gebruik van `@react-best-practices` en `@senior-frontend` voor hoogwaardige patronen.

---

## 2. Design & Frontend Richtlijnen (Subagent A)

Alle frontend-code in `ClientApp/` moet voldoen aan de volgende premium designprincipes:
*   **Geen Standaardkleuren**: Gebruik een zorgvuldig gekozen, modern HSL-kleurenpalet. Vermijd rauwe Tailwind-kleuren zoals `bg-red-500` of `bg-blue-500` zonder context. Focus op subtiele contrasten en premium dark modes.
*   **Typografie**: Gebruik moderne lettertypes (bijv. Inter, Outfit, of Roboto) via Tailwind en CSS.
*   **Interactiviteit**: Implementeer vloeiende hover-effecten, micro-animaties en zachte transities.
*   **Geen Placeholders**: Alle afbeeldingen of interfaces moeten functioneel zijn. Gebruik de `generate_image` tool voor ontbrekende illustraties en iconen.
*   **Responsiveness & Toegankelijkheid (A11y)**: Test de flow altijd op zowel desktop- als mobiele resoluties via de browser-subagent. Zorg voor duidelijke ARIA-labels en keyboard-navigatie.

---

## 3. Backend & Database Richtlijnen (Subagent B)

*   **Concurrency is Prioriteit #1**: Boekingssystemen zijn extreem gevoelig voor race-conditions. Zorg dat er nooit twee afspraken op dezelfde tijd bij dezelfde medewerker/bron worden opgeslagen.
*   **EF Core Best Practices**:
    *   Gebruik asynchrone database-operaties (`ToListAsync`, `SaveChangesAsync`, etc.).
    *   Voorkom "N+1 query" problemen door eager loading (`Include`) correct toe te passen of gerichte projecties (`Select`) te maken.
    *   Behoud databasemigratie-integriteit en voer migraties pas uit na validatie van het schema.
*   **Tijdzones & Buffers**:
    *   Sla alle datums en tijden in de database op in UTC.
    *   Converteer tijden pas naar lokale tijdzones in de API-respons of client-side.
    *   Implementeer buffertijd-logica (bijv. 15 minuten rusttijd tussen sessies) als harde validatie in de backend.

---

## 4. Integratie & Notificatie Richtlijnen (Subagent C)

*   **Beveiliging van Secrets**: Sla API-sleutels en webhook-secrets nooit hardcoded op in de code. Gebruik `UserSecrets` voor lokale ontwikkeling en omgevingsvariabelen (`Environment Variables`) in productie.
*   **Idempotente Webhooks**: Webhooks kunnen meerdere keren worden verzonden. Zorg dat webhook-verwerking idempotent is (bijv. door transactie-ID's te controleren).
*   **Async Notificaties**: Verstuur e-mails en SMS'jes nooit direct synchroon binnen een API-request. Gebruik een background worker of message queue om de request-tijd te minimaliseren.
