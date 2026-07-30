# %% [markdown]
# # Reinforcement Learning Simulator Showcase: Psychology Scheduling MDP
# 
# Dit notebook implementeert de Markov Decision Process (MDP) omgeving voor afsprakenbeheer en 
# vergelijkt verschillende planningsstrategieën (policies). 
# Het toont aan hoe een tabular Q-learning agent leert om patiënten op het juiste moment te 
# behandelen en veilig te ontslaan (discharge), en vergelijkt dit met traditionele handmatige regels.

# %%
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

# Stel visualisatie-stijl in
sns.set_theme(style="whitegrid")
plt.rcParams['figure.figsize'] = [12, 6]
plt.rcParams['figure.dpi'] = 100

# %% [markdown]
# ## 1. De MDP Omgeving: `PsychologyClientEnv`
# 
# Dit is de Python-versie van de simulator in de C# backend (`AIService.cs`).

# %%
class PsychologyClientEnv:
    """
    Simulatie-omgeving voor de planning, retentie en behandeloptimalisatie van psychologie-cliënten.
    Modelleert de cliëntrespons als een Markov Decision Process (MDP).
    """
    def __init__(self, start_sessions=0, start_stability=5.0, start_sentiment=0.0, max_sessions=24):
        self.max_sessions = max_sessions
        self.reset_env(start_sessions, start_stability, start_sentiment)
        self.action_space = [0, 1, 2, 3] # 0=Standaard, 1=Intensief, 2=Digitaal, 3=Discharge
        
    def reset_env(self, start_sessions=0, start_stability=5.0, start_sentiment=0.0):
        self.sessions_completed = start_sessions
        self.stability = start_stability
        self.gap = 7
        self.raw_sentiment = start_sentiment
        self.sentiment_ema = start_sentiment
        return self.get_state()
        
    def get_state(self):
        return np.array([
            self.sessions_completed, 
            self.stability, 
            self.gap, 
            self.raw_sentiment, 
            self.sentiment_ema
        ], dtype=np.float32)
        
    def step(self, action):
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
            if self.stability >= 7.0:
                # Succesvol ontslag: bonus gebaseerd op stabiliteit
                r_discharge = 50.0 + 10.0 * self.stability
            else:
                # Voortijdig ontslag: zware kwadratische penalty
                r_discharge = -100.0 * ((7.0 - self.stability) ** 2)
                
            return self.get_state(), r_discharge, done, {
                'attended': True, 'safety_triggered': safety_triggered, 'discharge_status': 'success' if self.stability >= 7.0 else 'premature', 'no_show_probability': 0.0
            }
            
        # 3. Normale planning-acties (0, 1, 2)
        if action == 0:    # Standaard (7 dagen)
            self.gap = 7
            action_cost = 0.0
            stability_change_mean = 0.1
            sentiment_change_mean = 0.05
        elif action == 1:  # Intensief (3 dagen)
            self.gap = 3
            action_cost = 3.0
            stability_change_mean = 0.4
            sentiment_change_mean = 0.15
        else:              # Digitaal (7 dagen + touchpoint)
            self.gap = 7
            action_cost = 1.0
            stability_change_mean = 0.25
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
        
        if attended == 1:
            self.sessions_completed += 1
            stability_delta = np.random.normal(stability_change_mean, 0.5)
            self.stability = max(1.0, min(10.0, self.stability + stability_delta))
            
            sentiment_target = (self.stability - 5.5) / 4.5
            raw_sentiment = np.random.normal(sentiment_target, 0.3)
            self.raw_sentiment = max(-1.0, min(1.0, raw_sentiment))
        else:
            self.gap += 7
            stability_delta = np.random.normal(-0.8, 0.6)
            self.stability = max(1.0, min(10.0, self.stability + stability_delta))
            self.raw_sentiment = max(-1.0, min(1.0, self.raw_sentiment - 0.25))
            
        alpha = 0.3
        self.sentiment_ema = alpha * self.raw_sentiment + (1.0 - alpha) * self.sentiment_ema
        
        # 4. Berekening van de Reward componenten
        delta_stability = self.stability - old_stability
        delta_sentiment_ema = self.sentiment_ema - old_sentiment_ema
        r_progress = 10.0 * max(0.0, delta_stability) + 5.0 * max(0.0, delta_sentiment_ema)
        
        # Penalty voor stagnatie
        c_stagnation = 2.0 * (8.0 - self.stability) if delta_stability <= 0.05 else 0.0
            
        # Overtreatment Penalty (Boete bij meer dan 12 sessies)
        session_limit = 12
        c_overtreatment = 1.5 * ((self.sessions_completed - session_limit) ** 1.5) if self.sessions_completed > session_limit else 0.0
            
        r_attendance = 5.0 if attended == 1 else -20.0
        c_delay = 0.5 * max(0.0, float(self.gap - 14))
        
        reward = r_progress + r_attendance - c_stagnation - c_overtreatment - c_delay - action_cost
        
        # 5. Einde episode
        done = False
        if self.stability <= 1.0:
            done = True
            reward -= 50.0  # Dropout boete
        elif self.sessions_completed >= self.max_sessions:
            done = True
            if self.stability < 7.0:
                reward -= 100.0  # Geforceerd onsuccesvol einde boete
                
        return self.get_state(), reward, done, {
            'attended': bool(attended), 'safety_triggered': safety_triggered, 'no_show_probability': float(no_show_prob)
        }

