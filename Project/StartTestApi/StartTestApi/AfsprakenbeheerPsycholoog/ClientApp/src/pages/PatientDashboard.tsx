import React, { useState, useEffect } from 'react';
import { patientPortaalApi } from '../services/api';
import { Afspraak } from '../types';
import { 
  Clock, AlertCircle, XCircle, Loader2, Calendar
} from 'lucide-react';
import { BookingWizard } from '../components/BookingWizard';
import { extractErrorMessage } from '../utils/errorUtils';

export const PatientDashboard: React.FC = () => {
  const [appointments, setAppointments] = useState<Afspraak[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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

  const handleCancel = async (id: number) => {
    if (!window.confirm('Weet u zeker dat u deze afspraak wilt annuleren?')) return;
    try {
      setLoading(true);
      await patientPortaalApi.cancel(id);
      await loadData();
      alert('Afspraak succesvol geannuleerd.');
    } catch (err: any) {
      console.error(err);
      alert(extractErrorMessage(err, 'Annuleren mislukt.'));
    } finally {
      setLoading(false);
    }
  };

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
        <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm p-6 space-y-6 transition-colors">
          <h2 className="text-lg font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <Clock className="h-5 w-5 text-brand-600 dark:text-brand-400" />
            <span>Mijn Geplande Afspraken</span>
          </h2>

          <div className="space-y-4 max-h-[550px] overflow-y-auto pr-1">
            {appointments.length > 0 ? (
              appointments.map((appt) => {
                const start = new Date(appt.starttijd);
                const isUpcoming = start > new Date() && appt.status === 'Gepland';
                return (
                  <div 
                    key={appt.id} 
                    className="p-4 bg-slate-50 dark:bg-brand-950/60 border border-slate-100 dark:border-brand-800/40 rounded-2xl flex flex-col xl:flex-row xl:items-center justify-between gap-3 hover:bg-white dark:hover:bg-brand-950/90 hover:shadow-sm transition"
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
                          onClick={() => handleCancel(appt.id)}
                          className="flex items-center justify-center space-x-1.5 text-xs font-medium bg-red-50 dark:bg-red-950/60 hover:bg-red-100 text-red-700 dark:text-red-300 py-1.5 px-3 rounded-xl transition whitespace-nowrap"
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
                U heeft momenteel geen geplande afspraken.
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
    </div>
  );
};
