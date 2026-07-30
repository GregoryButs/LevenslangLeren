# Reinforcement Learning (RL) Mentor: Afsprakenbeheer MDP Formalisatie

Dit document formaliseert de afspraakplanning, cliëntretentie en behandelbeëindiging als een **Markov Decision Process (MDP)**. In plaats van reactieve planning of ad-hoc ontslagbeslissingen, gebruikt het RL-systeem een proactieve benadering om de behandelcontinuïteit, het emotioneel welzijn van de cliënt, en een veilige behandelingsduur te optimaliseren.

---

## 1. MDP Formalisatie

We modelleren de interactie tussen de praktijk (de agent) en de cliënt (de omgeving) als een MDP gedefinieerd door het tuple $(S, A, P, R, \gamma)$.

### 1.1 State Space ($S$)
De toestand $s_t \in S$ op tijdstap $t$ combineert harde data (sessie-tellingen, tijdsgaten) met zachte data (sentiment-analyse van sessieverslagen). De toestandsvector is gedefinieerd als:

$$s_t = \begin{bmatrix} S_t \\ E_t \\ G_t \\ M_t \\ \bar{M}_t \end{bmatrix}$$

Waar:
*   **$S_t \in \mathbb{N}_{\ge 0}$ (Sessies Voltooid):** Het aantal succesvol afgeronde therapiesessies tot nu toe (bereik: $0 \dots S_{\text{max}}$). (Harde data)
*   **$E_t \in [1.0, 10.0]$ (Emotionele Stabiliteitsscore):** Een klinische toestandsevaluatie waarbij $1.0$ kritiek is en $10.0$ optimaal. (Klinische harde/zachte data)
*   **$G_t \in \mathbb{N}_{> 0}$ (Tijdgap):** Het aantal dagen sinds de laatste sessie of het geplande interval. (Harde data)
*   **$M_t \in [-1.0, 1.0]$ (Sentiment-score):** De ruwe sentiment-score geëxtraheerd uit het meest recente sessieverslag middels NLP (waarbij $-1.0$ zeer negatief/crisis en $+1.0$ zeer positief is). (Zachte data)
*   **$\bar{M}_t \in [-1.0, 1.0]$ (Exponential Moving Average Sentiment):** Een exponentieel voortschrijdend gemiddelde van het sentiment om ruis uit individuele verslagen te dempen:
    $$\bar{M}_t = \alpha \cdot M_t + (1 - \alpha) \cdot \bar{M}_{t-1}$$
    Waarbij $\alpha \in (0, 1]$ de smoothing-factor is (bijv. $\alpha = 0.3$).

#### Normalisatie voor RL Netwerken
Voor stabiele neurale netwerktraining (zoals PPO/DQN) wordt de toestand genormaliseerd tot $s'_t \in [-1.0, 1.0]^5$:
$$s'_t = \begin{bmatrix} S_t / S_{\text{max}} \\ (E_t - 5.5) / 4.5 \\ G_t / 30.0 \\ M_t \\ \bar{M}_t \end{bmatrix}$$

---

### 1.2 Action Space ($A$)
Op elke stap kiest de planning-agent een actie $a_t \in A$:
*   **$a_t = 0$ (Standaard Interval):** Plan de volgende sessie over 7 dagen.
*   **$a_t = 1$ (Intensief Traject):** Plan de volgende sessie over 3 dagen.
*   **$a_t = 2$ (Digitale Ondersteuning):** Plan de volgende sessie over 7 dagen en activeer een tussentijdse digitale check-in.
*   **$a_t = 3$ (Afronden / Discharge):** Beëindig de actieve behandeling (de agent schat in dat de cliënt voldoende hersteld is).

---

### 1.3 Reward Function ($R$)
De beloningsfunctie $R(s_t, a_t, s_{t+1})$ is ontworpen om een veilige en effectieve behandeling te stimuleren. Het voorkomt premature beëindiging, straft stilstand en voorkomt overbehandeling:

$$R_t = R_{\text{progress}} + R_{\text{attendance}} + R_{\text{discharge}} - C_{\text{stagnation}} - C_{\text{overtreatment}} - C_{\text{delay}} - C_{\text{action}}$$

