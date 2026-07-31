import React, { useState, useEffect } from 'react';
import { afspraakApi, settingsApi, patientApi, afspraakTypeApi } from '../services/api';
import { Afspraak, SettingsData } from '../types';
import { 
  Calendar as CalendarIcon, Clock, ChevronLeft, ChevronRight, 
  RefreshCw, Loader2, Plus 
} from 'lucide-react';
import { AfspraakDetailModal } from '../components/AfspraakDetailModal';
import { formatDateTimeInput, formatHourString, formatShortDutchDate } from '../utils/dateUtils';
import { getPatientDisplayName } from '../utils/patientUtils';

export const CalendarPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Afspraak[]>([]);
  const [settings, setSettings] = useState<SettingsData | null>(null);
  const [currentDate, setCurrentDate] = useState<Date>(new Date());
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<'week' | 'day'>(() => window.innerWidth < 768 ? 'day' : 'week');
  const [showFull24h, setShowFull24h] = useState(false);

  // Modal State
  const [selectedAfspraak, setSelectedAfspraak] = useState<Afspraak | null>(null);

  // Drag-to-Select State
  const [dragSelect, setDragSelect] = useState<{
    isDragging: boolean;
    dayKey: string | null;
    startHour: number | null;
    currentHour: number | null;
  }>({
    isDragging: false,
    dayKey: null,
    startHour: null,
    currentHour: null
  });

  // New Booking Modal State
  const [isBookModalOpen, setIsBookModalOpen] = useState(false);
  const [bookingPatients, setBookingPatients] = useState<Array<{ id: number; naam: string }>>([]);
  const [bookingTypes, setBookingTypes] = useState<Array<{ id: number; naam: string; standaardDuurMinuten: number }>>([]);
  const [newBooking, setNewBooking] = useState({
    typeId: '',
    patientId: '',
    starttijd: '',
    duurMinuten: 60,
    opmerkingen: '',
    herhaling: 0,
    herhaalTot: ''
  });

  const loadBookingOptions = async () => {
    try {
      const [patientsData, typesData] = await Promise.all([
        patientApi.getAll().catch(() => []),
        afspraakTypeApi.getAll().catch(() => [])
      ]);
      const pList = patientsData.map((p: any) => ({
        id: p.id,
        naam: getPatientDisplayName(p)
      }));
      const tList = typesData.map((t: any) => ({ id: t.id, naam: t.naam, standaardDuurMinuten: t.standaardDuurMinuten }));
      setBookingPatients(pList);
      setBookingTypes(tList);
      return { pList, tList };
    } catch {
      return { pList: [], tList: [] };
    }
  };

  const handleOpenBookModal = async (prefilledDate?: Date, prefilledHour?: number, prefilledDurationMin?: number) => {
    const { tList } = await loadBookingOptions();
    const formattedStarttijd = formatDateTimeInput(prefilledDate, prefilledHour);
    const defaultTypeDuration = tList.length > 0 ? tList[0].standaardDuurMinuten : 60;
    const initialDuration = prefilledDurationMin && prefilledDurationMin > 0 ? prefilledDurationMin : defaultTypeDuration;

    setNewBooking({
      typeId: tList.length > 0 ? tList[0].id.toString() : '',
      patientId: '',
      starttijd: formattedStarttijd,
      duurMinuten: initialDuration,
      opmerkingen: '',
      herhaling: 0,
      herhaalTot: ''
    });
    setIsBookModalOpen(true);
  };

  const handleCreateBooking = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newBooking.starttijd) {
      alert("Selecteer a.u.b. een starttijd.");
      return;
    }

    try {
      const payload = {
        typeId: newBooking.typeId ? parseInt(newBooking.typeId, 10) : null,
        patientId: newBooking.patientId ? parseInt(newBooking.patientId, 10) : null,
        starttijd: new Date(newBooking.starttijd).toISOString(),
        customDuurMinuten: Number(newBooking.duurMinuten),
        opmerkingen: newBooking.opmerkingen,
        herhaling: newBooking.herhaling,
        herhaalTot: newBooking.herhaling !== 0 && newBooking.herhaalTot ? new Date(newBooking.herhaalTot).toISOString() : null
      };

      await afspraakApi.create(payload);
      setIsBookModalOpen(false);
      await fetchData();
    } catch (err: any) {
      console.error("Fout bij inplannen van afspraak:", err);
      alert(err?.response?.data?.message || "Kon afspraak niet inplannen.");
    }
  };

  // Drag selection helpers
  const getDayKey = (date: Date) => {
    const d = new Date(date);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  };

  const isSlotSelectedByDrag = (day: Date, hour: number): boolean => {
    if (!dragSelect.isDragging || !dragSelect.dayKey || dragSelect.startHour === null || dragSelect.currentHour === null) {
      return false;
    }
    const key = getDayKey(day);
    if (dragSelect.dayKey !== key) return false;
    const minH = Math.min(dragSelect.startHour, dragSelect.currentHour);
    const maxH = Math.max(dragSelect.startHour, dragSelect.currentHour);
    return hour >= minH && hour <= maxH;
  };

  const handleMouseDownSlot = (day: Date, hour: number, hasAppts: boolean, e: React.MouseEvent) => {
    if (hasAppts || e.button !== 0) return;
    e.preventDefault();
    const key = getDayKey(day);
    setDragSelect({
      isDragging: true,
      dayKey: key,
      startHour: hour,
      currentHour: hour
    });
  };

  const handleMouseEnterSlot = (day: Date, hour: number) => {
    if (!dragSelect.isDragging || !dragSelect.dayKey) return;
    const key = getDayKey(day);
    if (dragSelect.dayKey === key) {
      setDragSelect(prev => ({ ...prev, currentHour: hour }));
    }
  };

  const handleMouseUpSlot = () => {
    if (dragSelect.isDragging && dragSelect.dayKey && dragSelect.startHour !== null && dragSelect.currentHour !== null) {
      const minH = Math.min(dragSelect.startHour, dragSelect.currentHour);
      const maxH = Math.max(dragSelect.startHour, dragSelect.currentHour);
      const parts = dragSelect.dayKey.split('-');
      const targetDate = new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10));
      
      const totalHours = maxH - minH + 1;
      const durationMin = totalHours * 60;

      setDragSelect({ isDragging: false, dayKey: null, startHour: null, currentHour: null });
      handleOpenBookModal(targetDate, minH, durationMin);
    }
  };

  useEffect(() => {
    const onGlobalMouseUp = () => {
      if (dragSelect.isDragging) {
        handleMouseUpSlot();
      }
    };
    window.addEventListener('mouseup', onGlobalMouseUp);
    return () => window.removeEventListener('mouseup', onGlobalMouseUp);
  }, [dragSelect]);

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

  const fetchData = async () => {
    try {
      setLoading(true);
      const [appData, settingsData] = await Promise.all([
        afspraakApi.getAll(),
        settingsApi.get().catch(() => null)
      ]);
      setAppointments(appData);
      if (settingsData) setSettings(settingsData);
    } catch (err) {
      console.error("Fout bij ophalen agendagegevens:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    if (window.innerWidth < 768) {
      setViewMode('day');
    }
  }, []);

  const isPracticeBookingHour = (day: Date, hour: number): boolean => {
    if (!settings) return true;

    const s: any = settings;
    const getVal = (camel: string, pascal: string, fallback: any = null) => {
      if (s[camel] !== undefined && s[camel] !== null) return s[camel];
      if (s[pascal] !== undefined && s[pascal] !== null) return s[pascal];
      return fallback;
    };

    const dayOfWeek = day.getDay(); // 0=Sun, 1=Mon, ..., 6=Sat
    let isActive1 = false, startStr1 = "09:00", endStr1 = "12:00";
    let isActive2 = false, startStr2 = "13:00", endStr2 = "17:00";

    switch (dayOfWeek) {
      case 1:
        isActive1 = !!getVal('maandagActief', 'MaandagActief'); startStr1 = getVal('maandagStart', 'MaandagStart', "09:00"); endStr1 = getVal('maandagEinde', 'MaandagEinde', "12:00");
        isActive2 = !!getVal('maandag2Actief', 'Maandag2Actief'); startStr2 = getVal('maandagStart2', 'MaandagStart2', "13:00"); endStr2 = getVal('maandagEinde2', 'MaandagEinde2', "17:00");
        break;
      case 2:
        isActive1 = !!getVal('dinsdagActief', 'DinsdagActief'); startStr1 = getVal('dinsdagStart', 'DinsdagStart', "09:00"); endStr1 = getVal('dinsdagEinde', 'DinsdagEinde', "12:00");
        isActive2 = !!getVal('dinsdag2Actief', 'Dinsdag2Actief'); startStr2 = getVal('dinsdagStart2', 'DinsdagStart2', "13:00"); endStr2 = getVal('dinsdagEinde2', 'DinsdagEinde2', "17:00");
        break;
      case 3:
        isActive1 = !!getVal('woensdagActief', 'WoensdagActief'); startStr1 = getVal('woensdagStart', 'WoensdagStart', "09:00"); endStr1 = getVal('woensdagEinde', 'WoensdagEinde', "12:00");
        isActive2 = !!getVal('woensdag2Actief', 'Woensdag2Actief'); startStr2 = getVal('woensdagStart2', 'WoensdagStart2', "13:00"); endStr2 = getVal('woensdagEinde2', 'WoensdagEinde2', "17:00");
        break;
      case 4:
        isActive1 = !!getVal('donderdagActief', 'DonderdagActief'); startStr1 = getVal('donderdagStart', 'DonderdagStart', "09:00"); endStr1 = getVal('donderdagEinde', 'DonderdagEinde', "12:00");
        isActive2 = !!getVal('donderdag2Actief', 'Donderdag2Actief'); startStr2 = getVal('donderdagStart2', 'DonderdagStart2', "13:00"); endStr2 = getVal('donderdagEinde2', 'DonderdagEinde2', "17:00");
        break;
      case 5:
        isActive1 = !!getVal('vrijdagActief', 'VrijdagActief'); startStr1 = getVal('vrijdagStart', 'VrijdagStart', "09:00"); endStr1 = getVal('vrijdagEinde', 'VrijdagEinde', "12:00");
        isActive2 = !!getVal('vrijdag2Actief', 'Vrijdag2Actief'); startStr2 = getVal('vrijdagStart2', 'VrijdagStart2', "13:00"); endStr2 = getVal('vrijdagEinde2', 'VrijdagEinde2', "17:00");
        break;
      case 6:
        isActive1 = !!getVal('zaterdagActief', 'ZaterdagActief'); startStr1 = getVal('zaterdagStart', 'ZaterdagStart', "10:00"); endStr1 = getVal('zaterdagEinde', 'ZaterdagEinde', "12:00");
        isActive2 = !!getVal('zaterdag2Actief', 'Zaterdag2Actief'); startStr2 = getVal('zaterdagStart2', 'ZaterdagStart2', "13:00"); endStr2 = getVal('zaterdagEinde2', 'ZaterdagEinde2', "17:00");
        break;
      case 0:
        isActive1 = !!getVal('zondagActief', 'ZondagActief'); startStr1 = getVal('zondagStart', 'ZondagStart', "10:00"); endStr1 = getVal('zondagEinde', 'ZondagEinde', "12:00");
        isActive2 = !!getVal('zondag2Actief', 'Zondag2Actief'); startStr2 = getVal('zondagStart2', 'ZondagStart2', "13:00"); endStr2 = getVal('zondagEinde2', 'ZondagEinde2', "17:00");
        break;
    }

    const check = (active: boolean, sStr: string, eStr: string) => {
      if (!active) return false;
      const sH = parseInt(sStr.split(':')[0], 10);
      const eH = parseInt(eStr.split(':')[0], 10);
      const eM = parseInt(eStr.split(':')[1] || '0', 10);
      const effectiveEndH = eM > 0 ? eH : eH - 1;
      return hour >= sH && hour <= effectiveEndH;
    };

    return check(isActive1, startStr1, endStr1) || check(isActive2, startStr2, endStr2);
  };

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
      <div className="flex flex-col md:flex-row md:items-center md:justify-between bg-white dark:bg-brand-900 p-4 sm:p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm gap-4 transition-colors">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <CalendarIcon className="h-6 w-6 text-brand-600 dark:text-brand-400" />
            <span>Praktijk Agenda</span>
          </h1>
        </div>
        
        <div className="flex flex-wrap items-center gap-2 sm:gap-3">
          <button 
            onClick={handleToday}
            className="px-3 sm:px-4 py-2 text-xs sm:text-sm font-semibold bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-brand-100 rounded-xl transition"
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
            <span className="px-2 sm:px-3 text-xs font-bold text-slate-600 dark:text-brand-200">
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
              className={`px-2.5 sm:px-3 py-1.5 text-xs font-bold rounded-lg transition ${
                viewMode === 'week' 
                  ? 'bg-white dark:bg-brand-800 text-brand-600 dark:text-white shadow-sm' 
                  : 'text-slate-500 dark:text-brand-400 hover:text-slate-700 dark:hover:text-brand-200'
              }`}
            >
              Week
            </button>
            <button 
              onClick={() => setViewMode('day')}
              className={`px-2.5 sm:px-3 py-1.5 text-xs font-bold rounded-lg transition ${
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
            className={`px-2.5 sm:px-3 py-1.5 text-xs font-bold rounded-xl border transition flex items-center space-x-1.5 ${
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
            onClick={() => handleOpenBookModal()}
            className="p-2 sm:px-4 sm:py-2 text-xs font-bold bg-brand-500 hover:bg-brand-600 active:scale-95 text-white rounded-xl shadow-sm transition flex items-center space-x-1.5 cursor-pointer"
            title="Afspraak Inplannen"
          >
            <Plus className="h-4 w-4" />
            <span className="hidden sm:inline">Afspraak Inplannen</span>
          </button>

          <button 
            onClick={fetchData}
            className="p-2 text-slate-500 dark:text-brand-300 hover:text-slate-700 dark:hover:text-white hover:bg-slate-50 dark:hover:bg-brand-800 rounded-xl border border-slate-200 dark:border-brand-800 transition"
            title="Agenda vernieuwen"
          >
            <RefreshCw className="h-4 w-4" />
          </button>
        </div>
      </div>

      {/* Legenda Praktijkuren & Boekbaarheid */}
      <div className="flex flex-wrap items-center gap-3 bg-white dark:bg-brand-900 px-4 sm:px-6 py-3 rounded-2xl border border-slate-100 dark:border-brand-800/40 text-xs shadow-xs">
        <div className="flex items-center space-x-1.5 px-2.5 py-1 rounded-lg bg-teal-50 dark:bg-teal-950/60 text-teal-700 dark:text-teal-300 border border-teal-200/60 dark:border-teal-900/40 font-semibold">
          <span className="h-2 w-2 rounded-full bg-teal-500 animate-pulse" />
          <span>Praktijkuren</span>
        </div>
        <div className="flex items-center space-x-1.5 px-2.5 py-1 rounded-lg bg-slate-100 dark:bg-brand-950 text-slate-500 dark:text-brand-400 border border-slate-200/60 dark:border-brand-800/60 font-medium">
          <span className="h-2 w-2 rounded-full bg-slate-400" />
          <span>Buiten praktijkuren</span>
        </div>
        <div className="flex items-center space-x-1.5 px-2.5 py-1 rounded-lg bg-brand-50 dark:bg-brand-950 text-brand-600 dark:text-brand-300 border border-brand-200/60 dark:border-brand-800/60 font-medium">
          <span className="h-2 w-2 rounded-full bg-brand-500" />
          <span>💡 Sleep over meerdere uren om te blokkeren</span>
        </div>
      </div>

      {loading ? (
        <div className="flex items-center justify-center min-h-[50vh]">
          <Loader2 className="animate-spin h-10 w-10 text-brand-600 dark:text-brand-400" />
        </div>
      ) : (
        <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-xl overflow-hidden transition-colors">
          <div className="overflow-x-auto">
            <table className={`w-full border-collapse table-fixed ${viewMode === 'week' ? 'min-w-[700px] md:min-w-[800px]' : 'min-w-full'}`}>
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
                      {formatHourString(hour)}
                    </td>

                    {viewMode === 'week' ? (
                      getWeekDays(currentDate).map((day, dayIdx) => {
                        const appts = getAppointmentsForDayAndHour(day, hour);
                        const isBookingSlot = isPracticeBookingHour(day, hour);
                        const isDragSelected = isSlotSelectedByDrag(day, hour);
                        return (
                          <td 
                            key={dayIdx}
                            onMouseDown={(e) => handleMouseDownSlot(day, hour, appts.length > 0, e)}
                            onMouseEnter={() => handleMouseEnterSlot(day, hour)}
                            onMouseUp={() => handleMouseUpSlot()}
                            className={`p-2 border-l border-slate-100 dark:border-brand-800/40 align-top min-h-[80px] transition-all select-none ${
                              isDragSelected
                                ? 'bg-brand-200/80 dark:bg-brand-800/60 border-l-2 border-brand-500 dark:border-brand-400 ring-2 ring-brand-400/50 dark:ring-brand-500/40 shadow-md cursor-grabbing'
                                : isBookingSlot
                                  ? 'bg-emerald-100/80 dark:bg-emerald-950/60 border-l-2 border-emerald-400 dark:border-emerald-700 font-semibold text-emerald-950 dark:text-emerald-100 hover:bg-emerald-200/90 dark:hover:bg-emerald-900/80 shadow-2xs cursor-pointer'
                                  : 'bg-slate-200/60 dark:bg-brand-950/90 text-slate-400 dark:text-brand-500 opacity-60 hover:opacity-80 cursor-pointer'
                            }`}
                            title={
                              appts.length > 0 
                                ? undefined 
                                : isDragSelected
                                  ? `Selectie: ${formatHourString(Math.min(dragSelect.startHour!, dragSelect.currentHour!))} - ${formatHourString(Math.max(dragSelect.startHour!, dragSelect.currentHour!) + 1)}`
                                  : `Sleep om meerdere uren te selecteren op ${formatShortDutchDate(day)} om ${formatHourString(hour)}`
                            }
                          >
                            <div className="space-y-1.5">
                              {isDragSelected && !appts.length ? (
                                <span className="text-[10px] text-brand-700 dark:text-brand-200 font-bold flex items-center justify-center h-full py-3 animate-pulse">
                                  ⬛ {formatHourString(hour)} — {formatHourString(hour + 1)}
                                </span>
                              ) : appts.length > 0 ? (
                                appts.map((appt) => (
                                  <div 
                                    key={appt.id} 
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      setSelectedAfspraak(appt);
                                    }}
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
                              ) : isBookingSlot ? (
                                <span className="text-[10px] text-teal-600/70 dark:text-teal-400/60 font-semibold flex items-center justify-center h-full py-3 opacity-0 group-hover:opacity-100 transition">
                                  <Plus className="h-3 w-3 mr-0.5 text-teal-600" /> Inplannen
                                </span>
                              ) : (
                                <span className="text-[10px] text-slate-300 dark:text-brand-700 italic block text-center py-3 group-hover:text-slate-500 transition">+ Inplannen</span>
                              )}
                            </div>
                          </td>
                        );
                      })
                    ) : (
                      (() => {
                        const isBookingSlot = isPracticeBookingHour(currentDate, hour);
                        const appts = getAppointmentsForDayAndHour(currentDate, hour);
                        const isDragSelected = isSlotSelectedByDrag(currentDate, hour);
                        return (
                          <td
                            onMouseDown={(e) => handleMouseDownSlot(currentDate, hour, appts.length > 0, e)}
                            onMouseEnter={() => handleMouseEnterSlot(currentDate, hour)}
                            onMouseUp={() => handleMouseUpSlot()}
                            className={`p-2 border-l border-slate-100 dark:border-brand-800/40 align-top min-h-[80px] transition-all select-none ${
                              isDragSelected
                                ? 'bg-brand-200/80 dark:bg-brand-800/60 border-l-2 border-brand-500 dark:border-brand-400 ring-2 ring-brand-400/50 dark:ring-brand-500/40 shadow-md cursor-grabbing'
                                : isBookingSlot
                                  ? 'bg-emerald-100/80 dark:bg-emerald-950/60 border-l-2 border-emerald-400 dark:border-emerald-700 font-semibold text-emerald-950 dark:text-emerald-100 hover:bg-emerald-200/90 dark:hover:bg-emerald-900/80 shadow-2xs cursor-pointer'
                                  : 'bg-slate-200/60 dark:bg-brand-950/90 text-slate-400 dark:text-brand-500 opacity-60 hover:opacity-80 cursor-pointer'
                            }`}
                          >
                            <div className="space-y-2">
                              {isDragSelected && !appts.length ? (
                                <span className="text-xs text-brand-700 dark:text-brand-200 font-bold flex items-center justify-center py-2 animate-pulse">
                                  ⬛ {formatHourString(hour)} — {formatHourString(hour + 1)}
                                </span>
                              ) : appts.length > 0 ? (
                                appts.map((appt) => (
                                  <div 
                                    key={appt.id} 
                                    onClick={(e) => {
                                      e.stopPropagation();
                                      setSelectedAfspraak(appt);
                                    }}
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
                                    </div>
                                    <div className="flex items-center space-x-4 text-xs font-semibold text-slate-500 dark:text-brand-300 sm:text-right">
                                      <span className="block flex items-center justify-end">
                                        <Clock className="h-3.5 w-3.5 text-slate-400 dark:text-brand-400 mr-1" />
                                        {formatLocalTime(appt.starttijd)} - {formatLocalTime(appt.eindtijd)}
                                      </span>
                                    </div>
                                  </div>
                                ))
                              ) : isBookingSlot ? (
                                <span className="text-xs text-teal-600/70 dark:text-teal-400/60 font-semibold flex items-center justify-center py-2">
                                  <Plus className="h-3.5 w-3.5 mr-1 text-teal-600" /> Praktijkuren — Sleep om te blokkeren
                                </span>
                              ) : (
                                <span className="text-xs text-slate-400 dark:text-brand-500 italic block text-center py-2">+ Sleep om in te plannen</span>
                              )}
                            </div>
                          </td>
                        );
                      })()
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
                Weergave uren: <strong className="text-slate-800 dark:text-white font-bold">{formatHourString(minHour)} - {formatHourString(maxHour + 1)}</strong> {showFull24h ? '(Volledige 24-uurs dag)' : '(Automatisch aangepast aan geplande afspraken)'}
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

              {/* Duur / Eindtijd Selector */}
              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Duur</label>
                <div className="flex flex-wrap gap-2 mb-2">
                  {[30, 60, 120, 180, 240, 480].map((min) => {
                    const label = min < 60 ? `${min}m` : min === 480 ? '8u (hele dag)' : `${min / 60}u`;
                    return (
                      <button
                        key={min}
                        type="button"
                        onClick={() => setNewBooking({ ...newBooking, duurMinuten: min })}
                        className={`px-3 py-1.5 text-xs font-bold rounded-lg border transition ${
                          Number(newBooking.duurMinuten) === min
                            ? 'bg-brand-600 text-white border-brand-600 shadow-sm'
                            : 'bg-slate-50 dark:bg-brand-950 text-slate-600 dark:text-brand-300 border-slate-200 dark:border-brand-800 hover:bg-slate-100 dark:hover:bg-brand-800'
                        }`}
                      >
                        {label}
                      </button>
                    );
                  })}
                </div>
                <div className="flex items-center gap-3">
                  <input
                    type="number"
                    min={15}
                    max={1440}
                    step={15}
                    value={newBooking.duurMinuten}
                    onChange={(e) => setNewBooking({ ...newBooking, duurMinuten: Math.max(15, Number(e.target.value)) })}
                    className="w-28 bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500 text-sm"
                  />
                  <span className="text-xs text-slate-500 dark:text-brand-400 font-medium">minuten</span>
                  {newBooking.starttijd && (
                    <span className="text-xs text-brand-600 dark:text-brand-300 font-bold flex items-center bg-brand-50 dark:bg-brand-950 px-2.5 py-1 rounded-lg border border-brand-200/60 dark:border-brand-800/60">
                      <Clock className="h-3 w-3 mr-1" />
                      {(() => {
                        const start = new Date(newBooking.starttijd);
                        const end = new Date(start.getTime() + Number(newBooking.duurMinuten) * 60000);
                        return `${start.toLocaleTimeString('nl-NL', { hour: '2-digit', minute: '2-digit' })} — ${end.toLocaleTimeString('nl-NL', { hour: '2-digit', minute: '2-digit' })}`;
                      })()}
                    </span>
                  )}
                </div>
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
        onSuccess={fetchData}
      />
    </div>
  );
};
