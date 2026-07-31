import React from 'react';
import { ShieldAlert, RotateCcw, Loader2 } from 'lucide-react';
import { InfoTooltip } from '../common/InfoTooltip';

interface AiPatientRiskTableProps {
  patientRisks: any[];
  risksLoading: boolean;
  onRefresh: () => void;
}

export const AiPatientRiskTable: React.FC<AiPatientRiskTableProps> = ({
  patientRisks,
  risksLoading,
  onRefresh,
}) => {
  return (
    <div className="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-6">
      <div className="flex justify-between items-center border-b border-slate-100 pb-4">
        <div>
          <h2 className="text-lg font-bold text-slate-800 flex items-center space-x-2">
            <ShieldAlert className="h-5 w-5 text-brand-600" />
            <span>Patiënt Risico & Aanbevelingen</span>
            <InfoTooltip content="Analyseert patiëntkenmerken (leeftijd, voltooide sessies, dagen sinds laatste afspraak) en voorspelt het no-show risico. De Slimme Expert en de Q-Learner stellen acties voor." />
          </h2>
          <p className="text-slate-500 text-xs mt-1">Real-time inference door het getrainde NoShowModel & Q-Learner policy.</p>
        </div>
        <button
          onClick={onRefresh}
          disabled={risksLoading}
          className="flex items-center space-x-1.5 bg-slate-100 hover:bg-slate-200 text-slate-700 font-semibold text-xs py-2 px-3.5 rounded-xl transition"
        >
          <RotateCcw className={`h-3.5 w-3.5 ${risksLoading ? 'animate-spin' : ''}`} />
          <span>Vernieuwen</span>
        </button>
      </div>

      {risksLoading ? (
        <div className="flex items-center justify-center py-12 text-slate-500 space-x-2">
          <Loader2 className="h-5 w-5 animate-spin text-brand-600" />
          <span>Analyseren via AI-modellen...</span>
        </div>
      ) : patientRisks.length > 0 ? (
        <div className="overflow-x-auto">
          <table className="w-full text-left border-collapse">
            <thead>
              <tr className="border-b border-slate-100 text-slate-400 text-xs font-semibold uppercase">
                <th className="py-3 px-4">Patiënt</th>
                <th className="py-3 px-4">Leeftijd</th>
                <th className="py-3 px-4">Voltooide Sessies</th>
                <th className="py-3 px-4">Tussenpoos (Gap)</th>
                <th className="py-3 px-4">Type Behandeling</th>
                <th className="py-3 px-4 text-center">No-Show Risico</th>
                <th className="py-3 px-4 text-center">Slimme Expert</th>
                <th className="py-3 px-4 text-center">Q-Learner (AI)</th>
                <th className="py-3 px-4">AI Verklaring</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100 text-sm">
              {patientRisks.map((r: any) => (
                <tr key={r.patientId} className="hover:bg-slate-50/50">
                  <td className="py-4 px-4 font-bold text-slate-800">{r.volledigeNaam}</td>
                  <td className="py-4 px-4 text-slate-600">{r.age} jaar</td>
                  <td className="py-4 px-4 text-slate-600">{r.sessionsCompleted} sessies</td>
                  <td className="py-4 px-4 text-slate-600">{r.lastSessionGap} dagen</td>
                  <td className="py-4 px-4">
                    <span className="capitalize px-2.5 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-700">
                      {r.treatmentType}
                    </span>
                  </td>
                  <td className="py-4 px-4 text-center">
                    <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-bold ${
                      r.riskCategory === 'High' ? 'bg-red-100 text-red-800' :
                      r.riskCategory === 'Medium' ? 'bg-amber-100 text-amber-800' :
                      'bg-emerald-100 text-emerald-800'
                    }`}>
                      {(r.noShowProbability * 100).toFixed(1)}% ({r.riskCategory})
                    </span>
                  </td>
                  <td className="py-4 px-4 text-center">
                    <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-bold ${
                      r.heuristicAction.includes("Discharge") || r.heuristicAction.includes("Ontslag") ? 'bg-blue-100 text-blue-800 border border-blue-200' :
                      r.heuristicAction.includes("Intensief") ? 'bg-purple-100 text-purple-800 border border-purple-200' :
                      r.heuristicAction.includes("Check-in") ? 'bg-cyan-100 text-cyan-800 border border-cyan-200' :
                      'bg-slate-100 text-slate-800 border border-slate-200'
                    }`}>
                      {r.heuristicAction}
                    </span>
                  </td>
                  <td className="py-4 px-4 text-center">
                    <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-bold ${
                      r.recommendedAction.includes("Discharge") || r.recommendedAction.includes("Ontslag") ? 'bg-emerald-100 text-emerald-800 border border-emerald-200' :
                      r.recommendedAction.includes("Intensief") ? 'bg-purple-100 text-purple-800 border border-purple-200' :
                      r.recommendedAction.includes("Check-in") ? 'bg-cyan-100 text-cyan-800 border border-cyan-200' :
                      r.recommendedAction.includes("Niet getraind") ? 'bg-amber-100 text-amber-800 border border-amber-200 font-semibold' :
                      'bg-slate-100 text-slate-800 border border-slate-200'
                    }`}>
                      {r.recommendedAction}
                    </span>
                  </td>
                  <td className="py-4 px-4 text-xs text-slate-500 italic max-w-xs">{r.reason}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="text-center py-12 text-slate-400">Geen actieve patiëntgegevens beschikbaar.</div>
      )}
    </div>
  );
};
