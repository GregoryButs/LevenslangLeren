import React, { useState, useEffect } from 'react';
import { patientPortaalApi, settingsApi, afspraakTypeApi } from '../services/api';
import { AfspraakType, Tijdslot } from '../types';
import { 
  Clock, MapPin, Video, Phone, CheckCircle, 
  ArrowLeft, ArrowRight, Loader2, CalendarDays, 
  AlertCircle, Briefcase, Calendar, Sparkles,
  ChevronLeft, ChevronRight
} from 'lucide-react';
import { extractErrorMessage } from '../utils/errorUtils';
import { InfoTooltip } from './common/InfoTooltip';

interface BookingWizardProps {
  onBookingSuccess: () => void;
}

export const BookingWizard: React.FC<BookingWizardProps> = ({ onBookingSuccess }) => {
  const [step, setStep] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Loaded Options
  const [availableTypes, setAvailableTypes] = useState<AfspraakType[]>([]);
  const [settings, setSettings] = useState<{
    locatiePraktijk: boolean;
    locatieGoogleMeet: boolean;
    locatieTelefoon: boolean;
  } | null>(null);
  const [timeSlots, setTimeSlots] = useState<Tijdslot[]>([]);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [firstNextSlot, setFirstNextSlot] = useState<{ date: string; time: string; displayDate: string } | null>(null);

  // User Selections
  const [selectedType, setSelectedType] = useState<AfspraakType | null>(null);
  const [selectedLocation, setSelectedLocation] = useState<string>('Praktijk');
  const [selectedDate, setSelectedDate] = useState<string>(() => {
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return tomorrow.toISOString().split('T')[0];
  });
  const [selectedSlot, setSelectedSlot] = useState<Tijdslot | null>(null);
  const [opmerkingen, setOpmerkingen] = useState('');
  const [bookingInProgress, setBookingInProgress] = useState(false);
  const [bookingSuccess, setBookingSuccess] = useState(false);

  // Date Navigation Helpers
  const navigateDays = (deltaDays: number) => {
    const curr = new Date(selectedDate);
    curr.setDate(curr.getDate() + deltaDays);
    setSelectedDate(curr.toISOString().split('T')[0]);
  };

  // Load initial options: types and settings
  useEffect(() => {
    const fetchInitialData = async () => {
      try {
        setLoading(true);
        const [typesData, settingsData] = await Promise.all([
          afspraakTypeApi.getAll(),
          settingsApi.get()
        ]);
        
        // Patienten kunnen uitsluitend 'Intake' en 'Consultatie'/'Therapie' boeken (geen Praktijkhuis of Crisis)
        const patientTypes = typesData.filter(t => {
          if (!t.vereistPatient) return false;
          const lowerName = t.naam.toLowerCase();
          if (lowerName.includes('praktijkhuis') || lowerName.includes('crisis')) return false;
          return lowerName.includes('intake') || lowerName.includes('consult') || lowerName.includes('therapie');
        });
        setAvailableTypes(patientTypes);

        // Voorkeur op ID (ID 2 = Consultatie/Therapie, ID 1 = Intake) of naam-match
        const defaultType = patientTypes.find(t => t.id === 2) ||
                            patientTypes.find(t => t.naam.trim().toLowerCase() === 'consultatie') ||
                            patientTypes.find(t => t.naam.trim().toLowerCase() === 'therapie') || 
                            patientTypes.find(t => t.naam.toLowerCase().includes('consult')) ||
                            patientTypes.find(t => t.naam.toLowerCase().includes('therapie')) || 
                            patientTypes.find(t => t.id === 1) ||
                            patientTypes[0];
        
        setSelectedType(defaultType || null);
        setSettings(settingsData);
        
        // Determine default location
        if (settingsData.locatiePraktijk) setSelectedLocation('Praktijk');
        else if (settingsData.locatieGoogleMeet) setSelectedLocation('GoogleMeet');
        else if (settingsData.locatieTelefoon) setSelectedLocation('Telefoon');
        
        setError(null);
      } catch (err) {
        console.error('Fout bij inladen boekingsgegevens:', err);
        setError(extractErrorMessage(err, 'Kon initialisatiegegevens niet laden. Probeer het later opnieuw.'));
      } finally {
        setLoading(false);
      }
    };
    
    fetchInitialData();
  }, []);

  // Fetch slots whenever selectedDate or step changes
  useEffect(() => {
    const fetchSlots = async () => {
      if (!selectedDate) return;
      try {
        setLoadingSlots(true);
        setSelectedSlot(null);
        const data = await patientPortaalApi.getBoeken(selectedDate);
        if (data && data.dagOverzicht) {
          setTimeSlots(data.dagOverzicht.tijdsloten || []);
          if (data.dagOverzicht.eerstVolgendeVrijeSlotDatumStr && data.dagOverzicht.eerstVolgendeVrijeSlotTijd) {
            const dStr = data.dagOverzicht.eerstVolgendeVrijeSlotDatumStr;
            const rawT = data.dagOverzicht.eerstVolgendeVrijeSlotTijd;
            let tStr = '09:00';
            if (typeof rawT === 'string') {
              if (rawT.includes('T')) {
                tStr = rawT.split('T')[1].substring(0, 5);
              } else if (rawT.includes(':')) {
                tStr = rawT.substring(0, 5);
              }
            }
            const dParts = dStr.split('-');
            const dObj = new Date(parseInt(dParts[0]), parseInt(dParts[1]) - 1, parseInt(dParts[2]));
            const displayD = dObj.toLocaleDateString('nl-NL', { weekday: 'long', day: 'numeric', month: 'long' });
            setFirstNextSlot({ date: dStr, time: tStr, displayDate: displayD });
          }
        }
      } catch (err) {
        console.error('Fout bij ophalen van slots:', err);
        setTimeSlots([]);
      } finally {
        setLoadingSlots(false);
      }
    };

    if (step === 3) {
      fetchSlots();
    }
  }, [selectedDate, step]);

  const handleNext = () => {
    if (step < 5) setStep(step + 1);
  };

  const handleBack = () => {
    if (step > 1) setStep(step - 1);
  };

  const handleConfirmBooking = async () => {
    if (!selectedSlot || !selectedType) return;
    try {
      setBookingInProgress(true);
      await patientPortaalApi.book({
        gekozeTijdslot: selectedSlot.tijd,
        opmerkingen,
        locatieType: selectedLocation,
        datum: selectedDate
      });
      setBookingSuccess(true);
      setStep(5);
      setTimeout(() => {
        onBookingSuccess();
        // Reset wizard
        setStep(1);
        setSelectedType(null);
        setSelectedSlot(null);
        setOpmerkingen('');
        setBookingSuccess(false);
      }, 4000);
    } catch (err: any) {
      console.error(err);
      alert(extractErrorMessage(err, 'Boeken mislukt. Kies een ander moment.'));
    } finally {
      setBookingInProgress(false);
    }
  };

  const formatTime = (timeStr?: string) => {
    if (!timeStr) return '';
    return timeStr.substring(0, 5); // '09:00:00' -> '09:00'
  };

  const formatDateDutch = (dateStr: string) => {
    const d = new Date(dateStr);
    return d.toLocaleDateString('nl-NL', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });
  };

  // Steps headers list
  const stepsList = [
    { num: 1, label: 'Behandeling' },
    { num: 2, label: 'Locatie' },
    { num: 3, label: 'Datum & Tijd' },
    { num: 4, label: 'Details' },
    { num: 5, label: 'Overzicht' }
  ];

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center p-12 bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm min-h-[400px]">
        <Loader2 className="animate-spin h-10 w-10 text-brand-600 dark:text-brand-400 mb-4" />
        <p className="text-slate-500 dark:text-brand-300 font-medium">Boekingsgegevens laden...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="p-6 bg-red-50 dark:bg-red-950/60 text-red-800 dark:text-red-300 rounded-3xl border border-red-100 dark:border-red-900/60 shadow-sm flex items-start space-x-3">
        <AlertCircle className="h-5 w-5 text-red-600 dark:text-red-400 shrink-0 mt-0.5" />
        <div>
          <h4 className="font-bold">Er is een fout opgetreden</h4>
          <p className="text-sm mt-1">{error}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm overflow-hidden transition-all duration-300">
      
      {/* Wizard Header Progress Bar */}
      <div className="bg-slate-50/50 dark:bg-brand-950/50 border-b border-slate-100 dark:border-brand-800/40 px-6 py-4">
        <div className="flex items-center justify-between max-w-xl mx-auto">
          {stepsList.map((s, idx) => (
            <React.Fragment key={s.num}>
              <button 
                disabled={s.num > step && !bookingSuccess}
                onClick={() => !bookingSuccess && setStep(s.num)}
                className="flex flex-col items-center space-y-1 group"
              >
                <div className={`h-8 w-8 rounded-full flex items-center justify-center text-xs font-bold transition-all duration-200 ${
                  step === s.num
                    ? 'bg-brand-500 text-white ring-4 ring-brand-100 dark:ring-brand-800'
                    : step > s.num
                      ? 'bg-brand-100 dark:bg-brand-800 text-brand-700 dark:text-brand-200'
                      : 'bg-slate-100 dark:bg-brand-950 text-slate-400 dark:text-brand-400'
                }`}>
                  {step > s.num && !bookingSuccess ? <CheckCircle className="h-5 w-5" /> : s.num}
                </div>
                <span className={`text-[10px] font-bold tracking-tight transition-colors duration-200 ${
                  step === s.num ? 'text-brand-600 dark:text-brand-300' : 'text-slate-400 dark:text-brand-400 group-hover:text-slate-600 dark:group-hover:text-brand-200'
                }`}>{s.label}</span>
              </button>
              {idx < stepsList.length - 1 && (
                <div className={`flex-1 h-0.5 mx-2 transition-all duration-300 ${
                  step > s.num ? 'bg-brand-300 dark:bg-brand-700' : 'bg-slate-100 dark:bg-brand-950'
                }`} />
              )}
            </React.Fragment>
          ))}
        </div>
      </div>

      {/* Wizard Body */}
      <div className="p-6 md:p-8 min-h-[300px]">
        {bookingSuccess ? (
          <div className="flex flex-col items-center justify-center py-12 text-center space-y-4 animate-fadeIn">
            <div className="h-16 w-16 bg-emerald-100 dark:bg-emerald-950 text-emerald-600 dark:text-emerald-400 rounded-full flex items-center justify-center shadow-inner">
              <CheckCircle className="h-10 w-10" />
            </div>
            <h3 className="text-xl font-bold text-slate-800 dark:text-white">Afspraak Succesvol Gereserveerd!</h3>
            <p className="text-slate-500 dark:text-brand-300 max-w-md text-sm">
              Je afspraak is bevestigd en toegevoegd aan je portaal. Er is een bevestiging per e-mail naar je verzonden.
            </p>
            <div className="p-4 bg-slate-50 dark:bg-brand-950/60 rounded-2xl border border-slate-100 dark:border-brand-800/40 text-left text-xs space-y-1.5 w-full max-w-sm">
              <p className="text-slate-400 dark:text-brand-400 font-bold uppercase tracking-wider text-[10px]">Overzicht</p>
              <p className="text-slate-700 dark:text-brand-100 font-semibold"><span className="text-slate-400 dark:text-brand-300 font-normal">Type:</span> {selectedType?.naam}</p>
              <p className="text-slate-700 dark:text-brand-100 font-semibold"><span className="text-slate-400 dark:text-brand-300 font-normal">Wanneer:</span> {formatDateDutch(selectedDate)} om {selectedSlot ? formatTime(selectedSlot.starttijd) : ''} uur</p>
              <p className="text-slate-700 dark:text-brand-100 font-semibold"><span className="text-slate-400 dark:text-brand-300 font-normal">Locatie:</span> {selectedLocation === 'GoogleMeet' ? 'Google Meet (online)' : selectedLocation === 'Telefoon' ? 'Telefonisch' : 'Op de praktijk'}</p>
            </div>
          </div>
        ) : (
          <>
            {/* STEP 1: SERVICE SELECTION */}
            {step === 1 && (
              <div className="space-y-4">
                <div>
                  <h3 className="text-lg font-bold text-slate-800 dark:text-white">Behandeling</h3>
                  <p className="text-xs text-slate-600 dark:text-slate-200">Selecteer het gewenste type consult voor jouw afspraak.</p>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 max-w-2xl">
                  {availableTypes.length > 0 ? (
                    availableTypes.map((type) => {
                      const isSelected = selectedType?.id === type.id;
                      const isIntake = type.naam.toLowerCase().includes('intake');
                      return (
                        <button
                          key={type.id}
                          type="button"
                          onClick={() => setSelectedType(type)}
                          className={`p-5 rounded-2xl border text-left transition-all flex items-start space-x-4 ${
                            isSelected
                              ? 'border-brand-400 bg-brand-900/90 ring-2 ring-brand-400/30 shadow-md'
                              : 'border-slate-300 dark:border-brand-700/60 bg-white dark:bg-brand-900/40 hover:bg-slate-50 dark:hover:bg-brand-800/60'
                          }`}
                        >
                          <div 
                            className="h-10 w-10 rounded-xl flex items-center justify-center text-white shrink-0 shadow-md transition-transform" 
                            style={{ backgroundColor: type.kleurcode || '#478d96' }}
                          >
                            <Briefcase className="h-5 w-5" />
                          </div>
                          <div className="space-y-1.5 flex-1 min-w-0">
                            <div className="flex items-center justify-between gap-1 flex-wrap">
                              <h4 className="font-bold text-slate-900 dark:text-white text-sm flex items-center gap-1">
                                <span>{type.naam}</span>
                                {isIntake && (
                                  <InfoTooltip content="Enkel bedoeld voor nieuwe patiënten bij een 1e consult" />
                                )}
                              </h4>
                              {isSelected && (
                                <span className="text-[10px] font-bold uppercase tracking-wider text-emerald-800 dark:text-emerald-300 bg-emerald-100 dark:bg-emerald-950/80 px-2 py-0.5 rounded-full shrink-0 border border-emerald-300/40">
                                  Geselecteerd
                                </span>
                              )}
                            </div>
                            <div className="flex items-center space-x-1.5 text-xs text-slate-600 dark:text-slate-200">
                              <Clock className="h-3.5 w-3.5 text-brand-500 dark:text-brand-300 shrink-0" />
                              <span>{type.standaardDuurMinuten} minuten</span>
                            </div>
                            <p className="text-xs text-slate-600 dark:text-slate-300 mt-1 leading-relaxed">
                              {isIntake 
                                ? 'Eerste kennismakingsgesprek (enkel voor 1e gesprek).' 
                                : 'Individueel consultatie- en therapiegesprek.'}
                            </p>
                          </div>
                        </button>
                      );
                    })
                  ) : (
                    <div className="p-5 rounded-2xl border border-brand-500 bg-brand-50/30 dark:bg-brand-950/60 ring-2 ring-brand-500/10 shadow-sm flex items-start space-x-4">
                      <div className="h-10 w-10 rounded-xl flex items-center justify-center text-white shrink-0 shadow-md" style={{ backgroundColor: selectedType?.kleurcode || '#478d96' }}>
                        <Briefcase className="h-5 w-5" />
                      </div>
                      <div className="space-y-1 flex-1">
                        <div className="flex items-center justify-between">
                          <h4 className="font-bold text-slate-800 dark:text-white text-sm">{selectedType?.naam || 'Consultatie'}</h4>
                          <span className="text-[10px] font-bold uppercase tracking-wider text-brand-700 dark:text-brand-200 bg-brand-100 dark:bg-brand-800 px-2 py-0.5 rounded-full">Standaard</span>
                        </div>
                        <div className="flex items-center space-x-1.5 text-xs text-slate-600 dark:text-slate-200">
                          <Clock className="h-3.5 w-3.5 text-brand-600 dark:text-brand-400" />
                          <span>{selectedType?.standaardDuurMinuten || 50} minuten</span>
                        </div>
                        <p className="text-xs text-slate-600 dark:text-slate-300 mt-1">Individueel consultatiegesprek.</p>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* STEP 2: LOCATION SELECTION */}
            {step === 2 && (
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-bold text-slate-800 dark:text-white">Kies een locatie</h3>
                  <p className="text-xs text-slate-400 dark:text-brand-300">Hoe wil je dat het consult plaatsvindt?</p>
                </div>
                
                {/* Praktijk Ninove Adreskaart */}
                <div className="bg-brand-50/50 dark:bg-brand-950/60 border border-brand-100/80 dark:border-brand-800/40 rounded-2xl p-4 flex items-start space-x-3.5 text-xs text-brand-900 dark:text-brand-100 shadow-sm">
                  <div className="h-10 w-10 bg-brand-500 text-white rounded-xl flex items-center justify-center shrink-0 shadow-sm">
                    <MapPin className="h-5 w-5" />
                  </div>
                  <div>
                    <span className="font-bold block text-sm text-brand-950 dark:text-white">Praktijk Ninove — De Verstandhouding</span>
                    <span className="text-slate-700 dark:text-brand-200 font-medium block mt-0.5">Brakelsesteenweg 559a bus 1, 9400 Ninove</span>
                    <span className="text-slate-500 dark:text-brand-300 italic block text-[11px] mt-0.5">(Bovenverdieping, ingang via trap rechts om de hoek)</span>
                  </div>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  {settings?.locatiePraktijk && (
                    <button
                      onClick={() => { setSelectedLocation('Praktijk'); handleNext(); }}
                      className={`p-5 rounded-2xl border transition-all duration-200 flex flex-col items-center text-center space-y-3 ${
                        selectedLocation === 'Praktijk'
                          ? 'border-brand-500 bg-brand-50/30 dark:bg-brand-950/80 ring-2 ring-brand-500/10'
                          : 'border-slate-100 dark:border-brand-800/40 bg-white dark:bg-brand-950/40 hover:border-slate-300 dark:hover:border-brand-700'
                      }`}
                    >
                      <div className={`h-12 w-12 rounded-full flex items-center justify-center ${selectedLocation === 'Praktijk' ? 'bg-brand-500 text-white' : 'bg-slate-50 dark:bg-brand-800 text-slate-600 dark:text-brand-200'}`}>
                        <MapPin className="h-6 w-6" />
                      </div>
                      <div>
                        <h4 className="font-bold text-slate-800 dark:text-white text-sm">Op de Praktijk (Ninove)</h4>
                        <p className="text-[11px] text-slate-400 dark:text-brand-300 mt-0.5">Brakelsesteenweg 559a bus 1, 9400 Ninove.</p>
                      </div>
                    </button>
                  )}
                  
                  {settings?.locatieGoogleMeet && (
                    <button
                      onClick={() => { setSelectedLocation('GoogleMeet'); handleNext(); }}
                      className={`p-5 rounded-2xl border transition-all duration-200 flex flex-col items-center text-center space-y-3 ${
                        selectedLocation === 'GoogleMeet'
                          ? 'border-brand-500 bg-brand-50/30 dark:bg-brand-950/80 ring-2 ring-brand-500/10'
                          : 'border-slate-100 dark:border-brand-800/40 bg-white dark:bg-brand-950/40 hover:border-slate-300 dark:hover:border-brand-700'
                      }`}
                    >
                      <div className={`h-12 w-12 rounded-full flex items-center justify-center ${selectedLocation === 'GoogleMeet' ? 'bg-brand-500 text-white' : 'bg-slate-50 dark:bg-brand-800 text-slate-600 dark:text-brand-200'}`}>
                        <Video className="h-6 w-6" />
                      </div>
                      <div>
                        <h4 className="font-bold text-slate-800 dark:text-white text-sm">Online (Google Meet)</h4>
                        <p className="text-[11px] text-slate-400 dark:text-brand-300 mt-0.5">Video-call via een veilige link.</p>
                      </div>
                    </button>
                  )}

                  {settings?.locatieTelefoon && (
                    <button
                      onClick={() => { setSelectedLocation('Telefoon'); handleNext(); }}
                      className={`p-5 rounded-2xl border transition-all duration-200 flex flex-col items-center text-center space-y-3 ${
                        selectedLocation === 'Telefoon'
                          ? 'border-brand-500 bg-brand-50/30 dark:bg-brand-950/80 ring-2 ring-brand-500/10'
                          : 'border-slate-100 dark:border-brand-800/40 bg-white dark:bg-brand-950/40 hover:border-slate-300 dark:hover:border-brand-700'
                      }`}
                    >
                      <div className={`h-12 w-12 rounded-full flex items-center justify-center ${selectedLocation === 'Telefoon' ? 'bg-brand-500 text-white' : 'bg-slate-50 dark:bg-brand-800 text-slate-600 dark:text-brand-200'}`}>
                        <Phone className="h-6 w-6" />
                      </div>
                      <div>
                        <h4 className="font-bold text-slate-800 dark:text-white text-sm">Telefonisch</h4>
                        <p className="text-[11px] text-slate-400 dark:text-brand-300 mt-0.5">Wij bellen je op je nummer.</p>
                      </div>
                    </button>
                  )}
                </div>
              </div>
            )}

            {/* STEP 3: DATE & TIME SELECTION */}
            {step === 3 && (
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-bold text-slate-800 dark:text-white">Kies datum en tijd</h3>
                  <p className="text-xs text-slate-400 dark:text-brand-300">Kies een beschikbare dag en een passend tijdstip.</p>
                </div>

                {/* Banner: Eerstvolgende beschikbare afspraak met knop */}
                {firstNextSlot && (
                  <div className="bg-gradient-to-r from-emerald-50 to-teal-50 dark:from-emerald-950/60 dark:to-teal-950/60 border border-emerald-200/80 dark:border-emerald-800/40 rounded-2xl p-4 flex flex-col sm:flex-row items-center justify-between gap-3 shadow-sm animate-fadeIn">
                    <div className="flex items-center space-x-3">
                      <div className="h-10 w-10 bg-emerald-500 text-white rounded-xl flex items-center justify-center shrink-0 shadow-sm">
                        <Sparkles className="h-5 w-5" />
                      </div>
                      <div>
                        <span className="text-[10px] font-extrabold uppercase tracking-wider text-emerald-800 dark:text-emerald-300 bg-emerald-100/80 dark:bg-emerald-900/60 px-2 py-0.5 rounded-full">Eerstvolgende Vrije Afspraak</span>
                        <h4 className="font-bold text-slate-800 dark:text-white text-sm mt-0.5">{firstNextSlot.displayDate} om {firstNextSlot.time} uur</h4>
                      </div>
                    </div>
                    <button
                      type="button"
                      onClick={() => {
                        setSelectedDate(firstNextSlot.date);
                      }}
                      className="px-4 py-2.5 bg-emerald-600 hover:bg-emerald-700 active:scale-95 text-white font-bold text-xs rounded-xl shadow-sm transition-all shrink-0 flex items-center space-x-1.5 cursor-pointer"
                    >
                      <span>Ga naar {firstNextSlot.displayDate.split(' ')[0]} {firstNextSlot.time}</span>
                      <ArrowRight className="h-4 w-4" />
                    </button>
                  </div>
                )}
                
                {/* Dagen-Navigatie Bar */}
                <div className="bg-slate-50 dark:bg-brand-950/60 border border-slate-100 dark:border-brand-800/40 p-2.5 rounded-2xl flex items-center justify-between">
                  <button
                    type="button"
                    onClick={() => navigateDays(-1)}
                    className="flex items-center space-x-1 px-3 py-1.5 text-xs font-bold text-slate-600 dark:text-brand-200 hover:text-brand-600 hover:bg-white dark:hover:bg-brand-900 rounded-xl border border-transparent hover:border-slate-200 dark:hover:border-brand-800 transition-all"
                  >
                    <ChevronLeft className="h-4 w-4" />
                    <span>Vorige dag</span>
                  </button>
                  <span className="text-xs font-bold text-slate-800 dark:text-white bg-white dark:bg-brand-900 px-3.5 py-1.5 rounded-xl border border-slate-200 dark:border-brand-800 shadow-2xs">
                    {formatDateDutch(selectedDate)}
                  </span>
                  <button
                    type="button"
                    onClick={() => navigateDays(1)}
                    className="flex items-center space-x-1 px-3 py-1.5 text-xs font-bold text-slate-600 dark:text-brand-200 hover:text-brand-600 hover:bg-white dark:hover:bg-brand-900 rounded-xl border border-transparent hover:border-slate-200 dark:hover:border-brand-800 transition-all"
                  >
                    <span>Volgende dag</span>
                    <ChevronRight className="h-4 w-4" />
                  </button>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
                  {/* Left part: Date picker card */}
                  <div className="md:col-span-5 bg-slate-50 dark:bg-brand-950/60 p-4 rounded-2xl border border-slate-100 dark:border-brand-800/40 flex flex-col justify-center">
                    <label className="block text-xs font-bold text-slate-500 dark:text-brand-300 mb-2 uppercase tracking-wide">Specifieke Datum Kiezen</label>
                    <input
                      type="date"
                      value={selectedDate}
                      onChange={(e) => setSelectedDate(e.target.value)}
                      min={new Date().toISOString().split('T')[0]}
                      className="w-full px-4 py-3 rounded-xl border border-slate-200 dark:border-brand-800 text-sm font-semibold text-slate-700 dark:text-white bg-white dark:bg-brand-900 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-brand-500 shadow-sm"
                    />
                    <div className="mt-4 text-xs text-slate-400 dark:text-brand-300 flex items-start space-x-2">
                      <CalendarDays className="h-4 w-4 shrink-0 text-brand-600 dark:text-brand-400" />
                      <span>Geselecteerd: {formatDateDutch(selectedDate)}.</span>
                    </div>
                  </div>

                  {/* Right part: Time Slots selection */}
                  <div className="md:col-span-7 space-y-3">
                    <label className="block text-xs font-bold text-slate-500 dark:text-brand-300 uppercase tracking-wide">Beschikbare Tijdstippen</label>
                    
                    {loadingSlots ? (
                      <div className="flex flex-col items-center justify-center p-8 bg-white dark:bg-brand-900 rounded-2xl border border-slate-100 dark:border-brand-800/40 min-h-[160px]">
                        <Loader2 className="animate-spin h-8 w-8 text-brand-600 dark:text-brand-400 mb-2" />
                        <span className="text-xs text-slate-400 dark:text-brand-300">Sloten laden...</span>
                      </div>
                    ) : timeSlots.length === 0 ? (
                      <div className="flex flex-col items-center justify-center p-8 bg-slate-50 dark:bg-brand-950/60 rounded-2xl border border-dashed border-slate-200 dark:border-brand-800/60 min-h-[160px] text-center space-y-2">
                        <Calendar className="h-8 w-8 text-slate-300 dark:text-brand-600" />
                        <span className="text-sm font-bold text-slate-700 dark:text-white">Geen slots beschikbaar op deze dag</span>
                        <span className="text-xs text-slate-400 dark:text-brand-300">Gebruik de snelknoppen hierboven of klik op de eerstvolgende afspraak.</span>
                      </div>
                    ) : (
                      <div className="grid grid-cols-3 sm:grid-cols-4 gap-2.5 max-h-[220px] overflow-y-auto pr-1">
                        {timeSlots.map((slot, idx) => (
                          <button
                            key={idx}
                            disabled={slot.isBezet}
                            onClick={() => setSelectedSlot(slot)}
                            className={`py-2.5 px-3 rounded-xl text-xs font-bold text-center border transition-all duration-150 ${
                              slot.isBezet
                                ? 'bg-slate-50 dark:bg-brand-950 border-slate-100 dark:border-brand-900 text-slate-300 dark:text-brand-600 cursor-not-allowed line-through'
                                : selectedSlot?.tijd === slot.tijd
                                  ? 'bg-brand-500 border-brand-500 text-white ring-4 ring-brand-100 dark:ring-brand-800'
                                  : 'bg-white dark:bg-brand-900 border-slate-100 dark:border-brand-800/60 text-slate-700 dark:text-brand-100 hover:border-brand-500 hover:text-brand-600 dark:hover:text-brand-300'
                            }`}
                          >
                            {formatTime(slot.starttijd)}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
              </div>
            )}

            {/* STEP 4: DETAILS */}
            {step === 4 && (
              <div className="space-y-4">
                <div>
                  <h3 className="text-lg font-bold text-slate-800 dark:text-white">Aanvullende opmerkingen</h3>
                  <p className="text-xs text-slate-400 dark:text-brand-300">Heb je opmerkingen of specifieke klachten die je vooraf wilt delen?</p>
                </div>
                <textarea
                  value={opmerkingen}
                  onChange={(e) => setOpmerkingen(e.target.value)}
                  placeholder="Voer hier je opmerkingen in (optioneel)..."
                  rows={5}
                  className="w-full px-4 py-3 rounded-2xl border border-slate-200 dark:border-brand-800 text-sm text-slate-700 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-brand-500 shadow-inner resize-none bg-slate-50/30 dark:bg-brand-950/50"
                />
              </div>
            )}

            {/* STEP 5: REVIEW & CONFIRM */}
            {step === 5 && (
              <div className="space-y-6">
                <div>
                  <h3 className="text-lg font-bold text-slate-800 dark:text-white">Controleer je boeking</h3>
                  <p className="text-xs text-slate-400 dark:text-brand-300">Controleer alle gegevens voordat je de boeking definitief bevestigt.</p>
                </div>

                <div className="bg-slate-50 dark:bg-brand-950/60 border border-slate-100 dark:border-brand-800/40 rounded-2xl p-5 space-y-4 max-w-xl mx-auto">
                  <div className="flex items-start space-x-3 pb-3 border-b border-slate-200/60 dark:border-brand-800/40">
                    <div className="h-8 w-8 rounded-lg bg-brand-100 dark:bg-brand-800 text-brand-700 dark:text-brand-200 flex items-center justify-center shrink-0">
                      <Briefcase className="h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-[10px] text-slate-400 dark:text-brand-400 font-bold uppercase tracking-wider">Behandeling</p>
                      <p className="text-sm font-bold text-slate-700 dark:text-white">{selectedType?.naam}</p>
                      <p className="text-xs text-slate-400 dark:text-brand-300">{selectedType?.standaardDuurMinuten} minuten</p>
                    </div>
                  </div>

                  <div className="flex items-start space-x-3 pb-3 border-b border-slate-200/60 dark:border-brand-800/40">
                    <div className="h-8 w-8 rounded-lg bg-brand-100 dark:bg-brand-800 text-brand-700 dark:text-brand-200 flex items-center justify-center shrink-0">
                      <MapPin className="h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-[10px] text-slate-400 dark:text-brand-400 font-bold uppercase tracking-wider">Locatie & Adres</p>
                      <p className="text-sm font-bold text-slate-700 dark:text-white">
                        {selectedLocation === 'GoogleMeet' && 'Online via Google Meet'}
                        {selectedLocation === 'Telefoon' && 'Telefonisch consult'}
                        {selectedLocation === 'Praktijk' && 'Praktijk Ninove — De Verstandhouding'}
                      </p>
                      {selectedLocation === 'Praktijk' && (
                        <p className="text-xs text-slate-500 dark:text-brand-300 mt-0.5 font-medium">Brakelsesteenweg 559a bus 1, 9400 Ninove</p>
                      )}
                    </div>
                  </div>

                  <div className="flex items-start space-x-3 pb-3 border-b border-slate-200/60 dark:border-brand-800/40">
                    <div className="h-8 w-8 rounded-lg bg-brand-100 dark:bg-brand-800 text-brand-700 dark:text-brand-200 flex items-center justify-center shrink-0">
                      <CalendarDays className="h-4 w-4" />
                    </div>
                    <div>
                      <p className="text-[10px] text-slate-400 dark:text-brand-400 font-bold uppercase tracking-wider">Datum & Tijd</p>
                      <p className="text-sm font-bold text-slate-700 dark:text-white">{formatDateDutch(selectedDate)}</p>
                      <p className="text-xs text-slate-500 dark:text-brand-200 font-semibold">Om {selectedSlot ? formatTime(selectedSlot.starttijd) : ''} - {selectedSlot ? formatTime(selectedSlot.eindtijd) : ''} uur</p>
                    </div>
                  </div>

                  {opmerkingen && (
                    <div className="flex items-start space-x-3">
                      <div className="h-8 w-8 rounded-lg bg-brand-100 dark:bg-brand-800 text-brand-700 dark:text-brand-200 flex items-center justify-center shrink-0">
                        <AlertCircle className="h-4 w-4" />
                      </div>
                      <div>
                        <p className="text-[10px] text-slate-400 dark:text-brand-400 font-bold uppercase tracking-wider">Opmerkingen</p>
                        <p className="text-xs text-slate-600 dark:text-brand-100 bg-white dark:bg-brand-900 border border-slate-100 dark:border-brand-800 p-2.5 rounded-xl mt-1 max-w-md italic">{opmerkingen}</p>
                      </div>
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Wizard Navigation Footer */}
            <div className="mt-8 pt-4 border-t border-slate-100 dark:border-brand-800/40 flex items-center justify-between">
              <button
                type="button"
                onClick={handleBack}
                disabled={step === 1 || bookingInProgress}
                className={`flex items-center space-x-2 py-2.5 px-4 rounded-xl text-xs font-bold transition-all duration-150 ${
                  step === 1 || bookingInProgress
                    ? 'text-slate-300 dark:text-brand-700 cursor-not-allowed'
                    : 'text-slate-600 dark:text-brand-200 hover:bg-slate-50 dark:hover:bg-brand-950 border border-slate-100 dark:border-brand-800 hover:border-slate-200 dark:hover:border-brand-700 bg-white dark:bg-brand-900'
                }`}
              >
                <ArrowLeft className="h-4 w-4" />
                <span>Vorige</span>
              </button>

              {step === 5 ? (
                <button
                  type="button"
                  onClick={handleConfirmBooking}
                  disabled={bookingInProgress}
                  className="flex items-center space-x-2 py-2.5 px-6 rounded-xl text-xs font-bold text-white bg-brand-500 hover:bg-brand-600 active:scale-95 transition-all duration-150 shadow-md shadow-brand-500/10"
                >
                  {bookingInProgress ? (
                    <>
                      <Loader2 className="animate-spin h-4 w-4" />
                      <span>Reserveren...</span>
                    </>
                  ) : (
                    <>
                      <CheckCircle className="h-4 w-4" />
                      <span>Bevestig Reservering</span>
                    </>
                  )}
                </button>
              ) : (
                <button
                  type="button"
                  onClick={handleNext}
                  disabled={
                    (step === 1 && !selectedType) ||
                    (step === 2 && !selectedLocation) ||
                    (step === 3 && !selectedSlot)
                  }
                  className={`flex items-center space-x-2 py-2.5 px-6 rounded-xl text-xs font-bold text-white transition-all duration-150 shadow-md ${
                    ((step === 1 && !selectedType) ||
                    (step === 2 && !selectedLocation) ||
                    (step === 3 && !selectedSlot))
                      ? 'bg-slate-200 dark:bg-brand-950 text-slate-400 dark:text-brand-700 cursor-not-allowed shadow-none'
                      : 'bg-brand-500 hover:bg-brand-600 active:scale-95 shadow-brand-500/10'
                  }`}
                >
                  <span>Volgende</span>
                  <ArrowRight className="h-4 w-4" />
                </button>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
};
