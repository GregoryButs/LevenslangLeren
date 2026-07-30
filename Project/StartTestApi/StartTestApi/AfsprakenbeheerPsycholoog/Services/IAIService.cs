using System.Collections.Generic;
using System.Threading.Tasks;

namespace AfsprakenbeheerPsycholoog.Services
{
    public interface IAIService
    {
        double GetNoShowProbability(int age, int completedSessions, string treatmentType, int lastSessionGap);
        
        RLStepResult SimulateRLStep(int currentSessions, double currentStability, int currentGap, double currentSentiment, double currentSentimentEma, int action);
        
        Task<(IEnumerable<SyntheticPatientModel> Patients, int TotalCount)> GetSyntheticPatientsAsync(int page, int pageSize, string search);
        
        byte[] GetCorrelationHeatmapBytes();
        
        string GetRecommendedAction(int sessions, double stability, int gap, double sentimentEma);
        
        string GetHeuristicAction(int sessions, double stability, int gap);
    }

    public class RLStepResult
    {
        public double[] NextState { get; set; } = null!;
        public double Reward { get; set; }
        public bool Done { get; set; }
        public object Info { get; set; } = null!;
        public string RecommendedAction { get; set; } = "";
        public string HeuristicAction { get; set; } = "";
    }

    public class SyntheticPatientModel
    {
        public string ClientId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Age { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public int LastSessionGap { get; set; }
        public string TreatmentType { get; set; } = null!;
        public int SessionsCompleted { get; set; }
        public double NoShowProbability { get; set; }
        public int NoShow { get; set; }
    }
}
