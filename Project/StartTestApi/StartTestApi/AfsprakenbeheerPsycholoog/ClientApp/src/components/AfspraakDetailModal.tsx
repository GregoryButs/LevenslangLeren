import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { afspraakApi } from '../services/api';
import { Afspraak } from '../types';
import { 
  Calendar as CalendarIcon, X, Edit3, Trash2, Loader2, Video, User, ExternalLink, Copy 
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
  const navigate = useNavigate();
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
    <div className="fixed inset-0 bg-slate-900/60 dark:bg-black/70 backdrop-blur-xs flex items-center justify-center p-4 z-50 overflow-y-auto animate-in fade-in duration-200">
      <div className="bg-white dark:bg-brand-900 rounded-3xl max-w-lg w-full max-h-[90vh] flex flex-col p-6 shadow-2xl border border-slate-100 dark:border-brand-800/40 transition-colors overflow-hidden">
        {/* Modal Header */}
        <div className="flex justify-between items-center border-b border-slate-100 dark:border-brand-800/40 pb-4 shrink-0">
          <h3 className="text-lg font-bold text-slate-900 dark:text-white flex items-center space-x-2">
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
          <div className="flex justify-center items-center py-12 flex-1">
            <Loader2 className="animate-spin h-8 w-8 text-brand-600 dark:text-brand-400" />
          </div>
        ) : (
          /* Details View */
          <div className="flex flex-col flex-1 min-h-0">
            <div className="flex-1 overflow-y-auto pr-1 py-3 space-y-4">
            {(() => {
              const isMeet = !!selectedAfspraakData.googleMeetLink || (selectedAfspraakData.opmerkingen && (selectedAfspraakData.opmerkingen.includes('GoogleMeet') || selectedAfspraakData.opmerkingen.includes('Google Meet')));
              if (!isMeet) return null;
              
              const activeMeetLink = (selectedAfspraakData.googleMeetLink && selectedAfspraakData.googleMeetLink.startsWith('https://meet.google.com/') && !selectedAfspraakData.googleMeetLink.includes('meet-dv') && !selectedAfspraakData.googleMeetLink.includes('lookup') && !selectedAfspraakData.googleMeetLink.includes('vst-hndg'))
                ? selectedAfspraakData.googleMeetLink 
                : 'https://meet.google.com/new';

              const startDate = new Date(selectedAfspraakData.starttijd);

              return (
                <div className="p-4 bg-purple-50 dark:bg-purple-950/60 border border-purple-200 dark:border-purple-800/40 rounded-2xl space-y-3 shadow-xs">
                  <div className="flex items-center justify-between">
                    <div className="flex items-center space-x-2 text-purple-900 dark:text-purple-200 font-bold text-sm">
                      <Video className="h-5 w-5 text-purple-600 dark:text-purple-400 shrink-0" />
                      <span>Online Google Meet Videoconsultatie</span>
                    </div>
                  </div>

                  <div className="text-xs text-purple-800 dark:text-purple-300 space-y-1 bg-white/60 dark:bg-purple-900/30 p-2.5 rounded-xl border border-purple-100 dark:border-purple-800/30">
                    <div className="flex items-center space-x-2">
                      <CalendarIcon className="h-4 w-4 text-purple-600 dark:text-purple-400 shrink-0" />
                      <span>Tijdstip: <strong>{startDate.toLocaleDateString('nl-BE', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })} om {startDate.toLocaleTimeString('nl-BE', { hour: '2-digit', minute: '2-digit' })}</strong></span>
                    </div>
                    <div className="flex items-center space-x-2">
                      <User className="h-4 w-4 text-purple-600 dark:text-purple-400 shrink-0" />
                      <span>Patiënt: <strong>{selectedAfspraakData.patientNaam || 'Onbekend'}</strong></span>
                    </div>
                  </div>

                  <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2 pt-1">
                    <a
                      href={activeMeetLink}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="flex-1 bg-purple-600 hover:bg-purple-700 text-white font-bold py-2.5 px-4 rounded-xl transition text-xs flex items-center justify-center space-x-2 shadow-sm text-center cursor-pointer"
                    >
                      <Video className="h-4 w-4" />
                      <span>Deelnemen aan Google Meet</span>
                      <ExternalLink className="h-3.5 w-3.5 opacity-80" />
                    </a>

                    <button
                      type="button"
                      onClick={() => {
                        navigator.clipboard.writeText(activeMeetLink);
                        alert('Google Meet link gekopieerd naar klembord!');
                      }}
                      className="bg-purple-100 dark:bg-purple-900/80 hover:bg-purple-200 dark:hover:bg-purple-800 text-purple-900 dark:text-purple-100 font-semibold py-2.5 px-3 rounded-xl transition text-xs flex items-center justify-center space-x-1.5 cursor-pointer shrink-0"
                      title="Kopieer vergaderlink"
                    >
                      <Copy className="h-4 w-4" />
                      <span className="sm:hidden">Kopiëren</span>
                    </button>
                  </div>
                </div>
              );
            })()}

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Patiënt</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-xs">
                {selectedAfspraakData.patientId ? (
                  <button
                    type="button"
                    onClick={() => {
                      onClose();
                      navigate(`/patients?id=${selectedAfspraakData.patientId}`);
                    }}
                    className="text-brand-600 dark:text-brand-300 hover:text-brand-800 dark:hover:text-white font-bold hover:underline inline-flex items-center space-x-1.5 transition text-left cursor-pointer group"
                    title="Bekijk patiëntendossier in patiëntenlijst"
                  >
                    <span>{selectedAfspraakData.patientNaam || '—'}</span>
                    <ExternalLink className="h-3.5 w-3.5 opacity-70 group-hover:opacity-100 transition shrink-0 text-brand-600 dark:text-brand-400" />
                  </button>
                ) : (
                  <span>{selectedAfspraakData.patientNaam || '—'}</span>
                )}
              </span>
            </div>

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Type</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-xs">{selectedAfspraakData.afspraakTypeNaam}</span>
            </div>

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Starttijd</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-xs">
                {new Date(selectedAfspraakData.starttijd).toLocaleDateString('nl-NL')} om {new Date(selectedAfspraakData.starttijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
              </span>
            </div>

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Duur</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-xs">{(selectedAfspraakData as any).duurMinuten || (selectedAfspraakData as any).customDuurMinuten || 60} minuten</span>
            </div>

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Locatie</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-xs">{(selectedAfspraakData as any).locatieType || 'Praktijk'}</span>
            </div>

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Tarief</span>
              <span className="col-span-2 text-slate-800 dark:text-white font-semibold text-xs flex items-center space-x-1">
                <span className={`px-2.5 py-0.5 rounded-full text-[10px] font-bold ${
                  selectedAfspraakData.tariefType === 'ELP' 
                    ? 'bg-brand-100 dark:bg-brand-950 text-brand-800 dark:text-brand-300 border border-brand-200 dark:border-brand-800' 
                    : 'bg-slate-100 dark:bg-brand-950/80 text-slate-700 dark:text-brand-200 border border-slate-200 dark:border-brand-800'
                }`}>
                  {selectedAfspraakData.tariefType === 'ELP' ? 'ELP (Eerstelijnszorg)' : 'Regulier'}
                </span>
              </span>
            </div>

            <div className="grid grid-cols-3 gap-2 items-center">
              <span className="text-slate-500 dark:text-brand-300 font-medium text-xs">Status</span>
              <span className="col-span-2">
                <span 
                  className="py-1 px-3 rounded-full text-xs font-bold text-white shadow-xs inline-block"
                  style={{ backgroundColor: selectedAfspraakData.kleurcode || '#478d96' }}
                >
                  {selectedAfspraakData.status}
                </span>
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
            </div>

            <div className="flex justify-between items-center pt-4 mt-2 border-t border-slate-100 dark:border-brand-800/40 shrink-0">
              <button
                onClick={() => handleDeleteBooking(selectedAfspraakData.id)}
                className="text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 hover:bg-red-50 dark:hover:bg-red-950/60 text-xs font-semibold flex items-center space-x-1.5 py-2.5 px-3 rounded-2xl transition cursor-pointer"
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
                  className="bg-brand-600 hover:bg-brand-700 dark:bg-brand-600 dark:hover:bg-brand-500 text-white font-bold py-2.5 px-5 rounded-2xl text-xs transition-all flex items-center space-x-2 shadow-md shadow-brand-500/15 cursor-pointer"
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
