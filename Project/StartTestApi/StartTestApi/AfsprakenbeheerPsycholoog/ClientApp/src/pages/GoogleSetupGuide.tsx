import React, { useState, useEffect } from 'react';
import { 
  MapPin, Video, Phone, 
  Settings, ShieldCheck, Link2, Clock, Sliders, Save,
  AlertCircle, Loader2, RefreshCw
} from 'lucide-react';
import { settingsApi, afspraakTypeApi } from '../services/api';
import { AfspraakType } from '../types';

interface SettingsData {
  googleCalendarId: string;
  
  maandagActief: boolean;
  maandagStart: string;
  maandagEinde: string;
  maandag2Actief: boolean;
  maandagStart2: string;
  maandagEinde2: string;

  dinsdagActief: boolean;
  dinsdagStart: string;
  dinsdagEinde: string;
  dinsdag2Actief: boolean;
  dinsdagStart2: string;
  dinsdagEinde2: string;

  woensdagActief: boolean;
  woensdagStart: string;
  woensdagEinde: string;
  woensdag2Actief: boolean;
  woensdagStart2: string;
  woensdagEinde2: string;

  donderdagActief: boolean;
  donderdagStart: string;
  donderdagEinde: string;
  donderdag2Actief: boolean;
  donderdagStart2: string;
  donderdagEinde2: string;

  vrijdagActief: boolean;
  vrijdagStart: string;
  vrijdagEinde: string;
  vrijdag2Actief: boolean;
  vrijdagStart2: string;
  vrijdagEinde2: string;

  zaterdagActief: boolean;
  zaterdagStart: string;
  zaterdagEinde: string;
  zaterdag2Actief: boolean;
  zaterdagStart2: string;
  zaterdagEinde2: string;

  zondagActief: boolean;
  zondagStart: string;
  zondagEinde: string;
  zondag2Actief: boolean;
  zondagStart2: string;
  zondagEinde2: string;

  slotDuurMinuten: number;
  bufferMinuten: number;
  locatiePraktijk: boolean;
  locatieGoogleMeet: boolean;
  locatieTelefoon: boolean;
  minimaalVoorafUren: number;
  maximaleToekomstDagen: number;
}

const normalizeSettings = (data: any): SettingsData => {
  const normalized = { ...data };
  const dayPrefixes = ['maandag', 'dinsdag', 'woensdag', 'donderdag', 'vrijdag', 'zaterdag', 'zondag'];
  dayPrefixes.forEach(prefix => {
    const isWeekend = prefix === 'zaterdag' || prefix === 'zondag';
    if (!normalized[`${prefix}Start`]) normalized[`${prefix}Start`] = isWeekend ? '10:00' : '09:00';
    if (!normalized[`${prefix}Einde`]) normalized[`${prefix}Einde`] = '12:00';
    if (!normalized[`${prefix}Start2`]) normalized[`${prefix}Start2`] = '13:00';
    if (!normalized[`${prefix}Einde2`]) normalized[`${prefix}Einde2`] = '17:00';
  });
  if (!normalized.googleCalendarId) normalized.googleCalendarId = 'primary';
  return normalized;
};

