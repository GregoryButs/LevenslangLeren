import React, { useState, useEffect } from 'react';
import { afspraakApi } from '../services/api';
import { Afspraak } from '../types';
import { 
  Calendar as CalendarIcon, Clock, ChevronLeft, ChevronRight, 
  RefreshCw, CheckCircle, Loader2 
} from 'lucide-react';
import { AfspraakDetailModal } from '../components/AfspraakDetailModal';

export const CalendarPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Afspraak[]>([]);
  const [currentDate, setCurrentDate] = useState<Date>(new Date());
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<'week' | 'day'>('week');
  const [showFull24h, setShowFull24h] = useState(false);

  // Modal State
  const [selectedAfspraak, setSelectedAfspraak] = useState<Afspraak | null>(null);

  const startOfWeek = (date: Date) => {
    const diff = date.getDate() - date.getDay() + (date.getDay() === 0 ? -6 : 1);
    return new Date(date.setDate(diff));
  };

  const getWeekDays = (date: Date) => {
    const start = startOfWeek(new Date(date));
    const days = [];
    for (let i = 0; i < 7; i++) {
      const day = new Date(start);
      day.setDate(start.getDate() + i);
      days.push(day);
    }
    return days;
  };

  const fetchAppointments = async () => {
    try {
      setLoading(true);
      const data = await afspraakApi.getAll();
      setAppointments(data);
    } catch (err) {
      console.error("Fout bij ophalen afspraken:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAppointments();
  }, []);

  const handlePrev = () => {
    const newDate = new Date(currentDate);
    if (viewMode === 'week') {
      newDate.setDate(currentDate.getDate() - 7);
    } else {
      newDate.setDate(currentDate.getDate() - 1);
    }
    setCurrentDate(newDate);
  };

  const handleNext = () => {
    const newDate = new Date(currentDate);
    if (viewMode === 'week') {
      newDate.setDate(currentDate.getDate() + 7);
    } else {
      newDate.setDate(currentDate.getDate() + 1);
    }
    setCurrentDate(newDate);
  };

  const handleToday = () => {
    setCurrentDate(new Date());
  };

  // Dynamic time slots calculation
  const visibleAppointments = appointments.filter(a => !a.isHeleDag && a.status !== 'Geannuleerd');

  let minHour = 8;
  let maxHour = 18;

  if (showFull24h) {
    minHour = 0;
    maxHour = 23;
  } else if (visibleAppointments.length > 0) {
    visibleAppointments.forEach(app => {
      const startH = new Date(app.starttijd).getHours();
      const endDate = new Date(app.eindtijd);
      let endH = endDate.getHours();
      if (endDate.getMinutes() > 0) {
        endH = Math.min(23, endH);
      } else if (endH > startH) {
        endH = endH - 1;
      }
      if (startH < minHour) {
        minHour = Math.max(0, startH);
      }
      if (endH > maxHour) {
        maxHour = Math.min(23, endH);
      }
    });
  }

  const hours = Array.from({ length: maxHour - minHour + 1 }, (_, i) => i + minHour);

  const getAppointmentsForDayAndHour = (day: Date, hour: number) => {
    return appointments.filter(app => {
      if (app.isHeleDag) return false;
      const appStart = new Date(app.starttijd);
      return appStart.toDateString() === day.toDateString() && appStart.getHours() === hour;
    });
  };

  const getAllDayNotificationsForDay = (day: Date) => {
    return appointments.filter(app => {
      if (!app.isHeleDag) return false;
      const appStart = new Date(app.starttijd);
      return appStart.toDateString() === day.toDateString();
    });
  };

  const formatLocalDate = (date: Date) => {
    return date.toLocaleDateString('nl-NL', { weekday: 'short', day: 'numeric', month: 'short' });
  };

  const formatLocalTime = (utcString: string) => {
    return new Date(utcString).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <div className="space-y-6 p-6 max-w-7xl mx-auto">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm gap-4 transition-colors">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <CalendarIcon className="h-6 w-6 text-brand-600 dark:text-brand-400" />
            <span>Praktijk Agenda</span>
          </h1>
          <p className="text-slate-500 dark:text-brand-300 mt-1">Beheer uw afspraken en synchronisatie met Google Calendar. Klik op een afspraak om te bekijken of te bewerken.</p>
        </div>
        
        <div className="flex flex-wrap items-center gap-3">
          <button 
            onClick={handleToday}
            className="px-4 py-2 text-sm font-semibold bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-brand-100 rounded-xl transition"
          >
            Vandaag
          </button>
          
          <div className="flex items-center bg-slate-100 dark:bg-brand-950 p-1 rounded-xl">
            <button 
              onClick={handlePrev}
              className="p-1.5 text-slate-600 dark:text-brand-300 hover:text-slate-800 dark:hover:text-white rounded-lg hover:bg-white dark:hover:bg-brand-900 transition"
            >
              <ChevronLeft className="h-4 w-4" />
            </button>
            <span className="px-3 text-xs font-bold text-slate-600 dark:text-brand-200">
              {viewMode === 'week' 
                ? `Week van ${formatLocalDate(startOfWeek(new Date(currentDate)))}` 
                : formatLocalDate(currentDate)
              }
            </span>
            <button 
              onClick={handleNext}
              className="p-1.5 text-slate-600 dark:text-brand-300 hover:text-slate-800 dark:hover:text-white rounded-lg hover:bg-white dark:hover:bg-brand-900 transition"
            >
              <ChevronRight className="h-4 w-4" />
            </button>
          </div>

          <div className="flex items-center bg-slate-100 dark:bg-brand-950 p-1 rounded-xl">
            <button 
              onClick={() => setViewMode('week')}
              className={`px-3 py-1.5 text-xs font-bold rounded-lg transition ${
                viewMode === 'week' 
                  ? 'bg-white dark:bg-brand-800 text-brand-600 dark:text-white shadow-sm' 
                  : 'text-slate-500 dark:text-brand-400 hover:text-slate-700 dark:hover:text-brand-200'
              }`}
            >
              Week
            </button>
            <button 
              onClick={() => setViewMode('day')}
              className={`px-3 py-1.5 text-xs font-bold rounded-lg transition ${
                viewMode === 'day' 
                  ? 'bg-white dark:bg-brand-800 text-brand-600 dark:text-white shadow-sm' 
                  : 'text-slate-500 dark:text-brand-400 hover:text-slate-700 dark:hover:text-brand-200'
              }`}
            >
              Dag
            </button>
          </div>

          <button 
            onClick={() => setShowFull24h(!showFull24h)}
            className={`px-3 py-1.5 text-xs font-bold rounded-xl border transition flex items-center space-x-1.5 ${
              showFull24h 
                ? 'bg-brand-50 dark:bg-brand-800 text-brand-700 dark:text-white border-brand-200 dark:border-brand-700 shadow-sm' 
                : 'bg-slate-100 dark:bg-brand-950 hover:bg-slate-200 dark:hover:bg-brand-800 text-slate-600 dark:text-brand-300 border-transparent'
            }`}
            title={showFull24h ? "Schakel naar automatische uren (verberg lege randuren)" : "Toon alle 24 uur (00:00 - 24:00)"}
          >
            <Clock className="h-3.5 w-3.5" />
            <span>{showFull24h ? '00:00 - 24:00' : 'Auto uren'}</span>
          </button>

          <button 
            onClick={fetchAppointments}
            className="p-2 text-slate-500 dark:text-brand-300 hover:text-slate-700 dark:hover:text-white hover:bg-slate-50 dark:hover:bg-brand-800 rounded-xl border border-slate-200 dark:border-brand-800 transition"
            title="Agenda vernieuwen"
          >
            <RefreshCw className="h-4 w-4" />
          </button>
        </div>
      </div>

      {loading ? (
        <div className="flex items-center justify-center min-h-[50vh]">
          <Loader2 className="animate-spin h-10 w-10 text-brand-600 dark:text-brand-400" />
        </div>
      ) : (
        <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-xl overflow-hidden transition-colors">
          <div className="overflow-x-auto">
            <table className="w-full border-collapse table-fixed min-w-[800px]">
              {/* Header Days */}
              <thead>
                <tr className="border-b border-slate-100 dark:border-brand-800/40 bg-slate-50 dark:bg-brand-950/80">
                  <th className="w-20 p-4 text-left text-xs font-bold uppercase tracking-wider text-slate-400 dark:text-brand-400">Tijd</th>
                  {viewMode === 'week' ? (
                    getWeekDays(currentDate).map((day, idx) => {
                      const isToday = day.toDateString() === new Date().toDateString();
                      return (
                        <th key={idx} className="p-4 text-center border-l border-slate-100 dark:border-brand-800/40">
                          <div className={`text-xs font-bold ${isToday ? 'text-brand-600 dark:text-brand-300' : 'text-slate-500 dark:text-brand-400'}`}>
                            {day.toLocaleDateString('nl-NL', { weekday: 'short' })}
                          </div>
                          <div className={`text-lg font-extrabold mt-0.5 ${
                            isToday 
                              ? 'bg-brand-600 text-white h-7 w-7 rounded-full flex items-center justify-center mx-auto shadow-md shadow-brand-500/10' 
                              : 'text-slate-800 dark:text-brand-100'
                          }`}>
                            {day.getDate()}
                          </div>
                        </th>
                      );
                    })
                  ) : (
                    <th className="p-4 text-center border-l border-slate-100 dark:border-brand-800/40">
                      <div className="text-xs font-bold text-brand-600 dark:text-brand-300">
                        {currentDate.toLocaleDateString('nl-NL', { weekday: 'long' })}
                      </div>
                      <div className="text-lg font-extrabold mt-0.5 text-slate-800 dark:text-brand-100">
                        {currentDate.getDate()}
                      </div>
                    </th>
                  )}
                </tr>
              </thead>

              {/* Time Slots Grid */}
              <tbody>
                {/* Hele-dag Meldingen & Notities Rij */}
                <tr className="border-b border-amber-200/60 dark:border-amber-900/40 bg-amber-50/40 dark:bg-amber-950/20">
                  <td className="p-3 text-xs font-bold text-amber-700 dark:text-amber-400 align-middle">
                    <span className="flex items-center space-x-1" title="Hele-dag meldingen en herinneringen uit Google Agenda (blokkeren geen uren slots)">
                      <span>📌 Notities</span>
                    </span>
                  </td>
                  {viewMode === 'week' ? (
                    getWeekDays(currentDate).map((day, dayIdx) => {
                      const notes = getAllDayNotificationsForDay(day);
                      return (
                        <td key={dayIdx} className="p-2 border-l border-amber-100/60 dark:border-amber-900/30 align-top bg-amber-50/20 dark:bg-amber-950/10">
                          {notes.length > 0 ? (
                            <div className="space-y-1">
                              {notes.map(note => (
                                <div
                                  key={note.id}
                                  onClick={() => setSelectedAfspraak(note)}
                                  className="p-1.5 rounded-xl bg-amber-100/90 dark:bg-amber-950/80 hover:bg-amber-200/90 border border-amber-200/80 dark:border-amber-800/60 text-amber-900 dark:text-amber-200 text-xs font-semibold shadow-xs flex items-center justify-between cursor-pointer transition"
                                  title="Klik om te bekijken of te bewerken"
                                >
                                  <span className="truncate">{note.opmerkingen || note.patientNaam || 'Hele-dag melding'}</span>
                                  <span className="text-[9px] font-black uppercase tracking-wider px-1.5 py-0.5 bg-amber-200/90 dark:bg-amber-900 text-amber-800 dark:text-amber-200 rounded-md ml-1 flex-shrink-0">
                                    Melding
                                  </span>
                                </div>
                              ))}
                            </div>
                          ) : (
                            <span className="text-[11px] text-slate-300 dark:text-brand-600 italic block text-center py-1">-</span>
                          )}
                        </td>
                      );
                    })
                  ) : (
                    <td className="p-2 border-l border-amber-100/60 dark:border-amber-900/30 align-top bg-amber-50/20 dark:bg-amber-950/10">
                      {getAllDayNotificationsForDay(currentDate).length > 0 ? (
                        <div className="space-y-1.5">
                          {getAllDayNotificationsForDay(currentDate).map(note => (
                            <div
                              key={note.id}
                              onClick={() => setSelectedAfspraak(note)}
                              className="p-2 rounded-xl bg-amber-100/90 dark:bg-amber-950/80 hover:bg-amber-200/90 border border-amber-200/80 dark:border-amber-800/60 text-amber-900 dark:text-amber-200 text-sm font-semibold shadow-xs flex items-center justify-between cursor-pointer transition"
                              title="Klik om te bekijken of te bewerken"
                            >
                              <span>{note.opmerkingen || note.patientNaam || 'Hele-dag melding'}</span>
                              <span className="text-xs font-black uppercase tracking-wider px-2 py-0.5 bg-amber-200/90 dark:bg-amber-900 text-amber-800 dark:text-amber-200 rounded-md ml-2 flex-shrink-0">
                                Melding / Notitie
                              </span>
                            </div>
                          ))}
                        </div>
                      ) : (
                        <span className="text-[11px] text-slate-300 dark:text-brand-600 italic block text-center py-1">Geen hele-dag meldingen op deze dag</span>
                      )}
                    </td>
                  )}
                </tr>

                {hours.map((hour) => (
                  <tr key={hour} className="border-b border-slate-100 dark:border-brand-800/40 group">
                    <td className="p-4 text-sm font-semibold text-slate-400 dark:text-brand-400 align-top">
                      {hour.toString().padStart(2, '0')}:00
                    </td>

                    {viewMode === 'week' ? (
                      getWeekDays(currentDate).map((day, dayIdx) => {
                        const appts = getAppointmentsForDayAndHour(day, hour);
                        return (
                          <td key={dayIdx} className="p-2 border-l border-slate-100 dark:border-brand-800/40 align-top min-h-[80px] bg-slate-50/10 dark:bg-brand-950/20 group-hover:bg-white dark:group-hover:bg-brand-950/50 transition-colors">
                            <div className="space-y-1.5">
                              {appts.length > 0 ? (
                                appts.map((appt) => (
                                  <div 
                                    key={appt.id} 
                                    onClick={() => setSelectedAfspraak(appt)}
                                    style={{ borderLeftColor: appt.status === 'Geannuleerd' ? '#94a3b8' : appt.kleurcode }}
                                    className={`p-2 border-l-4 rounded-r-xl bg-white dark:bg-brand-950 shadow-sm border border-slate-100 dark:border-brand-800/60 hover:shadow-md transition text-left cursor-pointer hover:scale-[1.01] ${
                                      appt.status === 'Geannuleerd' ? 'opacity-60 bg-slate-50/80 dark:bg-brand-950/40' : ''
                                    }`}
                                  >
                                    <div className="flex justify-between items-start gap-1">
                                      <span className={`text-xs font-bold truncate block ${appt.status === 'Geannuleerd' ? 'line-through text-slate-400 dark:text-brand-400' : 'text-slate-800 dark:text-white'}`}>
                                        {appt.patientNaam}
                                      </span>
                                      {appt.googleEventId ? (
                                        <span className="flex h-2 w-2 rounded-full bg-blue-500 flex-shrink-0" title="Gesynchroniseerd met Google Calendar" />
                                      ) : null}
                                    </div>
                                    <span className="text-[10px] text-slate-500 dark:text-brand-300 font-semibold block truncate mt-0.5">
                                      {appt.afspraakTypeNaam}
                                    </span>
                                    <span className="text-[9px] text-slate-400 dark:text-brand-400 font-bold block mt-1 flex items-center">
                                      <Clock className="h-3 w-3 mr-0.5" />
                                      {formatLocalTime(appt.starttijd)} - {formatLocalTime(appt.eindtijd)}
                                    </span>
                                  </div>
                                ))
                              ) : null}
                            </div>
                          </td>
                        );
                      })
                    ) : (
                      <td className="p-2 border-l border-slate-100 dark:border-brand-800/40 align-top min-h-[80px]">
                        <div className="space-y-2">
                          {getAppointmentsForDayAndHour(currentDate, hour).map((appt) => (
                            <div 
                              key={appt.id} 
                              onClick={() => setSelectedAfspraak(appt)}
                              style={{ borderLeftColor: appt.status === 'Geannuleerd' ? '#94a3b8' : appt.kleurcode }}
                              className={`p-3 border-l-4 rounded-r-xl bg-white dark:bg-brand-950 shadow-sm border border-slate-100 dark:border-brand-800/60 hover:shadow-md transition flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3 text-left cursor-pointer hover:scale-[1.01] ${
                                appt.status === 'Geannuleerd' ? 'opacity-60 bg-slate-50/80 dark:bg-brand-950/40' : ''
                              }`}
                            >
                              <div>
                                <h4 className={`text-sm font-bold flex items-center space-x-1.5 ${appt.status === 'Geannuleerd' ? 'line-through text-slate-400 dark:text-brand-400' : 'text-slate-800 dark:text-white'}`}>
                                  <span>{appt.patientNaam}</span>
                                  <span className={`text-[9px] font-bold uppercase tracking-wider px-1.5 py-0.5 rounded-full ${
                                    appt.status === 'Geannuleerd' ? 'bg-red-100 dark:bg-red-950/60 text-red-700 dark:text-red-300 border border-red-100 dark:border-red-900/40' :
                                    appt.status === 'Voltooid' ? 'bg-emerald-100 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300 border border-emerald-100 dark:border-emerald-900/40' :
                                    'bg-blue-50 dark:bg-blue-950/60 text-blue-700 dark:text-blue-300 border border-blue-100 dark:border-blue-900/40'
                                  }`}>
                                    {appt.status}
                                  </span>
                                </h4>
                                <span className="text-xs text-slate-500 dark:text-brand-300 font-semibold mt-0.5 block">
                                  {appt.afspraakTypeNaam}
                                </span>
                                {appt.opmerkingen ? (
                                  <p className="text-xs text-slate-400 dark:text-brand-400 mt-1 italic">"{appt.opmerkingen.replace('[PH9500]', '').trim()}"</p>
                                ) : null}
                              </div>

                              <div className="flex items-center space-x-4 text-xs font-semibold text-slate-500 dark:text-brand-300 sm:text-right">
                                <div className="space-y-1">
                                  <span className="block flex items-center justify-end">
                                    <Clock className="h-3.5 w-3.5 text-slate-400 dark:text-brand-400 mr-1" />
                                    {formatLocalTime(appt.starttijd)} - {formatLocalTime(appt.eindtijd)}
                                  </span>
                                  {appt.googleEventId ? (
                                    <span className="text-[10px] text-blue-500 dark:text-blue-400 font-bold block flex items-center justify-end">
                                      <CheckCircle className="h-3 w-3 mr-0.5" /> Google Synced
                                    </span>
                                  ) : null}
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          
          {/* Status Footer */}
          <div className="px-6 py-3 bg-slate-50 dark:bg-brand-950/80 border-t border-slate-100 dark:border-brand-800/40 flex flex-col sm:flex-row items-center justify-between text-xs text-slate-500 dark:text-brand-300 font-medium gap-2">
            <span className="flex items-center space-x-1.5">
              <Clock className="h-3.5 w-3.5 text-brand-600 dark:text-brand-400" />
              <span>
                Weergave uren: <strong className="text-slate-800 dark:text-white font-bold">{minHour.toString().padStart(2, '0')}:00 - {(maxHour + 1).toString().padStart(2, '0')}:00</strong> {showFull24h ? '(Volledige 24-uurs dag)' : '(Automatisch aangepast aan geplande afspraken)'}
              </span>
            </span>
            <button 
              onClick={() => setShowFull24h(!showFull24h)} 
              className="text-brand-600 dark:text-brand-300 hover:text-brand-800 dark:hover:text-white font-bold hover:underline transition"
            >
              {showFull24h ? 'Schakel over naar automatische uren' : 'Toon alle uren (00:00 - 24:00)'}
            </button>
          </div>
        </div>
      )}

      {/* Herbruikbare AfspraakDetailModal */}
      <AfspraakDetailModal
        afspraak={selectedAfspraak}
        onClose={() => setSelectedAfspraak(null)}
        onSuccess={fetchAppointments}
      />
    </div>
  );
};
