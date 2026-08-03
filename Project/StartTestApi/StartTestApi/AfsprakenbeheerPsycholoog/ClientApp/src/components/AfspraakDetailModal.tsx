import React, { useState, useEffect } from 'react';
import { afspraakApi } from '../services/api';
import { Afspraak } from '../types';
import { 
  Calendar as CalendarIcon, X, Edit3, Trash2, Loader2, Video 
} from 'lucide-react';
import { extractErrorMessage } from '../utils/errorUtils';

interface AfspraakDetailModalProps {
  afspraak: Afspraak | null;
  onClose: () => void;
  onSuccess: () => void;
  onEdit?: (afspraak: Afspraak) => void;
}

export const AfspraakDetailModal: React.FC<AfspraakDetailModalProps> = ({
  afspraak,
  onClose,
  onSuccess,
  onEdit
}) => {
  const [loading, setLoading] = useState(true);
  const [selectedAfspraakData, setSelectedAfspraakData] = useState<Afspraak | null>(afspraak);

  const loadData = async () => {
    if (!afspraak) return;
    try {
      setLoading(true);
      const res = await afspraakApi.getEditData(afspraak.id);
      const fullAfspraak = {
        ...afspraak,
        patientId: res.viewModel.patientId,
        typeId: res.viewModel.typeId,
        opmerkingen: res.viewModel.opmerkingen,
        status: res.viewModel.status
      };
      setSelectedAfspraakData(fullAfspraak);
    } catch (err) {
      console.error('Fout bij ophalen van afspraakdetails:', err);
      setSelectedAfspraakData(afspraak);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [afspraak?.id]);

  if (!afspraak || !selectedAfspraakData) return null;

  const handleDeleteBooking = async (id: number) => {
    if (!window.confirm('Weet u zeker dat u deze afspraak wilt verwijderen?')) return;
    try {
      await afspraakApi.delete(id);
      onSuccess();
      onClose();
    } catch (err) {
      console.error(err);
      alert(extractErrorMessage(err, 'Verwijderen mislukt.'));
    }
  };

  return (
    <div className="fixed inset-0 bg-slate-900/60 dark:bg-black/70 backdrop-blur-xs flex items-center justify-center p-4 z-50 animate-in fade-in duration-200">
      <div className="bg-white dark:bg-brand-900 rounded-3xl max-w-lg w-full p-6 shadow-2xl border border-slate-100 dark:border-brand-800/40 space-y-5 relative max-h-[90vh] overflow-y-auto transition-colors">
        {/* Modal Header */}
        <div className="flex justify-between items-center border-b border-slate-100 dark:border-brand-800/40 pb-4">
          <h3 className="text-lg font-bold text-slate-800 dark:text-white flex items-center space-x-2">
            <CalendarIcon className="h-5 w-5 text-brand-600 dark:text-brand-400" />
            <span>Afspraak Details</span>
          </h3>
          <button 
            onClick={onClose}
            className="p-1.5 text-slate-400 dark:text-brand-400 hover:text-slate-600 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-brand-800 rounded-xl transition cursor-pointer"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {loading ? (
          <div className="flex justify-center items-center py-12">
            <Loader2 className="animate-spin h-8 w-8 text-brand-600 dark:text-brand-400" />
          </div>
        ) : (
          /* Details View */
          <div className="space-y-4">
            {(() => {
              const isMeet = !!selectedAfspraakData.googleMeetLink || (selectedAfspraakData.opmerkingen && (selectedAfspraakData.opmerkingen.includes('GoogleMeet') || selectedAfspraakData.opmerkingen.includes('Google Meet')));
              if (!isMeet) return null;
              
              const activeMeetLink = (selectedAfspraakData.googleMeetLink && selectedAfspraakData.googleMeetLink.startsWith('https://meet.google.com/') && !selectedAfspraakData.googleMeetLink.includes('meet-dv') && !selectedAfspraakData.googleMeetLink.includes('lookup') && !selectedAfspraakData.googleMeetLink.includes('vst-hndg'))
                ? selectedAfspraakData.googleMeetLink 
                : 'https://meet.google.com/new';

              const startDate = new Date(selectedAfspraakData.starttijd);
              const now = new Date();
              const diffMs = startDate.getTime() - now.getTime();
              const diffMins = Math.round(diffMs / 60000);
              const diffHours = Math.round(diffMs / 3600000);
              const diffDays = Math.round(diffMs / 86400000);

              let statusText = '';
              if (diffMins < -60) statusText = 'Afgelopen';
              else if (diffMins < 0) statusText = '🟢 NU BEZIG';
              else if (diffMins <= 15) statusText = '⚡ Start binnenkort (< 15 min)!';
              else if (diffHours <= 24) statusText = `⏳ Start over ca. ${diffHours} uur`;
              else statusText = `📅 Gepland over ${diffDays} dagen`;

              return (
                <div className="p-4 bg-purple-50 dark:bg-purple-950/60 border border-purple-200 dark:border-purple-800/40 rounded-2xl space-y-3 shadow-xs">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2 text-purple-900 dark:text-purple-200 font-bold text-sm">
                      <Video className="h-5 w-5 text-purple-600 dark:text-purple-400 shrink-0" />
                      <span>Online Google Meet Videoconsultatie</span>
                    </div>
                    <span className="text-xs font-semibold px-2.5 py-1 rounded-full bg-purple-100 dark:bg-purple-900/60 text-purple-800 dark:text-purple-300">
                      {statusText}
                    </span>
                  </div>

                  <div className="text-xs text-purple-800 dark:text-purple-300 space-y-1 bg-white/60 dark:bg-purple-900/30 p-2.5 rounded-xl border border-purple-100 dark:border-purple-800/30">
                    <div className="flex items-center space-x-2">
                      <CalendarIcon className="h-4 w-4 text-purple-600 dark:text-purple-400 shrink-0" />
                      <span>Tijdstip: <strong>{startDate.toLocaleDateString('nl-BE', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })} om {startDate.toLocaleTimeString('nl-BE', { hour: '2-digit', minute: '2-digit' })}</strong></span>
                    </div>
                  </div>

                  <div>
                    <a 
                      href={activeMeetLink} 
                      target="_blank" 
                      rel="noopener noreferrer"
                      className="inline-flex items-center space-x-2 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold py-2.5 px-4 rounded-xl transition shadow-sm"
                    >
                      <Video className="h-4 w-4" />
                      <span>Deelnemen via Google Meet</span>
                    </a>
                  </div>
                </div>
              );
            })()}

            {selectedAfspraakData.opmerkingen && (selectedAfspraakData.opmerkingen.includes('Praktijkhuis') || selectedAfspraakData.opmerkingen.includes('[PH9500]')) && (
              <div className="p-3 bg-purple-50 dark:bg-purple-950/60 border border-purple-200 dark:border-purple-800/40 rounded-2xl flex items-center space-x-3 text-purple-800 dark:text-purple-300 text-xs">
                <span className="text-lg">🏥</span>
                <div>
                  <p className="font-bold text-purple-900 dark:text-purple-200">Ingeboekt via Praktijkhuis 9500</p>
                  <p className="text-xs text-purple-700 dark:text-purple-300 font-normal">Deze afspraak is binnengekomen via de Praktijkhuis 9500 integratie.</p>
                </div>
              </div>
            )}

            <div className="grid grid-cols-3 gap-2">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">
                {selectedAfspraakData.patientId ? 'Patiënt' : 'Titel / Reden'}
              </span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-sm">{selectedAfspraakData.patientNaam}</span>
            </div>

            <div className="grid grid-cols-3 gap-2">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Type</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-sm">{selectedAfspraakData.afspraakTypeNaam}</span>
            </div>

            <div className="grid grid-cols-3 gap-2">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Tijdstip</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-sm">
                {new Date(selectedAfspraakData.starttijd).toLocaleString([], { dateStyle: 'medium', timeStyle: 'short' })}
              </span>
            </div>

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Status</span>
              <span className="col-span-2 flex items-center space-x-2">
                <span className={`inline-block text-xs font-semibold py-1 px-3 rounded-full ${
                  selectedAfspraakData.status === 'Geannuleerd' ? 'bg-red-100 dark:bg-red-950/60 text-red-800 dark:text-red-300 border border-red-200 dark:border-red-800/40' :
                  selectedAfspraakData.status === 'Voltooid' ? 'bg-emerald-100 dark:bg-emerald-950/60 text-emerald-800 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800/40' :
                  'bg-brand-100 dark:bg-brand-800 text-brand-800 dark:text-brand-100 border border-brand-200 dark:border-brand-700'
                }`}>
                  {selectedAfspraakData.status}
                </span>
                {selectedAfspraakData.status === 'Geannuleerd' && (
                  <span className="text-xs text-emerald-600 dark:text-emerald-400 font-semibold">
                    ✓ Tijdstip is vrij voor nieuwe boeking
                  </span>
                )}
              </span>
            </div>

            {selectedAfspraakData.opmerkingen && selectedAfspraakData.opmerkingen.replace('[PH9500]', '').replace(/^\[Praktijkhuis9500\]\s*/, '').trim() !== '' && (
              <div className="grid grid-cols-3 gap-2">
                <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Opmerkingen</span>
                <span className="col-span-2 text-slate-600 dark:text-brand-100 bg-slate-50 dark:bg-brand-950 p-2.5 rounded-xl border border-slate-100 dark:border-brand-800 text-xs whitespace-pre-wrap">
                  {selectedAfspraakData.opmerkingen.replace('[PH9500]', '').replace(/^\[Praktijkhuis9500\]\s*/, '').trim()}
                </span>
              </div>
            )}

            <div className="flex justify-between items-center pt-4 border-t border-slate-100 dark:border-brand-800/40">
              <button
                onClick={() => handleDeleteBooking(selectedAfspraakData.id)}
                className="text-red-600 dark:text-red-400 hover:text-red-700 text-xs font-semibold flex items-center space-x-1 py-2 px-3 rounded-xl hover:bg-red-50 dark:hover:bg-red-950/60 transition cursor-pointer"
              >
                <Trash2 className="h-4 w-4" />
                <span>Verwijderen</span>
              </button>
              {onEdit && (
                <button
                  onClick={() => {
                    const apptToEdit = selectedAfspraakData;
                    onClose();
                    onEdit(apptToEdit);
                  }}
                  className="bg-brand-600 hover:bg-brand-700 text-white py-2 px-4 rounded-xl text-xs font-semibold transition flex items-center space-x-1.5 shadow-sm cursor-pointer"
                >
                  <Edit3 className="h-4 w-4" />
                  <span>Bewerken</span>
                </button>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
