import React, { useState, useEffect } from 'react';
import { patientApi } from '../services/api';
import { Patient } from '../types';
import { 
  Users, Search, Plus, Edit2, Trash2, ArrowLeft, 
  Link2, Link2Off, RefreshCw, Calendar, Loader2
} from 'lucide-react';
import { InfoTooltip } from '../components/common/InfoTooltip';
import { getPatientDisplayName } from '../utils/patientUtils';
import { extractErrorMessage } from '../utils/errorUtils';

export const Patients: React.FC = () => {
  const [activePatients, setActivePatients] = useState<Patient[]>([]);
  const [archivedPatients, setArchivedPatients] = useState<Patient[]>([]);
  const [searchQuery, setSearchQuery] = useState('');
  const [activeTab, setActiveTab] = useState<'active' | 'archived'>('active');
  const [loading, setLoading] = useState(true);

  // Detail Modal / Panel State
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [patientDetails, setPatientDetails] = useState<Patient | null>(null);

  // Edit / Create Modal State
  const [isFormModalOpen, setIsFormModalOpen] = useState(false);
  const [formPatient, setFormPatient] = useState<any>({
    id: null,
    voornaam: '',
    achternaam: '',
    geboortedatum: '',
    email: '',
    secundairEmail: '',
    telefoonnummer: '',
    dossierNummer: ''
  });

  // Link Modal State
  const [isLinkModalOpen, setIsLinkModalOpen] = useState(false);
  const [linkEmail, setLinkEmail] = useState('');
  const [linkPatientId, setLinkPatientId] = useState<number | null>(null);

  const loadPatients = async () => {
    try {
      setLoading(true);
      const active = await patientApi.getAll();
      setActivePatients(active);
      const archived = await patientApi.getArchive();
      setArchivedPatients(archived);
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
    setFormPatient({
      id: null,
      voornaam: '',
      achternaam: '',
      geboortedatum: '',
      email: '',
      secundairEmail: '',
      telefoonnummer: '',
      dossierNummer: '',
      emotioneleStabiliteit: 5.5
    });
    setIsFormModalOpen(true);
  };

  const handleOpenEditModal = (patient: Patient) => {
    setFormPatient({
      id: patient.id,
      voornaam: patient.voornaam,
      achternaam: patient.achternaam,
      geboortedatum: patient.geboortedatum,
      email: patient.email,
      secundairEmail: patient.secundairEmail || '',
      telefoonnummer: patient.telefoonnummer || '',
      dossierNummer: patient.dossierNummer || '',
      emotioneleStabiliteit: patient.emotioneleStabiliteit !== null && patient.emotioneleStabiliteit !== undefined ? patient.emotioneleStabiliteit : 5.5
    });
    setIsFormModalOpen(true);
  };

  const handleSavePatient = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = {
        ...formPatient,
        emotioneleStabiliteit: formPatient.emotioneleStabiliteit ? Number(formPatient.emotioneleStabiliteit) : null
      };

      if (formPatient.id === null) {
        await patientApi.create({
          voornaam: payload.voornaam,
          achternaam: payload.achternaam,
          geboortedatum: payload.geboortedatum,
          email: payload.email,
          telefoonnummer: payload.telefoonnummer || null,
          dossierNummer: payload.dossierNummer || null,
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
      alert('Opslaan mislukt. Controleer de velden.');
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

  const filteredPatients = (activeTab === 'active' ? activePatients : archivedPatients).filter(
    (p) =>
      getPatientDisplayName(p).toLowerCase().includes(searchQuery.toLowerCase()) ||
      p.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (p.dossierNummer && p.dossierNummer.toLowerCase().includes(searchQuery.toLowerCase()))
  );

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

  return (
    <div className="p-6 max-w-7xl mx-auto space-y-8">
      {/* Header */}
      <div className="flex justify-between items-center bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm transition-colors">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <Users className="h-7 w-7 text-brand-600 dark:text-brand-400" />
            <span>Patiëntenbeheer</span>
          </h1>
          <p className="text-slate-500 dark:text-brand-300 mt-1">
            Beheer patiëntendossiers en gebruikersaccountkoppelingen.
          </p>
        </div>
        <button
          onClick={handleOpenCreateModal}
          className="flex items-center space-x-2 bg-brand-600 hover:bg-brand-700 text-white font-semibold py-3 px-6 rounded-2xl transition shadow-lg shadow-brand-500/10"
        >
          <Plus className="h-5 w-5" />
          <span>Nieuwe Patiënt</span>
        </button>
      </div>

      {/* Main View: Split screen if selected patient */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        {/* Left pane: Search and List */}
        <div className={`lg:col-span-${selectedPatient ? '1' : '3'} space-y-6`}>
          <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-4 transition-colors">
            {/* Search filter */}
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-400">
                <Search className="h-5 w-5" />
              </div>
              <input
                type="text"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                placeholder="Zoek op naam, email of dossiernummer..."
                className="pl-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-800 bg-slate-50/50 dark:bg-brand-950/60 py-2.5 px-4 text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition"
              />
            </div>

            {/* Tabs */}
            <div className="flex border-b border-slate-100 dark:border-brand-800/40">
              <button
                onClick={() => { setActiveTab('active'); setSelectedPatient(null); }}
                className={`py-2 px-4 font-semibold text-sm transition-all border-b-2 ${
                  activeTab === 'active' 
                    ? 'border-brand-600 text-brand-700 dark:text-brand-300' 
                    : 'border-transparent text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-brand-200'
                }`}
              >
                Actief ({activePatients.length})
              </button>
              <button
                onClick={() => { setActiveTab('archived'); setSelectedPatient(null); }}
                className={`py-2 px-4 font-semibold text-sm transition-all border-b-2 ${
                  activeTab === 'archived' 
                    ? 'border-brand-600 text-brand-700 dark:text-brand-300' 
                    : 'border-transparent text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-brand-200'
                }`}
              >
                Archief / Inactief ({archivedPatients.length})
              </button>
            </div>
          </div>

          {/* Patients Listing */}
          <div className="space-y-4 max-h-[500px] overflow-y-auto pr-1">
            {filteredPatients.length > 0 ? (
              filteredPatients.map((p) => (
                <div
                  key={p.id}
                  onClick={() => { setSelectedPatient(p); loadPatientDetail(p.id); }}
                  className={`p-4 rounded-2xl border cursor-pointer transition ${
                    selectedPatient?.id === p.id
                      ? 'bg-brand-50/50 dark:bg-brand-800/60 border-brand-200 dark:border-brand-700'
                      : 'bg-white dark:bg-brand-900 border-slate-100 dark:border-brand-800/40 hover:border-slate-200 dark:hover:border-brand-700 hover:shadow-sm'
                  }`}
                >
                  <div className="flex justify-between items-start">
                    <div className="space-y-1">
                      <h4 className="font-semibold text-slate-800 dark:text-white">{p.volledigeNaam}</h4>
                      <p className="text-xs text-slate-500 dark:text-brand-300">
                        {p.email}
                        {p.secundairEmail && <span className="text-slate-400 dark:text-brand-400 font-normal ml-1">({p.secundairEmail})</span>}
                      </p>
                      <div className="flex flex-wrap gap-1.5 pt-1">
                        {p.dossierNummer && (
                          <span className="text-[10px] bg-slate-100 dark:bg-brand-950 text-slate-600 dark:text-brand-200 py-0.5 px-2 rounded-full font-mono">
                            {p.dossierNummer}
                          </span>
                        )}
                        {activeTab === 'active' && (() => {
                          const risk = getNoShowRisk(p);
                          if (risk.category !== 'Low') {
                            return (
                              <span className={`text-[10px] py-0.5 px-2 rounded-full font-bold ${
                                risk.category === 'High' ? 'bg-red-100 dark:bg-red-950/60 text-red-800 dark:text-red-300' : 'bg-amber-100 dark:bg-amber-950/60 text-amber-800 dark:text-amber-300'
                              }`}>
                                Risk: {risk.category}
                              </span>
                            );
                          }
                          return null;
                        })()}
                      </div>
                    </div>
                    {activeTab === 'archived' && (
                      <button
                        onClick={(e) => { e.stopPropagation(); handleReactivate(p.id); }}
                        className="text-xs font-semibold bg-emerald-50 dark:bg-emerald-950/60 hover:bg-emerald-100 text-emerald-700 dark:text-emerald-300 py-1.5 px-3 rounded-xl transition flex items-center space-x-1"
                      >
                        <RefreshCw className="h-3.5 w-3.5" />
                        <span>Heractiveer</span>
                      </button>
                    )}
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center py-12 bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 text-slate-400 dark:text-brand-400">
                Geen patiënten gevonden.
              </div>
            )}
          </div>
        </div>

        {/* Right pane: Details View */}
        {selectedPatient && (
          <div className="lg:col-span-2 bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-6 space-y-6 transition-colors">
            <div className="flex justify-between items-start border-b border-slate-100 dark:border-brand-800/40 pb-4">
              <div>
                <button 
                  onClick={() => { setSelectedPatient(null); setPatientDetails(null); }}
                  className="flex items-center text-xs font-bold text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-brand-200 uppercase tracking-wider mb-2"
                >
                  <ArrowLeft className="h-3 w-3 mr-1" /> Terug naar lijst
                </button>
                <h2 className="text-xl font-bold text-slate-800 dark:text-white">
                  {patientDetails ? patientDetails.volledigeNaam : selectedPatient.volledigeNaam}
                </h2>
              </div>
              
              <div className="flex space-x-2">
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
            {patientDetails && (
              <div className="space-y-6">
                <div className="grid grid-cols-2 gap-4 text-sm bg-slate-50 dark:bg-brand-950/80 p-4 rounded-2xl border border-slate-100 dark:border-brand-800/60">
                  <div>
                    <span className="text-slate-400 dark:text-brand-300 font-medium block">Dossiernummer</span>
                    <span className="text-slate-800 dark:text-white font-semibold font-mono">{patientDetails.dossierNummer || 'DOS-N/A'}</span>
                  </div>
                  <div>
                    <span className="text-slate-400 dark:text-brand-300 font-medium block">Geboortedatum</span>
                    <span className="text-slate-800 dark:text-white font-semibold">
                      {new Date(patientDetails.geboortedatum).toLocaleDateString('nl-NL')}
                    </span>
                  </div>
                  <div>
                    <span className="text-slate-400 dark:text-brand-300 font-medium block">E-mailadres</span>
                    <span className="text-slate-800 dark:text-brand-100 font-semibold">{patientDetails.email}</span>
                    {patientDetails.secundairEmail && (
                      <div className="text-xs text-slate-500 dark:text-brand-300 mt-0.5">
                        <span className="font-medium text-slate-400 dark:text-brand-400">Secundair:</span> {patientDetails.secundairEmail}
                      </div>
                    )}
                  </div>
                  <div>
                    <span className="text-slate-400 dark:text-brand-300 font-medium block">Telefoon</span>
                    <span className="text-slate-800 dark:text-white font-semibold">{patientDetails.telefoonnummer || '—'}</span>
                  </div>
                  <div>
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
                  <div>
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
                <div className="border border-slate-100 dark:border-brand-800/60 p-4 rounded-2xl flex justify-between items-center bg-white dark:bg-brand-950/80">
                  <div className="space-y-0.5">
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
                      className="flex items-center space-x-1.5 text-xs font-semibold bg-amber-50 dark:bg-amber-950/60 hover:bg-amber-100 dark:hover:bg-amber-900/60 text-amber-700 dark:text-amber-300 py-2 px-3 rounded-xl transition"
                    >
                      <Link2Off className="h-4 w-4" />
                      <span>Ontkoppelen</span>
                    </button>
                  ) : (
                    <button
                      onClick={() => { setLinkPatientId(patientDetails.id); setLinkEmail(''); setIsLinkModalOpen(true); }}
                      className="flex items-center space-x-1.5 text-xs font-semibold bg-brand-50 dark:bg-brand-800/60 hover:bg-brand-100 dark:hover:bg-brand-700 text-brand-700 dark:text-brand-200 py-2 px-3 rounded-xl transition"
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
                  
                  <div className="space-y-2 max-h-[220px] overflow-y-auto pr-1">
                    {patientDetails.afspraken && patientDetails.afspraken.length > 0 ? (
                      patientDetails.afspraken.map((appt) => {
                        const start = new Date(appt.starttijd);
                        return (
                          <div key={appt.id} className="p-3 bg-slate-50/50 dark:bg-brand-950/60 rounded-xl border border-slate-100 dark:border-brand-800/40 flex justify-between items-center text-xs">
                            <div>
                              <span className="font-semibold text-slate-700 dark:text-brand-100 mr-2">{appt.afspraakTypeNaam}</span>
                              <span className="text-slate-500 dark:text-brand-300">
                                {start.toLocaleDateString('nl-NL')} om {start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                              </span>
                            </div>
                            <span 
                              className="py-0.5 px-2 rounded-full text-[10px] font-bold uppercase tracking-wider text-white shadow-xs"
                              style={{ backgroundColor: appt.kleurcode }}
                            >
                              {appt.status}
                            </span>
                          </div>
                        );
                      })
                    ) : (
                      <p className="text-xs text-slate-400 dark:text-brand-400 italic text-center py-4">Nog geen afspraken gepland voor deze patiënt.</p>
                    )}
                  </div>
                </div>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Add / Edit Patient Modal */}
      {isFormModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 dark:bg-black/70 backdrop-blur-sm p-4 animate-in fade-in duration-200">
          <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-lg p-6 border border-slate-100 dark:border-brand-800/40 transition-colors">
            <h3 className="text-xl font-bold text-slate-800 dark:text-white mb-4">
              {formPatient.id === null ? 'Patiënt Toevoegen' : 'Patiënt Bewerken'}
            </h3>
            <form onSubmit={handleSavePatient} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Voornaam</label>
                  <input
                    type="text"
                    required
                    value={formPatient.voornaam}
                    onChange={(e) => setFormPatient({ ...formPatient, voornaam: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="Voornaam"
                  />
                </div>
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Achternaam</label>
                  <input
                    type="text"
                    required
                    value={formPatient.achternaam}
                    onChange={(e) => setFormPatient({ ...formPatient, achternaam: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="Achternaam"
                  />
                </div>
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Geboortedatum</label>
                <input
                  type="date"
                  required
                  value={formPatient.geboortedatum ? formPatient.geboortedatum.split('T')[0] : ''}
                  onChange={(e) => setFormPatient({ ...formPatient, geboortedatum: e.target.value })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Primair E-mailadres</label>
                  <input
                    type="email"
                    required
                    value={formPatient.email}
                    onChange={(e) => setFormPatient({ ...formPatient, email: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="primair@adres.be"
                  />
                </div>

                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Secundair E-mailadres <span className="text-xs font-normal text-slate-400">(Optioneel)</span></label>
                  <input
                    type="email"
                    value={formPatient.secundairEmail || ''}
                    onChange={(e) => setFormPatient({ ...formPatient, secundairEmail: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="secundair@adres.be (of leegmaken)"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Telefoonnummer</label>
                  <input
                    type="tel"
                    value={formPatient.telefoonnummer}
                    onChange={(e) => setFormPatient({ ...formPatient, telefoonnummer: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="04..."
                  />
                </div>
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Dossiernummer</label>
                  <input
                    type="text"
                    value={formPatient.dossierNummer}
                    onChange={(e) => setFormPatient({ ...formPatient, dossierNummer: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white font-mono placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="DOS-001"
                  />
                </div>
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Emotionele Stabiliteit (1.0 - 10.0)</label>
                <input
                  type="number"
                  step="0.1"
                  min="1.0"
                  max="10.0"
                  required
                  value={formPatient.emotioneleStabiliteit}
                  onChange={(e) => setFormPatient({ ...formPatient, emotioneleStabiliteit: Number(e.target.value) })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white font-semibold"
                  placeholder="5.5"
                />
              </div>

              <div className="flex justify-end space-x-3 pt-4 border-t border-slate-100 dark:border-brand-800/40">
                <button
                  type="button"
                  onClick={() => setIsFormModalOpen(false)}
                  className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-5 rounded-xl font-semibold transition"
                >
                  Annuleren
                </button>
                <button
                  type="submit"
                  className="bg-brand-600 hover:bg-brand-700 text-white py-2.5 px-5 rounded-xl font-semibold transition shadow-sm"
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
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 dark:bg-black/70 backdrop-blur-sm p-4 animate-in fade-in duration-200">
          <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-sm p-6 border border-slate-100 dark:border-brand-800/40 transition-colors">
            <h3 className="text-lg font-bold text-slate-800 dark:text-white mb-2">Gebruikersaccount Koppelen</h3>
            <p className="text-xs text-slate-500 dark:text-brand-300 mb-4">
              Geef het e-mailadres op van het geregistreerde patiëntenaccount dat gekoppeld moet worden aan dit medisch dossier.
            </p>
            <form onSubmit={handleLink} className="space-y-4">
              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Gebruiker e-mailadres</label>
                <input
                  type="email"
                  required
                  value={linkEmail}
                  onChange={(e) => setLinkEmail(e.target.value)}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                  placeholder="patient@email.be"
                />
              </div>
              <div className="flex justify-end space-x-2 pt-2 border-t border-slate-100 dark:border-brand-800/40">
                <button
                  type="button"
                  onClick={() => setIsLinkModalOpen(false)}
                  className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2 px-4 rounded-xl font-semibold transition text-sm"
                >
                  Annuleren
                </button>
                <button
                  type="submit"
                  className="bg-brand-600 hover:bg-brand-700 text-white py-2 px-4 rounded-xl font-semibold transition text-sm shadow-sm"
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
