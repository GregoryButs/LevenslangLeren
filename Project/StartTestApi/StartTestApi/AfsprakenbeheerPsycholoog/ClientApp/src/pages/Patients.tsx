import React, { useState, useEffect, useRef } from 'react';
import { useSearchParams } from 'react-router-dom';
import { patientApi } from '../services/api';
import { Patient } from '../types';
import { 
  Users, Plus, Edit2, Trash2, ArrowLeft, 
  Link2, Link2Off, Calendar, Loader2, AlertCircle, FileSpreadsheet
} from 'lucide-react';
import { InfoTooltip } from '../components/common/InfoTooltip';
import { extractErrorMessage } from '../utils/errorUtils';
import { isValidRijksregisternummer } from '../utils/validationUtils';
import { PatientTable } from '../components/patients/PatientTable';
import { PatientMergeModal } from '../components/patients/PatientMergeModal';

export const Patients: React.FC = () => {
  const [searchParams] = useSearchParams();
  const [activePatients, setActivePatients] = useState<Patient[]>([]);
  const [archivedPatients, setArchivedPatients] = useState<Patient[]>([]);
  const [activeTab, setActiveTab] = useState<'active' | 'archived'>('active');
  const [loading, setLoading] = useState(true);

  // Detail Modal / Panel State
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [patientDetails, setPatientDetails] = useState<Patient | null>(null);
  const mobileDetailRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    if (selectedPatient && window.innerWidth < 1024) {
      setTimeout(() => {
        mobileDetailRef.current?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
      }, 100);
    }
  }, [selectedPatient]);

  // Edit / Create Modal State
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<{ [key: string]: boolean }>({});
  const [formPatient, setFormPatient] = useState<any>({
    id: null,
    voornaam: '',
    achternaam: '',
    geboortedatum: '',
    email: '',
    secundairEmail: '',
    telefoonnummer: '',
    dossierNummer: '',
    rijksregisternummer: '',
    standaardTariefType: 'Regulier',
    emotioneleStabiliteit: 5.5
  });

  // Link Modal State
  const [isLinkModalOpen, setIsLinkModalOpen] = useState(false);
  const [linkEmail, setLinkEmail] = useState('');
  const [linkPatientId, setLinkPatientId] = useState<number | null>(null);
  const [unlinkedUsers, setUnlinkedUsers] = useState<any[]>([]);
  const [loadingUnlinked, setLoadingUnlinked] = useState(false);
  const [useManualEmailInput, setUseManualEmailInput] = useState(false);

  // Merge Modal State
  const [mergeModalPatients, setMergeModalPatients] = useState<[Patient, Patient] | null>(null);

  useEffect(() => {
    if (isLinkModalOpen) {
      setLoadingUnlinked(true);
      patientApi.getAanmeldingen()
        .then((users) => {
          setUnlinkedUsers(users || []);
          if (users && users.length > 0) {
            setLinkEmail(users[0].email || '');
            setUseManualEmailInput(false);
          } else {
            setLinkEmail('');
            setUseManualEmailInput(false);
          }
        })
        .catch((err) => console.error('Fout bij laden ongekoppelde accounts:', err))
        .finally(() => setLoadingUnlinked(false));
    }
  }, [isLinkModalOpen]);

  const loadPatients = async () => {
    try {
      setLoading(true);
      const active = await patientApi.getAll();
      setActivePatients(active);
      const archived = await patientApi.getArchive();
      setArchivedPatients(archived);

      const targetId = searchParams.get('id');
      if (targetId) {
        const targetNum = parseInt(targetId, 10);
        const match = active.find((p: any) => p.id === targetNum) || archived.find((p: any) => p.id === targetNum);
        if (match) {
          if (!active.some((p: any) => p.id === targetNum)) {
            setActiveTab('archived');
          }
          setSelectedPatient(match);
          loadPatientDetail(targetNum);
        }
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadPatients();
  }, []);

  const loadPatientDetail = async (id: number) => {
    try {
      const details = await patientApi.getById(id);
      setPatientDetails(details);
    } catch (err) {
      console.error(err);
    }
  };

  const handleOpenCreateModal = () => {
    setFormError(null);
    setFieldErrors({});
    setFormPatient({
      id: null,
      voornaam: '',
      achternaam: '',
      geboortedatum: '',
      email: '',
      secundairEmail: '',
      telefoonnummer: '',
      dossierNummer: '',
      rijksregisternummer: '',
      standaardTariefType: 'Regulier',
      emotioneleStabiliteit: 5.5
    });
    setIsFormModalOpen(true);
  };

  const handleOpenEditModal = (patient: Patient) => {
    setFormError(null);
    setFieldErrors({});
    setFormPatient({
      id: patient.id,
      voornaam: patient.voornaam,
      achternaam: patient.achternaam,
      geboortedatum: patient.geboortedatum,
      email: patient.email,
      secundairEmail: patient.secundairEmail || '',
      telefoonnummer: patient.telefoonnummer || '',
      dossierNummer: patient.dossierNummer || '',
      rijksregisternummer: patient.rijksregisternummer || '',
      standaardTariefType: patient.standaardTariefType || 'Regulier',
      emotioneleStabiliteit: patient.emotioneleStabiliteit !== null && patient.emotioneleStabiliteit !== undefined ? patient.emotioneleStabiliteit : 5.5
    });
    setIsFormModalOpen(true);
  };

  const handleSavePatient = async (e: React.FormEvent) => {
    e.preventDefault();
    setFormError(null);
    const errors: { [key: string]: boolean } = {};

    if (!formPatient.voornaam?.trim()) errors.voornaam = true;
    if (!formPatient.achternaam?.trim()) errors.achternaam = true;
    if (!formPatient.geboortedatum) errors.geboortedatum = true;
    if (!formPatient.email?.trim()) errors.email = true;

    let stabVal: number | null = null;
    if (formPatient.emotioneleStabiliteit !== undefined && formPatient.emotioneleStabiliteit !== null && formPatient.emotioneleStabiliteit !== '') {
      const rawStr = String(formPatient.emotioneleStabiliteit).replace(',', '.');
      const parsed = parseFloat(rawStr);
      if (isNaN(parsed) || parsed < 1.0 || parsed > 10.0) {
        errors.emotioneleStabiliteit = true;
      } else {
        stabVal = parsed;
      }
    } else {
      errors.emotioneleStabiliteit = true;
    }

    if (formPatient.rijksregisternummer?.trim()) {
      if (!isValidRijksregisternummer(formPatient.rijksregisternummer.trim())) {
        errors.rijksregisternummer = true;
      }
    }

    if (Object.keys(errors).length > 0) {
      setFieldErrors(errors);
      setFormError(errors.rijksregisternummer ? 'Ongeldig rijksregisternummer (moet 11 cijfers bevatten).' : 'Vul alle verplichte velden (*) correct in.');
      return;
    }

    setFieldErrors({});

    try {
      let formattedDate = formPatient.geboortedatum ? formPatient.geboortedatum.split('T')[0] : '';
      if (formattedDate.includes('/')) {
        const parts = formattedDate.split('/');
        if (parts.length === 3 && parts[2].length === 4) {
          formattedDate = `${parts[2]}-${parts[1].padStart(2, '0')}-${parts[0].padStart(2, '0')}`;
        }
      }

      const payload = {
        ...formPatient,
        id: formPatient.id,
        voornaam: formPatient.voornaam.trim(),
        achternaam: formPatient.achternaam.trim(),
        geboortedatum: formattedDate,
        email: formPatient.email.trim(),
        secundairEmail: formPatient.secundairEmail?.trim() || null,
        telefoonnummer: formPatient.telefoonnummer?.trim() || null,
        dossierNummer: formPatient.dossierNummer?.trim() || null,
        rijksregisternummer: formPatient.rijksregisternummer?.trim() || null,
        emotioneleStabiliteit: stabVal
      };

      if (formPatient.id === null) {
        await patientApi.create({
          voornaam: payload.voornaam,
          achternaam: payload.achternaam,
          geboortedatum: payload.geboortedatum,
          email: payload.email,
          secundairEmail: payload.secundairEmail,
          telefoonnummer: payload.telefoonnummer,
          dossierNummer: payload.dossierNummer,
          rijksregisternummer: payload.rijksregisternummer,
          emotioneleStabiliteit: payload.emotioneleStabiliteit
        });
        alert('Patiënt succesvol aangemaakt!');
      } else {
        await patientApi.update(formPatient.id, payload);
        alert('Patiëntgegevens succesvol bijgewerkt!');
      }
      setIsFormModalOpen(false);
      loadPatients();
      if (selectedPatient && selectedPatient.id === formPatient.id) {
        loadPatientDetail(selectedPatient.id);
      }
    } catch (err) {
      console.error(err);
      const errMsg = extractErrorMessage(err, 'Opslaan mislukt. Controleer de velden.');
      setFormError(errMsg);
    }
  };

  const handleDeactivate = async (id: number) => {
    if (!window.confirm('Weet u zeker dat u deze patiënt wilt deactiveren?')) return;
    try {
      await patientApi.delete(id);
      alert('Patiënt succesvol op inactief gezet.');
      setSelectedPatient(null);
      setPatientDetails(null);
      loadPatients();
    } catch (err) {
      console.error(err);
      alert('Deactiveren mislukt.');
    }
  };

  const handleReactivate = async (id: number) => {
    try {
      await patientApi.reactivate(id);
      alert('Patiënt succesvol heractiveerd.');
      loadPatients();
    } catch (err) {
      console.error(err);
      alert('Heractiveren mislukt.');
    }
  };

  const handleLink = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!linkPatientId) return;
    try {
      await patientApi.link(linkPatientId, linkEmail);
      alert('Account succesvol gekoppeld.');
      setIsLinkModalOpen(false);
      loadPatients();
      loadPatientDetail(linkPatientId);
    } catch (err: any) {
      console.error(err);
      alert(extractErrorMessage(err, 'Koppelen mislukt. Bestaat dit e-mailadres en is het nog niet gekoppeld?'));
    }
  };

  const handleUnlink = async (patientId: number) => {
    if (!window.confirm('Weet u zeker dat u dit account wilt ontkoppelen van de patiënt?')) return;
    try {
      await patientApi.unlink(patientId);
      alert('Account succesvol ontkoppeld.');
      loadPatients();
      loadPatientDetail(patientId);
    } catch (err) {
      console.error(err);
      alert(extractErrorMessage(err, 'Ontkoppelen mislukt.'));
    }
  };

  const getNoShowRisk = (patient: Patient) => {
    const birthYear = new Date(patient.geboortedatum).getFullYear();
    const age = new Date().getFullYear() - birthYear;
    const completedSessions = patient.afspraken?.filter(a => a.status === 'Voltooid').length || 0;
    
    let lastSessionGap = 7;
    const completed = patient.afspraken?.filter(a => a.status === 'Voltooid') || [];
    if (completed.length > 0) {
      const dates = completed.map(a => new Date(a.starttijd).getTime());
      const lastDate = Math.max(...dates);
      lastSessionGap = Math.floor((new Date().getTime() - lastDate) / (1000 * 60 * 60 * 24));
      if (lastSessionGap < 0) lastSessionGap = 0;
    }
    
    const latestType = patient.afspraken && patient.afspraken.length > 0
      ? patient.afspraken[patient.afspraken.length - 1].afspraakTypeNaam
      : 'Therapie';
      
    let prob = 0.10;
    if (lastSessionGap > 21) prob += 0.30;
    if (latestType?.toLowerCase() === 'depressie') prob += 0.20;
    if (age > 60) prob += 0.05;
    prob -= 0.005 * completedSessions;
    
    prob = Math.max(0.02, Math.min(0.95, prob));
    
    let category = "Low";
    if (prob > 0.50) category = "High";
    else if (prob > 0.25) category = "Medium";
    
    return { probability: prob, category };
  };

  if (loading && activePatients.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-[80vh]">
        <Loader2 className="animate-spin h-10 w-10 text-brand-600" />
      </div>
    );
  }

  const renderDetailsPanel = (isMobile: boolean = false) => {
    if (!selectedPatient) return null;
    return (
      <div 
        ref={isMobile ? mobileDetailRef : undefined}
        className={`bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-4 sm:p-6 space-y-6 transition-colors ${
          isMobile ? 'mt-4 animate-in fade-in slide-in-from-top-2 duration-200' : 'lg:col-span-2'
        }`}
      >
        <div className="flex justify-between items-start border-b border-slate-100 dark:border-brand-800/40 pb-4 gap-2">
          <div className="min-w-0 flex-1">
            <button 
              onClick={() => { setSelectedPatient(null); setPatientDetails(null); }}
              className="flex items-center text-xs font-bold text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-brand-200 uppercase tracking-wider mb-2"
            >
              <ArrowLeft className="h-3 w-3 mr-1" /> Terug naar lijst
            </button>
            <h2 className="text-xl font-bold text-slate-800 dark:text-white truncate">
              {patientDetails ? patientDetails.volledigeNaam : selectedPatient.volledigeNaam}
            </h2>
          </div>
          
          <div className="flex space-x-2 flex-shrink-0">
            <button
              onClick={() => handleOpenEditModal(patientDetails || selectedPatient)}
              className="p-2 bg-slate-50 dark:bg-brand-950 hover:bg-slate-100 dark:hover:bg-brand-800 border border-slate-200 dark:border-brand-800 text-slate-600 dark:text-brand-200 rounded-xl transition"
              title="Bewerken"
            >
              <Edit2 className="h-4 w-4" />
            </button>
            {activeTab === 'active' && (
              <button
                onClick={() => handleDeactivate(selectedPatient.id)}
                className="p-2 bg-red-50 dark:bg-red-950/60 hover:bg-red-100 border border-red-100 dark:border-red-900/40 text-red-600 dark:text-red-300 rounded-xl transition"
                title="Deactiveren"
              >
                <Trash2 className="h-4 w-4" />
              </button>
            )}
          </div>
        </div>

        {/* Profile Detail Grid */}
        {patientDetails ? (
          <div className="space-y-6">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 text-sm bg-slate-50 dark:bg-brand-950/80 p-4 rounded-2xl border border-slate-100 dark:border-brand-800/60">
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block">Dossiernummer</span>
                <span className="text-slate-800 dark:text-white font-semibold font-mono">{patientDetails.dossierNummer || 'DOS-N/A'}</span>
              </div>
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block">Rijksregisternummer</span>
                <span className="text-slate-800 dark:text-white font-semibold font-mono">{patientDetails.rijksregisternummer || '—'}</span>
              </div>
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block">Geboortedatum</span>
                <span className="text-slate-800 dark:text-white font-semibold">
                  {new Date(patientDetails.geboortedatum).toLocaleDateString('nl-NL')}
                </span>
              </div>
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block">E-mailadres</span>
                <span className="text-slate-800 dark:text-brand-100 font-semibold break-all">{patientDetails.email}</span>
                {patientDetails.secundairEmail && (
                  <div className="text-xs text-slate-500 dark:text-brand-300 mt-0.5 break-all">
                    <span className="font-medium text-slate-400 dark:text-brand-400">Secundair:</span> {patientDetails.secundairEmail}
                  </div>
                )}
              </div>
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block">Telefoon</span>
                <span className="text-slate-800 dark:text-white font-semibold">{patientDetails.telefoonnummer || '—'}</span>
              </div>
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block">Standaard Tarieftype</span>
                <span className={`inline-block px-2.5 py-0.5 mt-0.5 rounded-full text-xs font-bold ${
                  patientDetails.standaardTariefType === 'ELP'
                    ? 'bg-brand-100 dark:bg-brand-950 text-brand-800 dark:text-brand-300 border border-brand-200 dark:border-brand-800'
                    : 'bg-slate-100 dark:bg-slate-800 text-slate-700 dark:text-slate-300'
                }`}>
                  {patientDetails.standaardTariefType === 'ELP' ? 'ELP (Eerstelijnszorg)' : 'Regulier'}
                </span>
              </div>
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block flex items-center">
                  <span>Emotionele Stabiliteit</span>
                  <InfoTooltip content="Een klinische score ingevoerd door de therapeut (bereik 1.0 tot 10.0) die de emotionele en psychische weerbaarheid van de patiënt kwantificeert." />
                </span>
                <span className="text-slate-800 dark:text-white font-semibold">
                  {patientDetails.emotioneleStabiliteit !== null && patientDetails.emotioneleStabiliteit !== undefined
                    ? `${patientDetails.emotioneleStabiliteit.toFixed(1)} / 10.0`
                    : '5.5 (Standaard)'}
                </span>
              </div>
              <div className="min-w-0">
                <span className="text-slate-400 dark:text-brand-300 font-medium block flex items-center">
                  <span>AI No-Show Risico</span>
                  <InfoTooltip content="Berekend risico dat deze specifieke patiënt de volgende sessie zal missen, gebaseerd op het aantal dagen sinds de laatste sessie en de diagnose." />
                </span>
                {(() => {
                  const risk = getNoShowRisk(patientDetails);
                  return (
                    <span className={`inline-block px-2 py-0.5 mt-0.5 rounded-full text-xs font-bold ${
                      risk.category === 'High' ? 'bg-red-100 dark:bg-red-950/80 text-red-800 dark:text-red-300 border border-red-200 dark:border-red-800' :
                      risk.category === 'Medium' ? 'bg-amber-100 dark:bg-amber-950/80 text-amber-800 dark:text-amber-300 border border-amber-200 dark:border-amber-800' :
                      'bg-emerald-100 dark:bg-emerald-950/80 text-emerald-800 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800'
                    }`}>
                      {(risk.probability * 100).toFixed(1)}% ({risk.category})
                    </span>
                  );
                })()}
              </div>
            </div>

            {/* Account linkage section */}
            <div className="border border-slate-100 dark:border-brand-800/60 p-4 rounded-2xl flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 bg-white dark:bg-brand-950/80">
              <div className="space-y-0.5 min-w-0">
                <h4 className="text-sm font-bold text-slate-800 dark:text-white">Gebruikersaccount</h4>
                <p className="text-xs text-slate-500 dark:text-brand-300">
                  {patientDetails.isGekoppeld 
                    ? 'Gekoppeld aan een gebruikersaccount' 
                    : 'Dit dossier is nog niet gekoppeld aan een gebruikersaccount.'}
                </p>
              </div>
              
              {patientDetails.isGekoppeld ? (
                <button
                  onClick={() => handleUnlink(patientDetails.id)}
                  className="flex items-center space-x-1.5 text-xs font-semibold bg-amber-50 dark:bg-amber-950/60 hover:bg-amber-100 dark:hover:bg-amber-900/60 text-amber-700 dark:text-amber-300 py-2 px-3 rounded-xl transition flex-shrink-0"
                >
                  <Link2Off className="h-4 w-4" />
                  <span>Ontkoppelen</span>
                </button>
              ) : (
                <button
                  onClick={() => { setLinkPatientId(patientDetails.id); setLinkEmail(''); setIsLinkModalOpen(true); }}
                  className="flex items-center space-x-1.5 text-xs font-semibold bg-brand-50 dark:bg-brand-800/60 hover:bg-brand-100 dark:hover:bg-brand-700 text-brand-700 dark:text-brand-200 py-2 px-3 rounded-xl transition flex-shrink-0"
                >
                  <Link2 className="h-4 w-4" />
                  <span>Account Koppelen</span>
                </button>
              )}
            </div>

            {/* Patient Appointments Chronological */}
            <div className="space-y-3">
              <h4 className="text-sm font-bold text-slate-800 dark:text-white flex items-center space-x-1.5">
                <Calendar className="h-4 w-4 text-brand-600 dark:text-brand-400" />
                <span>Afsprakenhistorie</span>
              </h4>
              
              <div className="space-y-2 max-h-[220px] overflow-y-auto overflow-x-hidden pr-1">
                {patientDetails.afspraken && patientDetails.afspraken.length > 0 ? (
                  patientDetails.afspraken.map((appt) => {
                    const start = new Date(appt.starttijd);
                    return (
                      <div key={appt.id} className="p-3 bg-slate-50/50 dark:bg-brand-950/60 rounded-xl border border-slate-100 dark:border-brand-800/40 flex justify-between items-center text-xs gap-2">
                        <div className="min-w-0 flex-1">
                          <span className="font-semibold text-slate-700 dark:text-brand-100 mr-2 block sm:inline truncate">{appt.afspraakTypeNaam}</span>
                          <span className="text-slate-500 dark:text-brand-300 text-[11px] sm:text-xs">
                            {start.toLocaleDateString('nl-NL')} om {start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                          </span>
                        </div>
                        <span 
                          className="py-0.5 px-2 rounded-full text-[10px] font-bold uppercase tracking-wider text-white shadow-xs flex-shrink-0"
                          style={{ backgroundColor: appt.kleurcode }}
                        >
                          {appt.status}
                        </span>
                      </div>
                    );
                  })
                ) : (
                  <p className="text-xs text-slate-400 dark:text-brand-400 font-medium text-center py-4">Nog geen afspraken gepland voor deze patiënt.</p>
                )}
              </div>
            </div>
          </div>
        ) : (
          <div className="flex items-center justify-center py-12 text-slate-400 dark:text-brand-400">
            <Loader2 className="animate-spin h-6 w-6 mr-2 text-brand-600" />
            <span>Details laden...</span>
          </div>
        )}
      </div>
    );
  };

  return (
    <div className="p-4 sm:p-6 max-w-7xl mx-auto space-y-6 sm:space-y-8 overflow-x-hidden">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between bg-white dark:bg-brand-900 p-4 sm:p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm gap-4 transition-colors">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <Users className="h-7 w-7 text-brand-600 dark:text-brand-400" />
            <span>Patiëntenbeheer</span>
          </h1>
          <p className="text-slate-500 dark:text-brand-300 mt-1 text-sm sm:text-base">
            Beheer patiëntendossiers, raadpleeg afspraken, exporteer gegevens en voeg dubbele patiënten samen.
          </p>
        </div>
        <button
          onClick={handleOpenCreateModal}
          className="flex items-center justify-center space-x-2 bg-brand-600 hover:bg-brand-700 text-white font-semibold py-3 px-6 rounded-2xl transition shadow-lg shadow-brand-500/10 w-full sm:w-auto text-sm"
        >
          <Plus className="h-5 w-5" />
          <span>Nieuwe Patiënt</span>
        </button>
      </div>

      {/* Active vs Archived Tab selector */}
      <div className="flex border-b border-slate-200 dark:border-brand-800">
        <button
          onClick={() => { setActiveTab('active'); setSelectedPatient(null); setPatientDetails(null); }}
          className={`py-3 px-6 font-bold text-sm transition-all border-b-2 ${
            activeTab === 'active'
              ? 'border-brand-600 text-brand-700 dark:text-brand-300'
              : 'border-transparent text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-brand-200'
          }`}
        >
          Actieve Patiënten ({activePatients.length})
        </button>
        <button
          onClick={() => { setActiveTab('archived'); setSelectedPatient(null); setPatientDetails(null); }}
          className={`py-3 px-6 font-bold text-sm transition-all border-b-2 ${
            activeTab === 'archived'
              ? 'border-brand-600 text-brand-700 dark:text-brand-300'
              : 'border-transparent text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-brand-200'
          }`}
        >
          Archief / Inactief ({archivedPatients.length})
        </button>
      </div>

      {/* Main View: Split screen if selected patient */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left / Main pane: TanStack Table */}
        <div className={`space-y-6 ${selectedPatient ? 'lg:col-span-1' : 'lg:col-span-3'}`}>
          <PatientTable
            data={activeTab === 'active' ? activePatients : archivedPatients}
            selectedPatientId={selectedPatient?.id || null}
            activeTab={activeTab}
            onSelectPatient={(p) => {
              if (selectedPatient?.id === p.id) {
                setSelectedPatient(null);
                setPatientDetails(null);
              } else {
                setSelectedPatient(p);
                loadPatientDetail(p.id);
              }
            }}
            onEditPatient={(p) => handleOpenEditModal(p)}
            onDeactivatePatient={(id) => handleDeactivate(id)}
            onReactivatePatient={(id) => handleReactivate(id)}
            onOpenMergeModal={(patients) => setMergeModalPatients(patients)}
          />

          {/* Mobile Inline Detail View */}
          {selectedPatient && (
            <div className="block lg:hidden">
              {renderDetailsPanel(true)}
            </div>
          )}
        </div>

        {/* Desktop Details Side Panel */}
        {selectedPatient && (
          <div className="hidden lg:block lg:col-span-2">
            {renderDetailsPanel(false)}
          </div>
        )}
      </div>

      {/* Patient Merge Comparison Modal */}
      {mergeModalPatients && (
        <PatientMergeModal
          patientA={mergeModalPatients[0]}
          patientB={mergeModalPatients[1]}
          onClose={() => setMergeModalPatients(null)}
          onSuccess={() => {
            setMergeModalPatients(null);
            setSelectedPatient(null);
            setPatientDetails(null);
            loadPatients();
          }}
        />
      )}

      {/* Add / Edit Patient Modal */}
      {isFormModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 dark:bg-black/70 backdrop-blur-sm p-4 overflow-y-auto animate-in fade-in duration-200">
          <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-lg max-h-[90vh] flex flex-col p-6 border border-slate-100 dark:border-brand-800/40 transition-colors overflow-hidden">
            <div className="shrink-0">
              <h3 className="text-xl font-bold text-slate-800 dark:text-white mb-1">
                {formPatient.id === null ? 'Patiënt Toevoegen' : 'Patiënt Bewerken'}
              </h3>
              <p className="text-xs text-slate-500 dark:text-brand-300 mb-3">
                Velden met een <span className="text-red-500 font-bold">*</span> zijn verplicht.
              </p>

              {formError && (
                <div className="mb-3 p-3.5 bg-red-50 dark:bg-red-950/70 border border-red-200 dark:border-red-800/60 rounded-2xl text-red-700 dark:text-red-300 text-sm flex items-start space-x-2.5 shadow-sm">
                  <AlertCircle className="w-5 h-5 text-red-500 shrink-0 mt-0.5" />
                  <div className="flex-1">
                    <p className="font-semibold text-xs uppercase tracking-wide text-red-800 dark:text-red-200">Opslaan Mislukt</p>
                    <p className="text-xs mt-0.5 leading-relaxed font-medium">{formError}</p>
                  </div>
                </div>
              )}
            </div>

            <form onSubmit={handleSavePatient} className="flex flex-col flex-1 min-h-0">
              <div className="flex-1 overflow-y-auto pr-1 space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                    Voornaam <span className="text-red-500 font-bold ml-0.5">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={formPatient.voornaam}
                    onChange={(e) => {
                      setFormPatient({ ...formPatient, voornaam: e.target.value });
                      if (fieldErrors.voornaam) setFieldErrors({ ...fieldErrors, voornaam: false });
                    }}
                    className={`w-full bg-slate-50 dark:bg-brand-950 border ${fieldErrors.voornaam ? 'border-red-500 ring-2 ring-red-500/30' : 'border-slate-200 dark:border-brand-800'} py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400`}
                    placeholder="Voornaam"
                  />
                </div>
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                    Achternaam <span className="text-red-500 font-bold ml-0.5">*</span>
                  </label>
                  <input
                    type="text"
                    required
                    value={formPatient.achternaam}
                    onChange={(e) => {
                      setFormPatient({ ...formPatient, achternaam: e.target.value });
                      if (fieldErrors.achternaam) setFieldErrors({ ...fieldErrors, achternaam: false });
                    }}
                    className={`w-full bg-slate-50 dark:bg-brand-950 border ${fieldErrors.achternaam ? 'border-red-500 ring-2 ring-red-500/30' : 'border-slate-200 dark:border-brand-800'} py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400`}
                    placeholder="Achternaam"
                  />
                </div>
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                  Geboortedatum <span className="text-red-500 font-bold ml-0.5">*</span>
                </label>
                <input
                  type="date"
                  required
                  value={formPatient.geboortedatum ? formPatient.geboortedatum.split('T')[0] : ''}
                  onChange={(e) => {
                    setFormPatient({ ...formPatient, geboortedatum: e.target.value });
                    if (fieldErrors.geboortedatum) setFieldErrors({ ...fieldErrors, geboortedatum: false });
                  }}
                  className={`w-full bg-slate-50 dark:bg-brand-950 border ${fieldErrors.geboortedatum ? 'border-red-500 ring-2 ring-red-500/30' : 'border-slate-200 dark:border-brand-800'} py-2.5 px-4 rounded-xl text-slate-800 dark:text-white`}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                    Primair E-mailadres <span className="text-red-500 font-bold ml-0.5">*</span>
                  </label>
                  <input
                    type="email"
                    required
                    value={formPatient.email}
                    onChange={(e) => {
                      setFormPatient({ ...formPatient, email: e.target.value });
                      if (fieldErrors.email) setFieldErrors({ ...fieldErrors, email: false });
                    }}
                    className={`w-full bg-slate-50 dark:bg-brand-950 border ${fieldErrors.email ? 'border-red-500 ring-2 ring-red-500/30' : 'border-slate-200 dark:border-brand-800'} py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400`}
                    placeholder="primair@adres.be"
                  />
                </div>

                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                    Secundair E-mailadres <span className="text-xs font-normal text-slate-400">(Optioneel)</span>
                  </label>
                  <input
                    type="email"
                    value={formPatient.secundairEmail || ''}
                    onChange={(e) => setFormPatient({ ...formPatient, secundairEmail: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="secundair@adres.be"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                    Telefoonnummer <span className="text-xs font-normal text-slate-400">(Optioneel)</span>
                  </label>
                  <input
                    type="tel"
                    value={formPatient.telefoonnummer || ''}
                    onChange={(e) => setFormPatient({ ...formPatient, telefoonnummer: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="04..."
                  />
                </div>
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                    Dossiernummer <span className="text-xs font-normal text-slate-400">(Optioneel)</span>
                  </label>
                  <input
                    type="text"
                    value={formPatient.dossierNummer || ''}
                    onChange={(e) => setFormPatient({ ...formPatient, dossierNummer: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white font-mono placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="DOS-001"
                  />
                </div>
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                  Rijksregisternummer <span className="text-xs font-normal text-slate-400">(Optioneel)</span>
                </label>
                <input
                  type="text"
                  value={formPatient.rijksregisternummer || ''}
                  onChange={(e) => {
                    setFormPatient({ ...formPatient, rijksregisternummer: e.target.value });
                    if (fieldErrors.rijksregisternummer) setFieldErrors({ ...fieldErrors, rijksregisternummer: false });
                  }}
                  className={`w-full bg-slate-50 dark:bg-brand-950 border ${fieldErrors.rijksregisternummer ? 'border-red-500 ring-2 ring-red-500/30' : 'border-slate-200 dark:border-brand-800'} py-2.5 px-4 rounded-xl text-slate-800 dark:text-white font-mono placeholder-slate-400 dark:placeholder-brand-400`}
                  placeholder="85.01.01-123.45 of 85010112345"
                />
              </div>

              {/* Standaard Tarieftype (Regulier / ELP) */}
              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                  Standaard Tarieftype
                </label>
                <div className="grid grid-cols-2 gap-3">
                  <button
                    type="button"
                    onClick={() => setFormPatient({ ...formPatient, standaardTariefType: 'Regulier' })}
                    className={`py-2.5 px-4 text-xs font-bold rounded-xl border transition flex items-center justify-center space-x-2 cursor-pointer ${
                      formPatient.standaardTariefType === 'Regulier' || !formPatient.standaardTariefType
                        ? 'bg-slate-800 text-white border-slate-800 dark:bg-brand-800 shadow-xs'
                        : 'bg-slate-50 dark:bg-brand-950 text-slate-600 dark:text-brand-300 border-slate-200 dark:border-brand-800 hover:bg-slate-100'
                    }`}
                  >
                    <span>Regulier</span>
                  </button>

                  <button
                    type="button"
                    onClick={() => setFormPatient({ ...formPatient, standaardTariefType: 'ELP' })}
                    className={`py-2.5 px-4 text-xs font-bold rounded-xl border transition flex items-center justify-center space-x-2 cursor-pointer ${
                      formPatient.standaardTariefType === 'ELP'
                        ? 'bg-brand-600 text-white border-brand-600 shadow-xs'
                        : 'bg-slate-50 dark:bg-brand-950 text-slate-600 dark:text-brand-300 border-slate-200 dark:border-brand-800 hover:bg-slate-100'
                    }`}
                  >
                    <FileSpreadsheet className="h-4 w-4 shrink-0 text-brand-300" />
                    <span>ELP (Eerstelijnszorg)</span>
                  </button>
                </div>
                <p className="mt-1.5 text-[11px] text-slate-400 dark:text-brand-400">
                  Nieuwe afspraken voor ELP-patiënten worden automatisch als ELP-sessies ingepland.
                </p>
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                  Emotionele Stabiliteit (1.0 - 10.0) <span className="text-red-500 font-bold ml-0.5">*</span>
                </label>
                <input
                  type="text"
                  required
                  value={formPatient.emotioneleStabiliteit !== undefined && formPatient.emotioneleStabiliteit !== null ? formPatient.emotioneleStabiliteit : ''}
                  onChange={(e) => {
                    setFormPatient({ ...formPatient, emotioneleStabiliteit: e.target.value });
                    if (fieldErrors.emotioneleStabiliteit) setFieldErrors({ ...fieldErrors, emotioneleStabiliteit: false });
                  }}
                  className={`w-full bg-slate-50 dark:bg-brand-950 border ${fieldErrors.emotioneleStabiliteit ? 'border-red-500 ring-2 ring-red-500/30' : 'border-slate-200 dark:border-brand-800'} py-2.5 px-4 rounded-xl text-slate-800 dark:text-white font-semibold`}
                  placeholder="5.5 (of 5,5)"
                />
              </div>

              </div>

              <div className="flex justify-end space-x-3 pt-4 mt-2 border-t border-slate-100 dark:border-brand-800/40 shrink-0">
                <button
                  type="button"
                  onClick={() => setIsFormModalOpen(false)}
                  className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-5 rounded-xl font-semibold transition cursor-pointer"
                >
                  Annuleren
                </button>
                <button
                  type="submit"
                  className="bg-brand-600 hover:bg-brand-700 text-white py-2.5 px-5 rounded-xl font-semibold transition shadow-sm cursor-pointer"
                >
                  Opslaan
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Link Account Modal */}
      {isLinkModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 dark:bg-black/70 backdrop-blur-sm p-4 overflow-y-auto animate-in fade-in duration-200">
          <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-md max-h-[90vh] flex flex-col p-6 border border-slate-100 dark:border-brand-800/40 transition-colors overflow-hidden">
            <div className="shrink-0">
              <h3 className="text-lg font-bold text-slate-800 dark:text-white mb-1">Gebruikersaccount Koppelen</h3>
              <p className="text-xs text-slate-500 dark:text-brand-300 mb-3">
                Selecteer een geregistreerd patiëntenaccount om te koppelen aan dit medisch dossier.
              </p>
            </div>
            <form onSubmit={handleLink} className="flex flex-col flex-1 min-h-0">
              <div className="flex-1 overflow-y-auto pr-1 space-y-4">
                {loadingUnlinked ? (
                  <div className="py-6 text-center text-xs text-slate-500 dark:text-brand-300">
                    Geregistreerde accounts laden...
                  </div>
                ) : !useManualEmailInput ? (
                  <div>
                    {unlinkedUsers.length > 0 ? (
                      <div>
                        <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                          Selecteer Geregistreerde Gebruiker
                        </label>
                        <select
                          required
                          value={linkEmail}
                          onChange={(e) => setLinkEmail(e.target.value)}
                          className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white text-sm focus:ring-2 focus:ring-brand-500 outline-none"
                        >
                          <option value="" disabled>-- Selecteer een geregistreerd account --</option>
                          {unlinkedUsers.map((u) => (
                            <option key={u.id} value={u.email}>
                              {u.voornaam} {u.achternaam} ({u.email})
                            </option>
                          ))}
                        </select>
                      </div>
                    ) : (
                      <div className="p-3.5 bg-amber-50 dark:bg-amber-950/40 border border-amber-200 dark:border-amber-800/50 rounded-2xl text-xs text-amber-800 dark:text-amber-200">
                        <p className="font-semibold mb-1">Geen ongekoppelde accounts beschikbaar</p>
                        <p>Er zijn momenteel geen geregistreerde accounts die nog niet gekoppeld zijn. De patiënt moet zich eerst registreren via het portaal.</p>
                      </div>
                    )}
                    <button
                      type="button"
                      onClick={() => setUseManualEmailInput(true)}
                      className="mt-2 text-xs text-brand-600 dark:text-brand-400 hover:underline inline-block font-medium"
                    >
                      Handmatig e-mailadres invoeren
                    </button>
                  </div>
                ) : (
                  <div>
                    <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                      Gebruiker e-mailadres (handmatig)
                    </label>
                    <input
                      type="email"
                      required
                      value={linkEmail}
                      onChange={(e) => setLinkEmail(e.target.value)}
                      className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400 text-sm focus:ring-2 focus:ring-brand-500 outline-none"
                      placeholder="patient@email.be"
                    />
                    {unlinkedUsers.length > 0 && (
                      <button
                        type="button"
                        onClick={() => {
                          setUseManualEmailInput(false);
                          if (unlinkedUsers.length > 0) setLinkEmail(unlinkedUsers[0].email || '');
                        }}
                        className="mt-2 text-xs text-brand-600 dark:text-brand-400 hover:underline inline-block font-medium"
                      >
                        Kies uit geregistreerde accounts ({unlinkedUsers.length})
                      </button>
                    )}
                  </div>
                )}
              </div>

              <div className="flex justify-end space-x-2 pt-3 mt-2 border-t border-slate-100 dark:border-brand-800/40 shrink-0">
                <button
                  type="button"
                  onClick={() => setIsLinkModalOpen(false)}
                  className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2 px-4 rounded-xl font-semibold transition text-sm cursor-pointer"
                >
                  Annuleren
                </button>
                <button
                  type="submit"
                  className="bg-brand-600 hover:bg-brand-700 text-white py-2 px-4 rounded-xl font-semibold transition text-sm shadow-sm cursor-pointer"
                >
                  Koppelen
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
