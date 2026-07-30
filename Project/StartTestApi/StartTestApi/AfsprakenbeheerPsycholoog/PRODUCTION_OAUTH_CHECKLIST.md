# Production OAuth Setup Checklist & Herinnering

Wanneer de toepassing naar productie gaat (bijvoorbeeld op `https://www.deverstandhouding.be`), moeten de volgende OAuth ontwikkelaarsaccounts en Redirect URI's worden geconfigureerd voor de 4 inlogproviders:

---

## 1. Google OAuth Client Configuration
1. Ga naar de [Google Cloud Console](https://console.cloud.google.com/).
2. Maak een nieuw project of selecteer het bestaande project.
3. Navigeer naar **APIs & Services > Credentials**.
4. Klik op **Create Credentials > OAuth Client ID** (Application type: *Web application*).
5. Voeg bij **Authorized redirect URIs** de volgende URL's toe:
   - `https://www.deverstandhouding.be/signin-google`
   - `https://deverstandhouding.be/signin-google`
6. Sla de gegenereerde `ClientId` en `ClientSecret` op in het `.env` bestand op de VPS:
   - `GOOGLE_CLIENT_ID`
   - `GOOGLE_CLIENT_SECRET`

---

## 2. Microsoft Entra ID (Azure AD / Outlook) Configuration
1. Ga naar de [Microsoft Entra Admin Center](https://entra.microsoft.com/) of [Azure Portal](https://portal.azure.com/).
2. Navigeer naar **Identity > Applications > App registrations**.
3. Registreer een nieuwe applicatie (Supported account types: *Accounts in any organizational directory and personal Microsoft accounts*).
4. Kies platform **Web** en voeg als Redirect URI toe:
   - `https://www.deverstandhouding.be/signin-microsoft`
   - `https://deverstandhouding.be/signin-microsoft`
5. Genereer een nieuw **Client Secret** onder *Certificates & secrets*.

---

## 3. Meta / Facebook App Configuration
1. Ga naar [Meta for Developers](https://developers.facebook.com/).
2. Maak een nieuwe App aan (Type: *Consumer* of *Business*).
3. Voeg de **Facebook Login** product-integratie toe.
4. Voeg onder **Facebook Login > Settings** bij **Valid OAuth Redirect URIs** toe:
   - `https://www.deverstandhouding.be/signin-facebook`
   - `https://deverstandhouding.be/signin-facebook`
5. Kopieer App ID en App Secret uit **Settings > Basic**:
   - `FACEBOOK_APP_ID`
   - `FACEBOOK_APP_SECRET`

---

## 4. Apple Sign In Configuration
1. Log in op het [Apple Developer Account](https://developer.apple.com/account/).
2. Ga naar **Certificates, Identifiers & Profiles > Identifiers**.
3. Maak een **App ID** (met *Sign In with Apple* ingeschakeld) en een **Services ID** aan.
4. Configureer de **Services ID** met de gewenste web-domeinen en voeg als Return URL toe:
   `https://www.deverstandhouding.be/api/auth/external-login-callback`
5. Genereer een private key (.p8) onder **Keys** om de client secret token te ondertekenen.
6. Vul de onderstaande gegevens in de productie-instellingen in:
   - `Authentication__Apple__ClientId` (Services ID)
   - `Authentication__Apple__TeamId`
   - `Authentication__Apple__KeyId`
   - `Authentication__Apple__PrivateKey`

---

## 5. Algemene Productie Aandachtspunten
- **HTTPS Verplicht**: Zowel Google, Microsoft, Facebook als Apple weigeren OAuth redirects over onbeveiligde HTTP in productie.
- **Cookies & SameSite**: In productie moet het domein `www.deverstandhouding.be` gebruikmaken van `SameSite=Lax` en `Secure=true`.
- **User Secrets / Omgevingsvariabelen**: Bewaar secrets NOOIT in `appsettings.json` of in de broncode.
