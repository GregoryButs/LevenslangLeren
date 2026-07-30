import React, { useState, useEffect } from 'react';
import { dashboardApi, afspraakApi, patientApi, aiApi } from '../services/api';
import { DashboardData, Afspraak, Patient, AfspraakType } from '../types';
import { 
  Calendar, Users, Clock, ArrowRight, ChevronLeft, ChevronRight, 
  Plus, UserCheck, AlertCircle, Loader2,
  Brain, LineChart, Activity, Database, RotateCcw, ShieldAlert, Search, HelpCircle
} from 'lucide-react';
import { GoogleSetupGuide } from './GoogleSetupGuide';
import { AfspraakDetailModal } from '../components/AfspraakDetailModal';

const InfoTooltip: React.FC<{ content: React.ReactNode; position?: 'top' | 'bottom' }> = ({ content, position = 'top' }) => {
  const isBottom = position === 'bottom';
  return (
    <div className="relative group inline-block cursor-help ml-2 align-middle select-none">
      <HelpCircle className="h-4 w-4 text-slate-400 hover:text-brand-500 transition-colors inline" />
      <div className={`absolute z-50 left-1/2 transform -translate-x-1/2 w-64 p-3 bg-slate-900 text-white text-[11px] font-normal leading-relaxed rounded-xl shadow-xl border border-slate-700 opacity-0 pointer-events-none group-hover:opacity-100 transition-opacity duration-200 ${
        isBottom ? 'top-full mt-2' : 'bottom-full mb-2'
      }`}>
        {content}
        <div className={`absolute left-1/2 transform -translate-x-1/2 border-[5px] border-solid border-transparent ${
          isBottom ? 'bottom-full border-b-slate-900' : 'top-full border-t-slate-900'
        }`}></div>
      </div>
    </div>
  );
};

interface PsycholoogDashboardProps {
  initialTab?: 'agenda' | 'ai_lab' | 'google_setup';
}

