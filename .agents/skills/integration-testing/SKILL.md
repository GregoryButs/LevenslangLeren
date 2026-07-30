---
name: integration-testing
description: Valideer en test externe API-koppelingen, e-mail/SMS-reminders en betalings-webhooks.
---

# Integration & Notification Testing Skill

Deze skill helpt de agent (Subagent C) bij het veilig integreren en testen van externe API's (zoals Google Calendar, Outlook Calendar, Stripe, Mollie, SendGrid en Twilio).

## Stappen voor Integratietests:

1.  **Omgevingsvariabelen Auditeren**:
    *   Controleer of er geen gevoelige API-sleutels hardcoded in de code staan.
    *   Controleer `appsettings.json` en valideer dat secrets geladen worden via `IConfiguration` of omgevingsvariabelen.

2.  **API Mocking & Sandboxing**:
    *   Gebruik bij voorkeur sandbox-omgevingen (bijv. Stripe Test-modus met de `sk_test_...` sleutel).
    *   Schrijf unit- of integratietests waarbij de externe netwerkaanroepen gemockt worden (met `HttpClient` mocks of wrapper-interfaces) om te testen hoe de applicatie reageert op API-timeouts of serverfouten (5xx) van de externe provider.

3.  **Webhook-handtekening Validatie**:
    *   Wanneer Stripe of Mollie een statusupdate stuurt via een webhook, moet de controller de handtekening (Signature) valideren.
    *   Test de webhook-endpoint met een gesimuleerde payload en controleer of ongeldige handtekeningen correct worden afgewezen met een `400 Bad Request`.

4.  **Achtergrondtaken & Hangfire Verificatie**:
    *   Zorg dat reminders niet blokkerend zijn. Controleer of de achtergrondtaak correct wordt geregistreerd in Hangfire of de ASP.NET Core `IHostedService`.
    *   Valideer dat bij annulering van een afspraak de bijbehorende e-mail/SMS-reminders ook direct uit de wachtrij worden verwijderd.