export const GoogleSetupGuide: React.FC = () => {
  const [settings, setSettings] = useState<SettingsData | null>(null);
  const [afspraakTypes, setAfspraakTypes] = useState<AfspraakType[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [syncing, setSyncing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchSettings = async () => {
      try {
        setLoading(true);
        const [data, typesData] = await Promise.all([
          settingsApi.get(),
          afspraakTypeApi.getAll().catch(() => [])
        ]);
        setSettings(normalizeSettings(data));
        setAfspraakTypes(typesData || []);
        setError(null);
      } catch (err) {
        console.error('Fout bij laden van instellingen:', err);
        setError('Laden van instellingen mislukt. Controleer uw verbinding.');
      } finally {
        setLoading(false);
      }
    };
    fetchSettings();
  }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;
    try {
      setSaving(true);
      const updated = await settingsApi.update(settings);
      setSettings(normalizeSettings(updated));
      alert('Agenda-instellingen succesvol opgeslagen en gesynchroniseerd!');
    } catch (err) {
      console.error('Fout bij opslaan:', err);
      alert('Opslaan mislukt. Controleer de velden.');
    } finally {
      setSaving(false);
    }
  };

  const handleSyncCalendar = async () => {
    try {
      setSyncing(true);
      const res = await settingsApi.syncCalendar();
      alert(res.message || 'Agenda succesvol gesynchroniseerd!');
    } catch (err: any) {
      console.error('Fout bij synchroniseren agenda:', err);
      const errMsg = err.response?.data?.error || 'Synchronisatie mislukt. Controleer de logs en of uw credentials correct zijn.';
      alert(errMsg);
    } finally {
      setSyncing(false);
    }
  };

  const handleCleanResync = async () => {
    if (!confirm('Weet u zeker dat u alle gesynchroniseerde afspraken wilt opschonen en 100% vers wilt ophalen uit Google Calendar?')) return;
    try {
      setSyncing(true);
      const res = await settingsApi.cleanResync();
      alert(res.message || 'Agenda opgeschoond en opnieuw gesynchroniseerd!');
    } catch (err: any) {
      console.error('Fout bij her-synchroniseren agenda:', err);
      const errMsg = err.response?.data?.error || 'Her-synchronisatie mislukt.';
      alert(errMsg);
    } finally {
      setSyncing(false);
    }
  };

  const updateField = (key: keyof SettingsData, value: any) => {
    if (!settings) return;
    setSettings({
      ...settings,
      [key]: value
    });
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center py-16">
        <Loader2 className="animate-spin h-10 w-10 text-brand-600" />
      </div>
    );
  }

  if (error || !settings) {
    return (
      <div className="p-6 bg-red-50 text-red-700 rounded-3xl border border-red-200 flex items-center space-x-2">
        <AlertCircle className="h-6 w-6" />
        <span>{error || 'Instellingen konden niet worden geladen.'}</span>
      </div>
    );
  }

  const days = [
    { 
      label: 'Maandag', 
      activeKey: 'maandagActief', startKey: 'maandagStart', endKey: 'maandagEinde',
      active2Key: 'maandag2Actief', start2Key: 'maandagStart2', end2Key: 'maandagEinde2' 
    },
    { 
      label: 'Dinsdag', 
      activeKey: 'dinsdagActief', startKey: 'dinsdagStart', endKey: 'dinsdagEinde',
      active2Key: 'dinsdag2Actief', start2Key: 'dinsdagStart2', end2Key: 'dinsdagEinde2' 
    },
    { 
      label: 'Woensdag', 
      activeKey: 'woensdagActief', startKey: 'woensdagStart', endKey: 'woensdagEinde',
      active2Key: 'woensdag2Actief', start2Key: 'woensdagStart2', end2Key: 'woensdagEinde2' 
    },
    { 
      label: 'Donderdag', 
      activeKey: 'donderdagActief', startKey: 'donderdagStart', endKey: 'donderdagEinde',
      active2Key: 'donderdag2Actief', start2Key: 'donderdagStart2', end2Key: 'donderdagEinde2' 
    },
    { 
      label: 'Vrijdag', 
      activeKey: 'vrijdagActief', startKey: 'vrijdagStart', endKey: 'vrijdagEinde',
      active2Key: 'vrijdag2Actief', start2Key: 'vrijdagStart2', end2Key: 'vrijdagEinde2' 
    },
    { 
      label: 'Zaterdag', 
      activeKey: 'zaterdagActief', startKey: 'zaterdagStart', endKey: 'zaterdagEinde',
      active2Key: 'zaterdag2Actief', start2Key: 'zaterdagStart2', end2Key: 'zaterdagEinde2' 
    },
    { 
      label: 'Zondag', 
      activeKey: 'zondagActief', startKey: 'zondagStart', endKey: 'zondagEinde',
      active2Key: 'zondag2Actief', start2Key: 'zondagStart2', end2Key: 'zondagEinde2' 
    },
  ] as const;

  return (
    <div className="space-y-8 max-w-7xl mx-auto p-2">
      {/* Intro Header */}
      <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm flex flex-col md:flex-row justify-between items-start md:items-center gap-4 transition-colors">
        <div>
          <h2 className="text-xl font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <Settings className="h-6 w-6 text-brand-600 dark:text-brand-400" />
            <span>Praktijk Planner Configuraties</span>
          </h2>
          <p className="text-slate-500 dark:text-brand-300 text-xs mt-1">
            Beheer uw werkuren, rusttijden, locatiekeuzes en boekingslimieten. Onze backend synchroniseert alle vrije plekken direct met Google Calendar.
          </p>
        </div>
        <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 w-full md:w-auto">
          <button
            type="button"
            onClick={handleSyncCalendar}
            disabled={syncing}
            className="flex items-center justify-center space-x-2 bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white font-bold py-2.5 px-4 rounded-xl transition disabled:opacity-50 text-xs"
          >
            {syncing ? <Loader2 className="animate-spin h-4 w-4" /> : <RefreshCw className="h-4 w-4" />}
            <span>Snel synchroniseren</span>
          </button>
          <button
            type="button"
            onClick={handleCleanResync}
            disabled={syncing}
            className="flex items-center justify-center space-x-2 bg-amber-50 dark:bg-amber-950/60 hover:bg-amber-100 border border-amber-200 dark:border-amber-800 text-amber-800 dark:text-amber-200 font-bold py-2.5 px-4 rounded-xl transition disabled:opacity-50 text-xs"
            title="Wist oude gesynchroniseerde entries en haalt alles 100% vers op uit Google"
          >
            {syncing ? <Loader2 className="animate-spin h-4 w-4" /> : <RefreshCw className="h-4 w-4 text-amber-600" />}
            <span>Volledige Her-sync</span>
          </button>
          <button
            onClick={handleSave}
            disabled={saving}
            className="flex items-center justify-center space-x-2 bg-brand-600 hover:bg-brand-700 text-white font-bold py-2.5 px-5 rounded-xl transition shadow-lg shadow-brand-500/10 disabled:opacity-50 text-xs"
          >
            {saving ? <Loader2 className="animate-spin h-4 w-4" /> : <Save className="h-4 w-4" />}
            <span>Instellingen Opslaan</span>
          </button>
        </div>
      </div>

      <form onSubmit={handleSave} className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Left Column: Work Hours per day */}
        <div className="lg:col-span-2 space-y-6">
          <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-6 transition-colors">
            <h3 className="font-bold text-slate-800 dark:text-white text-sm uppercase tracking-wider flex items-center space-x-2 pb-2 border-b border-slate-100 dark:border-brand-800/40">
              <Clock className="h-4 w-4 text-brand-500 dark:text-brand-400" />
              <span>Wekelijkse Werkuren per Dag</span>
            </h3>

            <div className="space-y-4">
              {days.map((day) => {
                const isActive1 = settings[day.activeKey];
                const isActive2 = settings[day.active2Key];
                return (
                  <div 
                    key={day.label}
                    className={`p-5 rounded-2xl border transition-all flex flex-col md:flex-row md:items-center justify-between gap-4 ${
                      isActive1 || isActive2
                        ? 'border-brand-100 dark:border-brand-800/60 bg-brand-50/5 dark:bg-brand-950/60' 
                        : 'border-slate-100 dark:border-brand-900 bg-slate-50/50 dark:bg-brand-950/30 opacity-70'
                    }`}
                  >
                    <span className="font-bold text-sm text-slate-700 dark:text-white min-w-[100px]">
                      {day.label}
                    </span>

                    <div className="flex flex-col gap-3 w-full md:w-auto">
                      {/* Interval 1 */}
                      <div className="flex items-center justify-between md:justify-start gap-4">
                        <div className="flex items-center space-x-2">
                          <input
                            type="checkbox"
                            id={`${day.label}-1`}
                            checked={isActive1}
                            onChange={(e) => updateField(day.activeKey, e.target.checked)}
                            className="h-4.5 w-4.5 text-brand-600 focus:ring-brand-500 border-slate-300 dark:border-brand-700 rounded cursor-pointer"
                          />
                          <label htmlFor={`${day.label}-1`} className="text-xs font-bold text-slate-500 dark:text-brand-200 cursor-pointer select-none">
                            Deel 1 (Voormiddag)
                          </label>
                        </div>
                        
                        {isActive1 ? (
                          <div className="flex items-center space-x-2">
                            <input
                              type="time"
                              value={settings[day.startKey]}
                              onChange={(e) => updateField(day.startKey, e.target.value)}
                              className="bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-700 py-1 px-2.5 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none focus:ring-2 focus:ring-brand-500"
                            />
                            <span className="text-slate-400 dark:text-brand-400 text-xs font-semibold">tot</span>
                            <input
                              type="time"
                              value={settings[day.endKey]}
                              onChange={(e) => updateField(day.endKey, e.target.value)}
                              className="bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-700 py-1 px-2.5 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none focus:ring-2 focus:ring-brand-500"
                            />
                          </div>
                        ) : (
                          <span className="text-slate-400 dark:text-brand-400 text-xs font-semibold">Niet actief</span>
                        )}
                      </div>

                      {/* Interval 2 */}
                      <div className="flex items-center justify-between md:justify-start gap-4">
                        <div className="flex items-center space-x-2">
                          <input
                            type="checkbox"
                            id={`${day.label}-2`}
                            checked={isActive2}
                            onChange={(e) => updateField(day.active2Key, e.target.checked)}
                            className="h-4.5 w-4.5 text-brand-600 focus:ring-brand-500 border-slate-300 dark:border-brand-700 rounded cursor-pointer"
                          />
                          <label htmlFor={`${day.label}-2`} className="text-xs font-bold text-slate-500 dark:text-brand-200 cursor-pointer select-none">
                            Deel 2 (Namiddag)
                          </label>
                        </div>

                        {isActive2 ? (
                          <div className="flex items-center space-x-2">
                            <input
                              type="time"
                              value={settings[day.start2Key]}
                              onChange={(e) => updateField(day.start2Key, e.target.value)}
                              className="bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-700 py-1 px-2.5 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none focus:ring-2 focus:ring-brand-500"
                            />
                            <span className="text-slate-400 dark:text-brand-400 text-xs font-semibold">tot</span>
                            <input
                              type="time"
                              value={settings[day.end2Key]}
                              onChange={(e) => updateField(day.end2Key, e.target.value)}
                              className="bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-700 py-1 px-2.5 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none focus:ring-2 focus:ring-brand-500"
                            />
                          </div>
                        ) : (
                          <span className="text-slate-400 dark:text-brand-400 text-xs font-semibold">Niet actief</span>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>

        {/* Right Column: Settings, Horizons & Locations */}
        <div className="space-y-6">
          
          {/* Location Choices */}
          <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-4 transition-colors">
            <h3 className="font-bold text-slate-800 dark:text-white text-sm uppercase tracking-wider flex items-center space-x-2 pb-2 border-b border-slate-100 dark:border-brand-800/40">
              <MapPin className="h-4 w-4 text-brand-500 dark:text-brand-400" />
              <span>Aangeboden Locaties</span>
            </h3>

            <div className="space-y-3 pt-1">
              <div className="flex items-start space-x-3">
                <input
                  type="checkbox"
                  id="locPraktijk"
                  checked={settings.locatiePraktijk}
                  onChange={(e) => updateField('locatiePraktijk', e.target.checked)}
                  className="mt-0.5 h-4.5 w-4.5 text-brand-600 focus:ring-brand-500 border-slate-300 dark:border-brand-700 rounded cursor-pointer"
                />
                <div className="text-xs">
                  <label htmlFor="locPraktijk" className="font-bold text-slate-700 dark:text-white cursor-pointer flex items-center space-x-1">
                    <MapPin className="h-3.5 w-3.5 text-slate-500 dark:text-brand-300 inline" />
                    <span>Fysieke Praktijk</span>
                  </label>
                  <p className="text-[10px] text-slate-400 dark:text-brand-300 mt-0.5">Sessie in-person op de praktijklocatie.</p>
                </div>
              </div>

              <div className="flex items-start space-x-3">
                <input
                  type="checkbox"
                  id="locMeet"
                  checked={settings.locatieGoogleMeet}
                  onChange={(e) => updateField('locatieGoogleMeet', e.target.checked)}
                  className="mt-0.5 h-4.5 w-4.5 text-brand-600 focus:ring-brand-500 border-slate-300 dark:border-brand-700 rounded cursor-pointer"
                />
                <div className="text-xs">
                  <label htmlFor="locMeet" className="font-bold text-slate-700 dark:text-white cursor-pointer flex items-center space-x-1">
                    <Video className="h-3.5 w-3.5 text-slate-500 dark:text-brand-300 inline" />
                    <span>Online (Google Meet)</span>
                  </label>
                  <p className="text-[10px] text-slate-400 dark:text-brand-300 mt-0.5">Automatische Meet-link aanmaken en meesturen.</p>
                </div>
              </div>

              <div className="flex items-start space-x-3">
                <input
                  type="checkbox"
                  id="locPhone"
                  checked={settings.locatieTelefoon}
                  onChange={(e) => updateField('locatieTelefoon', e.target.checked)}
                  className="mt-0.5 h-4.5 w-4.5 text-brand-600 focus:ring-brand-500 border-slate-300 dark:border-brand-700 rounded cursor-pointer"
                />
                <div className="text-xs">
                  <label htmlFor="locPhone" className="font-bold text-slate-700 dark:text-white cursor-pointer flex items-center space-x-1">
                    <Phone className="h-3.5 w-3.5 text-slate-500 dark:text-brand-300 inline" />
                    <span>Telefonisch consult</span>
                  </label>
                  <p className="text-[10px] text-slate-400 dark:text-brand-300 mt-0.5">Patiënt wordt op het gekozen tijdstip gebeld.</p>
                </div>
              </div>
            </div>
          </div>

          {/* Time settings (Buffer / Duration) */}
          <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-4 transition-colors">
            <h3 className="font-bold text-slate-800 dark:text-white text-sm uppercase tracking-wider flex items-center space-x-2 pb-2 border-b border-slate-100 dark:border-brand-800/40">
              <Sliders className="h-4 w-4 text-brand-500 dark:text-brand-400" />
              <span>Sessie & Rusttijd</span>
            </h3>

            <div className="space-y-4 pt-1">
              <div className="space-y-1">
                <label className="block text-[10px] font-bold text-slate-400 dark:text-brand-300 uppercase tracking-wider">
                  Sessieduur (Minuten)
                </label>
                <select
                  value={settings.slotDuurMinuten}
                  onChange={(e) => updateField('slotDuurMinuten', Number(e.target.value))}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none"
                >
                  {afspraakTypes.length > 0 && (
                    <optgroup label="Afspraaktypes uit uw beheer">
                      {afspraakTypes.map((type) => (
                        <option key={`type-${type.id}`} value={type.standaardDuurMinuten}>
                          {type.standaardDuurMinuten} minuten — {type.naam}
                        </option>
                      ))}
                    </optgroup>
                  )}
                  <optgroup label="Standaardduur opties">
                    <option value={30}>30 minuten</option>
                    <option value={45}>45 minuten</option>
                    <option value={50}>50 minuten</option>
                    <option value={60}>60 minuten</option>
                    <option value={90}>90 minuten</option>
                  </optgroup>
                </select>

                {afspraakTypes.length > 0 && (
                  <div className="mt-2.5 p-2.5 bg-slate-50 dark:bg-brand-950 border border-slate-100 dark:border-brand-800 rounded-xl space-y-1.5">
                    <span className="text-[10px] font-bold text-slate-400 dark:text-brand-300 uppercase tracking-wider block">
                      Geconfigureerde types & duur:
                    </span>
                    <div className="flex flex-wrap gap-1.5">
                      {afspraakTypes.map((t) => (
                        <span 
                          key={t.id} 
                          className="inline-flex items-center space-x-1.5 px-2 py-1 rounded-lg text-[11px] bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-700 font-medium text-slate-700 dark:text-brand-100 shadow-2xs"
                        >
                          <span className="h-2 w-2 rounded-full flex-shrink-0" style={{ backgroundColor: t.kleurcode }} />
                          <span>{t.naam}:</span>
                          <strong className="text-slate-900 dark:text-white">{t.standaardDuurMinuten}m</strong>
                        </span>
                      ))}
                    </div>
                  </div>
                )}
              </div>

              <div className="space-y-1">
                <label className="block text-[10px] font-bold text-slate-400 dark:text-brand-300 uppercase tracking-wider">
                  Rusttijd tussen sessies (Buffer)
                </label>
                <select
                  value={settings.bufferMinuten}
                  onChange={(e) => updateField('bufferMinuten', Number(e.target.value))}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none"
                >
                  <option value={0}>Geen buffer</option>
                  <option value={5}>5 minuten</option>
                  <option value={10}>10 minuten</option>
                  <option value={15}>15 minuten</option>
                  <option value={30}>30 minuten</option>
                </select>
              </div>
            </div>
          </div>

          {/* Limits Window */}
          <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-4 transition-colors">
            <h3 className="font-bold text-slate-800 dark:text-white text-sm uppercase tracking-wider flex items-center space-x-2 pb-2 border-b border-slate-100 dark:border-brand-800/40">
              <ShieldCheck className="h-4 w-4 text-brand-500 dark:text-brand-400" />
              <span>Boekingslimieten</span>
            </h3>

            <div className="space-y-4 pt-1">
              <div className="space-y-1">
                <label className="block text-[10px] font-bold text-slate-400 dark:text-brand-300 uppercase tracking-wider">
                  Minimaal vooraf boeken (Uren)
                </label>
                <input
                  type="number"
                  min={0}
                  value={settings.minimaalVoorafUren}
                  onChange={(e) => updateField('minimaalVoorafUren', Number(e.target.value))}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none"
                />
                <p className="text-[9px] text-slate-400 dark:text-brand-300">Voorkomt onverwachte last-minute boekingen.</p>
              </div>

              <div className="space-y-1">
                <label className="block text-[10px] font-bold text-slate-400 dark:text-brand-300 uppercase tracking-wider">
                  Maximaal in de toekomst boeken (Dagen)
                </label>
                <input
                  type="number"
                  min={1}
                  value={settings.maximaleToekomstDagen}
                  onChange={(e) => updateField('maximaleToekomstDagen', Number(e.target.value))}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none"
                />
                <p className="text-[9px] text-slate-400 dark:text-brand-300">Houdt de lange-termijn agenda overzichtelijk.</p>
              </div>
            </div>
          </div>

          {/* Calendar ID Configuration */}
          <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-4 transition-colors">
            <h3 className="font-bold text-slate-800 dark:text-white text-sm uppercase tracking-wider flex items-center space-x-2 pb-2 border-b border-slate-100 dark:border-brand-800/40">
              <Link2 className="h-4 w-4 text-brand-500 dark:text-brand-400" />
              <span>Google Agenda ID</span>
            </h3>

            <div className="space-y-1">
              <input
                type="text"
                value={settings.googleCalendarId}
                onChange={(e) => updateField('googleCalendarId', e.target.value)}
                className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-slate-700 dark:text-white text-xs font-bold focus:outline-none"
              />
              <p className="text-[9px] text-slate-400 dark:text-brand-300">Gebruik "primary" of de e-mail van uw professionele agenda.</p>
            </div>
          </div>

        </div>

      </form>
    </div>
  );
};