export const PsycholoogDashboard: React.FC<PsycholoogDashboardProps> = ({ initialTab = 'agenda' }) => {
  const [data, setData] = useState<DashboardData | null>(null);
  const [selectedDate, setSelectedDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // AI Tab & Sub-tab State
  const [currentTab, setCurrentTab] = useState<'agenda' | 'ai_lab' | 'google_setup'>(initialTab);
  const [activeAiSubTab, setActiveAiSubTab] = useState<'risks' | 'explorer' | 'simulator' | 'heatmap'>('risks');

  useEffect(() => {
    setCurrentTab(initialTab);
  }, [initialTab]);

  // AI Risks Data
  const [patientRisks, setPatientRisks] = useState<any[]>([]);
  const [risksLoading, setRisksLoading] = useState(false);

  // Dataset Explorer Data
  const [syntheticPatients, setSyntheticPatients] = useState<any[]>([]);
  const [synTotal, setSynTotal] = useState(0);
  const [synPage, setSynPage] = useState(1);
  const [synPageSize] = useState(10);
  const [synSearch, setSynSearch] = useState('');
  const [synLoading, setSynLoading] = useState(false);

  // RL Simulator State
  const [simState, setSimState] = useState({
    sessionsCompleted: 0,
    stability: 5.0,
    gap: 7,
    sentiment: 0.0,
    sentimentEma: 0.0,
    recommendedAction: 'Standaard (7d)',
    heuristicAction: 'Standaard (7d)'
  });
  const [simHistory, setSimHistory] = useState<any[]>([]);
  const [simCumulativeReward, setSimCumulativeReward] = useState(0);
  const [simDone, setSimDone] = useState(false);
  const [simLoading, setSimLoading] = useState(false);

  // New Booking Modal State
  const [isBookModalOpen, setIsBookModalOpen] = useState(false);
  const [bookingPatients, setBookingPatients] = useState<{ id: number; naam: string }[]>([]);
  const [bookingTypes, setBookingTypes] = useState<AfspraakType[]>([]);
  const [newBooking, setNewBooking] = useState({
    patientId: '' as string | number,
    typeId: '',
    starttijd: '',
    opmerkingen: '',
    herhaling: 0,
    herhaalTot: ''
  });

  // Details Modal State
  const [selectedAfspraak, setSelectedAfspraak] = useState<Afspraak | null>(null);

  // Pending user applications for psychologist approval
  const [aanmeldingen, setAanmeldingen] = useState<any[]>([]);
  const [selectedPatientForLink, setSelectedPatientForLink] = useState<{ [key: string]: string }>({});
  const [patientsList, setPatientsList] = useState<Patient[]>([]);

  const loadDashboard = async (dateStr: string) => {
    try {
      setLoading(true);
      const dashboardData = await dashboardApi.getDashboard(dateStr);
      setData(dashboardData);
      
      const pending = await patientApi.getAanmeldingen();
      setAanmeldingen(pending);

      const patients = await patientApi.getAll();
      setPatientsList(patients);
      setError(null);
    } catch (err: any) {
      console.error(err);
      setError('Laden van dashboard gegevens mislukt.');
    } finally {
      setLoading(false);
    }
  };

  const fetchPatientRisks = async () => {
    try {
      setRisksLoading(true);
      const res = await aiApi.getPatientRisks();
      setPatientRisks(res);
    } catch (err) {
      console.error(err);
    } finally {
      setRisksLoading(false);
    }
  };

  const fetchSyntheticPatients = async () => {
    try {
      setSynLoading(true);
      const res = await aiApi.getSyntheticPatients(synPage, synPageSize, synSearch);
      setSyntheticPatients(res.patients);
      setSynTotal(res.totalCount);
    } catch (err) {
      console.error(err);
    } finally {
      setSynLoading(false);
    }
  };

  useEffect(() => {
    loadDashboard(selectedDate);
  }, [selectedDate]);

  useEffect(() => {
    if (currentTab === 'ai_lab') {
      if (activeAiSubTab === 'risks') {
        fetchPatientRisks();
      } else if (activeAiSubTab === 'explorer') {
        fetchSyntheticPatients();
      }
    }
  }, [currentTab, activeAiSubTab, synPage, synSearch]);

  const handleSimStep = async (action: number) => {
    try {
      setSimLoading(true);
      const res = await aiApi.simulateStep(
        simState.sessionsCompleted,
        simState.stability,
        simState.gap,
        simState.sentiment,
        simState.sentimentEma,
        action
      );
      
      const nextStateArr = res.nextState;
      const stepReward = res.reward;
      const stepDone = res.done;
      const stepInfo = res.info;

      const actionNames = [
        "Standaard Interval (7d)", 
        "Intensief Traject (3d)", 
        "Digitale Check-in (7d)", 
        "Discharge (Ontslag)"
      ];
      
      const newHistoryItem = {
        action: actionNames[action],
        attended: stepInfo.attended,
        safetyTriggered: stepInfo.safetyTriggered,
        reward: stepReward,
        stability: nextStateArr[1],
        sentimentEma: nextStateArr[4],
        dischargeStatus: stepInfo.dischargeStatus,
        noShowProbability: stepInfo.noShowProbability
      };

      setSimHistory([newHistoryItem, ...simHistory]);
      setSimCumulativeReward(prev => Math.round((prev + stepReward) * 100) / 100);
      setSimDone(stepDone);
      setSimState({
        sessionsCompleted: nextStateArr[0],
        stability: nextStateArr[1],
        gap: nextStateArr[2],
        sentiment: nextStateArr[3],
        sentimentEma: nextStateArr[4],
        recommendedAction: res.recommendedAction,
        heuristicAction: res.heuristicAction
      });
    } catch (err) {
      console.error(err);
      alert('Fout tijdens simulatiestap.');
    } finally {
      setSimLoading(false);
    }
  };

  const handleSimReset = () => {
    setSimState({
      sessionsCompleted: 0,
      stability: 5.0,
      gap: 7,
      sentiment: 0.0,
      sentimentEma: 0.0,
      recommendedAction: 'Standaard (7d)',
      heuristicAction: 'Standaard (7d)'
    });
    setSimHistory([]);
    setSimCumulativeReward(0);
    setSimDone(false);
  };

  const handleDateChange = (days: number) => {
    const current = new Date(selectedDate);
    current.setDate(current.getDate() + days);
    setSelectedDate(current.toISOString().split('T')[0]);
  };

  const handleOpenBookModal = async () => {
    try {
      const res = await afspraakApi.getCreateData();
      setBookingPatients(res.patienten);
      setBookingTypes(res.types);
      setNewBooking({
        patientId: res.patienten[0]?.id || '',
        typeId: res.types[0]?.id.toString() || '',
        starttijd: `${selectedDate}T09:00`,
        opmerkingen: '',
        herhaling: 0,
        herhaalTot: ''
      });
      setIsBookModalOpen(true);
    } catch (err) {
      console.error(err);
      alert('Fout bij het ophalen van boekingsdata.');
    }
  };

  const handleCreateBooking = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await afspraakApi.create({
        patientId: newBooking.patientId ? Number(newBooking.patientId) : null,
        typeId: Number(newBooking.typeId),
        starttijd: newBooking.starttijd,
        opmerkingen: newBooking.opmerkingen,
        herhaling: Number(newBooking.herhaling),
        herhaalTot: newBooking.herhaalTot ? newBooking.herhaalTot : null
      });
      setIsBookModalOpen(false);
      loadDashboard(selectedDate);
      alert('Afspraak succesvol ingepland!');
    } catch (err: any) {
      console.error(err);
      alert(err.response?.data?.message || 'Inplannen mislukt. Controleer op eventuele overlappingen.');
    }
  };



  // Aanmeldingsacties
  const handleApproveNewPatient = async (userId: string) => {
    try {
      await patientApi.approveAanmelding(userId);
      alert('Patiëntendossier succesvol aangemaakt en gekoppeld.');
      loadDashboard(selectedDate);
    } catch (err) {
      console.error(err);
      alert('Goedkeuring mislukt.');
    }
  };

  const handleLinkExistingPatient = async (userEmail: string, userId: string) => {
    const patientId = selectedPatientForLink[userId];
    if (!patientId) {
      alert('Selecteer eerst een bestaande patiënt.');
      return;
    }

    try {
      await patientApi.link(Number(patientId), userEmail);
      alert('Gebruiker succesvol gekoppeld aan de bestaande patiënt.');
      loadDashboard(selectedDate);
    } catch (err) {
      console.error(err);
      alert('Koppelen mislukt.');
    }
  };

  if (loading && !data) {
    return (
      <div className="flex items-center justify-center min-h-[80vh]">
        <Loader2 className="animate-spin h-10 w-10 text-brand-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-4xl mx-auto mt-8 p-4 bg-red-50 text-red-700 rounded-xl border border-red-200">
        <div className="flex items-center space-x-2">
          <AlertCircle className="h-5 w-5" />
          <span>{error}</span>
        </div>
      </div>
    );
  }

  const getSelectedDayAppointments = (): Afspraak[] => {
    if (!data?.weekOverzicht?.dagen) return [];
    const matchingKey = Object.keys(data.weekOverzicht.dagen).find(
      (key) => key.split('T')[0] === selectedDate
    );
    return matchingKey ? (data.weekOverzicht.dagen[matchingKey] as Afspraak[]) : [];
  };

  const dayAppointments = getSelectedDayAppointments();

  return (
    <div className="space-y-8 p-6 max-w-7xl mx-auto">
      {/* Welcome/Page Header Banner */}
      <div className="flex justify-between items-center bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm transition-colors">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-brand-50">
            {currentTab === 'agenda' && `Welkom terug, ${data?.psycholoogNaam || ''}`}
            {currentTab === 'ai_lab' && 'AI Lab & Simulator'}
            {currentTab === 'google_setup' && 'Google Agenda Synchronisatie'}
          </h1>
          <p className="text-slate-500 dark:text-slate-200 mt-1 font-medium">
            {currentTab === 'agenda' && 'Hier is uw agenda-overzicht voor vandaag.'}
            {currentTab === 'ai_lab' && "Analyseer patiëntrisico's en simuleer Q-learning beslissingen."}
            {currentTab === 'google_setup' && 'Koppel uw Google Agenda en configureer synchronisatie-instellingen.'}
          </p>
        </div>
        {currentTab === 'agenda' && (
          <button
            onClick={handleOpenBookModal}
            className="flex items-center space-x-2 bg-brand-600 hover:bg-brand-700 text-white font-semibold py-3 px-6 rounded-2xl transition shadow-lg shadow-brand-500/10"
          >
            <Plus className="h-5 w-5" />
            <span>Afspraak Plannen</span>
          </button>
        )}
      </div>

      {currentTab === 'agenda' ? (
        <>
          {/* Stats Cards */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm flex items-center space-x-4 transition-colors">
              <div className="h-12 w-12 rounded-2xl bg-brand-50 dark:bg-brand-800 flex items-center justify-center text-brand-600 dark:text-brand-300">
                <Clock className="h-6 w-6" />
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-500 dark:text-brand-300">Afspraken vandaag</p>
                <p className="text-2xl font-bold text-slate-800 dark:text-brand-50">{data?.aantalAfsprakenVandaag}</p>
              </div>
            </div>

            <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm flex items-center space-x-4 transition-colors">
              <div className="h-12 w-12 rounded-2xl bg-emerald-50 dark:bg-emerald-950 flex items-center justify-center text-emerald-600 dark:text-emerald-400">
                <Calendar className="h-6 w-6" />
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-500 dark:text-brand-300">Afspraken deze week</p>
                <p className="text-2xl font-bold text-slate-800 dark:text-brand-50">{data?.aantalAfsprakenDezeWeek}</p>
              </div>
            </div>

            <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm flex items-center space-x-4 transition-colors">
              <div className="h-12 w-12 rounded-2xl bg-blue-50 dark:bg-blue-950 flex items-center justify-center text-blue-600 dark:text-blue-400">
                <Users className="h-6 w-6" />
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-500 dark:text-brand-300">Totaal patiënten</p>
                <p className="text-2xl font-bold text-slate-800 dark:text-brand-50">{data?.aantalPatienten}</p>
              </div>
            </div>
          </div>

          {/* Volgende Afspraak */}
          {data?.volgendeAfspraak && (
            <div className="bg-gradient-to-r from-brand-600 to-emerald-600 p-6 rounded-3xl text-white shadow-xl flex justify-between items-center">
              <div className="space-y-1">
                <span className="text-xs uppercase tracking-wider font-semibold opacity-75 bg-white/20 py-1 px-2.5 rounded-full">
                  Volgende Afspraak
                </span>
                <h3 className="text-xl font-bold pt-2">
                  {data.volgendeAfspraak.patientNaam}
                </h3>
                <p className="text-sm opacity-90 flex items-center space-x-1 pt-1">
                  <Clock className="h-4 w-4 inline" />
                  <span>
                    {new Date(data.volgendeAfspraak.starttijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - {' '}
                    {new Date(data.volgendeAfspraak.eindtijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                  </span>
                  <span className="opacity-50">|</span>
                  <span className="font-semibold">{data.volgendeAfspraak.afspraakTypeNaam}</span>
                </p>
              </div>
              <button
                onClick={() => setSelectedAfspraak(data.volgendeAfspraak!)}
                className="flex items-center space-x-1.5 bg-white/20 hover:bg-white/30 text-white font-semibold py-2.5 px-4 rounded-xl transition"
              >
                <span>Details</span>
                <ArrowRight className="h-4 w-4" />
              </button>
            </div>
          )}

          {/* Main Grid: Planning & Aanmeldingen */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
            {/* Day Agenda Planning */}
            <div className="lg:col-span-2 bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-6 space-y-6 transition-colors">
              <div className="flex justify-between items-center border-b border-slate-100 dark:border-brand-800/40 pb-4">
                <h2 className="text-lg font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
                  <Calendar className="h-5 w-5 text-brand-600 dark:text-brand-400" />
                  <span>Dagplanning</span>
                </h2>
                <div className="flex items-center space-x-3 bg-slate-50 dark:bg-brand-950/60 p-1.5 rounded-2xl border border-slate-200 dark:border-brand-800/60">
                  <button
                    onClick={() => handleDateChange(-1)}
                    className="p-1.5 hover:bg-white dark:hover:bg-brand-900 rounded-xl transition hover:shadow-sm"
                  >
                    <ChevronLeft className="h-5 w-5 text-slate-600 dark:text-brand-200" />
                  </button>
                  <input
                    type="date"
                    value={selectedDate}
                    onChange={(e) => setSelectedDate(e.target.value)}
                    className="bg-transparent font-semibold text-slate-700 dark:text-brand-100 text-sm focus:outline-none border-none cursor-pointer"
                  />
                  <button
                    onClick={() => handleDateChange(1)}
                    className="p-1.5 hover:bg-white dark:hover:bg-brand-900 rounded-xl transition hover:shadow-sm"
                  >
                    <ChevronRight className="h-5 w-5 text-slate-600 dark:text-brand-200" />
                  </button>
                </div>
              </div>

              {/* Agenda List */}
              <div className="space-y-4 max-h-[500px] overflow-y-auto pr-2">
                {dayAppointments && dayAppointments.length > 0 ? (
                  dayAppointments.map((afspraak) => (
                    <div
                      key={afspraak.id}
                      onClick={() => setSelectedAfspraak(afspraak)}
                      className={`flex items-center justify-between p-4 rounded-2xl border cursor-pointer transition-all border-l-4 ${
                        afspraak.status === 'Geannuleerd'
                          ? 'bg-slate-100/70 dark:bg-brand-950/40 border-slate-200 dark:border-brand-800/40 opacity-60'
                          : 'bg-slate-50 dark:bg-brand-950/60 border-slate-100 dark:border-brand-800/40 hover:bg-white dark:hover:bg-brand-950 hover:shadow-md'
                      }`}
                      style={{ borderLeftColor: afspraak.status === 'Geannuleerd' ? '#94a3b8' : afspraak.kleurcode }}
                    >
                      <div className="space-y-1">
                        <div className="flex items-center space-x-2">
                          <span className={`font-semibold ${afspraak.status === 'Geannuleerd' ? 'line-through text-slate-500 dark:text-brand-400' : 'text-slate-800 dark:text-white'}`}>
                            {afspraak.patientNaam}
                          </span>
                          <span
                            className="text-xs font-semibold py-0.5 px-2 rounded-full text-white"
                            style={{ backgroundColor: afspraak.kleurcode }}
                          >
                            {afspraak.afspraakTypeNaam}
                          </span>
                          {afspraak.status !== 'Gepland' && (
                            <span className={`text-[10px] uppercase tracking-wider font-bold py-0.5 px-2 rounded-full ${
                              afspraak.status === 'Voltooid' ? 'bg-emerald-100 dark:bg-emerald-950/60 text-emerald-800 dark:text-emerald-300' : 'bg-red-100 dark:bg-red-950/60 text-red-800 dark:text-red-300'
                            }`}>
                              {afspraak.status}
                            </span>
                          )}
                          {afspraak.opmerkingen && (afspraak.opmerkingen.includes('Praktijkhuis') || afspraak.opmerkingen.includes('[PH9500]')) && (
                            <span className="inline-flex items-center gap-1 text-[10px] font-bold py-0.5 px-2 rounded-full bg-purple-100 dark:bg-purple-950/60 text-purple-800 dark:text-purple-300 border border-purple-200 dark:border-purple-800/40" title="Ingeboekt via Praktijkhuis 9500">
                              🏥 Praktijkhuis 9500
                            </span>
                          )}
                        </div>
                        <p className="text-xs text-slate-500 dark:text-brand-300">
                          {new Date(afspraak.starttijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - {' '}
                          {new Date(afspraak.eindtijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </p>
                        {afspraak.opmerkingen && afspraak.opmerkingen.replace('[PH9500]', '').replace(/^\[Praktijkhuis9500\]\s*/, '').trim() !== '' && (
                          <p className="text-xs text-slate-400 dark:text-brand-400 italic pt-1 line-clamp-2">
                            {afspraak.opmerkingen.replace('[PH9500]', '').replace(/^\[Praktijkhuis9500\]\s*/, '').trim()}
                          </p>
                        )}
                      </div>
                      <ChevronRight className="h-5 w-5 text-slate-400 dark:text-brand-400" />
                    </div>
                  ))
                ) : (
                  <div className="text-center py-12 text-slate-400 dark:text-brand-400">
                    Geen afspraken gepland op deze dag.
                  </div>
                )}
              </div>
            </div>

            {/* New Signups Card */}
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
                          onClick={() => handleApproveNewPatient(app.id)}
                          className="w-full text-xs font-semibold bg-brand-50 dark:bg-brand-800/60 hover:bg-brand-100 dark:hover:bg-brand-800 text-brand-700 dark:text-brand-200 py-2 px-3 rounded-xl transition flex items-center justify-center space-x-1"
                        >
                          <Plus className="h-3.5 w-3.5" />
                          <span>Maak & koppel nieuwe patiënt</span>
                        </button>

                        <div className="flex gap-1.5 mt-1">
                          <select
                            value={selectedPatientForLink[app.id] || ''}
                            onChange={(e) => setSelectedPatientForLink({
                              ...selectedPatientForLink,
                              [app.id]: e.target.value
                            })}
                            className="text-xs w-full bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-800 py-1.5 px-2 rounded-xl text-slate-700 dark:text-brand-100 focus:outline-none"
                          >
                            <option value="">-- Bestaande patiënt --</option>
                            {patientsList.map(p => (
                              <option key={p.id} value={p.id}>{p.volledigeNaam}</option>
                            ))}
                          </select>
                          <button
                            onClick={() => handleLinkExistingPatient(app.email, app.id)}
                            className="text-xs font-semibold bg-emerald-600 hover:bg-emerald-700 text-white py-1.5 px-3 rounded-xl transition"
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
          </div>
        </>
      ) : currentTab === 'ai_lab' ? (
        <div className="space-y-6">
          {/* AI Showcase Tabs */}
          <div className="flex space-x-2 bg-slate-100 p-1.5 rounded-2xl w-fit border border-slate-200 shadow-sm">
            <button
              onClick={() => setActiveAiSubTab('risks')}
              className={`flex items-center space-x-2 py-2 px-4 rounded-xl text-xs font-bold transition-all ${
                activeAiSubTab === 'risks'
                  ? 'bg-white text-brand-700 shadow-md'
                  : 'text-slate-500 hover:text-slate-700'
              }`}
            >
              <Activity className="h-4 w-4" />
              <span>No-Show Risico's</span>
            </button>
            <button
              onClick={() => setActiveAiSubTab('explorer')}
              className={`flex items-center space-x-2 py-2 px-4 rounded-xl text-xs font-bold transition-all ${
                activeAiSubTab === 'explorer'
                  ? 'bg-white text-brand-700 shadow-md'
                  : 'text-slate-500 hover:text-slate-700'
              }`}
            >
              <Database className="h-4 w-4" />
              <span>Dataset Explorer (5000)</span>
            </button>
            <button
              onClick={() => setActiveAiSubTab('simulator')}
              className={`flex items-center space-x-2 py-2 px-4 rounded-xl text-xs font-bold transition-all ${
                activeAiSubTab === 'simulator'
                  ? 'bg-white text-brand-700 shadow-md'
                  : 'text-slate-500 hover:text-slate-700'
              }`}
            >
              <Brain className="h-4 w-4" />
              <span>RL Policy Simulator</span>
            </button>
            <button
              onClick={() => setActiveAiSubTab('heatmap')}
              className={`flex items-center space-x-2 py-2 px-4 rounded-xl text-xs font-bold transition-all ${
                activeAiSubTab === 'heatmap'
                  ? 'bg-white text-brand-700 shadow-md'
                  : 'text-slate-500 hover:text-slate-700'
              }`}
            >
              <LineChart className="h-4 w-4" />
              <span>Causale Heatmap</span>
            </button>
          </div>

          {/* Sub-tab 1: AI Risks */}
          {activeAiSubTab === 'risks' && (
            <div className="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-6">
              <div className="flex justify-between items-center border-b border-slate-100 pb-4">
                <div>
                  <h2 className="text-lg font-bold text-slate-800 flex items-center space-x-2">
                    <Activity className="h-5 w-5 text-brand-600" />
                    <span>AI No-Show Risico Monitor (CRM Cliënten)</span>
                    <InfoTooltip content="Berekent de kans dat een actieve cliënt niet op een afspraak verschijnt. Gebaseerd op de tijd sinds de vorige afspraak (last session gap), het type behandeling en de geschiedenis van voltooide sessies." />
                  </h2>
                  <p className="text-slate-500 text-xs mt-1">Real-time predicties op basis van actieve dossiers in de database.</p>
                </div>
                <button 
                  onClick={fetchPatientRisks}
                  className="text-xs font-semibold bg-brand-50 hover:bg-brand-100 text-brand-700 py-2 px-4 rounded-xl transition flex items-center space-x-1"
                >
                  <RotateCcw className="h-3.5 w-3.5 animate-pulse" />
                  <span>Verversen</span>
                </button>
              </div>

              {risksLoading ? (
                <div className="flex justify-center items-center py-16">
                  <Loader2 className="animate-spin h-8 w-8 text-brand-600" />
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
          )}

          {/* Sub-tab 2: Dataset Explorer */}
          {activeAiSubTab === 'explorer' && (
            <div className="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-6">
              <div className="flex justify-between items-center border-b border-slate-100 pb-4">
                <div>
                  <h2 className="text-lg font-bold text-slate-800 flex items-center space-x-2">
                    <Database className="h-5 w-5 text-brand-600" />
                    <span>Dataset Explorer (5000 Synthetische Cliënten)</span>
                    <InfoTooltip content="Toont de geanonimiseerde dataset van 5.000 gesimuleerde patiëntenprofielen. Deze dataset wordt gebruikt om de machine learning en regression-modellen te trainen." />
                  </h2>
                  <p className="text-slate-500 text-xs mt-1">Blader live door de geanonimiseerde profielen gegenereerd door de Data-Strategist.</p>
                </div>
                
                {/* Search Input */}
                <div className="flex items-center space-x-2 bg-slate-50 border border-slate-200 py-1.5 px-3 rounded-xl w-64 focus-within:ring-2 focus-within:ring-brand-500">
                  <Search className="h-4 w-4 text-slate-400" />
                  <input
                    type="text"
                    placeholder="Zoeken op naam of ID..."
                    value={synSearch}
                    onChange={(e) => {
                      setSynSearch(e.target.value);
                      setSynPage(1);
                    }}
                    className="bg-transparent text-xs text-slate-700 w-full focus:outline-none"
                  />
                </div>
              </div>

              {synLoading ? (
                <div className="flex justify-center items-center py-16">
                  <Loader2 className="animate-spin h-8 w-8 text-brand-600" />
                </div>
              ) : syntheticPatients.length > 0 ? (
                <div className="space-y-4">
                  <div className="overflow-x-auto">
                    <table className="w-full text-left border-collapse">
                      <thead>
                        <tr className="border-b border-slate-100 text-slate-400 text-xs font-semibold uppercase">
                          <th className="py-3 px-4">Client ID</th>
                          <th className="py-3 px-4">Naam</th>
                          <th className="py-3 px-4">Leeftijd</th>
                          <th className="py-3 px-4">Tussenpoos (Gap)</th>
                          <th className="py-3 px-4">Behandeling</th>
                          <th className="py-3 px-4">Sessies</th>
                          <th className="py-3 px-4 text-center">
                            <span className="inline-block align-middle">No-Show Risico</span>
                            <InfoTooltip position="bottom" content="De door het ML-regressiemodel berekende no-show kans (0-100%) voor dit gesimuleerde profiel." />
                          </th>
                          <th className="py-3 px-4 text-center">
                            <span className="inline-block align-middle">No-Show Status</span>
                            <InfoTooltip position="bottom" content="De uiteindelijke, werkelijke uitkomst van de afspraak (Aanwezig of No-Show) in de dataset." />
                          </th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-100 text-sm">
                        {syntheticPatients.map((p: any) => (
                          <tr key={p.clientId} className="hover:bg-slate-50/50">
                            <td className="py-4 px-4 font-mono font-semibold text-xs text-slate-500">{p.clientId}</td>
                            <td className="py-4 px-4 font-bold text-slate-800">{p.name}</td>
                            <td className="py-4 px-4 text-slate-600">{p.age} jr</td>
                            <td className="py-4 px-4 text-slate-600">{p.lastSessionGap} dagen</td>
                            <td className="py-4 px-4">
                              <span className="capitalize px-2 py-0.5 rounded-full text-xs font-semibold bg-brand-50 text-brand-700">
                                {p.treatmentType}
                              </span>
                            </td>
                            <td className="py-4 px-4 text-slate-600">{p.sessionsCompleted}</td>
                            <td className="py-4 px-4 text-center font-semibold text-slate-750">{(p.noShowProbability * 100).toFixed(1)}%</td>
                            <td className="py-4 px-4 text-center">
                              <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-bold ${
                                p.noShow === 1 ? 'bg-red-100 text-red-800' : 'bg-emerald-100 text-emerald-800'
                              }`}>
                                {p.noShow === 1 ? 'No-Show' : 'Aanwezig'}
                              </span>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  {/* Pagination Controls */}
                  <div className="flex justify-between items-center pt-4 border-t border-slate-100 text-xs text-slate-500">
                    <span>
                      Tonen van <strong>{((synPage - 1) * synPageSize) + 1}</strong> tot <strong>{Math.min(synPage * synPageSize, synTotal)}</strong> van de <strong>{synTotal}</strong> records
                    </span>
                    <div className="flex space-x-2">
                      <button
                        onClick={() => setSynPage(p => Math.max(1, p - 1))}
                        disabled={synPage === 1}
                        className="py-1.5 px-3 rounded-lg border border-slate-200 hover:bg-slate-50 disabled:opacity-50 disabled:hover:bg-transparent font-semibold transition"
                      >
                        Vorige
                      </button>
                      <span className="py-1.5 px-3 bg-slate-50 border border-slate-200 rounded-lg text-slate-700 font-bold">
                        Pagina {synPage} van {Math.ceil(synTotal / synPageSize)}
                      </span>
                      <button
                        onClick={() => setSynPage(p => Math.min(Math.ceil(synTotal / synPageSize), p + 1))}
                        disabled={synPage >= Math.ceil(synTotal / synPageSize)}
                        className="py-1.5 px-3 rounded-lg border border-slate-200 hover:bg-slate-50 disabled:opacity-50 disabled:hover:bg-transparent font-semibold transition"
                      >
                        Volgende
                      </button>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="text-center py-12 text-slate-400">Geen records gevonden die voldoen aan de zoekcriteria.</div>
              )}
            </div>
          )}

          {/* Sub-tab 3: RL Policy Simulator */}
          {activeAiSubTab === 'simulator' && (
            <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
              {/* Simulator Panel */}
              <div className="lg:col-span-2 bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-6">
                <div className="flex justify-between items-center border-b border-slate-100 pb-4">
                  <div>
                    <h2 className="text-lg font-bold text-slate-800 flex items-center space-x-2">
                      <Brain className="h-5 w-5 text-brand-600" />
                      <span>Interactive Reinforcement Learning Planner</span>
                      <InfoTooltip content="Een interactieve simulator gebaseerd op een Markov Decision Process (MDP). De AI leert planningsbeslissingen te nemen om de klinische uitkomst te maximaliseren en no-shows te minimaliseren." />
                    </h2>
                    <p className="text-slate-500 text-xs mt-1">Trainingsomgeving (MDP) om planning en continuïteits-rewards te simuleren.</p>
                  </div>
                  <button 
                    onClick={handleSimReset}
                    className="text-xs font-semibold bg-red-50 hover:bg-red-100 text-red-700 py-2 px-4 rounded-xl transition flex items-center space-x-1"
                  >
                    <RotateCcw className="h-3.5 w-3.5" />
                    <span>Reset Agent</span>
                  </button>
                </div>

                {/* Current Client State Indicators */}
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  {/* Stability Gauge */}
                  <div className="bg-slate-50 border border-slate-100 p-4 rounded-2xl space-y-2">
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider flex items-center">
                      <span>Klinische Stabiliteit (E_t)</span>
                      <InfoTooltip content="De psychische toestand van de patiënt (van 1.0 tot 10.0). Stijgt door bijgewoonde sessies en daalt sterk bij no-shows. Onder de 3.0 treedt een automatische crisis-override in werking." />
                    </p>
                    <div className="flex justify-between items-end">
                      <span className="text-3xl font-extrabold text-slate-800">{simState.stability.toFixed(2)}</span>
                      <span className="text-xs text-slate-400 font-semibold mb-0.5">/ 10.0</span>
                    </div>
                    {/* Progress bar */}
                    <div className="w-full bg-slate-200 h-2 rounded-full overflow-hidden">
                      <div 
                        className={`h-full rounded-full transition-all duration-300 ${
                          simState.stability < 3.0 ? 'bg-red-500' :
                          simState.stability < 7.0 ? 'bg-amber-500' : 'bg-emerald-500'
                        }`} 
                        style={{ width: `${simState.stability * 10}%` }}
                      />
                    </div>
                    {simState.stability < 3.0 && (
                      <div className="flex items-center space-x-1 text-red-600 text-[10px] font-bold pt-1">
                        <ShieldAlert className="h-3.5 w-3.5 shrink-0" />
                        <span>Crisis Overrule Actief!</span>
                      </div>
                    )}
                  </div>

                  {/* Sentiment EMA */}
                  <div className="bg-slate-50 border border-slate-100 p-4 rounded-2xl space-y-2">
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider flex items-center">
                      <span>Sentiment EMA (EMA_M)</span>
                      <InfoTooltip content="Het exponentieel voortschrijdend gemiddelde van de sentiment-scores uit de meest recente sessiverslagen. Geeft de emotionele trend aan (bereik -1.0 tot 1.0)." />
                    </p>
                    <div className="flex justify-between items-end">
                      <span className={`text-3xl font-extrabold ${
                        simState.sentimentEma < 0.0 ? 'text-red-650' : 'text-emerald-600'
                      }`}>
                        {simState.sentimentEma > 0 ? '+' : ''}{simState.sentimentEma.toFixed(2)}
                      </span>
                      <span className="text-xs text-slate-400 font-semibold mb-0.5">[-1, +1]</span>
                    </div>
                    <div className="text-[10px] text-slate-400">Glijdend gemiddelde van sessieverslagen.</div>
                  </div>

                  {/* Sessions completed & Gap */}
                  <div className="bg-slate-50 border border-slate-100 p-4 rounded-2xl space-y-2">
                    <p className="text-[10px] font-bold text-slate-400 uppercase tracking-wider flex items-center">
                      <span>Voortgang & Interval</span>
                      <InfoTooltip content="Het totaal aantal voltooide sessies en het huidige aantal dagen (gap) sinds de laatste afspraak. Langere intervallen verhogen de kans op no-shows." />
                    </p>
                    <div className="space-y-1">
                      <p className="text-xs font-semibold text-slate-600">
                        Sessies: <span className="text-brand-600 text-sm font-extrabold">{simState.sessionsCompleted}</span> / 24
                      </p>
                      <p className="text-xs font-semibold text-slate-600">
                        Gap: <span className="text-slate-800 text-sm font-extrabold">{simState.gap}</span> dagen
                      </p>
                    </div>
                  </div>
                </div>

                {/* Actions Panel */}
                <div className="bg-slate-50 border border-slate-100 p-6 rounded-3xl space-y-4">
                  <div className="flex flex-col md:flex-row justify-between items-start md:items-center border-b border-slate-200/50 pb-3 gap-2">
                    <h3 className="text-sm font-bold text-slate-800">Selecteer Beheerder Actie:</h3>
                    <div className="flex flex-wrap gap-2">
                      {simState.heuristicAction && (
                        <div className="text-xs flex items-center space-x-1.5 bg-white py-1 px-2.5 rounded-xl border border-slate-200 shadow-sm">
                          <span className="text-slate-400 font-semibold">Expert-regel:</span>
                          <span className={`font-bold px-2 py-0.5 rounded-full ${
                            simState.heuristicAction.includes("Discharge") || simState.heuristicAction.includes("Ontslag") ? 'bg-blue-100 text-blue-800' :
                            simState.heuristicAction.includes("Intensief") ? 'bg-purple-100 text-purple-800' :
                            simState.heuristicAction.includes("Check-in") ? 'bg-cyan-100 text-cyan-800' :
                            'bg-slate-100 text-slate-800'
                          }`}>
                            {simState.heuristicAction}
                          </span>
                        </div>
                      )}
                      {simState.recommendedAction && (
                        <div className="text-xs flex items-center space-x-1.5 bg-white py-1 px-2.5 rounded-xl border border-slate-200 shadow-sm">
                          <span className="text-slate-400 font-semibold">Q-Learner (AI):</span>
                          <span className={`font-bold px-2 py-0.5 rounded-full ${
                            simState.recommendedAction.includes("Discharge") || simState.recommendedAction.includes("Ontslag") ? 'bg-emerald-100 text-emerald-800' :
                            simState.recommendedAction.includes("Intensief") ? 'bg-purple-100 text-purple-800' :
                            simState.recommendedAction.includes("Check-in") ? 'bg-cyan-100 text-cyan-800' :
                            simState.recommendedAction.includes("Niet getraind") ? 'bg-amber-100 text-amber-800 font-semibold' :
                            'bg-slate-100 text-slate-800'
                          }`}>
                            {simState.recommendedAction}
                          </span>
                        </div>
                      )}
                    </div>
                  </div>
                  {simDone ? (
                    <div className="bg-blue-50 border border-blue-100 text-blue-800 p-4 rounded-2xl text-center space-y-3 shadow-sm">
                      <p className="font-bold text-base">De behandelcyclus is beëindigd!</p>
                      <p className="text-xs text-blue-700">
                        {simState.stability >= 7.0 
                          ? "Succesvol Ontslag: De cliënt is stabiel en de therapie is succesvol afgerond." 
                          : simState.stability <= 1.0 
                            ? "Uitval (Dropout): Cliënt is uit het vizier geraakt door stagnerend contact."
                            : "Episode beëindigd: Maximale behandelduur (24 sessies) bereikt."}
                      </p>
                      <button 
                        onClick={handleSimReset}
                        className="bg-brand-600 hover:bg-brand-700 text-white font-semibold py-2 px-6 rounded-xl transition text-xs shadow-sm"
                      >
                        Start Nieuwe Cliënt
                      </button>
                    </div>
                  ) : (
                    <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                      <button
                        onClick={() => handleSimStep(0)}
                        disabled={simLoading}
                        className="bg-white border border-slate-200 hover:border-brand-500 text-slate-700 p-3 rounded-2xl flex flex-col items-center space-y-1 transition text-[10px] disabled:opacity-50 shadow-sm"
                      >
                        <span className="font-bold text-xs text-slate-850">Standaard (7d)</span>
                        <span className="text-slate-400">Kosten: 0</span>
                      </button>
                      <button
                        onClick={() => handleSimStep(1)}
                        disabled={simLoading}
                        className="bg-white border border-slate-200 hover:border-brand-500 text-slate-700 p-3 rounded-2xl flex flex-col items-center space-y-1 transition text-[10px] disabled:opacity-50 shadow-sm"
                      >
                        <span className="font-bold text-xs text-brand-600">Intensief (3d)</span>
                        <span className="text-slate-400">Kosten: 3</span>
                      </button>
                      <button
                        onClick={() => handleSimStep(2)}
                        disabled={simLoading}
                        className="bg-white border border-slate-200 hover:border-brand-500 text-slate-700 p-3 rounded-2xl flex flex-col items-center space-y-1 transition text-[10px] disabled:opacity-50 shadow-sm"
                      >
                        <span className="font-bold text-xs text-emerald-600">Check-in (7d)</span>
                        <span className="text-slate-400">Kosten: 1 | -15% risk</span>
                      </button>
                      <button
                        onClick={() => handleSimStep(3)}
                        disabled={simLoading || simState.stability < 3.0}
                        className="bg-white border border-slate-200 hover:border-red-500 text-slate-700 p-3 rounded-2xl flex flex-col items-center space-y-1 transition text-[10px] disabled:opacity-50 shadow-sm"
                      >
                        <span className="font-bold text-xs text-red-650">Discharge</span>
                        <span className="text-slate-400">E &gt;= 7.0</span>
                      </button>
                    </div>
                  )}
                </div>
              </div>

              {/* Simulator Metrics */}
              <div className="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-6">
                <h3 className="text-base font-bold text-slate-850">Simulatie Resultaten</h3>
                
                <div className="bg-slate-50 p-6 rounded-3xl text-center space-y-1">
                  <span className="text-[10px] uppercase tracking-wider text-slate-400 font-semibold flex items-center justify-center">
                    <span>Cumulatieve Beloning</span>
                    <InfoTooltip content="De totale wiskundige beloning (reward) behaald door de planningsagent. Combineert de voortgangsbeloning (progress), aanwezigheidsbonus, en penalties voor no-shows, stagnatie, overtreatment en delays." />
                  </span>
                  <p className={`text-3xl font-extrabold ${
                    simCumulativeReward >= 0 ? 'text-emerald-600' : 'text-red-600'
                  }`}>
                    {simCumulativeReward >= 0 ? '+' : ''}{simCumulativeReward.toFixed(2)}
                  </p>
                  <p className="text-[10px] text-slate-400 leading-normal pt-1.5 border-t border-slate-200/50 mt-2">
                    Beloont stabiliteit en sentimentgroei. Straft stagnatie, no-shows en overbehandeling (&gt;12 sessies).
                  </p>
                </div>

                {/* History Logs */}
                <div className="space-y-3">
                  <h4 className="font-bold text-xs text-slate-600 uppercase tracking-wider">Logboek</h4>
                  <div className="space-y-2.5 max-h-[220px] overflow-y-auto pr-1 text-xs">
                    {simHistory.length > 0 ? (
                      simHistory.map((item, idx) => (
                        <div key={idx} className="p-3 bg-slate-50 border border-slate-100 rounded-xl space-y-1">
                          <div className="flex justify-between font-bold text-slate-700">
                            <span>{item.action}</span>
                            <span className={item.reward >= 0 ? 'text-emerald-600' : 'text-red-600'}>
                              {item.reward >= 0 ? '+' : ''}{item.reward}
                            </span>
                          </div>
                          <div className="flex justify-between text-slate-500">
                            <span>Aanwezig: {item.attended ? '✅ Ja' : '❌ No-Show'}</span>
                            <span>E: {item.stability.toFixed(2)} (Sentiment: {item.sentimentEma.toFixed(2)})</span>
                          </div>
                          <div className="flex justify-between text-[10px] text-slate-400 pt-0.5">
                            <span>No-Show Kans: {(item.noShowProbability * 100).toFixed(1)}%</span>
                            {item.safetyTriggered && <span className="text-red-500 font-bold">Safety Override!</span>}
                            {item.dischargeStatus !== "none" && <span className="text-blue-500 font-bold">Status: {item.dischargeStatus}</span>}
                          </div>
                        </div>
                      ))
                    ) : (
                      <div className="text-center py-8 text-slate-400 text-xs">Geen eerdere stappen. Selecteer een actie links!</div>
                    )}
                  </div>
                </div>
              </div>
            </div>
          )}

          {/* Sub-tab 4: Heatmap */}
          {activeAiSubTab === 'heatmap' && (
            <div className="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-6">
              <div>
                <h2 className="text-lg font-bold text-slate-800 flex items-center space-x-2">
                  <LineChart className="h-5 w-5 text-brand-600" />
                  <span>Causale Analyse & Correlatie Matrix</span>
                  <InfoTooltip content="Deze correlatiematrix toont de statistische samenhang tussen de variabelen. Een hoge correlatie (>0.2) met No-Show bevestigt de causale effecten van behandeltype en tussenpozen." />
                </h2>
                <p className="text-slate-500 text-xs mt-1">Kwantitatief bewijs van de data-integriteit van de Data-Strategist.</p>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-8 items-center">
                <div className="flex justify-center border border-slate-100 p-4 rounded-3xl bg-slate-50/50">
                  <img 
                    src={aiApi.getHeatmapUrl()} 
                    alt="Correlation Heatmap Matrix"
                    className="max-h-[380px] rounded-2xl shadow-sm object-contain"
                    onError={(e) => {
                      (e.target as HTMLImageElement).src = "https://placehold.co/500x380/f8fafc/64748b?text=Correlatie+Heatmap+Laden...";
                    }}
                  />
                </div>
                
                <div className="space-y-3.5 text-xs text-slate-650 leading-relaxed">
                  <h3 className="font-bold text-slate-800 text-sm">Uitleg van de data-causaliteit:</h3>
                  <ul className="space-y-3 list-disc pl-5">
                    <li>
                      <strong>Last Session Gap effect (+30%):</strong> Cliënten die langer dan 21 dagen geleden een sessie hebben gehad, tonen statistisch een no-show percentage van ca. <strong>44%</strong> (vs 14% bij kortere gaps). Dit komt overeen met een correlatiecoëfficiënt van ca. <strong>0.30</strong>.
                    </li>
                    <li>
                      <strong>Diagnose 'depressie' effect (+20%):</strong> Cliënten met de specifieke therapie-focus depressie vertonen een no-show percentage van ca. <strong>46%</strong> (vs 26% bij andere behandelingen), wat leidt tot een correlatiecoëfficiënt van ca. <strong>0.20</strong>.
                    </li>
                    <li>
                      <strong>Ruis-effect (±5%):</strong> De toevoeging van willekeurige uniforme ruis in de dataset maskeert individuele records, wat zorgt voor realistische imperfecties zonder de causale trends aan te tasten.
                    </li>
                    <li>
                      <strong>Demografische onafhankelijkheid:</strong> Leeftijd en voltooide sessies zijn in het synthese-script niet causaal gekoppeld aan no-shows, wat in de correlatie-matrix correct tot uiting komt met waarden rond de <strong>0.00</strong>.
                    </li>
                  </ul>
                </div>
              </div>
            </div>
          )}
        </div>
      ) : (
        <GoogleSetupGuide />
      )}

      {/* Book Appointment Modal */}
      {isBookModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 dark:bg-black/70 backdrop-blur-sm p-4 animate-in fade-in duration-200">
          <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-lg p-6 relative border border-slate-100 dark:border-brand-800/40 transition-colors">
            <h3 className="text-xl font-bold text-slate-800 dark:text-white mb-4">Afspraak Inplannen</h3>
            <form onSubmit={handleCreateBooking} className="space-y-4">
              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Type afspraak</label>
                <select
                  required
                  value={newBooking.typeId}
                  onChange={(e) => setNewBooking({ ...newBooking, typeId: e.target.value })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                >
                  {bookingTypes.map((t) => (
                    <option key={t.id} value={t.id}>
                      {t.naam} ({t.standaardDuurMinuten} min)
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Patiënt</label>
                <select
                  value={newBooking.patientId}
                  onChange={(e) => setNewBooking({ ...newBooking, patientId: e.target.value })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
                >
                  <option value="">-- Geen patiënt (Blokkering) --</option>
                  {bookingPatients.map((p) => (
                    <option key={p.id} value={p.id}>{p.naam}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Starttijd</label>
                <input
                  type="datetime-local"
                  required
                  value={newBooking.starttijd}
                  onChange={(e) => setNewBooking({ ...newBooking, starttijd: e.target.value })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none"
                />
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Opmerkingen</label>
                <textarea
                  value={newBooking.opmerkingen}
                  onChange={(e) => setNewBooking({ ...newBooking, opmerkingen: e.target.value })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400 focus:outline-none h-20 resize-none"
                  placeholder="Eventuele opmerkingen..."
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Herhaling</label>
                  <select
                    value={newBooking.herhaling}
                    onChange={(e) => setNewBooking({ ...newBooking, herhaling: Number(e.target.value) })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none"
                  >
                    <option value={0}>Geen</option>
                    <option value={1}>Dagelijks</option>
                    <option value={2}>Wekelijks</option>
                  </select>
                </div>
                {Number(newBooking.herhaling) !== 0 && (
                  <div>
                    <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Herhalen t.e.m.</label>
                    <input
                      type="date"
                      required
                      value={newBooking.herhaalTot}
                      onChange={(e) => setNewBooking({ ...newBooking, herhaalTot: e.target.value })}
                      className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none"
                    />
                  </div>
                )}
              </div>

              <div className="flex justify-end space-x-3 pt-4 border-t border-slate-100 dark:border-brand-800/40">
                <button
                  type="button"
                  onClick={() => setIsBookModalOpen(false)}
                  className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-5 rounded-xl font-semibold transition"
                >
                  Annuleren
                </button>
                <button
                  type="submit"
                  className="bg-brand-600 hover:bg-brand-700 text-white py-2.5 px-5 rounded-xl font-semibold transition shadow-sm"
                >
                  Boeken
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Herbruikbare AfspraakDetailModal */}
      <AfspraakDetailModal
        afspraak={selectedAfspraak}
        onClose={() => setSelectedAfspraak(null)}
        onSuccess={() => loadDashboard(selectedDate)}
      />
    </div>
  );
};
