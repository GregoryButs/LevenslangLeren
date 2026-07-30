---
name: visual-flow-testing
description: Voer autonome visuele en functionele tests uit op de React booking wizard met de Chrome browser-subagent.
---

# Visual Flow Testing Skill

Deze skill stelt de agent (Subagent A) in staat om autonome UI- en UX-verificaties uit te voeren op de React-applicatie van de boekingswebsite met behulp van de Chrome browser-subagent van Antigravity.

## Stappenplan voor UI-verificatie:

1.  **Start de Lokale Ontwikkelserver**:
    *   Navigeer naar `Project/StartTestApi/StartTestApi/AfsprakenbeheerPsycholoog/ClientApp/`.
    *   Start de ontwikkelserver met `npm run dev` (of controleer via `run_command` of de server al draait).
    *   Noteer de lokale URL (meestal `http://localhost:5173` of vergelijkbaar).

2.  **Start de Chrome Browser-subagent**:
    *   Gebruik de `browser_subagent` tool met een specifieke taakomschrijving.
    *   Stel de `Task` zo op dat de subagent naar de boekingspagina navigeert, alle stappen van de 'booking wizard' doorloopt en klikt op interactieve elementen.

3.  **Visualiseer & Valideer**:
    *   Controleer of er layout-verschuivingen (Layout Shifts) zijn tijdens de transitie tussen stappen.
    *   Verifieer dat invoervelden (zoals datum, tijdstip, naam en e-mail) correcte validatie-outlines tonen.
    *   Controleer de responsive states door de browser-subagent te instrueren het venster te resizen (bijv. naar mobiel formaat).

4.  **Genereer Artifacts**:
    *   Sla screenshots op van elke stap in de boekingsflow onder `/artifacts/visual_tests/`.
    *   Geef de opnames duidelijke namen, bijv. `step_1_service_selection.webp`, `step_2_time_slot.webp`, etc.
    *   Schrijf een kort testrapport (`ui_test_report.md` in `/artifacts`) waarin je de screenshots insluit.
