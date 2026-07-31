---
name: social-auth-integration
description: Richtlijnen, protocollen en code-voorbeelden voor het integreren en verifiëren van Google, Facebook, Microsoft en Apple OAuth2 / OIDC logins in ASP.NET Core backend en React frontend.
---

# Social Authentication & Identity Integration Protocol

Deze skill biedt richtlijnen en protocollen voor **Subagent F (Social Auth & Identity Integration Specialist)** bij het opzetten, beheren en uitbreiden van sociale inlogmethoden (Google, Facebook, Microsoft, Apple) binnen het afsprakenbeheersysteem van Praktijk De Verstandhouding.

---

## 1. OAuth2 / OpenID Connect Architectuur Overzicht

Het systeem gebruikt ASP.NET Core Identity gecombineerd met externe OAuth2/OIDC providers:

1. **Client Initiatie**:
   - De React frontend stelt knoppen beschikbaar voor Google, Facebook, Microsoft en Apple.
   - Bij het klikken op een knop wordt de browser geredirect naar:
     `GET /api/auth/external-login?provider={Google|Microsoft|Facebook|Apple}&returnUrl={ClientCallbackUrl}`

2. **Provider Redirect & Challenge**:
   - `AuthController.cs` stuurt een Challenge uit naar de relevante authentication scheme.
   - De gebruiker authenticeert bij de provider en geeft toestemming voor e-mail & profiel.

3. **Backend Callback & User Mapping**:
   - De provider redirect terug naar `/api/auth/external-login-callback`.
   - `SignInManager.GetExternalLoginInfoAsync()` haalt de claims op.
   - Indien het e-mailadres bekend is:
     - Koppelt het externe account (`UserManager.AddLoginAsync`).
     - Maakt zo nodig een `ApplicationUser` aan als deze nog niet bestaat met `EmailConfirmed = true`.
     - Logt de gebruiker in via cookie / JWT session.
   - Stuurt de browser terug naar de React client callback URL (`/external-auth-callback?status=success`).

---

## 2. Configuratie & Secrets Beheer

Geen enkele Client Key, Client Secret of Private Key mag hardcoded in de broncode of `appsettings.json` staan.

### Lokale Ontwikkeling (`UserSecrets`)
Sla OAuth credentials op in ASP.NET Core User Secrets:
```json
{
  "Authentication": {
    "Google": {
      "ClientId": "your-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-google-client-secret"
    },
    "Microsoft": {
      "ClientId": "your-microsoft-app-id",
      "ClientSecret": "your-microsoft-client-secret"
    },
    "Facebook": {
      "AppId": "your-facebook-app-id",
      "AppSecret": "your-facebook-app-secret"
    },
    "Apple": {
      "ClientId": "be.deverstandhouding.client",
      "TeamId": "YOUR_TEAM_ID",
      "KeyId": "YOUR_KEY_ID",
      "PrivateKey": "YOUR_PRIVATE_KEY"
    }
  }
}
```

### Productie
Gebruik omgevingsvariabelen in de deployment container/server:
- `Authentication__Google__ClientId`
- `Authentication__Google__ClientSecret`
- `Authentication__Microsoft__ClientId`
- `Authentication__Microsoft__ClientSecret`
- `Authentication__Facebook__AppId`
- `Authentication__Facebook__AppSecret`
- `Authentication__Apple__ClientId`

---

## 3. Apple Sign-In Specifieke Overwegingen

- **Hide My Email**: Apple kan anonieme e-mailadressen genereren (`@privaterelay.appleid.com`). Zorg dat de backend hier correct mee omgaat.
- **Form Post Response**: Apple stuurt OAuth antwoorden via HTTP `POST` naar de redirect URI. Zorg dat de callback endpoint `[HttpPost]` en `[HttpGet]` ondersteunt waar nodig.
- **Client Secret Generation**: Apple vereist een ondertekende JWT als Client Secret (opgebouwd met de Apple Private Key `.p8`).

---

## 4. Frontend Callback Handling (`ClientApp`)

In `ClientApp/src/pages/ExternalAuthCallback.tsx`:
- Vang de query parameters op (`status`, `message`).
- Bij `status=success`: roep `/api/auth/me` aan om de ingelogde gebruiker (claims, rol, patiënt ID) in de React Auth Context bij te werken.
- Redirect de gebruiker naar het patiëntenportaal (`/portal`) of psychologendashboard (`/dashboard`).
- Bij `status=failed`: toon een duidelijke, vriendelijke foutmelding en geef de mogelijkheid om het opnieuw te proberen of in te loggen via e-mail/wachtwoord.

---

## 5. Verificatie & Test Protocol

1. **Build Validatie**:
   ```bash
   dotnet build Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/AfsprakenbeheerPsycholoog.csproj
   ```
2. **Browser Flow Testing**:
   - Gebruik de `browser_subagent` om het inlogscherm te navigeren.
   - Verifieer dat alle social login knoppen aanwezig zijn, correct gestyled zijn volgens de merkidentiteit van De Verstandhouding, en de juiste OAuth endpoints aanroepen.
