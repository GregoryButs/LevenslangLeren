using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    public class AIService : IAIService
    {
        private static readonly Random _random = new Random(42);
        private Dictionary<string, double[]>? _qTable = null;
        private readonly object _qTableLock = new object();
        private bool _qTableLoadAttempted = false;

        private void EnsureQTableLoaded()
        {
            if (_qTableLoadAttempted) return;

            lock (_qTableLock)
            {
                if (_qTableLoadAttempted) return;
                _qTableLoadAttempted = true;

                try
                {
                    string jsonPath = FindFile(
                        "q_table.json",
                        "/home/gregory/project/AD-GregoryButs-2526/ml_training/q_table.json"
                    );

                    if (!string.IsNullOrEmpty(jsonPath) && File.Exists(jsonPath))
                    {
                        string jsonString = File.ReadAllText(jsonPath);
                        _qTable = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, double[]>>(jsonString);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading Q-table: {ex.Message}");
                }
            }
        }

        private string GetStateKey(int sessions, double stability, int gap, double sentimentEma)
        {
            int sessionsBin = sessions;
            int stabilityBin = (int)Math.Clamp(stability, 1.0, 10.0);
            int gapBin = Math.Min(3, gap / 7);
            int sentimentBin = (int)Math.Clamp((sentimentEma + 1.0) * 2, 0, 3);
            return $"{sessionsBin},{stabilityBin},{gapBin},{sentimentBin}";
        }

        public string GetRecommendedAction(int sessions, double stability, int gap, double sentimentEma)
        {
            EnsureQTableLoaded();

            // Safety Constraint: als stabiliteit te laag is, forceer intensief
            if (stability < 3.0)
            {
                return "Intensief Traject (3d)";
            }

            string stateKey = GetStateKey(sessions, stability, gap, sentimentEma);

            if (_qTable != null && _qTable.TryGetValue(stateKey, out var qValues) && qValues.Length > 0)
            {
                int bestActionIdx = 0;
                double maxQ = qValues[0];
                for (int i = 1; i < qValues.Length; i++)
                {
                    if (qValues[i] > maxQ)
                    {
                        maxQ = qValues[i];
                        bestActionIdx = i;
                    }
                }

                string[] actionNames = new[] {
                    "Standaard (7d)",
                    "Intensief Traject (3d)",
                    "Digitale Check-in (7d)",
                    "Discharge (Ontslag)"
                };
                return actionNames[bestActionIdx];
            }

            return "Niet getraind (Draai model)";
        }

        public string GetHeuristicAction(int sessions, double stability, int gap)
        {
            if (stability < 3.0)
            {
                return "Intensief Traject (3d)";
            }
            if (stability >= 7.5) return "Discharge (Ontslag)";
            if (stability < 4.5) return "Intensief Traject (3d)";
            if (gap > 10) return "Digitale Check-in (7d)";
            return "Standaard (7d)";
        }

        public double GetNoShowProbability(int age, int completedSessions, string treatmentType, int lastSessionGap)
        {
            // Ethische/Causale model van generate_synthetic_data.py
            double baseProbability = 0.10;
            double gapPenalty = lastSessionGap > 21 ? 0.30 : 0.0;
            double treatmentPenalty = (treatmentType?.ToLower() == "depressie") ? 0.20 : 0.0;
            
            // Kleine statistische invloeden ter demonstratie van meervoudige regressie
            double ageInfluence = (age > 60) ? 0.05 : (age < 25 ? -0.02 : 0.0);
            double sessionInfluence = -0.005 * completedSessions; // Meer sessies = betere therapietrouw
            
            double probability = baseProbability + gapPenalty + treatmentPenalty + ageInfluence + sessionInfluence;
            
            // Terugval/Ruis mock
            double noise = (_random.NextDouble() * 0.08) - 0.04; // [-4%, +4%]
            
            return Math.Max(0.02, Math.Min(0.95, Math.Round(probability + noise, 4)));
        }

        public RLStepResult SimulateRLStep(int currentSessions, double currentStability, int currentGap, double currentSentiment, double currentSentimentEma, int action)
        {
            // Harde Safety Constraint: Crisis-interventie forceert actie 1 bij lage stabiliteit
            bool safetyTriggered = false;
            if (currentStability < 3.0 && action != 1)
            {
                action = 1;
                safetyTriggered = true;
            }

            double oldStability = currentStability;
            double oldSentimentEma = currentSentimentEma;
            
            int nextSessions = currentSessions;
            double nextStability = currentStability;
            int nextGap = currentGap;
            double nextSentiment = currentSentiment;
            double nextSentimentEma = currentSentimentEma;
            bool done = false;
            string dischargeStatus = "none";
            double reward = 0.0;

            // 1. Ontslaggerichte logica (Actie 3: Discharge)
            if (action == 3)
            {
                done = true;
                double rDischarge;
                if (currentStability >= 7.0)
                {
                    rDischarge = 50.0 + 10.0 * currentStability;
                    dischargeStatus = "success";
                }
                else
                {
                    rDischarge = -100.0 * Math.Pow(7.0 - currentStability, 2);
                    dischargeStatus = "premature";
                }
                
                return new RLStepResult
                {
                    NextState = new double[] { currentSessions, currentStability, currentGap, currentSentiment, currentSentimentEma },
                    Reward = Math.Round(rDischarge, 2),
                    Done = true,
                    Info = new { attended = true, safetyTriggered, dischargeStatus, noShowProbability = 0.0 },
                    RecommendedAction = "Discharge (Ontslag)",
                    HeuristicAction = "Discharge (Ontslag)"
                };
            }

            // 2. Normale planningsacties (0, 1, 2)
            double stabilityChangeMean = 0.1;
            double sentimentChangeMean = 0.05;
            double actionCost = 0.0;

            if (action == 0) // Standaard (7 dagen)
            {
                nextGap = 7;
                actionCost = 0.0;
                stabilityChangeMean = 0.1;
                sentimentChangeMean = 0.05;
            }
            else if (action == 1) // Intensief (3 dagen)
            {
                nextGap = 3;
                actionCost = 3.0;
                stabilityChangeMean = 0.4;
                sentimentChangeMean = 0.15;
            }
            else if (action == 2) // Digitaal check-in (7 dagen + contact)
            {
                nextGap = 7;
                actionCost = 1.0;
                stabilityChangeMean = 0.25;
                sentimentChangeMean = 0.10;
            }

            // Bereken no-show kans gebaseerd op toestand
            double baseNoShow = 0.10;
            double gapPenalty = nextGap > 14 ? 0.35 : 0.0;
            double stabilityPenalty = currentStability < 4.0 ? 0.25 : 0.0;
            double sentimentPenalty = currentSentimentEma < 0.0 ? 0.20 : 0.0;
            double digitalBonus = action == 2 ? -0.15 : 0.0;

            double noShowProb = Math.Max(0.02, Math.Min(0.95, baseNoShow + gapPenalty + stabilityPenalty + sentimentPenalty + digitalBonus));

            // Bepaal aanwezigheid via Bernoulli trial
            bool attended = _random.NextDouble() >= noShowProb;

            if (attended)
            {
                nextSessions++;
                
                // Update emotionele stabiliteit met normale distributie (Box-Muller)
                double stabilityDelta = GenerateNormalRandom(stabilityChangeMean, 0.5);
                nextStability = Math.Max(1.0, Math.Min(10.0, currentStability + stabilityDelta));

                // Sentiment gecorreleerd met stabiliteit
                double sentimentTarget = (nextStability - 5.5) / 4.5;
                double rawSentiment = GenerateNormalRandom(sentimentTarget, 0.3);
                nextSentiment = Math.Max(-1.0, Math.Min(1.0, rawSentiment));
            }
            else
            {
                nextGap += 7; // Gap groeit
                double stabilityDelta = GenerateNormalRandom(-0.8, 0.6);
                nextStability = Math.Max(1.0, Math.Min(10.0, currentStability + stabilityDelta));
                
                nextSentiment = Math.Max(-1.0, Math.Min(1.0, currentSentiment - 0.25));
            }

            // Update EMA sentiment
            double alpha = 0.3;
            nextSentimentEma = alpha * nextSentiment + (1.0 - alpha) * currentSentimentEma;

            // 3. Bereken Rewards
            double deltaStability = nextStability - oldStability;
            double deltaSentimentEma = nextSentimentEma - oldSentimentEma;
            double rProgress = 10.0 * Math.Max(0.0, deltaStability) + 5.0 * Math.Max(0.0, deltaSentimentEma);

            double cStagnation = 0.0;
            if (deltaStability <= 0.05)
            {
                cStagnation = 2.0 * (8.0 - nextStability);
            }

            double cOvertreatment = 0.0;
            int sessionLimit = 12;
            if (nextSessions > sessionLimit)
            {
                cOvertreatment = 1.5 * Math.Pow(nextSessions - sessionLimit, 1.5);
            }

            double rAttendance = attended ? 5.0 : -20.0;
            double cDelay = 0.5 * Math.Max(0.0, nextGap - 14);

            reward = rProgress + rAttendance - cStagnation - cOvertreatment - cDelay - actionCost;

            // Episode beëindiging
            if (nextStability <= 1.0)
            {
                done = true;
                reward -= 50.0; // Extra dropout boete
            }
            else if (nextSessions >= 24)
            {
                done = true;
                if (nextStability < 7.0)
                {
                    reward -= 100.0; // Onvoltooid einde boete
                }
            }

            return new RLStepResult
            {
                NextState = new double[] { nextSessions, Math.Round(nextStability, 2), nextGap, Math.Round(nextSentiment, 2), Math.Round(nextSentimentEma, 2) },
                Reward = Math.Round(reward, 2),
                Done = done,
                Info = new { attended, safetyTriggered, dischargeStatus, noShowProbability = Math.Round(noShowProb, 4) },
                RecommendedAction = GetRecommendedAction(nextSessions, nextStability, nextGap, nextSentimentEma),
                HeuristicAction = GetHeuristicAction(nextSessions, nextStability, nextGap)
            };
        }

        public async Task<(IEnumerable<SyntheticPatientModel> Patients, int TotalCount)> GetSyntheticPatientsAsync(int page, int pageSize, string search)
        {
            string csvPath = FindFile(
                "synthetic_patients.csv",
                "/home/gregory/project/AD-GregoryButs-2526/ml_training/synthetic_patients.csv"
            );

            if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
            {
                // Fallback: genereer in-memory mock data om crashen te voorkomen als het script niet gedraaid is
                var mockList = GenerateMockSyntheticPatients(5000);
                return FilterAndPaginate(mockList, page, pageSize, search);
            }

            try
            {
                var list = new List<SyntheticPatientModel>();
                var lines = await File.ReadAllLinesAsync(csvPath);
                
                // Skip header line
                for (int i = 1; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    var parts = line.Split(',');
                    if (parts.Length < 10) continue;

                    list.Add(new SyntheticPatientModel
                    {
                        ClientId = parts[0],
                        Name = parts[1].Replace("\"", ""),
                        Age = int.TryParse(parts[2], out var a) ? a : 30,
                        Email = parts[3],
                        Phone = parts[4],
                        LastSessionGap = int.TryParse(parts[5], out var g) ? g : 7,
                        TreatmentType = parts[6],
                        SessionsCompleted = int.TryParse(parts[7], out var s) ? s : 0,
                        NoShowProbability = double.TryParse(parts[8], out var np) ? np : 0.10,
                        NoShow = int.TryParse(parts[9], out var ns) ? ns : 0
                    });
                }

                return FilterAndPaginate(list, page, pageSize, search);
            }
            catch (Exception)
            {
                var mockList = GenerateMockSyntheticPatients(5000);
                return FilterAndPaginate(mockList, page, pageSize, search);
            }
        }

        public byte[] GetCorrelationHeatmapBytes()
        {
            string pngPath = FindFile(
                "no_show_correlations.png",
                "/home/gregory/project/AD-GregoryButs-2526/ml_training/no_show_correlations.png"
            );

            if (!string.IsNullOrEmpty(pngPath) && File.Exists(pngPath))
            {
                return File.ReadAllBytes(pngPath);
            }

            // Retourneer een lege byte array (controller zal dan NotFound sturen)
            return Array.Empty<byte>();
        }

        // --- Helper Methods ---

        private string FindFile(string filename, string defaultAbsolutePath)
        {
            if (File.Exists(defaultAbsolutePath)) return defaultAbsolutePath;

            string[] paths = new[]
            {
                defaultAbsolutePath,
                Path.Combine(AppContext.BaseDirectory, "../../../../ml_training/" + filename),
                Path.Combine(Directory.GetCurrentDirectory(), "ml_training/" + filename),
                Path.Combine(Directory.GetCurrentDirectory(), "../ml_training/" + filename),
                Path.Combine(Directory.GetCurrentDirectory(), "../../../../ml_training/" + filename)
            };

            foreach (var p in paths)
            {
                if (File.Exists(p)) return p;
            }

            return "";
        }

        private (IEnumerable<SyntheticPatientModel> Patients, int TotalCount) FilterAndPaginate(List<SyntheticPatientModel> list, int page, int pageSize, string search)
        {
            var query = list.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchLower) || 
                    p.ClientId.ToLower().Contains(searchLower) ||
                    p.Email.ToLower().Contains(searchLower)
                );
            }

            int totalCount = query.Count();
            var paginated = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (paginated, totalCount);
        }

        private List<SyntheticPatientModel> GenerateMockSyntheticPatients(int count)
        {
            var list = new List<SyntheticPatientModel>();
            string[] treatments = { "depressie", "angst", "burnout", "ptss", "relatie" };
            string[] names = { "Gregory Buts", "Jan Janssens", "Marie Peeters", "Pieter De Smedt", "Sofie Maes", "Annelies Visser", "Daan de Boer", "Eva Bakker" };

            for (int i = 0; i < count; i++)
            {
                var name = names[i % names.Length] + " " + (i / names.Length);
                var treatment = treatments[i % treatments.Length];
                var gap = _random.Next(1, 45);
                var age = _random.Next(18, 75);
                var sessions = _random.Next(0, 20);
                
                double baseProb = 0.10;
                double gapPen = gap > 21 ? 0.30 : 0.0;
                double treatPen = treatment == "depressie" ? 0.20 : 0.0;
                double noise = _random.NextDouble() * 0.10 - 0.05;
                double probability = Math.Max(0.02, Math.Min(0.95, baseProb + gapPen + treatPen + noise));
                int noShow = _random.NextDouble() < probability ? 1 : 0;

                list.Add(new SyntheticPatientModel
                {
                    ClientId = $"CLI-{10000 + i}",
                    Name = name,
                    Age = age,
                    Email = $"client{i}@example.com",
                    Phone = $"+32 471 {i:D6}",
                    LastSessionGap = gap,
                    TreatmentType = treatment,
                    SessionsCompleted = sessions,
                    NoShowProbability = Math.Round(probability, 4),
                    NoShow = noShow
                });
            }
            return list;
        }

        private double GenerateNormalRandom(double mean, double stdDev)
        {
            // Box-Muller transform
            double u1 = 1.0 - _random.NextDouble();
            double u2 = 1.0 - _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stdDev * randStdNormal;
        }
    }
}
