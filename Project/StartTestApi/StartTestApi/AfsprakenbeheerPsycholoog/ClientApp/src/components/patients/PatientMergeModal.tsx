import React, { useState } from 'react';
import { Patient } from '../../types';
import { patientApi } from '../../services/api';
import { getPatientDisplayName } from '../../utils/patientUtils';
import { extractErrorMessage } from '../../utils/errorUtils';
import { Merge, AlertCircle, CheckCircle2, Loader2, ArrowRight } from 'lucide-react';

interface PatientMergeModalProps {
  patientA: Patient;
  patientB: Patient;
  onClose: () => void;
  onSuccess: () => void;
}

export const PatientMergeModal: React.FC<PatientMergeModalProps> = ({
  patientA,
  patientB,
  onClose,
  onSuccess
}) => {
  // Target patient determines which ID is kept active
  const [targetId, setTargetId] = useState<number>(patientA.id);
  const sourceId = targetId === patientA.id ? patientB.id : patientA.id;

  // Selected values for merged record
  const [selectedFields, setSelectedFields] = useState({
    voornaam: patientA.voornaam || '',
    achternaam: patientA.achternaam || '',
    geboortedatum: patientA.geboortedatum ? patientA.geboortedatum.split('T')[0] : '',
    email: patientA.email || '',
    secundairEmail: patientA.secundairEmail || patientB.email || '',
    telefoonnummer: patientA.telefoonnummer || patientB.telefoonnummer || '',
    dossierNummer: patientA.dossierNummer || patientB.dossierNummer || '',
    rijksregisternummer: patientA.rijksregisternummer || patientB.rijksregisternummer || '',
    emotioneleStabiliteit: patientA.emotioneleStabiliteit !== null && patientA.emotioneleStabiliteit !== undefined 
      ? patientA.emotioneleStabiliteit 
      : (patientB.emotioneleStabiliteit ?? 5.5)
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleFieldSelect = (field: string, value: any) => {
    setSelectedFields((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleMergeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      let formattedDate = selectedFields.geboortedatum;
      if (formattedDate.includes('T')) {
        formattedDate = formattedDate.split('T')[0];
      }

      await patientApi.merge({
        targetPatientId: targetId,
        sourcePatientId: sourceId,
        voornaam: selectedFields.voornaam.trim(),
        achternaam: selectedFields.achternaam.trim(),
        geboortedatum: formattedDate,
        email: selectedFields.email.trim(),
        secundairEmail: selectedFields.secundairEmail?.trim() || null,
        telefoonnummer: selectedFields.telefoonnummer?.trim() || null,
        dossierNummer: selectedFields.dossierNummer?.trim() || null,
        rijksregisternummer: selectedFields.rijksregisternummer?.trim() || null,
        emotioneleStabiliteit: typeof selectedFields.emotioneleStabiliteit === 'number' 
          ? selectedFields.emotioneleStabiliteit 
          : parseFloat(String(selectedFields.emotioneleStabiliteit)) || 5.5
      });

      alert(`Patiënten succesvol samengevoegd! Dossier #${sourceId} is overgezet naar #${targetId}.`);
      onSuccess();
    } catch (err) {
      console.error(err);
      setError(extractErrorMessage(err, 'Samenvoegen mislukt. Controleer de geselecteerde gegevens.'));
    } finally {
      setLoading(false);
    }
  };

  const apptCountA = patientA.afspraken?.length || 0;
  const apptCountB = patientB.afspraken?.length || 0;
  const totalAppts = apptCountA + apptCountB;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/70 dark:bg-black/80 backdrop-blur-md p-4 animate-in fade-in duration-200">
      <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-3xl max-h-[90vh] flex flex-col border border-slate-100 dark:border-brand-800/40 transition-colors overflow-hidden">
        {/* Header */}
        <div className="p-6 border-b border-slate-100 dark:border-brand-800/40 flex justify-between items-center bg-slate-50/50 dark:bg-brand-950/50">
          <div className="flex items-center space-x-3">
            <div className="p-2.5 bg-brand-100 dark:bg-brand-800/60 rounded-2xl text-brand-700 dark:text-brand-300">
              <Merge className="h-6 w-6" />
            </div>
            <div>
              <h3 className="text-xl font-bold text-slate-800 dark:text-white">Patiënten Samenvoegen (Merge)</h3>
              <p className="text-xs text-slate-500 dark:text-brand-300 mt-0.5">
                Vergelijk 2 dossiers en stel de gewenste gegevens samen voor het definitieve profiel.
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="text-slate-400 hover:text-slate-600 dark:hover:text-white font-bold text-lg px-2"
          >
            ✕
          </button>
        </div>

        {/* Form Body */}
        <form onSubmit={handleMergeSubmit} className="flex flex-col flex-1 min-h-0">
          <div className="flex-1 overflow-y-auto p-6 space-y-6">
            {error && (
            <div className="p-4 bg-red-50 dark:bg-red-950/70 border border-red-200 dark:border-red-800 rounded-2xl text-red-700 dark:text-red-300 text-sm flex items-start space-x-3">
              <AlertCircle className="w-5 h-5 text-red-500 shrink-0 mt-0.5" />
              <div>
                <p className="font-bold text-xs uppercase tracking-wide">Fout bij Samenvoegen</p>
                <p className="text-xs mt-1">{error}</p>
              </div>
            </div>
          )}

          {/* Primary Record Selection */}
          <div className="bg-brand-50/60 dark:bg-brand-950/80 p-4 rounded-2xl border border-brand-100 dark:border-brand-800/60 space-y-2">
            <h4 className="text-sm font-bold text-brand-900 dark:text-brand-100">1. Kies het Primaire Dossier (Target ID)</h4>
            <p className="text-xs text-brand-700 dark:text-brand-300">
              Het niet-gekozen dossier wordt samengevoegd en vervolgens gearchiveerd. Alle afspraken ({totalAppts} totaal) worden overgezet.
            </p>
            <div className="grid grid-cols-2 gap-3 pt-2">
              <button
                type="button"
                onClick={() => setTargetId(patientA.id)}
                className={`p-3 rounded-xl border text-left transition flex items-center justify-between ${
                  targetId === patientA.id
                    ? 'bg-white dark:bg-brand-800 border-brand-500 ring-2 ring-brand-500/20 text-brand-900 dark:text-white shadow-xs font-semibold'
                    : 'bg-slate-50 dark:bg-brand-950/50 border-slate-200 dark:border-brand-800 text-slate-600 dark:text-brand-300'
                }`}
              >
                <div>
                  <span className="text-xs font-mono font-bold block text-slate-400 dark:text-brand-400">Dossier #{patientA.id}</span>
                  <span className="truncate block font-semibold">{getPatientDisplayName(patientA)}</span>
                  <span className="text-[11px] text-slate-500 dark:text-brand-300 block">{apptCountA} afspraken</span>
                </div>
                {targetId === patientA.id && <CheckCircle2 className="h-5 w-5 text-brand-600 dark:text-brand-400" />}
              </button>

              <button
                type="button"
                onClick={() => setTargetId(patientB.id)}
                className={`p-3 rounded-xl border text-left transition flex items-center justify-between ${
                  targetId === patientB.id
                    ? 'bg-white dark:bg-brand-800 border-brand-500 ring-2 ring-brand-500/20 text-brand-900 dark:text-white shadow-xs font-semibold'
                    : 'bg-slate-50 dark:bg-brand-950/50 border-slate-200 dark:border-brand-800 text-slate-600 dark:text-brand-300'
                }`}
              >
                <div>
                  <span className="text-xs font-mono font-bold block text-slate-400 dark:text-brand-400">Dossier #{patientB.id}</span>
                  <span className="truncate block font-semibold">{getPatientDisplayName(patientB)}</span>
                  <span className="text-[11px] text-slate-500 dark:text-brand-300 block">{apptCountB} afspraken</span>
                </div>
                {targetId === patientB.id && <CheckCircle2 className="h-5 w-5 text-brand-600 dark:text-brand-400" />}
              </button>
            </div>
          </div>

          {/* Field-by-Field Comparison Table */}
          <div className="space-y-3">
            <h4 className="text-sm font-bold text-slate-800 dark:text-white">2. Selecteer welke velden behouden blijven</h4>
            <div className="border border-slate-200 dark:border-brand-800 rounded-2xl overflow-hidden text-xs">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="bg-slate-100 dark:bg-brand-950 text-slate-600 dark:text-brand-300 font-bold border-b border-slate-200 dark:border-brand-800">
                    <th className="p-3 w-1/4">Veld</th>
                    <th className="p-3 w-1/3">Patiënt A (Dossier #{patientA.id})</th>
                    <th className="p-3 w-1/3">Patiënt B (Dossier #{patientB.id})</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 dark:divide-brand-800/60 text-slate-800 dark:text-brand-100">
                  {/* Voornaam */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Voornaam</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="voornaam"
                          checked={selectedFields.voornaam === patientA.voornaam}
                          onChange={() => handleFieldSelect('voornaam', patientA.voornaam)}
                          className="accent-brand-600"
                        />
                        <span className={selectedFields.voornaam === patientA.voornaam ? 'font-bold text-brand-700 dark:text-brand-300' : ''}>
                          {patientA.voornaam || '—'}
                        </span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="voornaam"
                          checked={selectedFields.voornaam === patientB.voornaam}
                          onChange={() => handleFieldSelect('voornaam', patientB.voornaam)}
                          className="accent-brand-600"
                        />
                        <span className={selectedFields.voornaam === patientB.voornaam ? 'font-bold text-brand-700 dark:text-brand-300' : ''}>
                          {patientB.voornaam || '—'}
                        </span>
                      </label>
                    </td>
                  </tr>

                  {/* Achternaam */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Achternaam</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="achternaam"
                          checked={selectedFields.achternaam === patientA.achternaam}
                          onChange={() => handleFieldSelect('achternaam', patientA.achternaam)}
                          className="accent-brand-600"
                        />
                        <span className={selectedFields.achternaam === patientA.achternaam ? 'font-bold text-brand-700 dark:text-brand-300' : ''}>
                          {patientA.achternaam || '—'}
                        </span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="achternaam"
                          checked={selectedFields.achternaam === patientB.achternaam}
                          onChange={() => handleFieldSelect('achternaam', patientB.achternaam)}
                          className="accent-brand-600"
                        />
                        <span className={selectedFields.achternaam === patientB.achternaam ? 'font-bold text-brand-700 dark:text-brand-300' : ''}>
                          {patientB.achternaam || '—'}
                        </span>
                      </label>
                    </td>
                  </tr>

                  {/* Geboortedatum */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Geboortedatum</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="geboortedatum"
                          checked={selectedFields.geboortedatum === (patientA.geboortedatum ? patientA.geboortedatum.split('T')[0] : '')}
                          onChange={() => handleFieldSelect('geboortedatum', patientA.geboortedatum ? patientA.geboortedatum.split('T')[0] : '')}
                          className="accent-brand-600"
                        />
                        <span className={selectedFields.geboortedatum === (patientA.geboortedatum ? patientA.geboortedatum.split('T')[0] : '') ? 'font-bold text-brand-700 dark:text-brand-300' : ''}>
                          {patientA.geboortedatum ? new Date(patientA.geboortedatum).toLocaleDateString('nl-NL') : '—'}
                        </span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="geboortedatum"
                          checked={selectedFields.geboortedatum === (patientB.geboortedatum ? patientB.geboortedatum.split('T')[0] : '')}
                          onChange={() => handleFieldSelect('geboortedatum', patientB.geboortedatum ? patientB.geboortedatum.split('T')[0] : '')}
                          className="accent-brand-600"
                        />
                        <span className={selectedFields.geboortedatum === (patientB.geboortedatum ? patientB.geboortedatum.split('T')[0] : '') ? 'font-bold text-brand-700 dark:text-brand-300' : ''}>
                          {patientB.geboortedatum ? new Date(patientB.geboortedatum).toLocaleDateString('nl-NL') : '—'}
                        </span>
                      </label>
                    </td>
                  </tr>

                  {/* Primair E-mail */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Primair E-mail</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="email"
                          checked={selectedFields.email === patientA.email}
                          onChange={() => {
                            handleFieldSelect('email', patientA.email);
                            if (patientB.email && patientB.email !== patientA.email) {
                              handleFieldSelect('secundairEmail', patientB.email);
                            }
                          }}
                          className="accent-brand-600"
                        />
                        <span className={`break-all ${selectedFields.email === patientA.email ? 'font-bold text-brand-700 dark:text-brand-300' : ''}`}>
                          {patientA.email}
                        </span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="email"
                          checked={selectedFields.email === patientB.email}
                          onChange={() => {
                            handleFieldSelect('email', patientB.email);
                            if (patientA.email && patientA.email !== patientB.email) {
                              handleFieldSelect('secundairEmail', patientA.email);
                            }
                          }}
                          className="accent-brand-600"
                        />
                        <span className={`break-all ${selectedFields.email === patientB.email ? 'font-bold text-brand-700 dark:text-brand-300' : ''}`}>
                          {patientB.email}
                        </span>
                      </label>
                    </td>
                  </tr>

                  {/* Telefoonnummer */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Telefoonnummer</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="telefoonnummer"
                          checked={selectedFields.telefoonnummer === (patientA.telefoonnummer || '')}
                          onChange={() => handleFieldSelect('telefoonnummer', patientA.telefoonnummer || '')}
                          className="accent-brand-600"
                        />
                        <span>{patientA.telefoonnummer || '—'}</span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="telefoonnummer"
                          checked={selectedFields.telefoonnummer === (patientB.telefoonnummer || '')}
                          onChange={() => handleFieldSelect('telefoonnummer', patientB.telefoonnummer || '')}
                          className="accent-brand-600"
                        />
                        <span>{patientB.telefoonnummer || '—'}</span>
                      </label>
                    </td>
                  </tr>

                  {/* Dossiernummer */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Dossiernummer</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="dossierNummer"
                          checked={selectedFields.dossierNummer === (patientA.dossierNummer || '')}
                          onChange={() => handleFieldSelect('dossierNummer', patientA.dossierNummer || '')}
                          className="accent-brand-600"
                        />
                        <span className="font-mono">{patientA.dossierNummer || '—'}</span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="dossierNummer"
                          checked={selectedFields.dossierNummer === (patientB.dossierNummer || '')}
                          onChange={() => handleFieldSelect('dossierNummer', patientB.dossierNummer || '')}
                          className="accent-brand-600"
                        />
                        <span className="font-mono">{patientB.dossierNummer || '—'}</span>
                      </label>
                    </td>
                  </tr>

                  {/* Rijksregisternummer */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Rijksregisternummer</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="rijksregisternummer"
                          checked={selectedFields.rijksregisternummer === (patientA.rijksregisternummer || '')}
                          onChange={() => handleFieldSelect('rijksregisternummer', patientA.rijksregisternummer || '')}
                          className="accent-brand-600"
                        />
                        <span className="font-mono">{patientA.rijksregisternummer || '—'}</span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="rijksregisternummer"
                          checked={selectedFields.rijksregisternummer === (patientB.rijksregisternummer || '')}
                          onChange={() => handleFieldSelect('rijksregisternummer', patientB.rijksregisternummer || '')}
                          className="accent-brand-600"
                        />
                        <span className="font-mono">{patientB.rijksregisternummer || '—'}</span>
                      </label>
                    </td>
                  </tr>

                  {/* Emotionele Stabiliteit */}
                  <tr className="hover:bg-slate-50/50 dark:hover:bg-brand-950/40">
                    <td className="p-3 font-semibold text-slate-500 dark:text-brand-300">Emotionele Stabiliteit</td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="emotioneleStabiliteit"
                          checked={selectedFields.emotioneleStabiliteit === (patientA.emotioneleStabiliteit ?? 5.5)}
                          onChange={() => handleFieldSelect('emotioneleStabiliteit', patientA.emotioneleStabiliteit ?? 5.5)}
                          className="accent-brand-600"
                        />
                        <span>{patientA.emotioneleStabiliteit !== null && patientA.emotioneleStabiliteit !== undefined ? `${patientA.emotioneleStabiliteit.toFixed(1)} / 10.0` : '5.5 (Standaard)'}</span>
                      </label>
                    </td>
                    <td className="p-3">
                      <label className="flex items-center space-x-2 cursor-pointer">
                        <input
                          type="radio"
                          name="emotioneleStabiliteit"
                          checked={selectedFields.emotioneleStabiliteit === (patientB.emotioneleStabiliteit ?? 5.5)}
                          onChange={() => handleFieldSelect('emotioneleStabiliteit', patientB.emotioneleStabiliteit ?? 5.5)}
                          className="accent-brand-600"
                        />
                        <span>{patientB.emotioneleStabiliteit !== null && patientB.emotioneleStabiliteit !== undefined ? `${patientB.emotioneleStabiliteit.toFixed(1)} / 10.0` : '5.5 (Standaard)'}</span>
                      </label>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          {/* Merge Result Summary Preview */}
          <div className="bg-slate-50 dark:bg-brand-950/60 p-4 rounded-2xl border border-slate-200 dark:border-brand-800 space-y-2 text-xs">
            <h5 className="font-bold text-slate-800 dark:text-white flex items-center space-x-1.5">
              <span>Voorvertoning van het Finale Profiel (Dossier #{targetId})</span>
              <ArrowRight className="h-3.5 w-3.5 text-brand-600 dark:text-brand-400" />
            </h5>
            <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 text-slate-600 dark:text-brand-200 pt-1">
              <div><span className="text-slate-400 dark:text-brand-400 block">Naam:</span> <strong className="text-slate-800 dark:text-white">{selectedFields.voornaam} {selectedFields.achternaam}</strong></div>
              <div><span className="text-slate-400 dark:text-brand-400 block">E-mail:</span> <strong className="break-all text-slate-800 dark:text-white">{selectedFields.email}</strong></div>
              <div><span className="text-slate-400 dark:text-brand-400 block">Secundair E-mail:</span> <span className="break-all">{selectedFields.secundairEmail || '—'}</span></div>
              <div><span className="text-slate-400 dark:text-brand-400 block">Totaal Afspraken:</span> <strong className="text-brand-600 dark:text-brand-400">{totalAppts} overgezet</strong></div>
            </div>
          </div>

          </div>

          {/* Modal Actions */}
          <div className="p-4 px-6 border-t border-slate-100 dark:border-brand-800/40 shrink-0 bg-slate-50/50 dark:bg-brand-950/50 flex justify-end space-x-3">
            <button
              type="button"
              onClick={onClose}
              disabled={loading}
              className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-5 rounded-xl font-semibold transition text-sm cursor-pointer"
            >
              Annuleren
            </button>
            <button
              type="submit"
              disabled={loading}
              className="flex items-center space-x-2 bg-brand-600 hover:bg-brand-700 text-white font-semibold py-2.5 px-6 rounded-xl transition shadow-lg shadow-brand-500/10 text-sm cursor-pointer"
            >
              {loading ? (
                <>
                  <Loader2 className="animate-spin h-4 w-4" />
                  <span>Samenvoegen...</span>
                </>
              ) : (
                <>
                  <Merge className="h-4 w-4" />
                  <span>Bevestig & Samenvoegen</span>
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
