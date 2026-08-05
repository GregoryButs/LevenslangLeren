import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authApi } from '../services/api';
import { User } from '../types';
import { Calendar, Phone, ShieldCheck, ArrowRight, Loader2, AlertCircle } from 'lucide-react';
import { extractErrorMessage } from '../utils/errorUtils';

interface CompleteProfileProps {
  user: User;
  setUser: (user: User | null) => void;
}

export const CompleteProfile: React.FC<CompleteProfileProps> = ({ user, setUser }) => {
  const navigate = useNavigate();
  const [geboortedatum, setGeboortedatum] = useState('');
  const [telefoonnummer, setTelefoonnummer] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!geboortedatum) {
      setError('Vul je geboortedatum in om door te gaan.');
      return;
    }

    setError(null);
    setLoading(true);

    try {
      const updatedUser = await authApi.completeProfile({
        geboortedatum,
        telefoonnummer: telefoonnummer.trim() || null
      });

      setUser(updatedUser);
      localStorage.setItem('user', JSON.stringify(updatedUser));
      navigate('/portal', { replace: true });
    } catch (err) {
      console.error('Fout bij voltooien profiel:', err);
      setError(extractErrorMessage(err, 'Opslaan mislukt. Controleer de ingevulde gegevens.'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 dark:bg-brand-950 flex flex-col justify-center py-12 sm:px-6 lg:px-8 transition-colors duration-300">
      <div className="sm:mx-auto sm:w-full sm:max-w-md text-center">
        <div className="flex justify-center mb-4">
          <img src="/images/logo_hero.png" alt="De Verstandhouding" className="h-16 w-auto object-contain dark:hidden" />
          <img 
            src="/images/logo_dark_compact.png" 
            onError={(e) => { e.currentTarget.src = '/images/logo_dark_compact.svg'; }}
            alt="De Verstandhouding" 
            className="h-16 w-auto object-contain hidden dark:block rounded-2xl overflow-hidden shadow-md" 
          />
        </div>
        
        <div className="inline-flex items-center space-x-2 px-3 py-1.5 rounded-full bg-brand-50 dark:bg-brand-900/60 border border-brand-200 dark:border-brand-800 text-brand-700 dark:text-brand-300 text-xs font-semibold mb-3">
          <ShieldCheck className="h-4 w-4 text-brand-600 dark:text-brand-400" />
          <span>Eenmalige Registratiestap</span>
        </div>

        <h2 className="text-2xl sm:text-3xl font-extrabold text-slate-800 dark:text-white tracking-tight">
          Voltooi je profiel
        </h2>
        <p className="mt-2 text-xs sm:text-sm text-slate-500 dark:text-brand-300 max-w-sm mx-auto leading-relaxed">
          Beste <strong className="text-slate-700 dark:text-brand-100">{user.voornaam}</strong>, om je account bij <strong>Praktijk De Verstandhouding</strong> veilig in te stellen is je geboortedatum vereist.
        </p>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md px-4 sm:px-0">
        <div className="bg-white dark:bg-brand-900 py-8 px-6 sm:px-10 shadow-xl rounded-3xl border border-slate-100 dark:border-brand-800/40 transition-colors">
          {error && (
            <div className="mb-6 p-4 bg-red-50 dark:bg-red-950/70 border border-red-200 dark:border-red-800/60 rounded-2xl text-red-700 dark:text-red-300 text-xs flex items-start space-x-3">
              <AlertCircle className="w-5 h-5 text-red-500 shrink-0 mt-0.5" />
              <div>
                <p className="font-bold uppercase tracking-wide">Fout bij opslaan</p>
                <p className="mt-0.5">{error}</p>
              </div>
            </div>
          )}

          {/* Waarom vragen we je geboortedatum? */}
          <div className="mb-6 p-4 bg-slate-50 dark:bg-brand-950/60 border border-slate-200/60 dark:border-brand-800/60 rounded-2xl text-xs space-y-2">
            <h3 className="font-bold text-slate-700 dark:text-brand-100 flex items-center">
              <ShieldCheck className="h-4 w-4 mr-1.5 text-brand-600 dark:text-brand-400" />
              Waarom vragen we je geboortedatum?
            </h3>
            <p className="text-slate-500 dark:text-brand-300 leading-relaxed">
              Om je zorgdossier veilig, nauwkeurig en AVG-proof te beheren, hebben we je geboortedatum nodig. Dit helpt om:
            </p>
            <ul className="list-disc pl-4 space-y-1.5 text-slate-500 dark:text-brand-300">
              <li>
                <strong>Verwisselingen te voorkomen:</strong> Zo weten we zeker dat we jou nooit verwarren met een andere patiënt met dezelfde naam.
              </li>
              <li>
                <strong>Wettelijke richtlijnen te volgen:</strong> Dit is verplicht voor een correcte administratie en om te controleren of er bij minderjarigen toestemming van ouders/verzorgers nodig is.
              </li>
            </ul>
          </div>

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label htmlFor="geboortedatum" className="block text-xs font-bold uppercase tracking-wider text-slate-600 dark:text-brand-200 mb-2 flex items-center">
                <Calendar className="h-4 w-4 mr-1.5 text-brand-600 dark:text-brand-400" />
                Geboortedatum <span className="text-red-500 ml-1 font-bold">*</span>
              </label>
              <input
                id="geboortedatum"
                type="date"
                required
                value={geboortedatum}
                onChange={(e) => setGeboortedatum(e.target.value)}
                className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-3 px-4 rounded-2xl text-slate-800 dark:text-white font-medium focus:ring-2 focus:ring-brand-500 outline-none transition"
              />
              <p className="mt-1.5 text-[11px] text-slate-400 dark:text-brand-400">
                Noodzakelijk voor de opbouw van je medisch patiëntendossier.
              </p>
            </div>

            <div>
              <label htmlFor="telefoonnummer" className="block text-xs font-bold uppercase tracking-wider text-slate-600 dark:text-brand-200 mb-2 flex items-center">
                <Phone className="h-4 w-4 mr-1.5 text-brand-600 dark:text-brand-400" />
                Telefoonnummer <span className="text-xs font-normal text-slate-400 dark:text-brand-400 lowercase ml-1">(optioneel)</span>
              </label>
              <input
                id="telefoonnummer"
                type="tel"
                value={telefoonnummer}
                onChange={(e) => setTelefoonnummer(e.target.value)}
                placeholder="0471 00 00 00"
                className="w-full bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 py-3 px-4 rounded-2xl text-slate-800 dark:text-white font-medium focus:ring-2 focus:ring-brand-500 outline-none transition placeholder-slate-400 dark:placeholder-brand-500"
              />
              <p className="mt-1.5 text-[11px] text-slate-400 dark:text-brand-400">
                Handig voor SMS-herinneringen voor je afspraken.
              </p>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full flex justify-center items-center space-x-2 py-3.5 px-4 border border-transparent rounded-2xl text-sm font-bold text-white bg-brand-600 hover:bg-brand-700 active:scale-[0.99] transition shadow-lg shadow-brand-500/20 disabled:opacity-50"
            >
              {loading ? (
                <>
                  <Loader2 className="animate-spin h-5 w-5" />
                  <span>Profiel Opslaan...</span>
                </>
              ) : (
                <>
                  <span>Profiel Opslaan & Doorgaan</span>
                  <ArrowRight className="h-4 w-4" />
                </>
              )}
            </button>
          </form>
        </div>
      </div>
    </div>
  );
};