#### 1. Stapsgewijze Klinische & Emotionele Progressie ($R_{\text{progress}}$)
De agent ontvangt een bonus voor stapsgewijze stijging in emotionele stabiliteit ($E$) en sentiment ($\bar{M}$):
$$R_{\text{progress}} = w_1 \cdot \max(0, E_{t+1} - E_t) + w_2 \cdot \max(0, \bar{M}_{t+1} - \bar{M}_t)$$
*Gewichten:* $w_1 = 10.0$, $w_2 = 5.0$. Dit beloont uitsluitend actieve vooruitgang.

#### 2. Boete voor Tijdsverloop zonder Resultaat ($C_{\text{stagnation}}$)
Als de behandeling voortduurt maar de cliënt stagneert (geen of negatieve verandering) terwijl hij/zij nog niet stabiel is ($E_{t+1} < 8.0$), krijgt de agent een boete per stap:
$$C_{\text{stagnation}} = \begin{cases} c_{\text{stagn}} \cdot (8.0 - E_{t+1}) & \text{als } E_{t+1} - E_t \le 0.05 \\ 0 & \text{als } E_{t+1} - E_t > 0.05 \end{cases}$$
*Gewicht:* $c_{\text{stagn}} = 2.0$. Dit voorkomt dat de agent de cliënt passief in behandeling houdt zonder effect te boeken.

#### 3. Ontslaggerichte Beloningen en Penalties ($R_{\text{discharge}}$)
Wanneer de agent de actie $a_t = 3$ (Discharge) kiest, beëindigt dit de episode. We straffen voortijdig ontslag zwaar en belonen een succesvolle afronding:
$$R_{\text{discharge}} = \begin{cases} B_{\text{success}} + w_3 \cdot E_{t+1} & \text{als } E_{t+1} \ge 7.0 \quad (\text{Veilig Ontslag}) \\ - P_{\text{premature}} \cdot (7.0 - E_{t+1})^2 & \text{als } E_{t+1} < 7.0 \quad (\text{Voortijdig Ontslag}) \end{cases}$$
*Gewichten:* $B_{\text{success}} = 50.0$, $w_3 = 10.0$, $P_{\text{premature}} = 100.0$. De kwadratische penalty zorgt ervoor dat ontslag bij een lage stabiliteit (bijv. $E = 4.0$) extreem zwaar bestraft wordt.

#### 4. Overtreatment Penalty / Safety Constraint ($C_{\text{overtreatment}}$)
Om te voorkomen dat de agent de patiënt onnodig lang in behandeling houdt om progressiebeloningen te verzamelen, introduceren we een progressieve boete zodra het aantal sessies een klinische richtlijn ($S_{\text{limit}} = 12$) overschrijdt:
$$C_{\text{overtreatment}} = \begin{cases} \gamma_{\text{over}} \cdot (S_t - S_{\text{limit}})^{1.5} & \text{als } S_t > S_{\text{limit}} \text{ en } a_t \ne 3 \\ 0 & \text{anders} \end{cases}$$
*Gewicht:* $\gamma_{\text{over}} = 1.5$. Dit dwingt de agent tot een actieve afweging om de behandeling tijdig af te ronden zodra de cliënt stabiel is.

#### 5. Overige Componenten
*   **$R_{\text{attendance}}$ (Aanwezigheid):** $+5.0$ als de cliënt verschijnt, $-20.0$ bij een no-show.
*   **$C_{\text{delay}}$ (Tijdgap boete):** $0.5 \cdot \max(0, G_{t+1} - 14)$ straft te grote tussenpozen.
*   **$C_{\text{action}}$ (Operationele kosten):** $0.0$ voor standaard ($a=0$), $3.0$ voor intensief ($a=1$, beslag op capaciteit), $1.0$ voor digitaal ($a=2$).

---

### 1.4 Harde Safety Constraints
Naast de beloningsstraf hanteren we een **harde veiligheidslimiet (Safety Constraint)** in de actie-overname:
1.  **Crisis-interventie:** Als $E_t < 3.0$ en de agent kiest een andere actie dan $1$ (Intensief), overschrijft de omgeving de actie naar $a_t = 1$ en activeert een klinische noodprocedure. Ontslag ($a_t = 3$) is in deze toestand geblokkeerd.
2.  **Harde Maximale Duur:** Als $S_t \ge S_{\text{max}} = 24$ sessies, wordt de episode geforceerd beëindigd (`done = True`). Als $E_t < 7.0$ op dat moment, krijgt de agent een zware penalty voor geforceerde onsuccesvolle beëindiging.

