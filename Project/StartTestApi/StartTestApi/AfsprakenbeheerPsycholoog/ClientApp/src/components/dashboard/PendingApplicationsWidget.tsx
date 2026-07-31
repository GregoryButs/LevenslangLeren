import React from 'react';
import { UserCheck, Plus } from 'lucide-react';
import { Patient } from '../../types';

interface PendingApplicationsWidgetProps {
  aanmeldingen: any[];
  patientsList: Patient[];
  selectedPatientForLink: { [key: string]: string };
  onSelectPatientChange: (appId: string | number, patientId: string) => void;
  onApproveNewPatient: (appId: any) => void;
  onLinkExistingPatient: (email: string, appId: any) => void;
}

export const PendingApplicationsWidget: React.FC<PendingApplicationsWidgetProps> = ({
  aanmeldingen,
  patientsList,
  selectedPatientForLink,
  onSelectPatientChange,
  onApproveNewPatient,
  onLinkExistingPatient,
}) => {
  return (
    <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-6 space-y-6 transition-colors">
      <h2 className="text-lg font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
        <UserCheck className="h-5 w-5 text-brand-600 dark:text-brand-400" />
        <span>Nieuwe Aanmeldingen</span>
      </h2>

      <div className="space-y-4 max-h-[500px] overflow-y-auto pr-1">
        {aanmeldingen.length > 0 ? (
          aanmeldingen.map((app) => (
            <div key={app.id} className="p-4 bg-slate-50 dark:bg-brand-950/60 border border-slate-100 dark:border-brand-800/40 rounded-2xl space-y-3">
              <div>
                <h4 className="font-semibold text-slate-800 dark:text-white">{app.voornaam} {app.achternaam}</h4>
                <p className="text-xs text-slate-500 dark:text-brand-300">{app.email}</p>
              </div>
              <div className="flex flex-col gap-2 pt-1 border-t border-slate-100 dark:border-brand-800/40">
                <button
                  onClick={() => onApproveNewPatient(app.id)}
                  className="w-full text-xs font-semibold bg-brand-50 dark:bg-brand-800/60 hover:bg-brand-100 dark:hover:bg-brand-800 text-brand-700 dark:text-brand-200 py-2 px-3 rounded-xl transition flex items-center justify-center space-x-1 cursor-pointer"
                >
                  <Plus className="h-3.5 w-3.5" />
                  <span>Maak & koppel nieuwe patiënt</span>
                </button>

                <div className="flex gap-1.5 mt-1">
                  <select
                    value={selectedPatientForLink[app.id] || ''}
                    onChange={(e) => onSelectPatientChange(app.id, e.target.value)}
                    className="text-xs w-full bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-800 py-1.5 px-2 rounded-xl text-slate-700 dark:text-brand-100 focus:outline-none"
                  >
                    <option value="">-- Bestaande patiënt --</option>
                    {patientsList.map(p => (
                      <option key={p.id} value={p.id}>{p.volledigeNaam}</option>
                    ))}
                  </select>
                  <button
                    onClick={() => onLinkExistingPatient(app.email, app.id)}
                    className="text-xs font-semibold bg-emerald-600 hover:bg-emerald-700 text-white py-1.5 px-3 rounded-xl transition cursor-pointer"
                  >
                    Koppel
                  </button>
                </div>
              </div>
            </div>
          ))
        ) : (
          <div className="text-center py-8 text-slate-400 dark:text-brand-400 text-sm">
            Geen openstaande nieuwe aanmeldingen.
          </div>
        )}
      </div>
    </div>
  );
};
