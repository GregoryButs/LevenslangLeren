import React, { useState, useEffect } from 'react';
import { UserCheck, X, Plus, Link as LinkIcon, Mail, Phone, User, Calendar, CheckCircle2, Clock, Search } from 'lucide-react';
import { Patient } from '../../types';
import { patientApi } from '../../services/api';
import { extractErrorMessage } from '../../utils/errorUtils';

interface AanmeldingDetailModalProps {
  aanmelding: any;
  patientsList: Patient[];
  selectedPatientId: string;
  onSelectPatientChange: (patientId: string) => void;
  onApproveNewPatient: (appId: any) => void;
  onLinkExistingPatient: (email: string, appId: any) => void;
  onClose: () => void;
  onSuccess: () => void;
}

export const AanmeldingDetailModal: React.FC<AanmeldingDetailModalProps> = ({
  aanmelding,
  patientsList,
  selectedPatientId,
  onSelectPatientChange,
  onApproveNewPatient,
  onLinkExistingPatient,
  onClose,
  onSuccess,
}) => {
  if (!aanmelding) return null;

  // Mode: 'create' (hergebruik/vooraf ingevuld patiëntenformulier) of 'link' (bestaande patiënt)
  const [activeTab, setActiveTab] = useState<'create' | 'link'>('create');
  const [loading, setLoading] = useState(false);
  const [patientSearchQuery, setPatientSearchQuery] = useState('');

  // Bewerkbare patiënt-velden vooraf ingevuld met gegevens van de registratie
  const [formData, setFormData] = useState({
    voornaam: aanmelding.voornaam || '',
    achternaam: aanmelding.achternaam || '',
    email: aanmelding.email || '',
    telefoonnummer: aanmelding.phoneNumber || aanmelding.telefoonnummer || '',
    geboortedatum: '1990-01-01',
    secundairEmail: '',
  });

  useEffect(() => {
    setFormData({
      voornaam: aanmelding.voornaam || '',
      achternaam: aanmelding.achternaam || '',
      email: aanmelding.email || '',
      telefoonnummer: aanmelding.phoneNumber || aanmelding.telefoonnummer || '',
      geboortedatum: '1990-01-01',
      secundairEmail: '',
    });
  }, [aanmelding]);

  // Gefilterde patiëntenlijst op basis van zoekopdracht
  const filteredPatients = patientsList.filter(p => {
    const query = patientSearchQuery.toLowerCase().trim();
    if (!query) return true;
    const fullName = `${p.voornaam || ''} ${p.achternaam || ''} ${p.volledigeNaam || ''}`.toLowerCase();
    const email = (p.email || '').toLowerCase();
    const dossier = (p.dossierNummer || '').toLowerCase();
    return fullName.includes(query) || email.includes(query) || dossier.includes(query);
  });

  // Nieuwe patiënt aanmaken met de ingevulde/aangepaste gegevens en direct koppelen
  const handleCreateAndLink = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);

    try {
      // 1. Maak nieuw patiëntendossier aan met (eventueel gewijzigde/aangevulde) formulierdata
      const newPatient = await patientApi.create({
        voornaam: formData.voornaam,
        achternaam: formData.achternaam,
        email: formData.email,
        secundairEmail: formData.secundairEmail || null,
        telefoonnummer: formData.telefoonnummer || null,
        geboortedatum: formData.geboortedatum,
        dossierNummer: null,
      });

      // 2. Koppel het geregistreerde account aan het nieuw aangemaakte patiëntendossier
      await patientApi.link(newPatient.id, aanmelding.email);

      alert(`Patiëntendossier '${formData.voornaam} ${formData.achternaam}' succesvol aangemaakt en gekoppeld!`);
      onSuccess();
      onClose();
    } catch (err) {
      console.error('Fout bij aanmaken/koppelen:', err);
      try {
        await onApproveNewPatient(aanmelding.id);
        onClose();
      } catch (fallbackErr) {
        alert(extractErrorMessage(err, 'Aanmaken & koppelen mislukt.'));
      }
    } finally {
      setLoading(false);
    }
  };

  // Koppelen aan bestaande patiënt
  const handleLinkExisting = async () => {
    if (!selectedPatientId) {
      alert('Selecteer eerst een bestaande patiënt om te koppelen.');
      return;
    }
    setLoading(true);
    try {
      await onLinkExistingPatient(aanmelding.email, aanmelding.id);
      onClose();
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 bg-slate-900/60 dark:bg-black/70 backdrop-blur-xs flex items-center justify-center p-4 animate-in fade-in duration-200">
      <div className="bg-white dark:bg-brand-900 rounded-3xl max-w-xl w-full p-6 shadow-2xl border border-slate-100 dark:border-brand-800/40 space-y-5 relative max-h-[90vh] overflow-y-auto transition-colors">
        
        {/* Header */}
        <div className="flex justify-between items-center border-b border-slate-100 dark:border-brand-800/40 pb-4">
          <div className="flex items-center space-x-2">
            <UserCheck className="h-5 w-5 text-brand-600 dark:text-brand-400" />
            <h3 className="text-lg font-bold text-slate-800 dark:text-white">
              Aanmelding & Patiëntendossier Beheren
            </h3>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-brand-800 rounded-xl transition cursor-pointer"
            title="Sluiten"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* User Registration Info Header Badge */}
        <div className="p-3.5 bg-brand-50/70 dark:bg-brand-950/70 border border-brand-100 dark:border-brand-800/60 rounded-2xl flex items-start justify-between gap-3">
          <div>
            <span className="text-[10px] font-bold text-brand-700 dark:text-brand-300 uppercase tracking-wider block">
              Geregistreerd Patiëntenaccount
            </span>
            <p className="text-xs font-bold text-slate-800 dark:text-white mt-0.5">
              {aanmelding.voornaam} {aanmelding.achternaam} ({aanmelding.email})
            </p>
          </div>
          {aanmelding.emailConfirmed ? (
            <span className="inline-flex items-center space-x-1 bg-emerald-100 dark:bg-emerald-950/80 text-emerald-800 dark:text-emerald-300 text-[10px] font-bold px-2.5 py-1 rounded-full border border-emerald-200 dark:border-emerald-800/40 shrink-0">
              <CheckCircle2 className="h-3 w-3 text-emerald-600 dark:text-emerald-400" />
              <span>E-mail Geverifieerd</span>
            </span>
          ) : (
            <span className="inline-flex items-center space-x-1 bg-amber-100 dark:bg-amber-950/80 text-amber-800 dark:text-amber-300 text-[10px] font-bold px-2.5 py-1 rounded-full border border-amber-200 dark:border-amber-800/40 shrink-0">
              <Clock className="h-3 w-3 text-amber-600 dark:text-amber-400" />
              <span>E-mail Onbevestigd</span>
            </span>
          )}
        </div>

        {/* Tabs Selection */}
        <div className="flex bg-slate-100 dark:bg-brand-950 p-1 rounded-2xl border border-slate-200 dark:border-brand-800">
          <button
            type="button"
            onClick={() => setActiveTab('create')}
            className={`flex-1 py-2 px-3 text-xs font-bold rounded-xl transition flex items-center justify-center space-x-1.5 cursor-pointer ${
              activeTab === 'create'
                ? 'bg-white dark:bg-brand-900 text-brand-700 dark:text-white shadow-xs'
                : 'text-slate-500 dark:text-brand-300 hover:text-slate-800 dark:hover:text-white'
            }`}
          >
            <Plus className="h-4 w-4" />
            <span>1. Nieuw Dossier (Vooraf Ingevuld)</span>
          </button>
          <button
            type="button"
            onClick={() => setActiveTab('link')}
            className={`flex-1 py-2 px-3 text-xs font-bold rounded-xl transition flex items-center justify-center space-x-1.5 cursor-pointer ${
              activeTab === 'link'
                ? 'bg-white dark:bg-brand-900 text-emerald-700 dark:text-white shadow-xs'
                : 'text-slate-500 dark:text-brand-300 hover:text-slate-800 dark:hover:text-white'
            }`}
          >
            <LinkIcon className="h-4 w-4" />
            <span>2. Koppelen aan Bestaande Patiënt</span>
          </button>
        </div>

        {/* TAB 1: PRE-FILLED PATIENT FORM FOR CREATING NEW PATIENT RECORD */}
        {activeTab === 'create' && (
          <form onSubmit={handleCreateAndLink} className="space-y-4">
            <div className="p-3 bg-slate-50 dark:bg-brand-950/50 rounded-2xl border border-slate-100 dark:border-brand-800/40">
              <p className="text-xs text-slate-600 dark:text-brand-200">
                De door de patiënt ingevulde registratiegegevens zijn hieronder reeds ingevuld. U kunt eventuele ontbrekende velden (zoals geboortedatum) nu direct aanvullen of aanpassen voordat u het dossier definitief opslaat en koppelt.
              </p>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs font-bold text-slate-700 dark:text-brand-100 block mb-1">
                  Voornaam *
                </label>
                <div className="relative">
                  <User className="absolute left-3 top-2.5 h-4 w-4 text-slate-400 dark:text-brand-300" />
                  <input
                    type="text"
                    required
                    value={formData.voornaam}
                    onChange={(e) => setFormData({ ...formData, voornaam: e.target.value })}
                    className="pl-9 w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-xs font-semibold text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                    placeholder="Voornaam"
                  />
                </div>
              </div>

              <div>
                <label className="text-xs font-bold text-slate-700 dark:text-brand-100 block mb-1">
                  Achternaam *
                </label>
                <input
                  type="text"
                  required
                  value={formData.achternaam}
                  onChange={(e) => setFormData({ ...formData, achternaam: e.target.value })}
                  className="w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-xs font-semibold text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                  placeholder="Achternaam"
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs font-bold text-slate-700 dark:text-brand-100 block mb-1">
                  E-mailadres *
                </label>
                <div className="relative">
                  <Mail className="absolute left-3 top-2.5 h-4 w-4 text-slate-400 dark:text-brand-300" />
                  <input
                    type="email"
                    required
                    value={formData.email}
                    onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    className="pl-9 w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-xs font-semibold text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                    placeholder="patient@voorbeeld.be"
                  />
                </div>
              </div>

              <div>
                <label className="text-xs font-bold text-slate-700 dark:text-brand-100 block mb-1">
                  Telefoonnummer
                </label>
                <div className="relative">
                  <Phone className="absolute left-3 top-2.5 h-4 w-4 text-slate-400 dark:text-brand-300" />
                  <input
                    type="text"
                    value={formData.telefoonnummer}
                    onChange={(e) => setFormData({ ...formData, telefoonnummer: e.target.value })}
                    className="pl-9 w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-xs font-semibold text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                    placeholder="0470 12 34 56"
                  />
                </div>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="text-xs font-bold text-slate-700 dark:text-brand-100 block mb-1">
                  Geboortedatum *
                </label>
                <div className="relative">
                  <Calendar className="absolute left-3 top-2.5 h-4 w-4 text-slate-400 dark:text-brand-300" />
                  <input
                    type="date"
                    required
                    value={formData.geboortedatum}
                    onChange={(e) => setFormData({ ...formData, geboortedatum: e.target.value })}
                    className="pl-9 w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-xs font-semibold text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                  />
                </div>
              </div>

              <div>
                <label className="text-xs font-bold text-slate-700 dark:text-brand-100 block mb-1">
                  Secundair E-mail (Optioneel)
                </label>
                <input
                  type="email"
                  value={formData.secundairEmail}
                  onChange={(e) => setFormData({ ...formData, secundairEmail: e.target.value })}
                  className="w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-xs font-semibold text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                  placeholder="extra@voorbeeld.be"
                />
              </div>
            </div>

            <div className="pt-3 border-t border-slate-100 dark:border-brand-800/40 flex justify-end space-x-2">
              <button
                type="button"
                onClick={onClose}
                className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-4 rounded-xl text-xs font-semibold transition cursor-pointer"
              >
                Annuleren
              </button>
              <button
                type="submit"
                disabled={loading}
                className="bg-brand-600 hover:bg-brand-700 disabled:opacity-50 text-white py-2.5 px-5 rounded-xl text-xs font-bold transition flex items-center space-x-1.5 shadow-md shadow-brand-500/10 cursor-pointer"
              >
                <Plus className="h-4 w-4" />
                <span>{loading ? 'Aanmaken...' : 'Patiëntendossier Aanmaken & Koppelen'}</span>
              </button>
            </div>
          </form>
        )}

        {/* TAB 2: LINK TO EXISTING PATIENT WITH SEARCH */}
        {activeTab === 'link' && (
          <div className="space-y-4">
            <div className="p-3 bg-emerald-50/60 dark:bg-brand-950/60 rounded-2xl border border-emerald-100 dark:border-brand-800/40">
              <p className="text-xs text-emerald-800 dark:text-brand-200">
                Zoek en selecteer hieronder een bestaande patiënt uit het praktijkbestand om het geregistreerde account aan te koppelen.
              </p>
            </div>

            <div className="space-y-3">
              <label className="text-xs font-bold text-slate-700 dark:text-brand-100 block">
                Bestaande patiënt zoeken & selecteren *
              </label>

              {/* Patient Search Input */}
              <div className="relative">
                <Search className="absolute left-3 top-2.5 h-4 w-4 text-slate-400 dark:text-brand-300" />
                <input
                  type="text"
                  value={patientSearchQuery}
                  onChange={(e) => setPatientSearchQuery(e.target.value)}
                  placeholder="Zoek op naam, e-mailadres of dossiernummer..."
                  className="pl-9 pr-8 w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-xs font-medium text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                />
                {patientSearchQuery && (
                  <button
                    type="button"
                    onClick={() => setPatientSearchQuery('')}
                    className="absolute right-3 top-2.5 text-xs text-slate-400 hover:text-slate-600 dark:hover:text-white"
                  >
                    <X className="h-3.5 w-3.5" />
                  </button>
                )}
              </div>

              {/* Patient Selection Dropdown */}
              <select
                value={selectedPatientId || ''}
                onChange={(e) => onSelectPatientChange(e.target.value)}
                className="text-xs w-full bg-white dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-3 rounded-xl text-slate-800 dark:text-brand-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 font-semibold"
              >
                <option value="">
                  {filteredPatients.length > 0 
                    ? `-- Kies een patiënt (${filteredPatients.length} gevonden) --` 
                    : '-- Geen patiënten gevonden --'}
                </option>
                {filteredPatients.map(p => (
                  <option key={p.id} value={p.id}>
                    {p.volledigeNaam} {p.email ? `(${p.email})` : ''} {p.dossierNummer ? `[Dossier: ${p.dossierNummer}]` : ''}
                  </option>
                ))}
              </select>

              {filteredPatients.length === 0 && (
                <p className="text-xs text-amber-600 dark:text-amber-400 font-medium">
                  Geen patiënten gevonden die overeenkomen met "{patientSearchQuery}".
                </p>
              )}
            </div>

            <div className="pt-4 border-t border-slate-100 dark:border-brand-800/40 flex justify-end space-x-2">
              <button
                type="button"
                onClick={onClose}
                className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-4 rounded-xl text-xs font-semibold transition cursor-pointer"
              >
                Annuleren
              </button>
              <button
                type="button"
                onClick={handleLinkExisting}
                disabled={!selectedPatientId || loading}
                className="bg-emerald-600 hover:bg-emerald-700 disabled:opacity-50 text-white py-2.5 px-5 rounded-xl text-xs font-bold transition flex items-center space-x-1.5 shadow-md shadow-emerald-500/10 cursor-pointer"
              >
                <LinkIcon className="h-4 w-4" />
                <span>{loading ? 'Koppelen...' : 'Koppelen aan Geselecteerde Patiënt'}</span>
              </button>
            </div>
          </div>
        )}

      </div>
    </div>
  );
};
