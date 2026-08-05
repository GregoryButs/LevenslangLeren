import React, { useState, useEffect } from 'react';
import { afspraakApi, settingsApi } from '../services/api';
import { Afspraak, SettingsData } from '../types';
import { 
  Calendar as CalendarIcon, Clock,
  RefreshCw, Loader2, Plus, Video 
} from 'lucide-react';
import { AfspraakDetailModal } from '../components/AfspraakDetailModal';
import { AfspraakInplannenModal } from '../components/AfspraakInplannenModal';
import { DateNavigator } from '../components/common/DateNavigator';
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

      const totalHours = (maxSlot + (1 / 12)) - minSlot;
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

  const isPracticeBookingSlot = (day: Date, slotVal: number): boolean => {
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
      const [sH, sM] = sStr.split(':').map(n => parseInt(n, 10) || 0);
      const [eH, eM] = eStr.split(':').map(n => parseInt(n, 10) || 0);
      const startVal = sH + sM / 60;
      const endVal = eH + eM / 60;
      return slotVal >= startVal && slotVal < endVal;
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

  // Determine working hours span dynamically
  let minHour = 8;
  let maxHour = 18;

  if (showFull24h) {
    minHour = 0;
    maxHour = 23;
  } else if (appointments.length > 0) {
    appointments.forEach(app => {
      if (app.isHeleDag) return;
      const startH = new Date(app.starttijd).getHours();
      let endH = new Date(app.eindtijd).getHours();
      const endM = new Date(app.eindtijd).getMinutes();
      if (endM > 0) {
        endH = endH;
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



  const getAllDayNotificationsForDay = (day: Date) => {
    return appointments.filter(app => {
      if (!app.isHeleDag) return false;
      const appStart = new Date(app.starttijd);
      return appStart.toDateString() === day.toDateString();
    });
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
          
          <DateNavigator
            value={`${currentDate.getFullYear()}-${String(currentDate.getMonth() + 1).padStart(2, '0')}-${String(currentDate.getDate()).padStart(2, '0')}`}
            onChange={(newDateStr) => {
              const parts = newDateStr.split('-');
              if (parts.length === 3) {
                setCurrentDate(new Date(parseInt(parts[0]), parseInt(parts[1]) - 1, parseInt(parts[2])));
              }
            }}
            onPrev={handlePrev}
            onNext={handleNext}
          />

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
                    <td className="p-0 border-b border-slate-100 dark:border-brand-800/40 align-top select-none h-[72px] max-h-[72px]">
                      <div className="flex flex-col h-[72px]">
                        <div className="h-[36px] px-3 py-0.5 text-xs sm:text-sm font-bold text-slate-800 dark:text-brand-100 border-b border-dashed border-slate-200/40 dark:border-brand-800/30 flex items-start pt-1 justify-between">
                          <span>{formatHourString(hour)}</span>
                        </div>
                        <div className="h-[36px] px-3 py-0.5 text-xs font-semibold text-slate-500 dark:text-brand-300 flex items-start pt-1 justify-between">
                          <span>{formatSlotTimeString(hour + 0.5)}</span>
                        </div>
                      </div>
                    </td>

                    {viewMode === 'week' ? (
                      getWeekDays(currentDate).map((day, dayIdx) => {
                        const dayKey = getDayKey(day);

                        // Find all appointments starting in this hour for this day
                        const hourStartAppts = appointments.filter(app => {
                          if (app.isHeleDag) return false;
                          const appStart = new Date(app.starttijd);
                          if (appStart.toDateString() !== day.toDateString()) return false;
                          return appStart.getHours() === hour;
                        });

                        return (
                          <td 
                            key={dayIdx}
                            className="p-0 border-l border-slate-100 dark:border-brand-800/40 align-top transition-all select-none relative h-[72px] max-h-[72px] overflow-visible"
                          >
                            {/* Grid Subslots */}
                            <div className="flex flex-col h-[72px] w-full">
                              {[0, 1/12, 2/12, 3/12, 4/12, 5/12, 6/12, 7/12, 8/12, 9/12, 10/12, 11/12].map((subOffset, subIdx) => {
                                const slotVal = Math.round((hour + subOffset) * 12) / 12;
                                const isBookingSlot = isPracticeBookingSlot(day, slotVal);
                                const isSelected = isSubSlotSelectedByDrag(day, slotVal);

                                return (
                                  <div
                                    key={subOffset}
                                    data-day-key={dayKey}
                                    data-slot-val={slotVal}
                                    onMouseDown={(e) => handleMouseDownSubSlot(day, slotVal, false, e)}
                                    onMouseEnter={() => handleMouseEnterSubSlot(day, slotVal)}
                                    onTouchStart={(e) => handleTouchStartSubSlot(day, slotVal, false, e)}
                                    className={`h-[6px] w-full flex items-center justify-between text-[9px] transition cursor-pointer box-border ${
                                      subIdx === 11 
                                        ? 'border-b border-solid border-slate-200/60 dark:border-brand-800/60' 
                                        : ''
                                    } ${
                                      isSelected
                                        ? 'bg-brand-500 text-white font-bold shadow-md z-20 relative -mb-[1px]'
                                        : isBookingSlot
                                          ? 'bg-teal-50/70 dark:bg-teal-950/35 border-l-2 border-teal-500 dark:border-teal-400/80 hover:bg-teal-100/80 dark:hover:bg-teal-900/50 text-slate-700 dark:text-brand-200 group/sub'
                                          : 'bg-slate-100/90 dark:bg-brand-950/95 border-l-2 border-transparent hover:bg-slate-200/90 dark:hover:bg-brand-900/60 text-slate-400 dark:text-brand-400 group/sub'
                                    }`}
                                    title={
                                      isSelected
                                        ? `Selectie: ${formatSlotTimeString(Math.min(dragSelect.startSlot!, dragSelect.currentSlot!))} - ${formatSlotTimeString(Math.max(dragSelect.startSlot!, dragSelect.currentSlot!) + (1 / 12))}`
                                        : `Klik of sleep vanaf ${formatSlotTimeString(slotVal)} op ${formatShortDutchDate(day)} ${isBookingSlot ? '' : '(buiten praktijkuren)'}`
                                    }
                                  >
                                    {isSelected && subIdx === 0 && (
                                      <span className="font-bold px-1 text-white text-[9px]">
                                        {formatSlotTimeString(slotVal)}
                                      </span>
                                    )}
                                  </div>
                                );
                              })}
                            </div>

                            {/* Appointment Cards Overlay starting in this hour */}
                            {hourStartAppts.map((coveringAppt, apptIdx) => {
                              const appStart = new Date(coveringAppt.starttijd);
                              const appEnd = new Date(coveringAppt.eindtijd);
                              const startM = appStart.getMinutes();
                              const durationMin = Math.max(15, Math.round((appEnd.getTime() - appStart.getTime()) / 60000));

                              const topPx = (startM / 60) * 72;
                              const heightPx = Math.max(20, (durationMin / 60) * 72);
                              const colorBorder = coveringAppt.status === 'Geannuleerd' ? '#94a3b8' : (coveringAppt.kleurcode || '#478d96');

                              const totalInHour = hourStartAppts.length;
                              const leftPercent = totalInHour > 1 ? (apptIdx / totalInHour) * 100 : 0;
                              const widthPercent = totalInHour > 1 ? (1 / totalInHour) * 100 : 100;

                              return (
                                <div 
                                  key={`appt-overlay-${coveringAppt.id}`} 
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    setSelectedAfspraak(coveringAppt);
                                  }}
                                  style={{ 
                                    position: 'absolute',
                                    top: `${topPx}px`,
                                    left: totalInHour > 1 ? `calc(${leftPercent}% + 2px)` : '2px',
                                    width: totalInHour > 1 ? `calc(${widthPercent}% - 6px)` : 'calc(100% - 14px)',
                                    height: `${heightPx}px`,
                                    borderLeftColor: colorBorder,
                                    zIndex: 10
                                  }}
                                  className={`p-1.5 border-l-4 rounded-lg shadow-xs hover:shadow-md transition text-left cursor-pointer overflow-hidden ${
                                    coveringAppt.status === 'Geannuleerd' 
                                      ? 'opacity-75 bg-slate-100 dark:bg-slate-800/90 border-dashed border-slate-300 dark:border-slate-600 text-slate-500 dark:text-slate-300' 
                                      : 'bg-white dark:bg-slate-900/95 text-slate-900 dark:text-white border border-slate-200/90 dark:border-slate-700/80 hover:border-teal-500 dark:hover:border-teal-400'
                                  }`}
                                  title={`${coveringAppt.patientNaam || 'Geblokkeerd'} (${formatLocalTime(coveringAppt.starttijd)} - ${formatLocalTime(coveringAppt.eindtijd)})`}
                                >
                                  <div className="flex justify-between items-center text-xs font-bold truncate leading-tight">
                                    <span className={`truncate ${coveringAppt.status === 'Geannuleerd' ? 'line-through text-slate-400 dark:text-slate-500' : 'text-slate-900 dark:text-white'}`}>
                                      {coveringAppt.patientNaam}
                                    </span>
                                    <span className="text-[10px] font-semibold text-slate-600 dark:text-slate-300 shrink-0 ml-1">
                                      {formatLocalTime(coveringAppt.starttijd)} - {formatLocalTime(coveringAppt.eindtijd)}
                                    </span>
                                  </div>
                                  {heightPx > 30 && (
                                    <div className="text-[10px] sm:text-xs text-teal-700 dark:text-teal-300 truncate font-semibold mt-0.5 flex items-center justify-between">
                                      <span className="truncate">{coveringAppt.afspraakTypeNaam}</span>
                                      {coveringAppt.googleMeetLink && <Video className="h-3.5 w-3.5 text-purple-600 dark:text-purple-400 shrink-0 inline ml-1" />}
                                    </div>
                                  )}
                                </div>
                              );
                            })}
                          </td>
                        );
                      })
                    ) : (
                      (() => {
                        const dayKey = getDayKey(currentDate);

                        const hourStartAppts = appointments.filter(app => {
                          if (app.isHeleDag) return false;
                          const appStart = new Date(app.starttijd);
                          if (appStart.toDateString() !== currentDate.toDateString()) return false;
                          return appStart.getHours() === hour;
                        });

                        return (
                          <td 
                            className="p-0 border-l border-slate-100 dark:border-brand-800/40 align-top transition-all select-none relative h-[72px] max-h-[72px] overflow-visible"
                          >
                            {/* Grid Subslots */}
                            <div className="flex flex-col h-[72px] w-full">
                              {[0, 1/12, 2/12, 3/12, 4/12, 5/12, 6/12, 7/12, 8/12, 9/12, 10/12, 11/12].map((subOffset, subIdx) => {
                                const slotVal = Math.round((hour + subOffset) * 12) / 12;
                                const isBookingSlot = isPracticeBookingSlot(currentDate, slotVal);
                                const isSelected = isSubSlotSelectedByDrag(currentDate, slotVal);

                                return (
                                  <div
                                    key={subOffset}
                                    data-day-key={dayKey}
                                    data-slot-val={slotVal}
                                    onMouseDown={(e) => handleMouseDownSubSlot(currentDate, slotVal, false, e)}
                                    onMouseEnter={() => handleMouseEnterSubSlot(currentDate, slotVal)}
                                    onTouchStart={(e) => handleTouchStartSubSlot(currentDate, slotVal, false, e)}
                                    className={`h-[6px] w-full flex items-center justify-between text-[9px] transition cursor-pointer box-border ${
                                      subIdx === 11 
                                        ? 'border-b border-solid border-slate-200/60 dark:border-brand-800/60' 
                                        : ''
                                    } ${
                                      isSelected
                                        ? 'bg-brand-500 text-white font-bold shadow-md z-20 relative -mb-[1px]'
                                        : isBookingSlot
                                          ? 'bg-teal-50/70 dark:bg-teal-950/35 border-l-2 border-teal-500 dark:border-teal-400/80 hover:bg-teal-100/80 dark:hover:bg-teal-900/50 text-slate-700 dark:text-brand-200 group/sub'
                                          : 'bg-slate-100/90 dark:bg-brand-950/95 border-l-2 border-transparent hover:bg-slate-200/90 dark:hover:bg-brand-900/60 text-slate-400 dark:text-brand-400 group/sub'
                                    }`}
                                    title={
                                      isSelected
                                        ? `Selectie: ${formatSlotTimeString(Math.min(dragSelect.startSlot!, dragSelect.currentSlot!))} - ${formatSlotTimeString(Math.max(dragSelect.startSlot!, dragSelect.currentSlot!) + (1 / 12))}`
                                        : `Klik of sleep vanaf ${formatSlotTimeString(slotVal)} op ${formatShortDutchDate(currentDate)} ${isBookingSlot ? '' : '(buiten praktijkuren)'}`
                                    }
                                  >
                                    {isSelected && subIdx === 0 && (
                                      <span className="font-bold px-1 text-white text-[9px]">
                                        {formatSlotTimeString(slotVal)}
                                      </span>
                                    )}
                                  </div>
                                );
                              })}
                            </div>

                            {/* Appointment Cards Overlay starting in this hour */}
                            {hourStartAppts.map((coveringAppt, apptIdx) => {
                              const appStart = new Date(coveringAppt.starttijd);
                              const appEnd = new Date(coveringAppt.eindtijd);
                              const startM = appStart.getMinutes();
                              const durationMin = Math.max(15, Math.round((appEnd.getTime() - appStart.getTime()) / 60000));

                              const topPx = (startM / 60) * 72;
                              const heightPx = Math.max(20, (durationMin / 60) * 72);
                              const colorBorder = coveringAppt.status === 'Geannuleerd' ? '#94a3b8' : (coveringAppt.kleurcode || '#478d96');

                              const totalInHour = hourStartAppts.length;
                              const leftPercent = totalInHour > 1 ? (apptIdx / totalInHour) * 100 : 0;
                              const widthPercent = totalInHour > 1 ? (1 / totalInHour) * 100 : 100;

                              return (
                                <div 
                                  key={`appt-overlay-${coveringAppt.id}`} 
                                  onClick={(e) => {
                                    e.stopPropagation();
                                    setSelectedAfspraak(coveringAppt);
                                  }}
                                  style={{ 
                                    position: 'absolute',
                                    top: `${topPx}px`,
                                    left: totalInHour > 1 ? `calc(${leftPercent}% + 2px)` : '2px',
                                    width: totalInHour > 1 ? `calc(${widthPercent}% - 6px)` : 'calc(100% - 14px)',
                                    height: `${heightPx}px`,
                                    borderLeftColor: colorBorder,
                                    zIndex: 10
                                  }}
                                  className={`p-2 border-l-4 rounded-lg shadow-xs hover:shadow-md transition text-left cursor-pointer overflow-hidden ${
                                    coveringAppt.status === 'Geannuleerd' 
                                      ? 'opacity-75 bg-slate-100 dark:bg-slate-800/90 border-dashed border-slate-300 dark:border-slate-600 text-slate-500 dark:text-slate-300' 
                                      : 'bg-white dark:bg-slate-900/95 text-slate-900 dark:text-white border border-slate-200/90 dark:border-slate-700/80 hover:border-teal-500 dark:hover:border-teal-400'
                                  }`}
                                  title={`${coveringAppt.patientNaam || 'Geblokkeerd'} (${formatLocalTime(coveringAppt.starttijd)} - ${formatLocalTime(coveringAppt.eindtijd)})`}
                                >
                                  <div className="flex justify-between items-center text-xs font-bold truncate leading-tight">
                                    <span className={`truncate ${coveringAppt.status === 'Geannuleerd' ? 'line-through text-slate-400 dark:text-slate-500' : 'text-slate-900 dark:text-white'}`}>
                                      {coveringAppt.patientNaam}
                                    </span>
                                    <span className="text-[10px] font-semibold text-slate-600 dark:text-slate-300 shrink-0 ml-1">
                                      {formatLocalTime(coveringAppt.starttijd)} - {formatLocalTime(coveringAppt.eindtijd)}
                                    </span>
                                  </div>
                                  {heightPx > 30 && (
                                    <div className="text-xs text-teal-700 dark:text-teal-300 truncate font-semibold mt-0.5 flex items-center justify-between">
                                      <span className="truncate">{coveringAppt.afspraakTypeNaam}</span>
                                      {coveringAppt.googleMeetLink && <Video className="h-3.5 w-3.5 text-purple-600 dark:text-purple-400 shrink-0 inline ml-1" />}
                                    </div>
                                  )}
                                </div>
                              );
                            })}
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
