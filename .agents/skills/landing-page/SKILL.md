---
name: landing-page
description: Ontwikkel, test en onderhoud de openbare landingspagina van De Verstandhouding binnen het React-framework.
---

# Landing Page Management Skill

Deze skill stelt de agent (Subagent D) in staat om autonoom de landingspagina van De Verstandhouding te beheren en door te ontwikkelen binnen het bestaande React + Tailwind CSS framework van de `ClientApp`.

## Richtlijnen & Ontwerpprotocollen:

1.  **Merk & Visuele Identiteit**:
    *   **Kleurcodes**:
        *   Accent/Merkkleur: `text-brand-500` / `bg-brand-500` (`#478d96`)
        *   Donker accent: `text-brand-950` / `bg-brand-950` (`#1a2c30`)
        *   Zachte achtergrond: `bg-brand-50` (`#f2f9f9`)
        *   Tekstkleur: `text-slate-600` of `text-slate-800`
    *   **Typografie**: Zorg ervoor dat alle koppen (`h1`, `h2`, `h3`) en paragrafen gebruik maken van `font-family: 'Plus Jakarta Sans', sans-serif`.
    *   **Consistentie**: Gebruik dezelfde componenten (zoals de `Brain` en `UserIcon` uit `lucide-react`) en stijl-tokens voor buttons en invoervelden als in de rest van de applicatie.

2.  **Structuur van de Landingspagina**:
    De landingspagina moet exact overeenkomen met de inhoudelijke secties van de officiële website `www.deverstandhouding.be`:
    *   **Navigatiebalk**: Responsive sticky nav-bar met logo, ankerlinks (`#welkom`, `#flexibiliteit`, `#verhaal`, `#gesprek`, `#contact`) en een actieknop ("Maak een afspraak" / "Naar dashboard" op basis van inlogstatus).
    *   **Hero (Welkom)**: Prikkelende openingstekst over psychologische flexibiliteit en begeleiding met een opvallende boekingsknop.
    *   **Psychologische Flexibiliteit (ACT)**: Kaartontwerpen met zachte animaties die uitleg geven over veerkracht, acceptatie en gedragspatronen.
    *   **Mijn Verhaal (Over Inge Debast)**: Biografie met details over haar VUB-opleiding, PhD en passie voor cliëntenondersteuning.
    *   **Kom op gesprek (Locaties)**: Duidelijke presentatie van de drie fysieke praktijken (Ninove, Aalst, Gooik) en videoconsultaties.
    *   **Contact**: Contactgegevens (telefoon, e-mail, registratienummer, visumnummer) met een werkend/gevalideerd contactformulier.

3.  **UI & Responsiveness Verificatie**:
    *   Gebruik de `browser_subagent` om de weergave te controleren op zowel mobiel als desktop.
    *   Controleer of ankerlinks vloeiend scrollen naar de juiste secties.
    *   Zorg dat interactieve elementen (zoals hover-effecten op knoppen en kaarten) soepel reageren.
