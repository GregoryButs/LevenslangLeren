import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../services/api';
import { User } from '../types';
import { Lock, Mail, UserIcon, Loader2, CheckCircle2 } from 'lucide-react';

interface RegisterProps {
  setUser: (user: User | null) => void;
}

export const Register: React.FC<RegisterProps> = ({ setUser }) => {
  const [voornaam, setVoornaam] = useState('');
  const [achternaam, setAchternaam] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [confirmationSent, setConfirmationSent] = useState(false);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (password !== confirmPassword) {
      setError('Wachtwoorden komen niet overeen.');
      return;
    }

    if (password.length < 6) {
      setError('Wachtwoord moet minimaal 6 tekens bevatten.');
      return;
    }

    setLoading(true);

    try {
      const res: any = await authApi.register(voornaam, achternaam, email, password);
      if (res && res.requireEmailConfirmation) {
        setConfirmationSent(true);
      } else {
        setUser(res);
        localStorage.setItem('user', JSON.stringify(res));
        navigate('/portal');
      }
    } catch (err: any) {
      console.error(err);
      if (err.response?.data) {
        const errors = err.response.data;
        if (typeof errors === 'object') {
          const firstErrorKey = Object.keys(errors)[0];
          const errorMsg = Array.isArray(errors[firstErrorKey]) 
            ? errors[firstErrorKey][0] 
            : errors[firstErrorKey];
          setError(errorMsg || 'Registreren mislukt. Mogelijk bestaat dit e-mailadres al.');
        } else {
          setError('Registreren mislukt.');
        }
      } else {
        setError('Er is een onbekende fout opgetreden.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-brand-950 text-slate-800 dark:text-brand-100 py-12 px-4 sm:px-6 lg:px-8 relative overflow-hidden transition-colors duration-300">
      {/* Decorative Background Elements */}
      <div className="absolute top-0 left-0 w-96 h-96 bg-brand-100 dark:bg-brand-900 rounded-full mix-blend-multiply filter blur-3xl opacity-30 -translate-x-20 -translate-y-20"></div>
      <div className="absolute bottom-0 right-0 w-96 h-96 bg-emerald-100 dark:bg-brand-800 rounded-full mix-blend-multiply filter blur-3xl opacity-20 translate-x-20 translate-y-20"></div>

      <div className="max-w-md w-full space-y-8 bg-white/70 dark:bg-brand-900/80 backdrop-blur-md p-8 rounded-3xl shadow-xl border border-white/40 dark:border-brand-800/40 relative z-10">
        <div className="text-center">
          <Link to="/" className="inline-block cursor-pointer transition-all duration-200 hover:scale-105 group" title="Ga naar de landingspagina van De Verstandhouding">
            <img src="/images/logo_normal.png" alt="De Verstandhouding" className="h-32 w-auto mx-auto mb-4 object-contain dark:hidden group-hover:opacity-90" />
            <img 
              src="/images/logo_dark_full.png" 
              onError={(e) => { e.currentTarget.src = '/images/logo_dark_full.svg'; }}
              alt="De Verstandhouding" 
              className="h-32 w-auto mx-auto mb-4 object-contain hidden dark:block group-hover:opacity-90 rounded-2xl shadow-md" 
            />
          </Link>
          <p className="mt-4 text-center text-sm text-slate-600 dark:text-brand-100 font-medium">
            Registreer uzelf om online afspraken te boeken
          </p>
        </div>

        {confirmationSent ? (
          <div className="py-6 text-center space-y-4">
            <div className="h-14 w-14 rounded-full bg-brand-100 dark:bg-brand-800 text-brand-600 dark:text-brand-300 flex items-center justify-center mx-auto">
              <CheckCircle2 className="h-8 w-8" />
            </div>
            <h3 className="text-lg font-bold text-slate-800 dark:text-brand-50">Bevestigingsmail Verzonden!</h3>
            <p className="text-sm text-slate-600 dark:text-brand-200 leading-relaxed">
              We hebben een e-mail gestuurd naar <strong className="text-brand-600 dark:text-brand-400">{email}</strong>.
            </p>
            <p className="text-xs text-slate-500 dark:text-brand-300">
              Klik op de verificatielink in de mail om uw account te activeren.
            </p>
            <div className="pt-4">
              <Link to="/login" className="inline-block w-full py-3 px-4 bg-brand-600 hover:bg-brand-700 text-white font-semibold rounded-2xl text-sm transition">
                Naar Inloggen
              </Link>
            </div>
          </div>
        ) : (
          <>
            <div className="p-4 rounded-2xl bg-brand-50/80 dark:bg-brand-950/80 border border-brand-200/80 dark:border-brand-700/60 flex flex-col space-y-1">
              <p className="text-xs text-slate-700 dark:text-brand-100 leading-relaxed text-center font-medium">
                <strong className="text-brand-700 dark:text-brand-300 font-bold block mb-0.5">Let op:</strong> 
                Na het verzenden van de registratie ontvangt u een e-mail met een bevestigingslink om uw account te activeren.
              </p>
            </div>

            {error && (
              <div className="bg-red-50 dark:bg-red-950/60 border-l-4 border-red-500 p-4 rounded-xl">
                <div className="flex">
                  <div className="text-red-700 dark:text-red-300 text-sm font-medium">{error}</div>
                </div>
              </div>
            )}

            <form className="mt-8 space-y-4" onSubmit={handleSubmit}>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label htmlFor="voornaam" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                    Voornaam
                  </label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-300">
                      <UserIcon className="h-5 w-5" />
                    </div>
                    <input
                      id="voornaam"
                      type="text"
                      required
                      value={voornaam}
                      onChange={(e) => setVoornaam(e.target.value)}
                      className="pl-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                      placeholder="Jan"
                    />
                  </div>
                </div>

                <div>
                  <label htmlFor="achternaam" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                    Achternaam
                  </label>
                  <input
                    id="achternaam"
                    type="text"
                    required
                    value={achternaam}
                    onChange={(e) => setAchternaam(e.target.value)}
                    className="block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                    placeholder="Janssens"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="email" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                  E-mailadres
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-300">
                    <Mail className="h-5 w-5" />
                  </div>
                  <input
                    id="email"
                    type="email"
                    required
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    className="pl-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                    placeholder="jan@voorbeeld.be"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="password" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                  Wachtwoord
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-300">
                    <Lock className="h-5 w-5" />
                  </div>
                  <input
                    id="password"
                    type="password"
                    required
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    className="pl-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                    placeholder="Minimaal 6 tekens"
                  />
                </div>
              </div>

              <div>
                <label htmlFor="confirm-password" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                  Wachtwoord bevestigen
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-300">
                    <Lock className="h-5 w-5" />
                  </div>
                  <input
                    id="confirm-password"
                    type="password"
                    required
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    className="pl-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                    placeholder="Bevestig wachtwoord"
                  />
                </div>
              </div>

              <div className="pt-2">
                <button
                  type="submit"
                  disabled={loading}
                  className="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-semibold rounded-2xl text-white bg-brand-600 hover:bg-brand-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-brand-500 transition shadow-lg shadow-brand-500/10 disabled:opacity-50"
                >
                  {loading ? (
                    <Loader2 className="animate-spin h-5 w-5 text-white" />
                  ) : (
                    'Registreren'
                  )}
                </button>
              </div>
            </form>

            <div className="text-center mt-6">
              <span className="text-sm text-slate-600 dark:text-brand-200">Heeft u al een account? </span>
              <Link to="/login" className="text-sm font-bold text-brand-600 dark:text-brand-300 hover:text-brand-700 dark:hover:text-white transition underline">
                Inloggen
              </Link>
            </div>
          </>
        )}
      </div>
    </div>
  );
};
