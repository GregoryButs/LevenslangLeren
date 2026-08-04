import React, { useState, useEffect } from 'react';
import { Clock, X, Loader2, MapPin, Video, Phone } from 'lucide-react';
import { patientApi, afspraakTypeApi, afspraakApi } from '../services/api';
import { getPatientDisplayName } from '../utils/patientUtils';
import { Afspraak } from '../types';

interface AfspraakInplannenModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  initialDate?: Date | null;
  initialSlot?: number | null;
  initialDurationMin?: number | null;
  afspraakToEdit?: Afspraak | null;
}

export const AfspraakInplannenModal: React.FC<AfspraakInplannenModalProps> = ({
  isOpen,
  onClose,
  onSuccess,
  initialDate,
  initialSlot,
  initialDurationMin,
  afspraakToEdit
}) => {
  const [bookingPatients, setBookingPatients] = useState<Array<{ id: number; naam: string }>>([]);
  const [bookingTypes, setBookingTypes] = useState<Array<{ id: number; naam: string; standaardDuurMinuten: number }>>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [newBooking, setNewBooking] = useState({
    typeId: '',
    patientId: '',
    starttijd: '',
    duurMinuten: 60,
    locatieType: 'Praktijk' as 'Praktijk' | 'GoogleMeet' | 'Telefoon',
    opmerkingen: '',
    status: 'Gepland' as 'Gepland' | 'Voltooid' | 'Geannuleerd',
    herhaling: 0,
    herhaalTot: ''
  });

  const formatDateTimeInput = (dateObj?: Date | null, hour?: number | null, minute?: number | null): string => {
    const d = dateObj ? new Date(dateObj) : new Date();
    if (hour !== undefined && hour !== null) {
      d.setHours(hour, minute || 0, 0, 0);
    }
    const yyyy = d.getFullYear();
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    const hh = String(d.getHours()).padStart(2, '0');
    const mi = String(d.getMinutes()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}T${hh}:${mi}`;
  };

  useEffect(() => {
    if (!isOpen) return;

    const loadOptions = async () => {
      setLoading(true);
      setError(null);
      try {
        if (afspraakToEdit) {
          // Edit Mode
          const res = await afspraakApi.getEditData(afspraakToEdit.id);
          const pList = (res.patienten || []).map((p: any) => ({
            id: p.id ?? p.Id,
            naam: p.naam || p.Naam || getPatientDisplayName(p)
          }));
          const tList = (res.types || []).map((t: any) => ({
            id: t.id ?? t.Id,
            naam: t.naam || t.Naam,
            standaardDuurMinuten: t.standaardDuurMinuten ?? 50
          }));

          setBookingPatients(pList);
          setBookingTypes(tList);

          const d = new Date(res.viewModel?.starttijd || afspraakToEdit.starttijd);
          const localIso = new Date(d.getTime() - (d.getTimezoneOffset() * 60000))
            .toISOString()
            .slice(0, 16);

          const start = new Date(afspraakToEdit.starttijd);
          const end = new Date(afspraakToEdit.eindtijd);
          const diffMin = Math.round((end.getTime() - start.getTime()) / 60000);
          const duration = res.viewModel?.customDuurMinuten || (diffMin > 0 ? diffMin : 60);

          setNewBooking({
            typeId: res.viewModel?.typeId ? String(res.viewModel.typeId) : '',
            patientId: res.viewModel?.patientId ? String(res.viewModel.patientId) : '',
            starttijd: localIso,
            duurMinuten: duration,
            locatieType: (res.viewModel?.locatieType || 'Praktijk') as any,
            opmerkingen: res.viewModel?.opmerkingen || '',
            status: res.viewModel?.status || 'Gepland',
            herhaling: 0,
            herhaalTot: ''
          });
        } else {
          // Create Mode
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

          const hour = initialSlot !== undefined && initialSlot !== null ? Math.floor(initialSlot) : undefined;
          const minute = initialSlot !== undefined && initialSlot !== null ? Math.round((initialSlot % 1) * 60) : 0;
          const formattedStarttijd = formatDateTimeInput(initialDate, hour, minute);

          const defaultTypeDuration = tList.length > 0 ? tList[0].standaardDuurMinuten : 60;
          const initialDuration = initialDurationMin && initialDurationMin > 0 ? initialDurationMin : defaultTypeDuration;

          setNewBooking({
            typeId: tList.length > 0 ? tList[0].id.toString() : '',
            patientId: '',
            starttijd: formattedStarttijd,
            duurMinuten: initialDuration,
            locatieType: 'Praktijk',
            opmerkingen: '',
            status: 'Gepland',
            herhaling: 0,
            herhaalTot: ''
          });
        }
      } catch (err: any) {
        console.error("Fout bij laden van opties:", err);
        setError("Kon gegevens niet laden.");
      } finally {
        setLoading(false);
      }
    };

    loadOptions();
  }, [isOpen, initialDate, initialSlot, initialDurationMin, afspraakToEdit]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newBooking.starttijd) {
      setError("Selecteer a.u.b. een starttijd.");
      return;
    }

    setSubmitting(true);
    setError(null);

    try {
      if (afspraakToEdit) {
        await afspraakApi.update(afspraakToEdit.id, {
          id: afspraakToEdit.id,
          typeId: newBooking.typeId ? parseInt(newBooking.typeId, 10) : null,
          patientId: newBooking.patientId ? parseInt(newBooking.patientId, 10) : null,
          starttijd: new Date(newBooking.starttijd).toISOString(),
          customDuurMinuten: Number(newBooking.duurMinuten),
          locatieType: newBooking.locatieType,
          opmerkingen: newBooking.opmerkingen,
          status: newBooking.status
        });
      } else {
        const payload = {
          typeId: newBooking.typeId ? parseInt(newBooking.typeId, 10) : null,
          patientId: newBooking.patientId ? parseInt(newBooking.patientId, 10) : null,
          starttijd: new Date(newBooking.starttijd).toISOString(),
          customDuurMinuten: Number(newBooking.duurMinuten),
          locatieType: newBooking.locatieType,
          opmerkingen: newBooking.opmerkingen,
          herhaling: newBooking.herhaling,
          herhaalTot: newBooking.herhaling !== 0 && newBooking.herhaalTot ? new Date(newBooking.herhaalTot).toISOString() : null
        };
        await afspraakApi.create(payload);
      }

      onSuccess();
      onClose();
    } catch (err: any) {
      console.error("Fout bij opslaan van afspraak:", err);
      setError(err?.response?.data?.message || "Kon afspraak niet opslaan.");
    } finally {
      setSubmitting(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 dark:bg-black/70 backdrop-blur-sm p-4 animate-in fade-in duration-200">
      <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-lg p-6 relative border border-slate-100 dark:border-brand-800/40 transition-colors">
        <button
          onClick={onClose}
          className="absolute top-5 right-5 text-slate-400 dark:text-brand-300 hover:text-slate-600 dark:hover:text-white p-1 rounded-full hover:bg-slate-100 dark:hover:bg-brand-800 transition cursor-pointer"
        >
          <X className="h-5 w-5" />
        </button>

        <h3 className="text-xl font-bold text-slate-800 dark:text-white mb-4">
          {afspraakToEdit ? 'Afspraak Bewerken' : 'Afspraak Inplannen'}
        </h3>

        {error && (
          <div className="mb-4 p-3 bg-red-50 dark:bg-red-950/60 border border-red-200 dark:border-red-900/50 rounded-xl text-xs font-semibold text-red-700 dark:text-red-300">
            {error}
          </div>
        )}

        {loading ? (
          <div className="flex flex-col items-center justify-center py-12 space-y-3">
            <Loader2 className="h-8 w-8 animate-spin text-brand-600 dark:text-brand-400" />
            <span className="text-xs font-semibold text-slate-500 dark:text-brand-300">Opties laden...</span>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                Type afspraak <span className="text-red-500 font-bold ml-0.5">*</span>
              </label>
              <select
                required
                value={newBooking.typeId}
                onChange={(e) => {
                  const tId = e.target.value;
                  const selectedTypeObj = bookingTypes.find(t => t.id.toString() === tId);
                  const updatedDuration = (newBooking.patientId !== '' && selectedTypeObj) 
                    ? selectedTypeObj.standaardDuurMinuten 
                    : newBooking.duurMinuten;
                  setNewBooking({ ...newBooking, typeId: tId, duurMinuten: updatedDuration });
                }}
                className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
              >
                {bookingTypes.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.naam} ({t.standaardDuurMinuten} min)
                  </option>
                ))}
              </select>
            </div>

            {/* Locatie / Consult Vorm Selector */}
            <div>
              <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                Locatie / Consult Vorm
              </label>
              <div className="grid grid-cols-3 gap-2">
                <button
                  type="button"
                  onClick={() => setNewBooking({ ...newBooking, locatieType: 'Praktijk' })}
                  className={`py-2 px-3 text-xs font-bold rounded-xl border transition flex items-center justify-center space-x-1.5 cursor-pointer ${
                    newBooking.locatieType === 'Praktijk'
                      ? 'bg-brand-600 text-white border-brand-600 shadow-xs'
                      : 'bg-slate-50 dark:bg-brand-950 text-slate-600 dark:text-brand-300 border-slate-200 dark:border-brand-800 hover:bg-slate-100 dark:hover:bg-brand-800'
                  }`}
                >
                  <MapPin className="h-4 w-4 shrink-0 text-brand-400" />
                  <span>Praktijk</span>
                </button>

                <button
                  type="button"
                  onClick={() => setNewBooking({ ...newBooking, locatieType: 'GoogleMeet' })}
                  className={`py-2 px-3 text-xs font-bold rounded-xl border transition flex items-center justify-center space-x-1.5 cursor-pointer ${
                    newBooking.locatieType === 'GoogleMeet'
                      ? 'bg-purple-600 text-white border-purple-600 shadow-xs'
                      : 'bg-slate-50 dark:bg-brand-950 text-slate-600 dark:text-brand-300 border-slate-200 dark:border-brand-800 hover:bg-slate-100 dark:hover:bg-brand-800'
                  }`}
                >
                  <Video className="h-4 w-4 shrink-0 text-purple-400" />
                  <span>Online (Meet)</span>
                </button>

                <button
                  type="button"
                  onClick={() => setNewBooking({ ...newBooking, locatieType: 'Telefoon' })}
                  className={`py-2 px-3 text-xs font-bold rounded-xl border transition flex items-center justify-center space-x-1.5 cursor-pointer ${
                    newBooking.locatieType === 'Telefoon'
                      ? 'bg-teal-600 text-white border-teal-600 shadow-xs'
                      : 'bg-slate-50 dark:bg-brand-950 text-slate-600 dark:text-brand-300 border-slate-200 dark:border-brand-800 hover:bg-slate-100 dark:hover:bg-brand-800'
                  }`}
                >
                  <Phone className="h-4 w-4 shrink-0 text-teal-400" />
                  <span>Telefonisch</span>
                </button>
              </div>
            </div>

            <div>
              <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                Patiënt <span className="text-xs font-normal text-slate-400">(Optioneel bij blokkering)</span>
              </label>
              <select
                value={newBooking.patientId}
                onChange={(e) => {
                  const pId = e.target.value;
                  const selectedTypeObj = bookingTypes.find(t => t.id.toString() === newBooking.typeId);
                  const updatedDuration = (pId !== '' && selectedTypeObj) 
                    ? selectedTypeObj.standaardDuurMinuten 
                    : newBooking.duurMinuten;
                  setNewBooking({ ...newBooking, patientId: pId, duurMinuten: updatedDuration });
                }}
                className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500"
              >
                <option value="">-- Geen patiënt (Blokkering) --</option>
                {bookingPatients.map((p) => (
                  <option key={p.id} value={p.id}>{p.naam}</option>
                ))}
              </select>
            </div>

            <div>
              <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                Starttijd <span className="text-red-500 font-bold ml-0.5">*</span>
              </label>
              <input
                type="datetime-local"
                required
                value={newBooking.starttijd}
                onChange={(e) => setNewBooking({ ...newBooking, starttijd: e.target.value })}
                className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none dark:scheme-dark"
              />
            </div>

            {/* Duur / Eindtijd Selector */}
            {(() => {
              const selectedTypeObj = bookingTypes.find(t => t.id.toString() === newBooking.typeId);
              const isPatientSelected = newBooking.patientId !== '';

              if (isPatientSelected) {
                const duration = newBooking.duurMinuten || selectedTypeObj?.standaardDuurMinuten || 50;
                return (
                  <div className="bg-brand-50/80 dark:bg-brand-950/70 p-3.5 rounded-2xl border border-brand-200/60 dark:border-brand-800/60 flex items-center justify-between text-xs text-brand-900 dark:text-brand-200 shadow-2xs">
                    <span className="flex items-center font-medium">
                      <Clock className="h-4 w-4 mr-1.5 text-brand-600 dark:text-brand-400 shrink-0" />
                      <span>Duur: <strong>{duration} minuten</strong></span>
                    </span>
                    {newBooking.starttijd && (
                      <span className="font-bold text-brand-700 dark:text-brand-300 bg-white/80 dark:bg-brand-900 px-2.5 py-1 rounded-xl border border-brand-200/60 dark:border-brand-700/60">
                        {(() => {
                          const start = new Date(newBooking.starttijd);
                          const end = new Date(start.getTime() + duration * 60000);
                          return `${start.toLocaleTimeString('nl-NL', { hour: '2-digit', minute: '2-digit' })} — ${end.toLocaleTimeString('nl-NL', { hour: '2-digit', minute: '2-digit' })}`;
                        })()}
                      </span>
                    )}
                  </div>
                );
              }

              return (
                <div>
                  <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Duur blokkering</label>
                  <div className="flex flex-wrap gap-2 mb-2">
                    {[30, 60, 120, 180, 240, 480].map((min) => {
                      const label = min < 60 ? `${min}m` : min === 480 ? '8u (hele dag)' : `${min / 60}u`;
                      return (
                        <button
                          key={min}
                          type="button"
                          onClick={() => setNewBooking({ ...newBooking, duurMinuten: min })}
                          className={`px-3 py-1.5 text-xs font-bold rounded-lg border transition cursor-pointer ${
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
                      className="w-28 bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2 px-3 rounded-xl text-slate-800 dark:text-white focus:outline-none focus:ring-2 focus:ring-brand-500 text-sm dark:scheme-dark"
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
              );
            })()}

            {afspraakToEdit && (
              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Status</label>
                <select
                  value={newBooking.status}
                  onChange={(e) => setNewBooking({ ...newBooking, status: e.target.value as any })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white focus:outline-none"
                >
                  <option value="Gepland">Gepland</option>
                  <option value="Voltooid">Voltooid</option>
                  <option value="Geannuleerd">Geannuleerd</option>
                </select>
              </div>
            )}

            <div>
              <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">
                {newBooking.patientId ? 'Opmerkingen' : 'Titel / Reden van blokkering'}
              </label>
              <textarea
                value={newBooking.opmerkingen}
                onChange={(e) => setNewBooking({ ...newBooking, opmerkingen: e.target.value })}
                className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400 focus:outline-none h-20 resize-none"
                placeholder={newBooking.patientId ? 'Eventuele opmerkingen...' : 'Titel / reden van de blokkering (bijv. Vrij nemen, Dokter, Tandarts)...'}
              />
            </div>

            {!afspraakToEdit && (
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
            )}

            <div className="flex justify-end space-x-3 pt-4 border-t border-slate-100 dark:border-brand-800/40">
              <button
                type="button"
                onClick={onClose}
                disabled={submitting}
                className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-5 rounded-xl font-semibold transition cursor-pointer"
              >
                Annuleren
              </button>
              <button
                type="submit"
                disabled={submitting}
                className="bg-brand-600 hover:bg-brand-700 text-white py-2.5 px-5 rounded-xl font-semibold transition shadow-sm flex items-center space-x-2 disabled:opacity-50 cursor-pointer"
              >
                {submitting && <Loader2 className="h-4 w-4 animate-spin" />}
                <span>{afspraakToEdit ? 'Opslaan' : 'Boeken'}</span>
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