---

## 2. Python Implementatie: `PsychologyClientEnv`

Hieronder staat de code voor de simulatie-omgeving (`Gym-like` interface) die dit MDP modelleert.

```python
import numpy as np

class PsychologyClientEnv:
    """
    Simulatie-omgeving voor de planning, retentie en behandeloptimalisatie van psychologie-cliënten.
    Modelleert de cliëntrespons als een Markov Decision Process (MDP) met State-Space,
    stagnatie-penalties en overtreatment constraints.
    """
    def __init__(self, start_sessions=0, start_stability=5.0, start_sentiment=0.0, max_sessions=24):
        self.max_sessions = max_sessions
        
        # State initialisatie
        self.sessions_completed = start_sessions
        self.stability = start_stability
        self.gap = 7  # Standaard startgap is 7 dagen
        self.raw_sentiment = start_sentiment
        self.sentiment_ema = start_sentiment  # Glijdend gemiddelde (EMA) van het sentiment
        
        # Actieruimte: 0 = Standaard, 1 = Intensief, 2 = Digitaal, 3 = Discharge (Afronden)
        self.action_space = [0, 1, 2, 3]
        
    def get_state(self):
        """
        Retourneert de huidige toestand [S_t, E_t, G_t, M_t, EMA_M_t].
        Voor RL modeling is het aan te raden deze vector te normaliseren.
        """
        return np.array([
            self.sessions_completed, 
            self.stability, 
            self.gap, 
            self.raw_sentiment, 
            self.sentiment_ema
        ], dtype=np.float32)
        
    def step(self, action):
        """
        Voert een planning-actie uit en berekent de overgang naar de volgende toestand,
        de beloning en of de behandelcyclus is voltooid.
        """
        assert action in self.action_space, f"Ongeldige actie: {action}"
        
        # 1. Harde Safety Constraint: Crisis-interventie
        safety_triggered = False
        if self.stability < 3.0 and action != 1:
            action = 1  # Forceer intensief traject bij crisis
            safety_triggered = True
            
        old_stability = self.stability
        old_sentiment_ema = self.sentiment_ema
        
        # 2. Behandel actie 3: Discharge (Afronden van de behandeling)
        if action == 3:
            done = True
            attended = 1  # Niet direct van toepassing bij ontslag
            
            # Berekening Discharge reward / penalty
            if self.stability >= 7.0:
                # Succesvol ontslag: bonus gebaseerd op stabiliteit
                r_discharge = 50.0 + 10.0 * self.stability
            else:
                # Voortijdig ontslag: zware kwadratische penalty
                r_discharge = -100.0 * ((7.0 - self.stability) ** 2)
                
            reward = r_discharge
            info = {
                'attended': True,
                'safety_triggered': safety_triggered,
                'discharge_status': 'success' if self.stability >= 7.0 else 'premature',
                'no_show_probability': 0.0
            }
            return self.get_state(), reward, done, info
            
        # 3. Normale planning-acties (0, 1, 2)
        # Bepaal het nieuwe interval en basis dynamics
        if action == 0:    # Standaard (7 dagen)
            self.gap = 7
            action_cost = 0.0
            stability_change_mean = 0.1
            sentiment_change_mean = 0.05
        elif action == 1:  # Intensief (3 dagen)
            self.gap = 3
            action_cost = 3.0
            stability_change_mean = 0.4  # Snel herstel door intensief contact
            sentiment_change_mean = 0.15
        else:              # Digitaal (7 dagen + touchpoint)
            self.gap = 7
            action_cost = 1.0
            stability_change_mean = 0.25 # Verbetering door actieve check-in
            sentiment_change_mean = 0.10
            
        # Bereken no-show kans op basis van gap, stabiliteit en sentiment
        base_no_show_prob = 0.10
        gap_penalty = 0.35 if self.gap > 14 else 0.0
        stability_penalty = 0.25 if self.stability < 4.0 else 0.0
        sentiment_penalty = 0.20 if self.sentiment_ema < 0.0 else 0.0
        digital_bonus = -0.15 if action == 2 else 0.0
        
        no_show_prob = max(0.02, min(0.95, base_no_show_prob + gap_penalty + stability_penalty + sentiment_penalty + digital_bonus))
        
        # Bepaal of de cliënt komt opdagen
        attended = np.random.binomial(1, 1.0 - no_show_prob)
        
        # Update toestand op basis van aanwezigheid
        if attended == 1:
            self.sessions_completed += 1
            
            # Update emotionele stabiliteit
            stability_delta = np.random.normal(stability_change_mean, 0.5)
            self.stability = max(1.0, min(10.0, self.stability + stability_delta))
            
            # Genereer sentiment score uit het sessieverslag (NLP mock)
            # Sentiment is klinisch gecorreleerd met emotionele stabiliteit
            sentiment_target = (self.stability - 5.5) / 4.5  # Map naar [-1, 1]
            raw_sentiment = np.random.normal(sentiment_target, 0.3)
            self.raw_sentiment = max(-1.0, min(1.0, raw_sentiment))
        else:
            # No-show leidt tot terugval en groter interval
            self.gap += 7
            stability_delta = np.random.normal(-0.8, 0.6)
            self.stability = max(1.0, min(10.0, self.stability + stability_delta))
            
            # Geen sessieverslag, sentiment daalt door gebrek aan contact
            self.raw_sentiment = max(-1.0, min(1.0, self.raw_sentiment - 0.25))
            
        # Update Exponential Moving Average (EMA) van het sentiment
        alpha = 0.3
        self.sentiment_ema = alpha * self.raw_sentiment + (1.0 - alpha) * self.sentiment_ema
        
        # 4. Berekening van de Reward componenten
        # Bonus voor stapsgewijze stijging
        delta_stability = self.stability - old_stability
        delta_sentiment_ema = self.sentiment_ema - old_sentiment_ema
        r_progress = 10.0 * max(0.0, delta_stability) + 5.0 * max(0.0, delta_sentiment_ema)
        
        # Penalty voor stagnatie (tijdverloop zonder resultaat)
        if delta_stability <= 0.05:
            c_stagnation = 2.0 * (8.0 - self.stability)  # Boete is groter naarmate patiënt verder van herstel is
        else:
            c_stagnation = 0.0
            
        # Overtreatment Penalty (Safety Constraint tegen onnodig lange behandeling)
        session_limit = 12
        if self.sessions_completed > session_limit:
            c_overtreatment = 1.5 * ((self.sessions_completed - session_limit) ** 1.5)
        else:
            c_overtreatment = 0.0
            
        r_attendance = 5.0 if attended == 1 else -20.0
        c_delay = 0.5 * max(0.0, float(self.gap - 14))
        
        # Totale Reward
        reward = r_progress + r_attendance - c_stagnation - c_overtreatment - c_delay - action_cost
        
        # 5. Controleer of de episode is afgelopen (Harde limiteringen)
        done = False
        if self.stability <= 1.0:
            done = True  # Cliënt is uitgevallen (dropout / crisis)
            reward -= 50.0  # Zware penalty voor voortijdige uitval
        elif self.sessions_completed >= self.max_sessions:
            done = True  # Harde limiet bereikt
            if self.stability < 7.0:
                reward -= 100.0  # Extra penalty als cliënt onvoldoende hersteld is
                
        info = {
            'attended': bool(attended),
            'safety_triggered': safety_triggered,
            'no_show_probability': float(no_show_prob)
        }
        
        return self.get_state(), reward, done, info

    def reset(self):
        """Reset de omgeving voor een nieuwe cliënt."""
        self.sessions_completed = 0
        self.stability = 5.0
        self.gap = 7
        self.raw_sentiment = 0.0
        self.sentiment_ema = 0.0
        return self.get_state()
```

---

## 3. Integratie in Afsprakenbeheer
In de praktijk kan deze MDP-omgeving worden gebruikt om een DQN (Deep Q-Network) of PPO (Proximal Policy Optimization) agent te trainen. De getrainde policy kan vervolgens via de API suggesties doen aan de psycholoog voor de optimale timing, digitale interacties en ontslagmomenten bij cliënten, gebaseerd op zowel administratieve data als sentiment-analyse van klinische verslagen.
