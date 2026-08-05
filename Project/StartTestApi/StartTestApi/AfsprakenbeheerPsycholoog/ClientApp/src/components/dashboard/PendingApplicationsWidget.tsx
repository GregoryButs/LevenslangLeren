import React, { useState } from 'react';
import { UserCheck, Plus, Eye, Link as LinkIcon, Search, X, Clock, RotateCcw } from 'lucide-react';
import { Patient } from '../../types';
import { AanmeldingDetailModal } from './AanmeldingDetailModal';
import { patientApi } from '../../services/api';
import { extractErrorMessage } from '../../utils/errorUtils';

interface PendingApplicationsWidgetProps {
  aanmeldingen: any[];
  wachtlijst?: any[];
  patientsList: Patient[];
  selectedPatientForLink: { [key: string]: string };
  onSelectPatientChange: (appId: string | number, patientId: string) => void;
  onApproveNewPatient: (appId: any) => void;
  onLinkExistingPatient: (email: string, appId: any) => void;
  onWeigerAanmelding?: (appId: any) => void;
  onHerstelWachtlijst?: (appId: any) => void;
  onRefreshDashboard?: () => void;
}

export const PendingApplicationsWidget: React.FC<PendingApplicationsWidgetProps> = ({
  aanmeldingen,
  wachtlijst = [],
  patientsList,
  selectedPatientForLink,
  onSelectPatientChange,
  onApproveNewPatient,
  onLinkExistingPatient,
  onWeigerAanmelding,
  onHerstelWachtlijst,
  onRefreshDashboard,
}) => {
  const [activeTab, setActiveTab] = useState<'aanmeldingen' | 'wachtlijst'>('aanmeldingen');
  const [selectedAppForModal, setSelectedAppForModal] = useState<any | null>(null);
  const [patientSearchQueries, setPatientSearchQueries] = useState<{ [appId: string]: string }>({});

  const handleSuccess = () => {
    if (onRefreshDashboard) {
      onRefreshDashboard();
    }
  };

  const handleWeiger = async (appId: string | number) => {
    if (onWeigerAanmelding) {
      onWeigerAanmelding(appId);
      return;
    }
    try {
      await patientApi.weigerAanmelding(String(appId));
      alert('Patiënt op de wachtlijst geplaatst.');
      handleSuccess();
    } catch (err) {
      console.error(err);
      alert(extractErrorMessage(err, 'Weigeren mislukt.'));
    }
  };

  const handleHerstel = async (appId: string | number) => {
    if (onHerstelWachtlijst) {
      onHerstelWachtlijst(appId);
      return;
    }
    try {
      await patientApi.herstelWachtlijst(String(appId));
      alert('Patiënt hersteld naar de actieve aanmeldingen.');
      handleSuccess();
    } catch (err) {
      console.error(err);
      alert(extractErrorMessage(err, 'Herstellen mislukt.'));
    }
  };

  const formatDate = (dateStr?: string) => {
    if (!dateStr) return '';
    try {
      return new Date(dateStr).toLocaleDateString('nl-BE', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
      });
    } catch {
      return dateStr;
    }
  };

  const currentList = activeTab === 'aanmeldingen' ? aanmeldingen : wachtlijst;

  return (
    <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-6 space-y-5 transition-colors">
      {/* Header & Tabs */}
      <div className="space-y-3">
        <div className="flex justify-between items-center">
          <h2 className="text-lg font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <UserCheck className="h-5 w-5 text-brand-600 dark:text-brand-400" />
            <span>Patiënten Aanmeldingen</span>
          </h2>
        </div>

        {/* Tab Selection */}
        <div className="flex space-x-1 bg-slate-100 dark:bg-brand-950 p-1 rounded-2xl border border-slate-200/50 dark:border-brand-800/40">
          <button
            type="button"
            onClick={() => setActiveTab('aanmeldingen')}
            className={`flex-1 py-1.5 px-3 rounded-xl text-xs font-bold transition flex items-center justify-center space-x-1.5 ${
              activeTab === 'aanmeldingen'
                ? 'bg-white dark:bg-brand-800 text-brand-700 dark:text-white shadow-xs'
                : 'text-slate-500 dark:text-brand-300 hover:text-slate-700 dark:hover:text-white'
            }`}
          >
            <span>Nieuw ({aanmeldingen.length})</span>
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('wachtlijst')}
            className={`flex-1 py-1.5 px-3 rounded-xl text-xs font-bold transition flex items-center justify-center space-x-1.5 ${
              activeTab === 'wachtlijst'
                ? 'bg-white dark:bg-brand-800 text-amber-700 dark:text-amber-300 shadow-xs'
                : 'text-slate-500 dark:text-brand-300 hover:text-slate-700 dark:hover:text-white'
            }`}
          >
            <Clock className="h-3.5 w-3.5 text-amber-500" />
            <span>Wachtlijst ({wachtlijst.length})</span>
          </button>
        </div>
      </div>

      {/* Applications / Waitlist List */}
      <div className="space-y-4 max-h-[500px] overflow-y-auto pr-1">
        {currentList.length > 0 ? (
          currentList.map((app) => {
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
                    <div className="flex items-center space-x-2">
                      <h4 className="font-semibold text-slate-800 dark:text-white truncate">{app.voornaam} {app.achternaam}</h4>
                      {activeTab === 'wachtlijst' && app.wachtlijstDatum && (
                        <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-amber-100 dark:bg-amber-950 text-amber-800 dark:text-amber-300 border border-amber-200/50 dark:border-amber-800/40">
                          {formatDate(app.wachtlijstDatum)}
                        </span>
                      )}
                    </div>
                    <p className="text-xs text-slate-500 dark:text-brand-300 truncate">{app.email}</p>
                  </div>
                  <div className="flex items-center space-x-1">
                    {activeTab === 'aanmeldingen' && (
                      <button
                        type="button"
                        onClick={() => handleWeiger(app.id)}
                        className="shrink-0 p-1.5 text-xs font-semibold text-amber-700 dark:text-amber-300 hover:bg-amber-100 dark:hover:bg-amber-900/60 border border-amber-200 dark:border-amber-800/50 rounded-xl transition cursor-pointer"
                        title="Tijdelijk weigeren (Op wachtlijst plaatsen)"
                      >
                        <Clock className="h-3.5 w-3.5" />
                      </button>
                    )}
                    {activeTab === 'wachtlijst' && (
                      <button
                        type="button"
                        onClick={() => handleHerstel(app.id)}
                        className="shrink-0 flex items-center space-x-1 text-xs font-semibold text-slate-700 dark:text-brand-200 hover:bg-slate-200 dark:hover:bg-brand-800 border border-slate-200 dark:border-brand-800 py-1.5 px-2.5 rounded-xl transition cursor-pointer"
                        title="Herstellen naar nieuwe aanmeldingen"
                      >
                        <RotateCcw className="h-3.5 w-3.5" />
                        <span>Herstel</span>
                      </button>
                    )}
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
            {activeTab === 'aanmeldingen' ? 'Geen openstaande nieuwe aanmeldingen.' : 'Geen patiënten op de wachtlijst.'}
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
          onWeigerAanmelding={handleWeiger}
          onClose={() => setSelectedAppForModal(null)}
          onSuccess={handleSuccess}
        />
      )}
    </div>
  );
};
