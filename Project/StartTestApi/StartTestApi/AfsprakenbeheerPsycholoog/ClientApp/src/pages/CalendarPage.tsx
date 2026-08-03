import React, { useState, useEffect } from 'react';
import { afspraakApi, settingsApi } from '../services/api';
import { Afspraak, SettingsData } from '../types';
import { 
  Calendar as CalendarIcon, Clock, ChevronLeft, ChevronRight, 
  RefreshCw, Loader2, Plus, Video 
} from 'lucide-react';
import { AfspraakDetailModal } from '../components/AfspraakDetailModal';
import { AfspraakInplannenModal } from '../components/AfspraakInplannenModal';
import { formatHourString, formatShortDutchDate, formatSlotTimeString } from '../utils/dateUtils';

export const CalendarPage: React.FC = () => {
  const [appointments, setAppointments] = useState<Afspraak[]>([]);
  const [settings, setSettings] = useState<SettingsData | null>(null);
  const [currentDate, setCurrentDate] = useState<Date>(new Date());
  const [loading, setLoading] = useState(true);
  const [viewMode, setViewMode] = useState<'week' | 'day'>(() => window.innerWidth < 768 ? 'day' : 'week');
  const [showFull24h, setShowFull24h] = useState(false);

  // Modal State
  const [selectedAfspraak, setSelectedAfspraak] = useState<Afspraak | null>(null);

  // Drag-to-Select State (sub-slot position values e.g. 12.0 or 12.5)
  const [dragSelect, setDragSelect] = useState<{
    isDragging: boolean;
    dayKey: string | null;
    startSlot: number | null;
    currentSlot: number | null;
  }>({
    isDragging: false,
    dayKey: null,
    startSlot: null,
    currentSlot: null
  });

  // New Booking & Edit Modal State
  const [isBookModalOpen, setIsBookModalOpen] = useState(false);
  const [editingAfspraak, setEditingAfspraak] = useState<Afspraak | null>(null);
  const [bookModalPrefills, setBookModalPrefills] = useState<{
    date?: Date | null;
    slot?: number | null;
    durationMin?: number | null;
  }>({});

  const handleOpenBookModal = (prefilledDate?: Date, prefilledSlot?: number, prefilledDurationMin?: number) => {
    setBookModalPrefills({
      date: prefilledDate || null,
      slot: prefilledSlot !== undefined ? prefilledSlot : null,
      durationMin: prefilledDurationMin || null
    });
    setIsBookModalOpen(true);
  };


  // Drag selection & Touch helpers
  const getDayKey = (date: Date) => {
    const d = new Date(date);
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  };

  const isSubSlotSelectedByDrag = (day: Date, slotVal: number): boolean => {
    if (!dragSelect.isDragging || !dragSelect.dayKey || dragSelect.startSlot === null || dragSelect.currentSlot === null) {
      return false;
    }
    const key = getDayKey(day);
    if (dragSelect.dayKey !== key) return false;
    const minSlot = Math.min(dragSelect.startSlot, dragSelect.currentSlot);
    const maxSlot = Math.max(dragSelect.startSlot, dragSelect.currentSlot);
    return slotVal >= minSlot && slotVal <= maxSlot;
  };

  const handleMouseDownSubSlot = (day: Date, slotVal: number, hasAppts: boolean, e: React.MouseEvent) => {
    if (hasAppts || e.button !== 0) return;
    e.preventDefault();
    const key = getDayKey(day);
    setDragSelect({
      isDragging: true,
      dayKey: key,
      startSlot: slotVal,
      currentSlot: slotVal
    });
  };

  const handleMouseEnterSubSlot = (day: Date, slotVal: number) => {
    if (!dragSelect.isDragging || !dragSelect.dayKey) return;
    const key = getDayKey(day);
    if (dragSelect.dayKey === key) {
      setDragSelect(prev => ({ ...prev, currentSlot: slotVal }));
    }
  };

  const handleTouchStartSubSlot = (day: Date, slotVal: number, hasAppts: boolean, e: React.TouchEvent) => {
    if (hasAppts) return;
    e.preventDefault(); // Prevent scroll from starting during drag selection
    const key = getDayKey(day);
    setDragSelect({
      isDragging: true,
      dayKey: key,
      startSlot: slotVal,
      currentSlot: slotVal
    });
  };

  const handleMouseUpSlot = () => {
    if (dragSelect.isDragging && dragSelect.dayKey && dragSelect.startSlot !== null && dragSelect.currentSlot !== null) {
      const minSlot = Math.min(dragSelect.startSlot, dragSelect.currentSlot);
      const maxSlot = Math.max(dragSelect.startSlot, dragSelect.currentSlot);
      const parts = dragSelect.dayKey.split('-');
      const targetDate = new Date(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10));

      const totalHours = (maxSlot + 0.5) - minSlot;
      const durationMin = Math.round(totalHours * 60);

      setDragSelect({ isDragging: false, dayKey: null, startSlot: null, currentSlot: null });
      handleOpenBookModal(targetDate, minSlot, durationMin);
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

  useEffect(() => {
    if (!dragSelect.isDragging) return;

    const onGlobalTouchMove = (e: TouchEvent) => {
      e.preventDefault(); // Block page scroll while drag-selecting
      if (e.touches.length === 0) return;
      const touch = e.touches[0];
      const targetEl = document.elementFromPoint(touch.clientX, touch.clientY);
      if (!targetEl) return;

      const slotEl = targetEl.closest('[data-slot-val]') as HTMLElement;
      if (slotEl) {
        const dayKey = slotEl.getAttribute('data-day-key');
        const slotValStr = slotEl.getAttribute('data-slot-val');
        if (dayKey === dragSelect.dayKey && slotValStr !== null) {
          const slotVal = parseFloat(slotValStr);
          setDragSelect(prev => ({ ...prev, currentSlot: slotVal }));
        }
      }
    };

    const onGlobalTouchEnd = () => {
      handleMouseUpSlot();
    };

    window.addEventListener('touchmove', onGlobalTouchMove, { passive: false });
    window.addEventListener('touchend', onGlobalTouchEnd);

    return () => {
      window.removeEventListener('touchmove', onGlobalTouchMove);
      window.removeEventListener('touchend', onGlobalTouchEnd);
    };
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

  const getApptSlotBounds = (appt: Afspraak) => {
    const start = new Date(appt.starttijd);
    const end = new Date(appt.eindtijd);
    
    const startH = start.getHours();
    const startM = start.getMinutes();
    const startSlot = startH + (startM >= 30 ? 0.5 : 0);

    const endH = end.getHours();
    const endM = end.getMinutes();
    let endSlot: number;
    if (endM === 0) {
      // Ends exactly on the hour → last occupied slot is previous hour's :30
      endSlot = endH - 0.5;
    } else {
      // Ends anywhere within the hour → fill entire hour row visually
      endSlot = endH + 0.5;
    }

    return { startSlot: Math.max(0, startSlot), endSlot: Math.max(startSlot, endSlot) };
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
                            <span className="text-[11px] text-slate-300 dark:text-brand-600 font-medium block text-center py-1">-</span>
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
                        <span className="text-[11px] text-slate-400 dark:text-brand-500 font-medium block text-center py-1">Geen hele-dag meldingen op deze dag</span>
                      )}
                    </td>
                  )}
                </tr>

                {hours.map((hour) => (
                  <tr key={hour} className="group">
                    <td className="p-4 text-sm font-semibold text-slate-400 dark:text-brand-400 align-top border-b border-slate-100 dark:border-brand-800/40">
                      {formatHourString(hour)}
                    </td>

                    {viewMode === 'week' ? (
                      getWeekDays(currentDate).map((day, dayIdx) => {
                        const isBookingSlot = isPracticeBookingHour(day, hour);
                        const dayKey = getDayKey(day);

                        return (
                          <td 
                            key={dayIdx}
                            className={`p-0 border-l border-slate-100 dark:border-brand-800/40 align-top min-h-[84px] transition-all select-none ${
                              isBookingSlot
                                ? 'bg-emerald-50/40 dark:bg-emerald-950/30 border-l-2 border-emerald-400 dark:border-emerald-700'
                                : 'bg-slate-200/40 dark:bg-brand-950/90'
                            }`}
                          >
                            <div className="flex flex-col h-full min-h-[84px]">
                              {[0, 0.5].map((subOffset) => {
                                const slotVal = hour + subOffset;
                                const isSelected = isSubSlotSelectedByDrag(day, slotVal);
                                const isStart = dragSelect.isDragging && dragSelect.dayKey === dayKey && Math.min(dragSelect.startSlot!, dragSelect.currentSlot!) === slotVal;

                                // Find all appointments covering this exact sub-slot
                                const matchingAppts = appointments.filter(app => {
                                  if (app.isHeleDag) return false;
                                  const appStart = new Date(app.starttijd);
                                  if (appStart.toDateString() !== day.toDateString()) return false;
                                  const { startSlot, endSlot } = getApptSlotBounds(app);
                                  return slotVal >= startSlot && slotVal <= endSlot;
                                });

                                const activeAppt = matchingAppts.find(a => a.status !== 'Geannuleerd');
                                const cancelledAppt = matchingAppts.find(a => a.status === 'Geannuleerd');

                                // Prioritize active appointment over cancelled appointment
                                const coveringAppt = activeAppt || cancelledAppt;
                                const isCancelledOnly = !activeAppt && !!cancelledAppt;

                                if (coveringAppt) {
                                  const { startSlot, endSlot } = getApptSlotBounds(coveringAppt);
                                  const isStartSlot = slotVal === startSlot;
                                  const isEndSlot = slotVal === endSlot;
                                  const isSingleSlot = isStartSlot && isEndSlot;
                                  const isMeet = !!coveringAppt.googleMeetLink || (coveringAppt.opmerkingen && (coveringAppt.opmerkingen.includes('GoogleMeet') || coveringAppt.opmerkingen.includes('Google Meet')));
                                  const colorBorder = coveringAppt.status === 'Geannuleerd' ? '#94a3b8' : isMeet ? '#8b5cf6' : coveringAppt.kleurcode;

                                  if (isStartSlot) {
                                    return (
                                      <div 
                                        key={`appt-${coveringAppt.id}-${slotVal}`} 
                                        data-day-key={dayKey}
                                        data-slot-val={slotVal}
                                        onMouseDown={(e) => {
                                          if (isCancelledOnly && !(e.target as HTMLElement).closest('.cancelled-action-btn')) {
                                            handleMouseDownSubSlot(day, slotVal, false, e);
                                          }
                                        }}
                                        onMouseEnter={() => {
                                          if (isCancelledOnly) handleMouseEnterSubSlot(day, slotVal);
                                        }}
                                        onTouchStart={(e) => {
                                          if (isCancelledOnly && !(e.target as HTMLElement).closest('.cancelled-action-btn')) {
                                            handleTouchStartSubSlot(day, slotVal, false, e);
                                          }
                                        }}
                                        style={{ borderLeftColor: colorBorder }}
                                        className={`flex-1 min-h-[42px] p-1.5 border-l-4 ${
                                          isSingleSlot ? 'rounded-xl border border-slate-200 dark:border-slate-700/60' : 'rounded-t-xl rounded-b-none border-t border-l border-r border-b-0 border-slate-200 dark:border-slate-700/60'
                                        } shadow-2xs hover:shadow-xs transition text-left cursor-pointer ${
                                          isSelected ? 'bg-brand-500 text-white font-bold shadow-md animate-pulse' :
                                          coveringAppt.status === 'Geannuleerd' ? 'opacity-70 bg-slate-100/90 dark:bg-slate-800/40 border-dashed border-slate-300 dark:border-slate-700' :
                                          isMeet ? 'bg-purple-50/90 dark:bg-purple-950/60 border-purple-200 dark:border-purple-800/60' :
                                          'bg-slate-50 dark:bg-slate-800/90 border-slate-200 dark:border-slate-700/60'
                                        }`}
                                        title={isSelected ? 'Selectie voor nieuwe afspraak' : `${coveringAppt.patientNaam || 'Geblokkeerd'} (${formatLocalTime(coveringAppt.starttijd)} - ${formatLocalTime(coveringAppt.eindtijd)})`}
                                      >
                                        {isSelected ? (
                                          <span className="font-bold flex items-center gap-1 truncate text-white text-xs">
                                            {isStart && <span>⬛</span>}
                                            <span>{formatSlotTimeString(slotVal)}</span>
                                          </span>
                                        ) : (
                                          <>
                                            <div className="flex justify-between items-start gap-1">
                                              <button
                                                type="button"
                                                onClick={(e) => {
                                                  e.stopPropagation();
                                                  setSelectedAfspraak(coveringAppt);
                                                }}
                                                className="cancelled-action-btn text-xs font-bold truncate block text-left hover:underline focus:outline-none"
                                              >
                                                <span className={coveringAppt.status === 'Geannuleerd' ? 'line-through text-slate-400 dark:text-brand-400' : 'text-slate-800 dark:text-white'}>
                                                  {coveringAppt.patientNaam}
                                                </span>
                                              </button>
                                              {coveringAppt.status === 'Geannuleerd' ? (
                                                <button
                                                  type="button"
                                                  onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleOpenBookModal(day, slotVal, 60);
                                                  }}
                                                  className="cancelled-action-btn px-1.5 py-0.5 rounded bg-emerald-500 hover:bg-emerald-600 active:scale-95 text-white text-[9px] font-bold shrink-0 shadow-xs transition cursor-pointer"
                                                  title="Nieuwe afspraak inplannen op dit uur"
                                                >
                                                  + Inplannen
                                                </button>
                                              ) : isMeet ? (
                                                <span className="flex items-center gap-0.5 px-1 py-0.5 rounded bg-purple-100 dark:bg-purple-900 text-purple-700 dark:text-purple-200 text-[9px] font-bold shrink-0 border border-purple-200 dark:border-purple-700" title="Online Google Meet Afspraak">
                                                  <Video className="h-2.5 w-2.5 text-purple-600 dark:text-purple-300" />
                                                  <span>Meet</span>
                                                </span>
                                              ) : coveringAppt.googleEventId ? (
                                                <span className="flex h-2 w-2 rounded-full bg-blue-500 flex-shrink-0" title="Gesynchroniseerd met Google Calendar" />
                                              ) : null}
                                            </div>
                                            <div className="flex items-center justify-between mt-0.5">
                                              <span className="text-[10px] text-slate-500 dark:text-brand-300 font-semibold block truncate">
                                                {coveringAppt.afspraakTypeNaam}
                                                {activeAppt && cancelledAppt && (
                                                  <span className="ml-1 text-[9px] text-amber-600 dark:text-amber-400 font-bold" title="Er is ook 1 geannuleerde afspraak op dit tijdstip">(+1 geannuleerd)</span>
                                                )}
                                              </span>
                                            </div>
                                            <span className="text-[9px] text-slate-400 dark:text-brand-400 font-bold block mt-0.5 flex items-center">
                                              <Clock className="h-2.5 w-2.5 mr-0.5" />
                                              {formatLocalTime(coveringAppt.starttijd)} - {formatLocalTime(coveringAppt.eindtijd)}
                                            </span>
                                          </>
                                        )}
                                      </div>
                                    );
                                  }

                                  return (
                                    <div 
                                      key={`ongoing-${coveringAppt.id}-${slotVal}`} 
                                      data-day-key={dayKey}
                                      data-slot-val={slotVal}
                                      onMouseDown={(e) => {
                                        if (isCancelledOnly) handleMouseDownSubSlot(day, slotVal, false, e);
                                      }}
                                      onMouseEnter={() => {
                                        if (isCancelledOnly) handleMouseEnterSubSlot(day, slotVal);
                                      }}
                                      onTouchStart={(e) => {
                                        if (isCancelledOnly) handleTouchStartSubSlot(day, slotVal, false, e);
                                      }}
                                      onClick={(e) => {
                                        e.stopPropagation();
                                        if (!isCancelledOnly) {
                                          setSelectedAfspraak(coveringAppt);
                                        }
                                      }}
                                      style={{ borderLeftColor: colorBorder }}
                                      className={`flex-1 min-h-[42px] w-full border-l-4 ${
                                        isEndSlot ? 'rounded-b-xl rounded-t-none border-b border-l border-r border-t-0 border-slate-200 dark:border-slate-700/60' : 'rounded-none border-l border-r border-t-0 border-b-0 border-slate-200 dark:border-slate-700/60'
                                      } ${
                                        isSelected ? 'bg-brand-500 text-white font-bold shadow-md animate-pulse' :
                                        coveringAppt.status === 'Geannuleerd' ? 'opacity-70 bg-slate-100/80 dark:bg-slate-800/40' :
                                        isMeet ? 'bg-purple-50/90 dark:bg-purple-950/60 border-purple-200 dark:border-purple-800/60' :
                                        'bg-slate-50 dark:bg-slate-800/90 border-slate-200 dark:border-slate-700/60'
                                      } cursor-pointer transition hover:opacity-90 flex items-center justify-between px-2`}
                                      title={isSelected ? 'Selectie voor nieuwe afspraak' : `${coveringAppt.patientNaam || 'Geblokkeerd'} (${formatLocalTime(coveringAppt.starttijd)} - ${formatLocalTime(coveringAppt.eindtijd)})`}
                                    />
                                  );
                                }

                                return (
                                  <div
                                    key={subOffset}
                                    data-day-key={dayKey}
                                    data-slot-val={slotVal}
                                    onMouseDown={(e) => handleMouseDownSubSlot(day, slotVal, false, e)}
                                    onMouseEnter={() => handleMouseEnterSubSlot(day, slotVal)}
                                    onTouchStart={(e) => handleTouchStartSubSlot(day, slotVal, false, e)}
                                    className={`flex-1 min-h-[38px] px-2 py-1 flex items-center justify-between text-[10px] transition cursor-pointer ${
                                      subOffset === 0 ? 'border-b border-dashed border-slate-200/40 dark:border-brand-800/30' : 'border-b border-slate-100 dark:border-brand-800/40'
                                    } ${
                                      isSelected
                                        ? 'bg-brand-500 text-white font-bold shadow-xs animate-pulse'
                                        : isBookingSlot
                                          ? 'hover:bg-emerald-200/70 dark:hover:bg-emerald-900/60 text-slate-500 dark:text-brand-300 group/sub'
                                          : 'hover:bg-slate-300/50 dark:hover:bg-brand-800/50 text-slate-400 dark:text-brand-500 group/sub'
                                    }`}
                                    title={
                                      isSelected
                                        ? `Selectie: ${formatSlotTimeString(Math.min(dragSelect.startSlot!, dragSelect.currentSlot!))} - ${formatSlotTimeString(Math.max(dragSelect.startSlot!, dragSelect.currentSlot!) + 0.5)}`
                                        : `Klik of sleep vanaf ${formatSlotTimeString(slotVal)} op ${formatShortDutchDate(day)}`
                                    }
                                  >
                                    {isSelected ? (
                                      <span className="font-bold flex items-center gap-1 truncate text-white">
                                        {isStart && <span>⬛</span>}
                                        <span>{formatSlotTimeString(slotVal)}</span>
                                      </span>
                                    ) : (
                                      <span className="opacity-0 group-hover/sub:opacity-100 transition text-[9px] font-semibold text-teal-600 dark:text-teal-400">
                                        + {formatSlotTimeString(slotVal)}
                                      </span>
                                    )}
                                  </div>
                                );
                              })}
                            </div>
                          </td>
                        );
                      })
                    ) : (
                      (() => {
                        const isBookingSlot = isPracticeBookingHour(currentDate, hour);
                        const dayKey = getDayKey(currentDate);

                        return (
                          <td 
                            className={`p-0 border-l border-slate-100 dark:border-brand-800/40 align-top min-h-[84px] transition-all select-none ${
                              isBookingSlot
                                ? 'bg-emerald-50/40 dark:bg-emerald-950/30 border-l-2 border-emerald-400 dark:border-emerald-700'
                                : 'bg-slate-200/40 dark:bg-brand-950/90'
                            }`}
                          >
                            <div className="flex flex-col h-full min-h-[84px]">
                              {[0, 0.5].map((subOffset) => {
                                const slotVal = hour + subOffset;
                                const isSelected = isSubSlotSelectedByDrag(currentDate, slotVal);
                                const isStart = dragSelect.isDragging && dragSelect.dayKey === dayKey && Math.min(dragSelect.startSlot!, dragSelect.currentSlot!) === slotVal;

                                // Find all appointments covering this exact sub-slot
                                const matchingAppts = appointments.filter(app => {
                                  if (app.isHeleDag) return false;
                                  const appStart = new Date(app.starttijd);
                                  if (appStart.toDateString() !== currentDate.toDateString()) return false;
                                  const { startSlot, endSlot } = getApptSlotBounds(app);
                                  return slotVal >= startSlot && slotVal <= endSlot;
                                });

                                const activeAppt = matchingAppts.find(a => a.status !== 'Geannuleerd');
                                const cancelledAppt = matchingAppts.find(a => a.status === 'Geannuleerd');

                                // Prioritize active appointment over cancelled appointment
                                const coveringAppt = activeAppt || cancelledAppt;
                                const isCancelledOnly = !activeAppt && !!cancelledAppt;

                                if (coveringAppt) {
                                  const { startSlot, endSlot } = getApptSlotBounds(coveringAppt);
                                  const isStartSlot = slotVal === startSlot;
                                  const isEndSlot = slotVal === endSlot;
                                  const isSingleSlot = isStartSlot && isEndSlot;
                                  const isMeet = !!coveringAppt.googleMeetLink || (coveringAppt.opmerkingen && (coveringAppt.opmerkingen.includes('GoogleMeet') || coveringAppt.opmerkingen.includes('Google Meet')));
                                  const colorBorder = coveringAppt.status === 'Geannuleerd' ? '#94a3b8' : isMeet ? '#8b5cf6' : coveringAppt.kleurcode;

                                  if (isStartSlot) {
                                    return (
                                      <div 
                                        key={`appt-${coveringAppt.id}-${slotVal}`} 
                                        data-day-key={dayKey}
                                        data-slot-val={slotVal}
                                        onMouseDown={(e) => {
                                          if (isCancelledOnly && !(e.target as HTMLElement).closest('.cancelled-action-btn')) {
                                            handleMouseDownSubSlot(currentDate, slotVal, false, e);
                                          }
                                        }}
                                        onMouseEnter={() => {
                                          if (isCancelledOnly) handleMouseEnterSubSlot(currentDate, slotVal);
                                        }}
                                        onTouchStart={(e) => {
                                          if (isCancelledOnly && !(e.target as HTMLElement).closest('.cancelled-action-btn')) {
                                            handleTouchStartSubSlot(currentDate, slotVal, false, e);
                                          }
                                        }}
                                        style={{ borderLeftColor: colorBorder }}
                                        className={`flex-1 min-h-[42px] p-2.5 border-l-4 ${
                                          isSingleSlot ? 'rounded-xl border border-slate-200 dark:border-slate-700/60' : 'rounded-t-xl rounded-b-none border-t border-l border-r border-b-0 border-slate-200 dark:border-slate-700/60'
                                        } shadow-2xs hover:shadow-xs transition text-left cursor-pointer ${
                                          isSelected ? 'bg-brand-500 text-white font-bold shadow-md animate-pulse' :
                                          coveringAppt.status === 'Geannuleerd' ? 'opacity-70 bg-slate-100/90 dark:bg-slate-800/40 border-dashed border-slate-300 dark:border-slate-700' :
                                          isMeet ? 'bg-purple-50/90 dark:bg-purple-950/60 border-purple-200 dark:border-purple-800/60' :
                                          'bg-slate-50 dark:bg-slate-800/90 border-slate-200 dark:border-slate-700/60'
                                        }`}
                                        title={isSelected ? 'Selectie voor nieuwe afspraak' : `${coveringAppt.patientNaam || 'Geblokkeerd'} (${formatLocalTime(coveringAppt.starttijd)} - ${formatLocalTime(coveringAppt.eindtijd)})`}
                                      >
                                        {isSelected ? (
                                          <span className="font-bold flex items-center gap-1 truncate text-white text-xs">
                                            {isStart && <span>⬛</span>}
                                            <span>{formatSlotTimeString(slotVal)}</span>
                                          </span>
                                        ) : (
                                          <>
                                            <div className="flex justify-between items-start gap-2">
                                              <h4 className="text-sm font-bold flex items-center space-x-1.5 flex-wrap gap-y-1">
                                                <button
                                                  type="button"
                                                  onClick={(e) => {
                                                    e.stopPropagation();
                                                    setSelectedAfspraak(coveringAppt);
                                                  }}
                                                  className="cancelled-action-btn hover:underline text-left focus:outline-none"
                                                >
                                                  <span className={coveringAppt.status === 'Geannuleerd' ? 'line-through text-slate-400 dark:text-brand-400' : 'text-slate-800 dark:text-white'}>
                                                    {coveringAppt.patientNaam}
                                                  </span>
                                                </button>
                                                <span className={`text-[9px] font-bold uppercase tracking-wider px-1.5 py-0.5 rounded-full ${
                                                  coveringAppt.status === 'Geannuleerd' ? 'bg-red-100 dark:bg-red-950/60 text-red-700 dark:text-red-300 border border-red-100 dark:border-red-900/40' :
                                                  coveringAppt.status === 'Voltooid' ? 'bg-emerald-100 dark:bg-emerald-950/60 text-emerald-700 dark:text-emerald-300 border border-emerald-100 dark:border-emerald-900/40' :
                                                  'bg-blue-50 dark:bg-blue-950/60 text-blue-700 dark:text-blue-300 border border-blue-100 dark:border-blue-900/40'
                                                }`}>
                                                  {coveringAppt.status}
                                                </span>
                                                {isMeet && (
                                                  <span className="text-[9px] font-bold uppercase tracking-wider px-2 py-0.5 rounded-full bg-purple-100 dark:bg-purple-900 text-purple-800 dark:text-purple-200 border border-purple-300 dark:border-purple-700 flex items-center gap-1">
                                                    <Video className="h-3 w-3 text-purple-600 dark:text-purple-300" />
                                                    <span>Online Meet</span>
                                                  </span>
                                                )}
                                              </h4>
                                              <div className="flex items-center gap-2 shrink-0">
                                                {coveringAppt.status === 'Geannuleerd' && (
                                                  <button
                                                    type="button"
                                                    onClick={(e) => {
                                                      e.stopPropagation();
                                                      handleOpenBookModal(currentDate, slotVal, 60);
                                                    }}
                                                    className="cancelled-action-btn px-2 py-1 rounded bg-emerald-500 hover:bg-emerald-600 active:scale-95 text-white text-xs font-bold shadow-xs transition cursor-pointer"
                                                    title="Nieuwe afspraak inplannen op dit uur"
                                                  >
                                                    + Inplannen
                                                  </button>
                                                )}
                                                <span className="text-xs text-slate-500 dark:text-brand-300 font-semibold">
                                                  <Clock className="h-3.5 w-3.5 text-slate-400 dark:text-brand-400 inline mr-1" />
                                                  {formatLocalTime(coveringAppt.starttijd)} - {formatLocalTime(coveringAppt.eindtijd)}
                                                </span>
                                              </div>
                                            </div>
                                            <span className="text-xs text-slate-500 dark:text-brand-300 font-semibold mt-0.5 block">
                                              {coveringAppt.afspraakTypeNaam}
                                              {activeAppt && cancelledAppt && (
                                                <span className="ml-1.5 text-xs text-amber-600 dark:text-amber-400 font-bold" title="Er is ook 1 geannuleerde afspraak op dit tijdstip">(+1 geannuleerd)</span>
                                              )}
                                            </span>
                                          </>
                                        )}
                                      </div>
                                    );
                                  }

                                  return (
                                    <div 
                                      key={`ongoing-${coveringAppt.id}-${slotVal}`} 
                                      data-day-key={dayKey}
                                      data-slot-val={slotVal}
                                      onMouseDown={(e) => {
                                        if (isCancelledOnly) handleMouseDownSubSlot(currentDate, slotVal, false, e);
                                      }}
                                      onMouseEnter={() => {
                                        if (isCancelledOnly) handleMouseEnterSubSlot(currentDate, slotVal);
                                      }}
                                      onTouchStart={(e) => {
                                        if (isCancelledOnly) handleTouchStartSubSlot(currentDate, slotVal, false, e);
                                      }}
                                      onClick={(e) => {
                                        e.stopPropagation();
                                        if (!isCancelledOnly) {
                                          setSelectedAfspraak(coveringAppt);
                                        }
                                      }}
                                      style={{ borderLeftColor: colorBorder }}
                                      className={`flex-1 min-h-[42px] w-full border-l-4 ${
                                        isEndSlot ? 'rounded-b-xl rounded-t-none border-b border-l border-r border-t-0 border-slate-200 dark:border-slate-700/60' : 'rounded-none border-l border-r border-t-0 border-b-0 border-slate-200 dark:border-slate-700/60'
                                      } ${
                                        isSelected ? 'bg-brand-500 text-white font-bold shadow-md animate-pulse' :
                                        coveringAppt.status === 'Geannuleerd' ? 'opacity-70 bg-slate-100/80 dark:bg-slate-800/40' :
                                        isMeet ? 'bg-purple-50/90 dark:bg-purple-950/60 border-purple-200 dark:border-purple-800/60' :
                                        'bg-slate-50 dark:bg-slate-800/90 border-slate-200 dark:border-slate-700/60'
                                      } cursor-pointer transition hover:opacity-90 flex items-center justify-between px-3`}
                                      title={isSelected ? 'Selectie voor nieuwe afspraak' : `${coveringAppt.patientNaam || 'Geblokkeerd'} (${formatLocalTime(coveringAppt.starttijd)} - ${formatLocalTime(coveringAppt.eindtijd)})`}
                                    />
                                  );
                                }

                                return (
                                  <div
                                    key={subOffset}
                                    data-day-key={dayKey}
                                    data-slot-val={slotVal}
                                    onMouseDown={(e) => handleMouseDownSubSlot(currentDate, slotVal, false, e)}
                                    onMouseEnter={() => handleMouseEnterSubSlot(currentDate, slotVal)}
                                    onTouchStart={(e) => handleTouchStartSubSlot(currentDate, slotVal, false, e)}
                                    className={`flex-1 min-h-[38px] px-3 py-1.5 flex items-center justify-between text-[10px] transition cursor-pointer ${
                                      subOffset === 0 ? 'border-b border-dashed border-slate-200/40 dark:border-brand-800/30' : 'border-b border-slate-100 dark:border-brand-800/40'
                                    } ${
                                      isSelected
                                        ? 'bg-brand-500 text-white font-bold shadow-xs animate-pulse'
                                        : isBookingSlot
                                          ? 'hover:bg-emerald-200/70 dark:hover:bg-emerald-900/60 text-slate-500 dark:text-brand-300 group/sub'
                                          : 'hover:bg-slate-300/50 dark:hover:bg-brand-800/50 text-slate-400 dark:text-brand-500 group/sub'
                                    }`}
                                    title={
                                      isSelected
                                        ? `Selectie: ${formatSlotTimeString(Math.min(dragSelect.startSlot!, dragSelect.currentSlot!))} - ${formatSlotTimeString(Math.max(dragSelect.startSlot!, dragSelect.currentSlot!) + 0.5)}`
                                        : `Klik of sleep vanaf ${formatSlotTimeString(slotVal)}`
                                    }
                                  >
                                    {isSelected ? (
                                      <span className="font-bold flex items-center gap-1 truncate text-white">
                                        {isStart && <span>⬛</span>}
                                        <span>{formatSlotTimeString(slotVal)}</span>
                                      </span>
                                    ) : (
                                      <span className="opacity-0 group-hover/sub:opacity-100 transition text-[9px] font-semibold text-teal-600 dark:text-teal-400">
                                        + {formatSlotTimeString(slotVal)}
                                      </span>
                                    )}
                                  </div>
                                );
                              })}
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

      {/* Herbruikbare AfspraakInplannenModal (voor zowel Nieuw als Bewerken) */}
      <AfspraakInplannenModal
        isOpen={isBookModalOpen || !!editingAfspraak}
        onClose={() => {
          setIsBookModalOpen(false);
          setEditingAfspraak(null);
        }}
        onSuccess={fetchData}
        initialDate={bookModalPrefills.date}
        initialSlot={bookModalPrefills.slot}
        initialDurationMin={bookModalPrefills.durationMin}
        afspraakToEdit={editingAfspraak}
      />

      {/* Herbruikbare AfspraakDetailModal */}
      <AfspraakDetailModal
        afspraak={selectedAfspraak}
        onClose={() => setSelectedAfspraak(null)}
        onSuccess={fetchData}
        onEdit={(appt) => {
          setSelectedAfspraak(null);
          setEditingAfspraak(appt);
        }}
      />
    </div>
  );
};
