import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams, Link } from 'react-router-dom';
import { authApi } from '../services/api';
import { Lock, Loader2, Sun, Moon, Eye, EyeOff, CheckCircle2, AlertCircle } from 'lucide-react';
import { extractErrorMessage } from '../utils/errorUtils';

export const ResetPassword: React.FC = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const email = searchParams.get('email') || '';
  const token = searchParams.get('token') || '';

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(false);

  const [isDark, setIsDark] = useState(() => {
    const saved = localStorage.getItem('theme');
    return saved === 'dark' || (!saved && window.matchMedia('(prefers-color-scheme: dark)').matches);
  });

  useEffect(() => {
    if (isDark) {
      document.documentElement.classList.add('dark');
      localStorage.setItem('theme', 'dark');
    } else {
      document.documentElement.classList.remove('dark');
      localStorage.setItem('theme', 'light');
    }
  }, [isDark]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!email || !token) {
      setError('Ongeldige of ontbrekende herstel-token. Vraag een nieuwe herstellink aan.');
      return;
    }

    if (newPassword.length < 6) {
      setError('Het nieuwe wachtwoord moet minimaal 6 tekens bevatten.');
      return;
    }

    if (newPassword !== confirmPassword) {
      setError('De wachtwoorden komen niet overeen.');
      return;
    }

    setLoading(true);

    try {
      await authApi.resetPassword(email, token, newPassword);
      setSuccess(true);
    } catch (err: any) {
      console.error('ResetPassword error:', err);
      setError(extractErrorMessage(err, 'Wachtwoord herstellen mislukt. Mogelijk is de link verlopen.'));
    } finally {
      setLoading(false);
    }
  };

  const isInvalidUrl = !email || !token;

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-brand-950 text-slate-800 dark:text-brand-100 py-12 px-4 sm:px-6 lg:px-8 relative overflow-hidden transition-colors duration-300">
      {/* Dark Mode Toggle Button */}
      <button
        onClick={() => setIsDark(!isDark)}
        className="absolute top-5 right-5 z-50 p-2.5 bg-white/80 dark:bg-brand-900/80 backdrop-blur-md border border-slate-200 dark:border-brand-800 rounded-2xl shadow-sm text-slate-600 dark:text-brand-200 hover:scale-105 transition cursor-pointer"
        title={isDark ? "Schakel naar lichte modus" : "Schakel naar donkere modus"}
      >
        {isDark ? <Sun className="h-5 w-5 text-amber-400" /> : <Moon className="h-5 w-5 text-slate-600" />}
      </button>

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
          <h1 className="text-2xl font-bold text-slate-900 dark:text-white">
            Nieuw Wachtwoord Instellen
          </h1>
          <p className="mt-2 text-sm text-slate-600 dark:text-brand-200">
            Voer hieronder uw nieuwe wachtwoord in om uw account te beveiligen.
          </p>
        </div>

        {isInvalidUrl ? (
          <div className="space-y-6">
            <div role="alert" className="bg-amber-50 dark:bg-amber-950/60 border border-amber-200 dark:border-amber-800 p-5 rounded-2xl flex items-start space-x-3">
              <AlertCircle className="h-6 w-6 text-amber-600 dark:text-amber-400 shrink-0 mt-0.5" />
              <div className="text-amber-800 dark:text-amber-200 text-sm font-medium leading-relaxed">
                Ongeldige of incomplete herstellink. Zorg ervoor dat u op de volledige link in de e-mail heeft geklikt.
              </div>
            </div>
            <div className="text-center">
              <Link
                to="/forgot-password"
                className="inline-flex items-center justify-center py-3 px-6 border border-transparent rounded-2xl text-sm font-semibold text-white bg-brand-600 hover:bg-brand-700 transition shadow-lg shadow-brand-500/10"
              >
                Opnieuw wachtwoordherstel aanvragen
              </Link>
            </div>
          </div>
        ) : success ? (
          <div className="space-y-6">
            <div role="status" className="bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-200 dark:border-emerald-800 p-5 rounded-2xl flex items-start space-x-3">
              <CheckCircle2 className="h-6 w-6 text-emerald-600 dark:text-emerald-400 shrink-0 mt-0.5" />
              <div className="text-emerald-800 dark:text-emerald-200 text-sm font-medium leading-relaxed">
                Uw wachtwoord is succesvol gewijzigd! U kunt nu inloggen met uw nieuwe wachtwoord.
              </div>
            </div>
            <div className="text-center">
              <button
                type="button"
                onClick={() => navigate('/login')}
                className="w-full py-3 px-6 border border-transparent rounded-2xl text-sm font-semibold text-white bg-brand-600 hover:bg-brand-700 transition shadow-lg shadow-brand-500/10"
              >
                Naar inlogpagina
              </button>
            </div>
          </div>
        ) : (
          <>
            {error && (
              <div id="reset-password-error" role="alert" className="bg-red-50 dark:bg-red-950/60 border-l-4 border-red-500 p-4 rounded-xl">
                <div className="text-red-700 dark:text-red-300 text-sm font-medium">{error}</div>
              </div>
            )}

            <form className="mt-8 space-y-5" onSubmit={handleSubmit}>
              <div>
                <label htmlFor="new-password" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                  Nieuw Wachtwoord
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-300">
                    <Lock className="h-5 w-5" />
                  </div>
                  <input
                    id="new-password"
                    name="newPassword"
                    type={showPassword ? "text" : "password"}
                    required
                    minLength={6}
                    aria-required="true"
                    aria-describedby={error ? "reset-password-error" : undefined}
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    className="pl-10 pr-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                    placeholder="••••••••"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword(!showPassword)}
                    className="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-400 hover:text-slate-600 dark:text-brand-300 dark:hover:text-white"
                  >
                    {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                  </button>
                </div>
              </div>

              <div>
                <label htmlFor="confirm-password" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                  Bevestig Nieuw Wachtwoord
                </label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-300">
                    <Lock className="h-5 w-5" />
                  </div>
                  <input
                    id="confirm-password"
                    name="confirmPassword"
                    type={showConfirmPassword ? "text" : "password"}
                    required
                    minLength={6}
                    aria-required="true"
                    aria-describedby={error ? "reset-password-error" : undefined}
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    className="pl-10 pr-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                    placeholder="••••••••"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    className="absolute inset-y-0 right-0 pr-3 flex items-center text-slate-400 hover:text-slate-600 dark:text-brand-300 dark:hover:text-white"
                  >
                    {showConfirmPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                  </button>
                </div>
              </div>

              <div>
                <button
                  type="submit"
                  disabled={loading}
                  className="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-semibold rounded-2xl text-white bg-brand-600 hover:bg-brand-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-brand-500 transition shadow-lg shadow-brand-500/10 disabled:opacity-50"
                >
                  {loading ? (
                    <Loader2 className="animate-spin h-5 w-5 text-white" />
                  ) : (
                    'Wachtwoord Opslaan'
                  )}
                </button>
              </div>
            </form>
          </>
        )}
      </div>
    </div>
  );
};
