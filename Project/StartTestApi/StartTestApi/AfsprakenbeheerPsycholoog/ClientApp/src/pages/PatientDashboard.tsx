import React, { useState, useEffect } from 'react';
import { patientPortaalApi } from '../services/api';
import { Afspraak } from '../types';
import { 
  Clock, AlertCircle, XCircle, Loader2, Calendar, Video, AlertTriangle
} from 'lucide-react';
import { BookingWizard } from '../components/BookingWizard';
import { extractErrorMessage } from '../utils/errorUtils';

export const PatientDashboard: React.FC = () => {
  const [appointments, setAppointments] = useState<Afspraak[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'actueel' | 'historiek'>('actueel');

  const [cancelModal, setCancelModal] = useState<{
    isOpen: boolean;
    afspraak: Afspraak | null;
    isLate: boolean;
    hoursLeft: number;
  }>({ isOpen: false, afspraak: null, isLate: false, hoursLeft: 0 });

  const loadData = async () => {
    try {
      setLoading(true);
      const appts = await patientPortaalApi.getMijnAfspraken();
      setAppointments(appts);
      setError(null);
    } catch (err: any) {
      console.error(err);
      setError(extractErrorMessage(err, 'Laden van patiëntenportaal mislukt. Zorg ervoor dat uw account is gekoppeld door de psycholoog.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const openCancelModal = (appt: Afspraak) => {
    const start = new Date(appt.starttijd);
    const diffHours = (start.getTime() - new Date().getTime()) / (1000 * 60 * 60);
    const isLate = diffHours < 48;
    setCancelModal({
      isOpen: true,
      afspraak: appt,
      isLate,
      hoursLeft: Math.max(0, Math.round(diffHours))
    });
  };

  const confirmCancel = async () => {
    if (!cancelModal.afspraak) return;
    const id = cancelModal.afspraak.id;
    const isLate = cancelModal.isLate;
    setCancelModal({ isOpen: false, afspraak: null, isLate: false, hoursLeft: 0 });
    try {
      setLoading(true);
      await patientPortaalApi.cancel(id);
      await loadData();
      if (isLate) {
        alert('Afspraak geannuleerd. De praktijk is per e-mail en melding op de hoogte gebracht van deze laattijdige annulering.');
      } else {
        alert('Afspraak succesvol en kosteloos geannuleerd.');
      }
    } catch (err: any) {
      console.error(err);
      alert(extractErrorMessage(err, 'Annuleren mislukt.'));
    } finally {
      setLoading(false);
    }
  };

  const [earlyMeetModal, setEarlyMeetModal] = useState<{
    isOpen: boolean;
    afspraak: Afspraak | null;
    activeMeetLink: string;
  }>({ isOpen: false, afspraak: null, activeMeetLink: '' });

  const handleMeetClick = (appt: Afspraak, activeLink: string) => {
    const start = new Date(appt.starttijd);
    const now = new Date();
    const diffMinutes = (start.getTime() - now.getTime()) / 60000;

    if (diffMinutes > 15) {
      setEarlyMeetModal({ isOpen: true, afspraak: appt, activeMeetLink: activeLink });
    } else {
      window.open(activeLink, '_blank', 'noopener,noreferrer');
    }
  };

  // Filter afspraken in Actueel (toekomstige geplande afspraken) vs Historiek (afgelopen of geannuleerd)
  const nu = new Date();
  const actueleAfspraken = appointments.filter(a => a.status === 'Gepland' && new Date(a.starttijd) >= nu);
  const historiekAfspraken = appointments.filter(a => a.status !== 'Gepland' || new Date(a.starttijd) < nu);

  const getoondLijst = activeTab === 'actueel' ? actueleAfspraken : historiekAfspraken;

  if (loading && appointments.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-[80vh]">
        <Loader2 className="animate-spin h-10 w-10 text-brand-600" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="max-w-4xl mx-auto mt-8 p-6 bg-amber-50 text-amber-800 rounded-3xl border border-amber-200 shadow-sm space-y-4">
        <div className="flex items-center space-x-2">
          <AlertCircle className="h-6 w-6 text-amber-600" />
          <h3 className="text-lg font-bold">Account nog niet gekoppeld</h3>
        </div>
        <p className="text-sm">
          Uw account is succesvol geregistreerd, maar de psycholoog moet uw account nog handmatig koppelen aan een patiëntendossier of uw aanmelding goedkeuren.
        </p>
        <p className="text-sm text-slate-500">
          Neem contact op met de praktijk of wacht tot de psycholoog uw registratie heeft verwerkt in het beheerpaneel.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-8 p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm transition-colors">
        <h1 className="text-2xl font-bold text-slate-800 dark:text-brand-50">Mijn Patiëntenportaal</h1>
        <p className="text-slate-500 dark:text-brand-300 mt-1">Beheer uw afspraken en plan online nieuwe consulten.</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Left Column: Appointments List */}
        <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-6 space-y-5 transition-colors">
          <div className="flex items-center justify-between flex-wrap gap-2">
            <h2 className="text-lg font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
              <Clock className="h-5 w-5 text-brand-600 dark:text-brand-400" />
              <span>Mijn Afspraken</span>
            </h2>
          </div>

          {/* Actueel vs Historiek Tabs */}
          <div className="flex bg-slate-100 dark:bg-brand-950/80 p-1 rounded-2xl border border-slate-200/60 dark:border-brand-800/40">
            <button
              type="button"
              onClick={() => setActiveTab('actueel')}
              className={`flex-1 py-2 px-3 text-xs font-semibold rounded-xl transition-all flex items-center justify-center space-x-2 ${
                activeTab === 'actueel'
                  ? 'bg-white dark:bg-brand-800 text-brand-700 dark:text-brand-100 shadow-sm'
                  : 'text-slate-500 dark:text-slate-300 hover:text-slate-800 dark:hover:text-white'
              }`}
            >
              <span>Actueel</span>
              <span className={`px-2 py-0.5 text-[10px] font-bold rounded-full ${
                activeTab === 'actueel' 
                  ? 'bg-brand-100 dark:bg-brand-950 text-brand-700 dark:text-brand-200' 
                  : 'bg-slate-200 dark:bg-brand-900 text-slate-600 dark:text-slate-300'
              }`}>
                {actueleAfspraken.length}
              </span>
            </button>
            
            <button
              type="button"
              onClick={() => setActiveTab('historiek')}
              className={`flex-1 py-2 px-3 text-xs font-semibold rounded-xl transition-all flex items-center justify-center space-x-2 ${
                activeTab === 'historiek'
                  ? 'bg-white dark:bg-brand-800 text-brand-700 dark:text-brand-100 shadow-sm'
                  : 'text-slate-500 dark:text-slate-300 hover:text-slate-800 dark:hover:text-white'
              }`}
            >
              <span>Historiek</span>
              <span className={`px-2 py-0.5 text-[10px] font-bold rounded-full ${
                activeTab === 'historiek' 
                  ? 'bg-brand-100 dark:bg-brand-950 text-brand-700 dark:text-brand-200' 
                  : 'bg-slate-200 dark:bg-brand-900 text-slate-600 dark:text-slate-300'
              }`}>
                {historiekAfspraken.length}
              </span>
            </button>
          </div>

          <div className="space-y-4 max-h-[550px] overflow-y-auto pr-1">
            {getoondLijst.length > 0 ? (
              getoondLijst.map((appt) => {
                const start = new Date(appt.starttijd);
                const isUpcoming = start > new Date() && appt.status === 'Gepland';
                const isMeet = !!appt.googleMeetLink || (appt.opmerkingen && (appt.opmerkingen.includes('GoogleMeet') || appt.opmerkingen.includes('Google Meet')));
                
                return (
                  <div 
                    key={appt.id} 
                    className={`p-4 rounded-2xl flex flex-col xl:flex-row xl:items-center justify-between gap-3 transition ${
                      isMeet 
                        ? 'bg-purple-50/50 dark:bg-purple-950/30 border-2 border-purple-200 dark:border-purple-800/60 hover:shadow-md' 
                        : 'bg-slate-50 dark:bg-brand-950/60 border border-slate-100 dark:border-brand-800/40 hover:bg-white dark:hover:bg-brand-950/90 hover:shadow-sm'
                    }`}
                  >
                    <div className="space-y-1 min-w-0">
                      <div className="flex items-center space-x-2 flex-wrap gap-y-1">
                        <span className="font-semibold text-slate-800 dark:text-brand-100 truncate">
                          {appt.afspraakTypeNaam}
                        </span>
                        <span className={`text-[10px] font-bold uppercase tracking-wider py-0.5 px-2 rounded-full ${
                          appt.status === 'Gepland' ? 'bg-blue-100 dark:bg-blue-950/60 text-blue-800 dark:text-blue-300' :
                          appt.status === 'Voltooid' ? 'bg-emerald-100 dark:bg-emerald-950/60 text-emerald-800 dark:text-emerald-300' :
                          'bg-red-100 dark:bg-red-950/60 text-red-800 dark:text-red-300'
                        }`}>
                          {appt.status}
                        </span>
                        {isMeet && (
                          <span className="text-[10px] font-bold uppercase tracking-wider py-0.5 px-2 rounded-full bg-purple-100 dark:bg-purple-900/80 text-purple-800 dark:text-purple-200 border border-purple-300/60 flex items-center gap-1">
                            <Video className="h-3 w-3 text-purple-600 dark:text-purple-300" />
                            <span>Online Meet</span>
                          </span>
                        )}
                      </div>
                      <p className="text-sm text-slate-600 dark:text-brand-200">
                        {start.toLocaleDateString('nl-NL', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}
                      </p>
                      <p className="text-xs text-slate-500 dark:text-brand-300">
                        {start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })} - {' '}
                        {new Date(appt.eindtijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </p>
                      {appt.opmerkingen && (
                        <p className="text-[10px] text-slate-400 dark:text-brand-400 italic mt-1 line-clamp-2">{appt.opmerkingen}</p>
                      )}
                    </div>

                    <div className="flex flex-wrap sm:flex-nowrap xl:flex-col gap-2 shrink-0 pt-2 xl:pt-0 border-t xl:border-t-0 border-slate-200/50 dark:border-brand-800/40">
                      {isMeet && appt.status === 'Gepland' && (() => {
                        const activeLink = (appt.googleMeetLink && appt.googleMeetLink.startsWith('https://meet.google.com/') && !appt.googleMeetLink.includes('meet-dv') && !appt.googleMeetLink.includes('lookup') && !appt.googleMeetLink.includes('vst-hndg'))
                          ? appt.googleMeetLink 
                          : 'https://meet.google.com/new';

                        return (
                          <button
                            onClick={() => handleMeetClick(appt, activeLink)}
                            title="Open Google Meet videogesprek"
                            className="flex items-center justify-center space-x-1.5 text-xs font-bold bg-emerald-600 hover:bg-emerald-700 text-white py-1.5 px-3 rounded-xl shadow-xs transition whitespace-nowrap cursor-pointer"
                          >
                            <Video className="h-3.5 w-3.5 shrink-0" />
                            <span>Deelnemen aan Meet</span>
                          </button>
                        );
                      })()}
                      {appt.status === 'Gepland' && (
                        <a
                          href={`/api/patientportaal/afspraak/${appt.id}/ics`}
                          download
                          title="Download .ics bestand voor uw agenda"
                          className="flex items-center justify-center space-x-1.5 text-xs font-medium bg-brand-50 dark:bg-brand-800/60 hover:bg-brand-100 dark:hover:bg-brand-800 text-brand-700 dark:text-brand-200 py-1.5 px-3 rounded-xl transition whitespace-nowrap"
                        >
                          <Calendar className="h-3.5 w-3.5 shrink-0 text-brand-600 dark:text-brand-400" />
                          <span>ICS Agenda</span>
                        </a>
                      )}
                      {isUpcoming && (
                        <button
                          onClick={() => openCancelModal(appt)}
                          className="flex items-center justify-center space-x-1.5 text-xs font-medium bg-red-50 dark:bg-red-950/60 hover:bg-red-100 text-red-700 dark:text-red-300 py-1.5 px-3 rounded-xl transition whitespace-nowrap cursor-pointer"
                        >
                          <XCircle className="h-3.5 w-3.5 shrink-0 text-red-500" />
                          <span>Annuleren</span>
                        </button>
                      )}
                    </div>
                  </div>
                );
              })
            ) : (
              <div className="text-center py-12 text-slate-400 dark:text-brand-400">
                {activeTab === 'actueel' 
                  ? 'U heeft momenteel geen geplande afspraken.' 
                  : 'Geen eerdere afspraken of annuleringen in uw historiek.'}
              </div>
            )}
          </div>
        </div>

        {/* Right Column: SimplyBook.me Style Booking Wizard */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm transition-colors">
            <h2 className="text-lg font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
              <Clock className="h-5 w-5 text-brand-600 dark:text-brand-400" />
              <span>Nieuwe Afspraak Inplannen</span>
            </h2>
            <p className="text-xs text-slate-400 dark:text-brand-300 mt-1">Doorloop de stappen om eenvoudig een afspraak te maken.</p>
          </div>
          
          <BookingWizard onBookingSuccess={loadData} />
        </div>

      </div>

      {/* Early Access Info Modal */}
      {earlyMeetModal.isOpen && earlyMeetModal.afspraak && (() => {
        const start = new Date(earlyMeetModal.afspraak.starttijd);
        const now = new Date();
        const diffDays = Math.round((start.getTime() - now.getTime()) / 86400000);
        const formattedDate = start.toLocaleDateString('nl-BE', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
        const formattedTime = start.toLocaleTimeString('nl-BE', { hour: '2-digit', minute: '2-digit' });

        return (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-in fade-in duration-200">
            <div className="bg-white dark:bg-slate-900 border border-purple-200 dark:border-purple-800/60 rounded-3xl p-6 max-w-md w-full shadow-2xl space-y-4">
              <div className="flex items-center space-x-3 text-purple-900 dark:text-purple-200">
                <div className="p-3 bg-purple-100 dark:bg-purple-900/60 rounded-2xl">
                  <Video className="h-6 w-6 text-purple-600 dark:text-purple-400" />
                </div>
                <div>
                  <h3 className="font-bold text-base">Online Videoconsultatie</h3>
                  <p className="text-xs text-purple-700 dark:text-purple-300">Google Meet Herinnering</p>
                </div>
              </div>

              <div className="bg-purple-50/70 dark:bg-purple-950/40 p-4 rounded-2xl border border-purple-100 dark:border-purple-900/40 text-xs space-y-2">
                <p className="font-semibold text-purple-900 dark:text-purple-200">
                  📅 Datum & Tijdstip:
                </p>
                <p className="text-purple-800 dark:text-purple-300 text-sm font-bold">
                  {formattedDate} om {formattedTime}
                </p>
                <div className="inline-block mt-1 px-2.5 py-1 bg-purple-200/80 dark:bg-purple-800/60 text-purple-900 dark:text-purple-100 font-semibold rounded-lg text-[11px]">
                  ⏳ Afspraak staat gepland over {diffDays} {diffDays === 1 ? 'dag' : 'dagen'}
                </div>
              </div>

              <div className="p-3 bg-amber-50 dark:bg-amber-950/50 border border-amber-200 dark:border-amber-900/40 rounded-2xl text-xs text-amber-900 dark:text-amber-200 flex items-start space-x-2.5">
                <AlertCircle className="h-4 w-4 text-amber-600 dark:text-amber-400 shrink-0 mt-0.5" />
                <span>
                  De videocall knop wordt officieel actief <strong>15 minuten voor aanvang</strong> (vanaf {new Date(start.getTime() - 15 * 60000).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}).
                </span>
              </div>

              <div className="flex flex-col sm:flex-row gap-2 pt-2">
                <button
                  onClick={() => {
                    window.open(earlyMeetModal.activeMeetLink, '_blank', 'noopener,noreferrer');
                    setEarlyMeetModal({ isOpen: false, afspraak: null, activeMeetLink: '' });
                  }}
                  className="flex-1 py-2.5 px-4 bg-emerald-600 hover:bg-emerald-700 text-white font-bold text-xs rounded-xl shadow-xs transition flex items-center justify-center space-x-1.5 cursor-pointer"
                >
                  <Video className="h-4 w-4" />
                  <span>Toch naar Google Meet</span>
                </button>
                <button
                  onClick={() => setEarlyMeetModal({ isOpen: false, afspraak: null, activeMeetLink: '' })}
                  className="py-2.5 px-4 bg-slate-100 dark:bg-slate-800 hover:bg-slate-200 text-slate-700 dark:text-slate-300 font-medium text-xs rounded-xl transition cursor-pointer"
                >
                  Begrepen
                </button>
              </div>
            </div>
          </div>
        );
      })()}

      {/* Cancellation Warning Modal */}
      {cancelModal.isOpen && cancelModal.afspraak && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs animate-fade-in">
          <div className="bg-white dark:bg-brand-900 w-full max-w-lg rounded-3xl border border-slate-100 dark:border-brand-800 shadow-2xl overflow-hidden p-6 space-y-5 transition-colors">
            
            <div className="flex items-start space-x-3">
              <div className={`h-10 w-10 rounded-2xl flex items-center justify-center shrink-0 ${
                cancelModal.isLate 
                  ? 'bg-red-100 dark:bg-red-950/80 text-red-600 dark:text-red-400' 
                  : 'bg-amber-100 dark:bg-amber-950/80 text-amber-600 dark:text-amber-400'
              }`}>
                {cancelModal.isLate ? <AlertTriangle className="h-6 w-6" /> : <AlertCircle className="h-6 w-6" />}
              </div>
              <div className="space-y-1 flex-1">
                <h3 className="text-lg font-bold text-slate-800 dark:text-white">
                  {cancelModal.isLate ? '⚠️ Laattijdige annulering (< 48 uur)' : 'Afspraak annuleren'}
                </h3>
                <p className="text-xs font-semibold text-slate-500 dark:text-brand-300">
                  {cancelModal.afspraak.afspraakTypeNaam} op{' '}
                  {new Date(cancelModal.afspraak.starttijd).toLocaleDateString('nl-NL', { weekday: 'long', day: 'numeric', month: 'long' })} om{' '}
                  {new Date(cancelModal.afspraak.starttijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </p>
              </div>
            </div>

            {cancelModal.isLate ? (
              <div className="space-y-3 bg-red-50 dark:bg-red-950/40 p-4 rounded-2xl border border-red-100 dark:border-red-900/60 text-xs text-red-900 dark:text-red-200">
                <p className="font-bold text-red-800 dark:text-red-300">
                  Let op: Je annuleert minder dan 48 uur op voorhand (nog {cancelModal.hoursLeft} uur tot de afspraak).
                </p>
                <ul className="list-disc list-inside space-y-1.5 font-medium">
                  <li>
                    <strong>Kosteloze annulatie is verstreken:</strong> Volgens het praktijkreglement wordt deze sessie aangerekend, tenzij er een geldig ziekteattest kan worden voorgelegd.
                  </li>
                  <li>
                    <strong>Melding naar de praktijk:</strong> De praktijk ontvangt direct een automatische e-mail en melding met het tijdstip van je laattijdige annulering.
                  </li>
                </ul>
                <p className="text-[11px] italic font-semibold text-red-700 dark:text-red-300 pt-1">
                  Je kunt wel doorgaan met annuleren om deze plek vrij te maken, maar accepteert daarmee de voorwaarden.
                </p>
              </div>
            ) : (
              <div className="bg-slate-50 dark:bg-brand-950 p-4 rounded-2xl border border-slate-100 dark:border-brand-800 text-xs text-slate-600 dark:text-brand-200">
                <p>
                  Deze afspraak vindt plaats over meer dan 48 uur. Annuleren is in dit geval <strong>volledig gratis</strong>.
                </p>
              </div>
            )}

            <div className="flex items-center justify-end space-x-3 pt-2">
              <button
                type="button"
                onClick={() => setCancelModal({ isOpen: false, afspraak: null, isLate: false, hoursLeft: 0 })}
                className="px-4 py-2.5 bg-slate-100 hover:bg-slate-200 dark:bg-brand-800 dark:hover:bg-brand-700 text-slate-700 dark:text-brand-200 font-bold text-xs rounded-xl transition cursor-pointer"
              >
                Behouden / Annuleren afbreken
              </button>
              <button
                type="button"
                onClick={confirmCancel}
                className={`px-5 py-2.5 text-white font-bold text-xs rounded-xl shadow-sm transition cursor-pointer ${
                  cancelModal.isLate 
                    ? 'bg-red-600 hover:bg-red-700 shadow-red-600/20' 
                    : 'bg-brand-600 hover:bg-brand-700 shadow-brand-600/20'
                }`}
              >
                {cancelModal.isLate ? 'Ja, annuleer & accepteer voorwaarden' : 'Ja, afspraak kosteloos annuleren'}
              </button>
            </div>

          </div>
        </div>
      )}
    </div>
  );
};
