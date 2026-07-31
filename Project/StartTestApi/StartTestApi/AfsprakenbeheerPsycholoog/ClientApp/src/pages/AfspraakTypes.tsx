import React, { useState, useEffect } from 'react';
import { afspraakTypeApi } from '../services/api';
import { AfspraakType } from '../types';
import { 
  Settings, Plus, Edit2, Trash2, Clock, 
  ShieldCheck, Loader2 
} from 'lucide-react';
import { extractErrorMessage } from '../utils/errorUtils';

export const AfspraakTypes: React.FC = () => {
  const [types, setTypes] = useState<AfspraakType[]>([]);
  const [loading, setLoading] = useState(true);

  // Form Modal State
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [formType, setFormType] = useState<any>({
    id: null,
    naam: '',
    standaardDuurMinuten: 60,
    kleurcode: '#6c757d',
    vereistPatient: true
  });

  const loadTypes = async () => {
    try {
      setLoading(true);
      const data = await afspraakTypeApi.getAll();
      setTypes(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTypes();
  }, []);

  const handleOpenCreateModal = () => {
    setFormType({
      id: null,
      naam: '',
      standaardDuurMinuten: 60,
      kleurcode: '#3b82f6',
      vereistPatient: true
    });
    setIsModalOpen(true);
  };

  const handleOpenEditModal = (type: AfspraakType) => {
    setFormType({
      id: type.id,
      naam: type.naam,
      standaardDuurMinuten: type.standaardDuurMinuten,
      kleurcode: type.kleurcode,
      vereistPatient: type.vereistPatient
    });
    setIsModalOpen(true);
  };

  const handleSaveType = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (formType.id === null) {
        const { id, ...createData } = formType;
        await afspraakTypeApi.create(createData);
        alert('Afspraaktype succesvol aangemaakt!');
      } else {
        await afspraakTypeApi.update(formType.id, formType);
        alert('Afspraaktype succesvol bijgewerkt!');
      }
      setIsModalOpen(false);
      loadTypes();
    } catch (err) {
      console.error(err);
      alert(extractErrorMessage(err, 'Opslaan mislukt.'));
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Weet u zeker dat u dit afspraaktype wilt verwijderen?')) return;
    try {
      await afspraakTypeApi.delete(id);
      alert('Afspraaktype succesvol verwijderd.');
      loadTypes();
    } catch (err) {
      console.error(err);
      alert(extractErrorMessage(err, 'Verwijderen mislukt (mogelijks zijn er nog afspraken van dit type).'));
    }
  };

  if (loading && types.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-[80vh]">
        <Loader2 className="animate-spin h-10 w-10 text-brand-600" />
      </div>
    );
  }

  return (
    <div className="p-4 sm:p-6 max-w-7xl mx-auto space-y-6 sm:space-y-8 overflow-x-hidden">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between bg-white dark:bg-brand-900 p-4 sm:p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm gap-4 transition-colors">
        <div>
          <h1 className="text-2xl font-bold text-slate-800 dark:text-brand-50 flex items-center space-x-2">
            <Settings className="h-7 w-7 text-brand-600 dark:text-brand-400" />
            <span>Afspraaktypes</span>
          </h1>
          <p className="text-slate-500 dark:text-brand-300 mt-1 text-sm sm:text-base">
            Configureer consultatietypes, kleurencodes en tijdsduur.
          </p>
        </div>
        <button
          onClick={handleOpenCreateModal}
          className="flex items-center justify-center space-x-2 bg-brand-600 hover:bg-brand-700 text-white font-semibold py-3 px-6 rounded-2xl transition shadow-lg shadow-brand-500/10 w-full sm:w-auto"
        >
          <Plus className="h-5 w-5" />
          <span>Nieuw Type</span>
        </button>
      </div>

      {/* Grid listing */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {types.map((t) => (
          <div 
            key={t.id} 
            className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-4 hover:shadow-md transition relative overflow-hidden"
          >
            {/* Color Strip Indicator */}
            <div className="absolute top-0 left-0 w-full h-2" style={{ backgroundColor: t.kleurcode }}></div>

            <div className="flex justify-between items-start">
              <div>
                <h3 className="text-lg font-bold text-slate-800 dark:text-white">{t.naam}</h3>
                <span className="text-[10px] font-mono bg-slate-100 dark:bg-brand-950 text-slate-500 dark:text-brand-300 py-0.5 px-2 rounded-full mt-1 inline-block">
                  ID: {t.id}
                </span>
              </div>
              <div className="flex space-x-1.5">
                <button
                  onClick={() => handleOpenEditModal(t)}
                  className="p-1.5 bg-slate-50 dark:bg-brand-950 hover:bg-slate-100 dark:hover:bg-brand-800 border border-slate-200 dark:border-brand-800 text-slate-600 dark:text-brand-200 rounded-lg transition"
                >
                  <Edit2 className="h-3.5 w-3.5" />
                </button>
                <button
                  onClick={() => handleDelete(t.id)}
                  className="p-1.5 bg-red-50 dark:bg-red-950/60 hover:bg-red-100 border border-red-100 dark:border-red-900/40 text-red-600 dark:text-red-300 rounded-lg transition"
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </button>
              </div>
            </div>

            <div className="space-y-2 text-sm text-slate-600 dark:text-brand-200 pt-2 border-t border-slate-50 dark:border-brand-800/40">
              <div className="flex items-center space-x-2">
                <Clock className="h-4 w-4 text-slate-400 dark:text-brand-400" />
                <span>Standaardduur: <strong className="text-slate-800 dark:text-white">{t.standaardDuurMinuten} minuten</strong></span>
              </div>
              <div className="flex items-center space-x-2">
                <ShieldCheck className="h-4 w-4 text-slate-400 dark:text-brand-400" />
                <span>
                  Patiënt vereist:{' '}
                  <strong className={t.vereistPatient ? 'text-brand-600 dark:text-brand-300' : 'text-amber-600 dark:text-amber-400'}>
                    {t.vereistPatient ? 'Ja (Consultatie)' : 'Nee (Blokkering/Pauze)'}
                  </strong>
                </span>
              </div>
            </div>

            <div className="flex items-center space-x-2">
              <span className="text-xs text-slate-400 dark:text-brand-400 font-medium">Kleurcode:</span>
              <span className="h-4 w-4 rounded-full border border-slate-200 dark:border-brand-800" style={{ backgroundColor: t.kleurcode }}></span>
              <span className="text-xs text-slate-600 dark:text-brand-200 font-mono">{t.kleurcode}</span>
            </div>
          </div>
        ))}
      </div>

      {/* Form Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 dark:bg-black/70 backdrop-blur-sm p-4 animate-in fade-in duration-200">
          <div className="bg-white dark:bg-brand-900 rounded-3xl shadow-2xl w-full max-w-md p-6 border border-slate-100 dark:border-brand-800/40 transition-colors">
            <h3 className="text-xl font-bold text-slate-800 dark:text-white mb-4">
              {formType.id === null ? 'Afspraaktype Aanmaken' : 'Afspraaktype Bewerken'}
            </h3>
            <form onSubmit={handleSaveType} className="space-y-4">
              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Naam</label>
                <input
                  type="text"
                  required
                  value={formType.naam}
                  onChange={(e) => setFormType({ ...formType, naam: e.target.value })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400"
                  placeholder="Bijv. Intake, Evaluatie, Supervisie"
                />
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Standaardduur (minuten)</label>
                <input
                  type="number"
                  min={5}
                  max={480}
                  required
                  value={formType.standaardDuurMinuten}
                  onChange={(e) => setFormType({ ...formType, standaardDuurMinuten: Number(e.target.value) })}
                  className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white"
                />
              </div>

              <div>
                <label className="text-sm font-semibold text-slate-600 dark:text-brand-200 block mb-1">Kleurcode (HEX)</label>
                <div className="flex space-x-2">
                  <input
                    type="color"
                    value={formType.kleurcode}
                    onChange={(e) => setFormType({ ...formType, kleurcode: e.target.value })}
                    className="h-10 w-12 bg-transparent border-0 cursor-pointer p-0"
                  />
                  <input
                    type="text"
                    required
                    pattern="^#([A-Fa-f0-9]{6})$"
                    value={formType.kleurcode}
                    onChange={(e) => setFormType({ ...formType, kleurcode: e.target.value })}
                    className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-2.5 px-4 rounded-xl text-slate-800 dark:text-white font-mono placeholder-slate-400 dark:placeholder-brand-400"
                    placeholder="#3b82f6"
                  />
                </div>
              </div>

              <div className="flex items-center space-x-2 pt-2 select-none">
                <input
                  id="vereistPatient"
                  type="checkbox"
                  checked={formType.vereistPatient}
                  onChange={(e) => setFormType({ ...formType, vereistPatient: e.target.checked })}
                  className="h-4 w-4 text-brand-600 focus:ring-brand-500 border-slate-300 dark:border-brand-700 rounded-lg transition"
                />
                <label htmlFor="vereistPatient" className="text-sm font-semibold text-slate-600 dark:text-brand-200 cursor-pointer">
                  Patiënt vereist (dit type is een consultatie met een patiënt)
                </label>
              </div>

              <div className="flex justify-end space-x-3 pt-4 border-t border-slate-100 dark:border-brand-800/40">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="bg-slate-100 dark:bg-brand-800 hover:bg-slate-200 dark:hover:bg-brand-700 text-slate-700 dark:text-white py-2.5 px-5 rounded-xl font-semibold transition"
                >
                  Annuleren
                </button>
                <button
                  type="submit"
                  className="bg-brand-600 hover:bg-brand-700 text-white py-2.5 px-5 rounded-xl font-semibold transition shadow-sm"
                >
                  Opslaan
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
