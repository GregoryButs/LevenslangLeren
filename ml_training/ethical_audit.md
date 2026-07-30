# Reviewer Agent: Ethical & Compliance Audit voor No-Show Voorspellingsmodel

Dit document bevat de ethische, juridische en verklaarbaarheidsaudit (Explainable AI) van de no-show voorspellingspipeline zoals geïmplementeerd voor het project **AfsprakenbeheerPsycholoog**.

---

## 1. Risicoanalyse & Bias-risico's (Algoritmische Eerlijkheid)

Het gebruik van machine learning in de zorg (in het bijzonder de geestelijke gezondheidszorg) brengt unieke risico's met zich mee. We identificeren de volgende bias-risico's in het huidige model:

### 1.1 Klinische Bias (Treatment-Type Bias)
*   **Risico:** In de synthetische dataset verhoogt de diagnose 'depressie' de no-show kans direct met 20%. 
*   **Impact:** Als het model dit leert, worden patiënten met een depressie structureel gecategoriseerd als "hoog risico op no-show". Dit kan ertoe leiden dat psychologen minder snel patiënten met een zware depressie aannemen, of dat het systeem hen automatisch degradeert in de planningsvolgorde (bijvoorbeeld door hen alleen ongunstige uren toe te wijzen).
*   **Mitigatie:** Behandeltype mag nooit de enige drijver zijn voor planningsbeslissingen. We moeten zorgen voor eerlijkheidskwalificaties (fairness metrics) zoals *Demographic Parity* over diagnoses heen.

### 1.2 Leeftijdsgerelateerde en Socio-economische Bias
*   **Risico:** Factoren zoals `age` en `last_session_gap` kunnen gecorreleerd zijn met externe factoren die niet in het model zitten. Bijvoorbeeld: oudere cliënten kunnen mobiliteitsproblemen hebben, of cliënten met een lagere sociaaleconomische status (SES) kunnen moeite hebben met vervoer of onregelmatige werktijden (wat leidt tot grotere gaps).
*   **Impact:** Het model straft cliënten voor factoren waar ze geen controle over hebben, wat leidt tot ongelijke toegang tot zorg.

---

## 2. Verklaarbaarheid (Explainable AI - XAI)

Om het vertrouwen van psychologen en cliënten te winnen, mag het model geen "black box" zijn. We hanteren de volgende principes voor verklaarbaarheid:

### 2.1 Waarom een Lineair / Eenvoudig Model?
*   In plaats van complexe deep learning-modellen (zoals neurale netwerken) gebruiken we in de `PipelineFactory` standaard **Logistische Regressie** of eenvoudige **Beslissingsbomen (Decision Trees)**.
*   **Voordeel:** Logistische regressie levert direct interpreteerbare coëfficiënten (odds ratios). Een psycholoog kan exact zien *waarom* een cliënt een risico-indicator krijgt (bijv. "Gap > 21 dagen verhoogt de kans op no-show met een factor X").

### 2.2 SHAP (SHapley Additive exPlanations) & Feature Importance
*   Voor complexere classifiers (bijv. Random Forest of Gradient Boosting) integreren we **SHAP-waarden**.
*   Hiermee splitsen we elke individuele voorspelling op in de bijdrage van elke feature. 
*   *Voorbeeld:* Voor cliënt X is de kans op no-show 35%: 10% (basis) + 15% (door gap van 25 dagen) + 10% (door leeftijd).

---

## 3. GDPR & Privacy Compliance

Gezondheidsgegevens vallen onder de "bijzondere persoonsgegevens" (Artikel 9 AVG/GDPR). Dit stelt strenge eisen aan onze dataverwerking.

### 3.1 Dataminimalisatie (Artikel 5(1)(c) AVG)
*   **Audit-bevinding:** In de pipeline worden identificerende gegevens zoals `name`, `email` en `phone` expliciet weggelaten via de `remainder='drop'` instelling in de `ColumnTransformer`. 
*   **Status:** **Conform.** Het model traint en voorspelt uitsluitend op geanonimiseerde/gepseudonimiseerde klinische en planningskenmerken.

### 3.2 Recht op Uitleg bij Geautomatiseerde Besluitvorming (Artikel 22 AVG)
*   Cliënten hebben het recht om niet te worden onderworpen aan besluiten die uitsluitend zijn gebaseerd op geautomatiseerde verwerking als dit rechtsgevolgen heeft.
*   **Status:** Ons systeem mag **nooit** zelfstandig afspraken annuleren of cliënten uitsluiten. Het model dient puur als *beslissingsondersteuning* voor de psycholoog (Human-in-the-loop).

---

## 4. Drie Concrete Verbeterpunten

Om het ML-systeem naar een ethisch en juridisch compliant niveau te tillen, implementeren we de volgende drie verbeteringen:

### 1. Ethische Planningsregels (Fair Scheduling Protocol)
*   **Maatregel:** We implementeren een harde regel in de applicatie: *No-show voorspellingen mogen nooit leiden tot het weigeren van een cliënt of het toewijzen van minderwaardige tijdstippen.*
*   **Toepassing:** Het risicoprofiel mag alleen worden gebruikt voor *ondersteunende acties*, zoals het sturen van een extra herinnering via sms of het proactief telefonisch contact opnemen om de afspraak te bevestigen.

### 2. Implementatie van Fairness Metrics in Model-evaluatie
*   **Maatregel:** We voegen fairness-auditing toe aan de trainingsfase. We berekenen de **Disparate Impact Ratio** en **Equalized Odds** voor subgroepen (bijv. leeftijdscategorieën en diagnoses).
*   **Toepassing:** Als het model voor de groep 'depressie' significant meer fout-positieven (onterecht voorspeld als no-show) oplevert dan voor andere groepen, moet het model worden gekalibreerd met een diagnose-specifieke drempelwaarde.

### 3. Transparante Toestemmingsflow & Opt-out (Consent Management)
*   **Maatregel:** Bij het registreren in het patiëntenportaal krijgt de cliënt een duidelijke, begrijpelijke uitleg over hoe planningsdata wordt geanalyseerd om de praktijkvoering te optimaliseren.
*   **Toepassing:** Cliënten krijgen een expliciete opt-in/opt-out checkbox voor "gepersonaliseerde planningsherinneringen". Bij een opt-out worden hun gegevens uitgesloten van de no-show voorspellingsdatabase en ontvangen ze uitsluitend de standaard herinneringstermijn.
