---
name: backend-concurrency-check
description: Controleer en voorkom database race-conditions en double-bookings in de C# EF Core API.
---

# Backend Concurrency Check Skill

Deze skill helpt de agent (Subagent B) om database-transacties en concurrency tokens in Entity Framework Core te analyseren, te schrijven en te testen om gelijktijdige boekingspogingen uit te sluiten.

## Richtlijnen voor Concurrency Control:

1.  **DbContext & Entity Auditing**:
    *   Controleer of de `Afspraak` (Appointment) entiteit is uitgerust met een concurrency token. In EF Core kan dit met een byte array `RowVersion` gedefinieerd in het model:
        ```csharp
        [Timestamp]
        public byte[] RowVersion { get; set; }
        ```
    *   Controleer in de `DbContext` of de configuratie dit token correct indexeert.

2.  **Transactiebeheer in de Service-laag**:
    *   Bij het opslaan van een nieuwe boeking moet de service controleren of het gekozen tijdslot nog steeds vrij is.
    *   Gebruik een expliciete database transactie met een passend isolatieniveau (`Serializable` of `RepeatableRead`):
        ```csharp
        using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        ```
    *   Vang een `DbUpdateConcurrencyException` op en handel dit netjes af door de gebruiker te melden dat het slot inmiddels bezet is, in plaats van een algemene serverfout (500) terug te sturen.

3.  **Concurrency Simulatortest**:
    *   Schrijf een integratietest in `tests/BackendTests` die `Task.WhenAll` gebruikt om 5 parallelle requests te sturen die op exact hetzelfde tijdstip proberen te boeken.
    *   Valideer dat er exact 1 boeking slaagt en de overige 4 netjes worden afgewezen met een duidelijke foutmelding (bijv. 409 Conflict).