# %% [markdown]
# ## 2. Implementatie van de Lerende Agent: `QLearningAgent`
# 
# Omdat de toestand continu is, discretiseren we de statuswaarden tot een compacte, discrete 
# toestandruimte. Dit stelt de tabular Q-learning agent in staat om razendsnel te convergeren.

# %%
class QLearningAgent:
    """
    Een Tabular Q-learning agent die leert door middel van interactie met de PsychologyClientEnv.
    """
    def __init__(self, action_space, alpha=0.1, gamma=0.95, epsilon=1.0, min_epsilon=0.01, decay_rate=0.998):
        self.action_space = action_space
        self.alpha = alpha          # Learning rate
        self.gamma = gamma          # Discount factor (lange-termijn focus)
        self.epsilon = epsilon      # Exploratie-kans
        self.min_epsilon = min_epsilon
        self.decay_rate = decay_rate
        self.q_table = {}           # Q-table: map van state_tuple -> array van Q-waarden voor acties
        
    def get_state_key(self, state):
        """
        Discretiseert de toestand [sessions, stability, gap, raw_sentiment, sentiment_ema]
        tot een compacte tuple key voor de Q-table.
        """
        sessions, stability, gap, _, sentiment_ema = state
        
        # Discretisatie bins
        sessions_bin = int(sessions) # 0 t/m 24
        stability_bin = int(np.clip(stability, 1.0, 10.0)) # 1 t/m 10
        gap_bin = min(3, int(gap // 7)) # 0: <7 dagen, 1: 7-13d, 2: 14-20d, 3: >=21d
        sentiment_bin = int(np.clip((sentiment_ema + 1.0) * 2, 0, 3)) # Maps sentiment -1..1 naar 0..3
        
        return (sessions_bin, stability_bin, gap_bin, sentiment_bin)
        
    def get_q_values(self, state_key):
        """Haalt de Q-waarden op of initialiseert ze op nul."""
        if state_key not in self.q_table:
            self.q_table[state_key] = np.zeros(len(self.action_space))
        return self.q_table[state_key]
        
    def choose_action(self, state, train=True):
        """Kiest een actie met epsilon-greedy exploratie."""
        state_key = self.get_state_key(state)
        q_values = self.get_q_values(state_key)
        
        if train and np.random.rand() < self.epsilon:
            return np.random.choice(self.action_space)
        else:
            # Kies de actie met de hoogste Q-waarde (kies willekeurig bij gelijke stand)
            max_q = np.max(q_values)
            best_actions = np.where(q_values == max_q)[0]
            return np.random.choice(best_actions)
            
    def learn(self, state, action, reward, next_state, done):
        """Update de Q-waarde met de Bellman update-vergelijking."""
        state_key = self.get_state_key(state)
        next_state_key = self.get_state_key(next_state)
        
        q_values = self.get_q_values(state_key)
        next_q_values = self.get_q_values(next_state_key)
        
        # Bellman vergelijking target
        max_next_q = np.max(next_q_values) if not done else 0.0
        td_target = reward + self.gamma * max_next_q
        
        # Update
        q_values[action] += self.alpha * (td_target - q_values[action])
        
    def decay_exploration(self):
        """Verlaag de exploratiekans na elke episode."""
        self.epsilon = max(self.min_epsilon, self.epsilon * self.decay_rate)

# %% [markdown]
# ## 3. Training van de Q-Learning Agent
# 
# We trainen de agent over 8000 episodes en houden de prestaties bij om de leercurve te kunnen plotten.

# %%
def train_agent(num_episodes=8000):
    env = PsychologyClientEnv()
    agent = QLearningAgent(action_space=env.action_space)
    
    episode_rewards = []
    epsilons = []
    
    print("Training gestart...")
    for ep in range(1, num_episodes + 1):
        state = env.reset_env()
        done = False
        cumulative_reward = 0.0
        
        while not done:
            action = agent.choose_action(state, train=True)
            next_state, reward, done, info = env.step(action)
            agent.learn(state, action, reward, next_state, done)
            state = next_state
            cumulative_reward += reward
            
        agent.decay_exploration()
        episode_rewards.append(cumulative_reward)
        epsilons.append(agent.epsilon)
        
        if ep % 1000 == 0:
            avg_rew = np.mean(episode_rewards[-100:])
            print(f"Episode {ep}/{num_episodes} | Gem. Reward (laatste 100): {avg_rew:.2f} | Epsilon: {agent.epsilon:.3f}")
            
    print("Training afgerond!")
    return agent, episode_rewards, epsilons

# Start de training
q_agent, training_rewards, training_epsilons = train_agent(8000)

# Exporteer de getrainde Q-tabel naar een JSON bestand voor de C# backend
import json
import os

q_table_serializable = {}
for state_tuple, q_values in q_agent.q_table.items():
    # Converteer de tuple key naar een comma-separated string: "sessions,stability,gap,sentiment"
    key_str = ",".join(map(str, state_tuple))
    q_table_serializable[key_str] = q_values.tolist()

output_path = os.path.join(os.path.dirname(__file__), "q_table.json")
with open(output_path, "w") as f:
    json.dump(q_table_serializable, f, indent=2)

print(f"Q-table succesvol opgeslagen in {output_path} ({len(q_table_serializable)} states)")

# %% [markdown]
# ### Visualisatie: Leercurve van de Agent
# 
# We plotten de rolling mean van de beloningen om te zien hoe de agent leert.

# %%
plt.figure(figsize=(10, 5))
rolling_rewards = pd.Series(training_rewards).rolling(window=100, min_periods=10).mean()
plt.plot(training_rewards, alpha=0.15, color='blue', label='Ruwe Episode Reward')
plt.plot(rolling_rewards, color='darkblue', linewidth=2.5, label='Voortschrijdend Gemiddelde (Window=100)')
plt.title("Leercurve van de Q-Learning Agent")
plt.xlabel("Episode")
plt.ylabel("Cumulatieve Reward")
plt.legend()
plt.tight_layout()
plt.show()

# %% [markdown]
# ## 4. Definiëren en Vergelijken van Planningsstrategieën (Policies)
# 
# We evalueren de getrainde Q-learning agent en vergelijken deze met drie andere strategieën:
# 1. **Altijd Intensief:** Plant altijd een intensieve sessie (actie 1) en vergeet te ontslaan.
# 2. **Altijd Standaard:** Plant altijd wekelijkse sessies (actie 0) en sluit nooit af.
# 3. **Slimme Drempelwaarde Policy (Heuristiek/Regel):** Plant intensief bij lage stabiliteit, checkt digitaal in bij gemiddelde stabiliteit, en ontslaat de patiënt ($a_t=3$) vanaf een stabiliteit van $\ge 7.5$.
# 4. **Trained AI (Q-Learning):** De zojuist getrainde policy.

# %%
def run_simulation(policy_type, q_agent=None, num_episodes=100):
    env = PsychologyClientEnv()
    all_rewards = []
    all_lengths = []
    all_final_stabilities = []
    
    for _ in range(num_episodes):
        state = env.reset_env(start_sessions=0, start_stability=5.0, start_sentiment=0.0)
        done = False
        cumulative_reward = 0.0
        
        while not done:
            sessions, stability, gap, _, _ = state
            
            # Kies actie op basis van policy
            if policy_type == 'altijd_intensief':
                action = 1
            elif policy_type == 'altijd_standaard':
                action = 0
            elif policy_type == 'slimme_expert':
                # Beslisboom / Expert heuristiek
                if stability >= 7.5:
                    action = 3 # Discharge!
                elif stability < 4.5:
                    action = 1 # Intensief traject
                elif gap > 10:
                    action = 2 # Check-in om no-show te voorkomen
                else:
                    action = 0 # Standaard
            elif policy_type == 'q_learning' and q_agent is not None:
                # Evalueer exploit-only actie
                action = q_agent.choose_action(state, train=False)
            else:
                action = 0 # Fallback
            
            state, reward, done, info = env.step(action)
            cumulative_reward += reward
            
        all_rewards.append(cumulative_reward)
        all_lengths.append(env.sessions_completed)
        all_final_stabilities.append(env.stability)
        
    return all_rewards, all_lengths, all_final_stabilities

# %% [markdown]
# ## 5. Uitvoeren en Vergelijken van de Simulaties

# %%
policies = ['altijd_intensief', 'altijd_standaard', 'slimme_expert', 'q_learning']
results = {}

for p in policies:
    rewards, lengths, stabilities = run_simulation(p, q_agent=q_agent, num_episodes=200)
    results[p] = {
        'rewards': rewards,
        'sessions': lengths,
        'final_stability': stabilities
    }

# %% [markdown]
# ## 6. Resultaten Analyseren en Visualiseren

# %%
# Bereken gemiddelden
df_summary = pd.DataFrame({
    'Gem. Beloning (Reward)': [np.mean(results[p]['rewards']) for p in policies],
    'Gem. Aantal Sessies': [np.mean(results[p]['sessions']) for p in policies],
    'Gem. Eindstabiliteit': [np.mean(results[p]['final_stability']) for p in policies]
}, index=['Altijd Intensief', 'Altijd Standaard', 'Slimme Expert (Regel)', 'Trained AI (Q-Learning)'])

print("--- Gemiddelde Resultaten Over 200 Trajecten ---")
print(df_summary.round(2))

# %% [markdown]
# ### Visualisatie 1: Verdeling van de Totale Beloning (Reward) per Policy

# %%
plt.figure(figsize=(12, 6))
data_plot = pd.DataFrame({
    'Altijd Intensief': results['altijd_intensief']['rewards'],
    'Altijd Standaard': results['altijd_standaard']['rewards'],
    'Slimme Expert': results['slimme_expert']['rewards'],
    'Trained AI (Q-Learning)': results['q_learning']['rewards']
})
sns.boxplot(data=data_plot, palette='Set2')
plt.title("Totale Beloning (Reward) Verdeling per Planningsstrategie")
plt.ylabel("Cumulatieve Reward")
plt.axhline(y=0, color='r', linestyle='--', alpha=0.5)
plt.show()

# %% [markdown]
# ### Visualisatie 2: Aantal Sessies versus Eindstabiliteit
# 
# Deze scatterplot laat zien of de policies voldoen aan de klinische richtlijn (max. 12 sessies) en de cliënt stabiel genoeg krijgen ($\ge 7.0$).

# %%
plt.figure(figsize=(12, 6))

colors = ['red', 'orange', 'green', 'blue']
labels = ['Altijd Intensief', 'Altijd Standaard', 'Slimme Expert', 'Trained AI (Q-Learning)']

for p, color, label in zip(policies, colors, labels):
    plt.scatter(
        results[p]['sessions'], 
        results[p]['final_stability'], 
        alpha=0.5, 
        label=label,
        s=60,
        edgecolors='none'
    )

plt.axvline(x=12, color='black', linestyle=':', alpha=0.6, label='Klinische Richtlijn (12 sessies)')
plt.axhline(y=7.0, color='darkgreen', linestyle=':', alpha=0.6, label='Drempel Veilig Ontslag (7.0)')
plt.xlabel("Aantal Voltooide Sessies")
plt.ylabel("Eindstabiliteit van de Cliënt")
plt.title("Behandelefficiëntie: Sessies vs. Eindstabiliteit per Planningsstrategie")
plt.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
plt.tight_layout()
plt.show()

# %% [markdown]
# ## 7. Model Drift & Hertraining Feedback Loop
# 
# We demonstreren hier hoe een feedback loop werkt wanneer de werkelijkheid verandert (Model Drift):
# *   **Oude wereld**: Grote gaten tussen afspraken (> 14 dagen) gaven een zware penalty op no-shows (+35%). De agent leerde dat grote gaps koste wat kost vermeden moesten worden (dus plande hij veel *Intensieve* of *Check-in* afspraken).
# *   **Nieuwe wereld (Drift)**: De kliniek introduceert een SMS-waarschuwingssysteem. De no-show boete voor grote gaten daalt drastisch naar slechts **+5%**.
# 
# We evalueren:
# 1. De **oude agent** in de nieuwe wereld (hij verspilt capaciteit omdat hij nog steeds denkt dat gaps gevaarlijk zijn).
# 2. De **nieuwe agent** getraind in de bijgewerkte omgeving (hij past zijn gedrag aan, ontspant de planning, en bespaart kosten).

# %%
class PsychologyClientEnvDrifted(PsychologyClientEnv):
    """
    Een aangepaste omgeving waarin de no-show boete voor grote gaps is verlaagd 
    van +35% naar slechts +5% door een SMS-service.
    """
    def step(self, action):
        safety_triggered = False
        if self.stability < 3.0 and action != 1:
            action = 1
            safety_triggered = True
            
        old_stability = self.stability
        old_sentiment_ema = self.sentiment_ema
        
        if action == 3:
            done = True
            r_discharge = 50.0 + 10.0 * self.stability if self.stability >= 7.0 else -100.0 * ((7.0 - self.stability) ** 2)
            return self.get_state(), r_discharge, done, {
                'attended': True, 'safety_triggered': safety_triggered, 'discharge_status': 'success' if self.stability >= 7.0 else 'premature', 'no_show_probability': 0.0
            }
            
        if action == 0:
            self.gap = 7
            action_cost = 0.0
            stability_change_mean = 0.1
            sentiment_change_mean = 0.05
        elif action == 1:
            self.gap = 3
            action_cost = 3.0
            stability_change_mean = 0.4
            sentiment_change_mean = 0.15
        else:
            self.gap = 7
            action_cost = 1.0
            stability_change_mean = 0.25
            sentiment_change_mean = 0.10
            
        # --- MODEL DRIFT: gap_penalty verlaagd naar 0.05 (was 0.35) ---
        base_no_show_prob = 0.10
        gap_penalty = 0.05 if self.gap > 14 else 0.0
        stability_penalty = 0.25 if self.stability < 4.0 else 0.0
        sentiment_penalty = 0.20 if self.sentiment_ema < 0.0 else 0.0
        digital_bonus = -0.15 if action == 2 else 0.0
        
        no_show_prob = max(0.02, min(0.95, base_no_show_prob + gap_penalty + stability_penalty + sentiment_penalty + digital_bonus))
        
        attended = np.random.binomial(1, 1.0 - no_show_prob)
        
        if attended == 1:
            self.sessions_completed += 1
            stability_delta = np.random.normal(stability_change_mean, 0.5)
            self.stability = max(1.0, min(10.0, self.stability + stability_delta))
            sentiment_target = (self.stability - 5.5) / 4.5
            raw_sentiment = np.random.normal(sentiment_target, 0.3)
            self.raw_sentiment = max(-1.0, min(1.0, raw_sentiment))
        else:
            self.gap += 7
            stability_delta = np.random.normal(-0.8, 0.6)
            self.stability = max(1.0, min(10.0, self.stability + stability_delta))
            self.raw_sentiment = max(-1.0, min(1.0, self.raw_sentiment - 0.25))
            
        alpha = 0.3
        self.sentiment_ema = alpha * self.raw_sentiment + (1.0 - alpha) * self.sentiment_ema
        
        delta_stability = self.stability - old_stability
        delta_sentiment_ema = self.sentiment_ema - old_sentiment_ema
        r_progress = 10.0 * max(0.0, delta_stability) + 5.0 * max(0.0, delta_sentiment_ema)
        c_stagnation = 2.0 * (8.0 - self.stability) if delta_stability <= 0.05 else 0.0
        
        session_limit = 12
        c_overtreatment = 1.5 * ((self.sessions_completed - session_limit) ** 1.5) if self.sessions_completed > session_limit else 0.0
        r_attendance = 5.0 if attended == 1 else -20.0
        c_delay = 0.5 * max(0.0, float(self.gap - 14))
        
        reward = r_progress + r_attendance - c_stagnation - c_overtreatment - c_delay - action_cost
        
        done = False
        if self.stability <= 1.0:
            done = True
            reward -= 50.0
        elif self.sessions_completed >= self.max_sessions:
            done = True
            if self.stability < 7.0:
                reward -= 100.0
                
        return self.get_state(), reward, done, {
            'attended': bool(attended), 'safety_triggered': safety_triggered, 'no_show_probability': float(no_show_prob)
        }

# %%
def run_simulation_drifted(q_agent, num_episodes=200):
    env = PsychologyClientEnvDrifted()
    rewards = []
    for _ in range(num_episodes):
        state = env.reset_env()
        done = False
        cum_rew = 0
        while not done:
            action = q_agent.choose_action(state, train=False)
            state, reward, done, info = env.step(action)
            cum_rew += reward
        rewards.append(cum_rew)
    return rewards

def train_agent_drifted(num_episodes=8000):
    env = PsychologyClientEnvDrifted()
    agent = QLearningAgent(action_space=env.action_space)
    print("Feedback Loop: Nieuwe agent aan het trainen in drifted omgeving...")
    for ep in range(1, num_episodes + 1):
        state = env.reset_env()
        done = False
        while not done:
            action = agent.choose_action(state, train=True)
            next_state, reward, done, info = env.step(action)
            agent.learn(state, action, reward, next_state, done)
            state = next_state
        agent.decay_exploration()
    print("Feedback Loop: Training nieuwe agent afgerond!")
    return agent

# %%
# 1. Evalueer de OUDE agent in de NIEUWE DRIFTED wereld
rewards_old_agent = run_simulation_drifted(q_agent, num_episodes=200)

# 2. Train een NIEUWE agent in de NIEUWE DRIFTED wereld (de feedback loop)
q_agent_drifted = train_agent_drifted(8000)
rewards_new_agent = run_simulation_drifted(q_agent_drifted, num_episodes=200)

# %%
# Analyseer de resultaten van de feedback loop
print("--- Resultaten Feedback Loop ---")
print(f"Oude Agent in Nieuwe Wereld (Model Mismatch): Gem. Reward = {np.mean(rewards_old_agent):.2f}")
print(f"Hertrainde Agent in Nieuwe Wereld (Hersteld):   Gem. Reward = {np.mean(rewards_new_agent):.2f}")

plt.figure(figsize=(10, 5))
df_drift = pd.DataFrame({
    'Oude Agent (Mismatch)': rewards_old_agent,
    'Hertrainde Agent (Aangepast)': rewards_new_agent
})
sns.boxplot(data=df_drift, palette='pastel')
plt.title("Feedback Loop: Effect van Hertraining na Model Drift (SMS-Systeem)")
plt.ylabel("Cumulatieve Reward")
plt.show()

