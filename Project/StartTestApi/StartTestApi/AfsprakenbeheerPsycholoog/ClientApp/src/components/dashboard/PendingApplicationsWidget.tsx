import React, { useState } from 'react';
import { UserCheck, Plus, Eye, Link as LinkIcon, Search, X } from 'lucide-react';
import { Patient } from '../../types';
import { AanmeldingDetailModal } from './AanmeldingDetailModal';

interface PendingApplicationsWidgetProps {
  aanmeldingen: any[];
  patientsList: Patient[];
  selectedPatientForLink: { [key: string]: string };
  onSelectPatientChange: (appId: string | number, patientId: string) => void;
  onApproveNewPatient: (appId: any) => void;
  onLinkExistingPatient: (email: string, appId: any) => void;
  onRefreshDashboard?: () => void;
}

export const PendingApplicationsWidget: React.FC<PendingApplicationsWidgetProps> = ({
  aanmeldingen,
  patientsList,
  selectedPatientForLink,
  onSelectPatientChange,
  onApproveNewPatient,
  onLinkExistingPatient,
  onRefreshDashboard,
}) => {
  const [selectedAppForModal, setSelectedAppForModal] = useState<any | null>(null);
  const [patientSearchQueries, setPatientSearchQueries] = useState<{ [appId: string]: string }>({});

  const handleSuccess = () => {
    if (onRefreshDashboard) {
      onRefreshDashboard();
    }
  };

  return (
    <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-6 space-y-6 transition-colors">
      <div className="flex justify-between items-center">
        <h2 className="text-lg font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
          <UserCheck className="h-5 w-5 text-brand-600 dark:text-brand-400" />
          <span>Nieuwe Aanmeldingen</span>
        </h2>
        {aanmeldingen.length > 0 && (
          <span className="bg-brand-100 dark:bg-brand-800 text-brand-800 dark:text-brand-200 text-xs font-bold px-2.5 py-1 rounded-full">
            {aanmeldingen.length} {aanmeldingen.length === 1 ? 'aanmelding' : 'aanmeldingen'}
          </span>
        )}
      </div>

      <div className="space-y-4 max-h-[500px] overflow-y-auto pr-1">
        {aanmeldingen.length > 0 ? (
          aanmeldingen.map((app) => {
            const searchVal = patientSearchQueries[app.id] || '';
            const filteredPatients = patientsList.filter(p => {
              const query = searchVal.toLowerCase().trim();
              if (!query) return true;
              const fullName = `${p.voornaam || ''} ${p.achternaam || ''} ${p.volledigeNaam || ''}`.toLowerCase();
              const email = (p.email || '').toLowerCase();
              const dossier = (p.dossierNummer || '').toLowerCase();
              return fullName.includes(query) || email.includes(query) || dossier.includes(query);
            });

            return (
              <div key={app.id} className="p-4 bg-slate-50 dark:bg-brand-950/60 border border-slate-100 dark:border-brand-800/40 rounded-2xl space-y-3 hover:border-brand-200 dark:hover:border-brand-700 transition">
                <div className="flex justify-between items-start gap-2">
                  <div className="min-w-0 flex-1">
                    <h4 className="font-semibold text-slate-800 dark:text-white truncate">{app.voornaam} {app.achternaam}</h4>
                    <p className="text-xs text-slate-500 dark:text-brand-300 truncate">{app.email}</p>
                  </div>
                  <button
                    type="button"
                    onClick={() => setSelectedAppForModal(app)}
                    className="shrink-0 flex items-center space-x-1 text-xs font-semibold text-brand-600 dark:text-brand-400 hover:text-brand-700 dark:hover:text-brand-300 bg-brand-50 dark:bg-brand-900/80 hover:bg-brand-100 dark:hover:bg-brand-800 border border-brand-200/60 dark:border-brand-800 py-1.5 px-2.5 rounded-xl transition cursor-pointer"
                    title="Bekijk alle registratie-invulvelden"
                  >
                    <Eye className="h-3.5 w-3.5 shrink-0" />
                    <span>Detail</span>
                  </button>
                </div>

                <div className="flex flex-col gap-2 pt-2 border-t border-slate-100 dark:border-brand-800/40">
                  <button
                    onClick={() => onApproveNewPatient(app.id)}
                    className="w-full text-xs font-semibold bg-brand-50 dark:bg-brand-800/60 hover:bg-brand-100 dark:hover:bg-brand-800 text-brand-700 dark:text-brand-200 py-2 px-3 rounded-xl transition flex items-center justify-center space-x-1 cursor-pointer"
                  >
                    <Plus className="h-3.5 w-3.5" />
                    <span>Maak & koppel nieuwe patiënt</span>
                  </button>

                  <div className="space-y-1.5 mt-1">
                    {/* Patient Search Input */}
                    <div className="relative">
                      <Search className="absolute left-2.5 top-2 h-3.5 w-3.5 text-slate-400 dark:text-brand-300" />
                      <input
                        type="text"
                        value={searchVal}
                        onChange={(e) => setPatientSearchQueries({ ...patientSearchQueries, [app.id]: e.target.value })}
                        placeholder="Zoek bestaande patiënt (naam/email)..."
                        className="pl-8 pr-7 text-xs w-full bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-800 py-1.5 px-2 rounded-xl text-slate-700 dark:text-brand-100 placeholder-slate-400 dark:placeholder-brand-400 focus:outline-none focus:ring-1 focus:ring-emerald-500 font-medium"
                      />
                      {searchVal && (
                        <button
                          type="button"
                          onClick={() => setPatientSearchQueries({ ...patientSearchQueries, [app.id]: '' })}
                          className="absolute right-2 top-2 text-slate-400 hover:text-slate-600 dark:hover:text-white"
                        >
                          <X className="h-3.5 w-3.5" />
                        </button>
                      )}
                    </div>

                    <div className="flex gap-1.5">
                      <select
                        value={selectedPatientForLink[app.id] || ''}
                        onChange={(e) => onSelectPatientChange(app.id, e.target.value)}
                        className="text-xs w-full bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-800 py-1.5 px-2 rounded-xl text-slate-700 dark:text-brand-100 focus:outline-none focus:ring-1 focus:ring-emerald-500 font-semibold"
                      >
                        <option value="">
                          {filteredPatients.length > 0 
                            ? `-- Bestaande patiënt (${filteredPatients.length}) --` 
                            : '-- Geen patiënten gevonden --'}
                        </option>
                        {filteredPatients.map(p => (
                          <option key={p.id} value={p.id}>{p.volledigeNaam} ({p.email || 'Geen email'})</option>
                        ))}
                      </select>
                      <button
                        onClick={() => onLinkExistingPatient(app.email, app.id)}
                        className="text-xs font-semibold bg-emerald-600 hover:bg-emerald-700 text-white py-1.5 px-3 rounded-xl transition cursor-pointer flex items-center space-x-1 shrink-0 shadow-xs"
                      >
                        <LinkIcon className="h-3 w-3" />
                        <span>Koppel</span>
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            );
          })
        ) : (
          <div className="text-center py-8 text-slate-400 dark:text-brand-400 text-sm">
            Geen openstaande nieuwe aanmeldingen.
          </div>
        )}
      </div>

      {/* Modal for viewing and editing registration details */}
      {selectedAppForModal && (
        <AanmeldingDetailModal
          aanmelding={selectedAppForModal}
          patientsList={patientsList}
          selectedPatientId={selectedPatientForLink[selectedAppForModal.id] || ''}
          onSelectPatientChange={(patientId) => onSelectPatientChange(selectedAppForModal.id, patientId)}
          onApproveNewPatient={onApproveNewPatient}
          onLinkExistingPatient={onLinkExistingPatient}
          onClose={() => setSelectedAppForModal(null)}
          onSuccess={handleSuccess}
        />
      )}
    </div>
  );
};
