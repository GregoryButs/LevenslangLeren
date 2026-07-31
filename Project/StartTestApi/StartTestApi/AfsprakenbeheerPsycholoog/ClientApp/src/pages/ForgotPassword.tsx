import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { authApi } from '../services/api';
import { Mail, Loader2, ArrowLeft, Sun, Moon, CheckCircle2 } from 'lucide-react';
import { extractErrorMessage } from '../utils/errorUtils';
import { isValidEmail } from '../utils/validationUtils';

export const ForgotPassword: React.FC = () => {
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
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
    setSuccessMessage(null);

    if (!isValidEmail(email)) {
      setError('Voer een geldig e-mailadres in.');
      return;
    }

    setLoading(true);

    try {
      const res = await authApi.forgotPassword(email);
      setSuccessMessage(res.message || 'Indien het e-mailadres bij ons bekend is, heeft u een e-mail ontvangen met instructies om uw wachtwoord te herstellen.');
    } catch (err: any) {
      console.error('ForgotPassword error:', err);
      setError(extractErrorMessage(err, 'Er is een fout opgetreden bij het aanvragen van het wachtwoordherstel.'));
    } finally {
      setLoading(false);
    }
  };

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
            Wachtwoord vergeten?
          </h1>
          <p className="mt-2 text-sm text-slate-600 dark:text-brand-200">
            Voer het e-mailadres van uw account in. Wij sturen u een e-mail met een link om een nieuw wachtwoord in te stellen.
          </p>
        </div>

        {error && (
          <div id="forgot-password-error" role="alert" className="bg-red-50 dark:bg-red-950/60 border-l-4 border-red-500 p-4 rounded-xl">
            <div className="text-red-700 dark:text-red-300 text-sm font-medium">{error}</div>
          </div>
        )}

        {successMessage ? (
          <div className="space-y-6">
            <div role="status" className="bg-emerald-50 dark:bg-emerald-950/60 border border-emerald-200 dark:border-emerald-800 p-5 rounded-2xl flex items-start space-x-3">
              <CheckCircle2 className="h-6 w-6 text-emerald-600 dark:text-emerald-400 shrink-0 mt-0.5" />
              <div className="text-emerald-800 dark:text-emerald-200 text-sm font-medium leading-relaxed">
                {successMessage}
              </div>
            </div>
            <p className="text-xs text-slate-500 dark:text-brand-300 text-center">
              Geen e-mail ontvangen? Controleer ook uw ongewenste e-mail (spam) of probeer het opnieuw.
            </p>
            <div className="pt-2 text-center">
              <Link
                to="/login"
                className="inline-flex items-center justify-center py-3 px-6 border border-slate-200 dark:border-brand-700 rounded-2xl text-sm font-semibold text-slate-700 dark:text-brand-100 bg-white/80 dark:bg-brand-950/80 hover:bg-slate-100 dark:hover:bg-brand-800 transition"
              >
                <ArrowLeft className="h-4 w-4 mr-2" />
                Terug naar inloggen
              </Link>
            </div>
          </div>
        ) : (
          <form className="mt-8 space-y-6" onSubmit={handleSubmit}>
            <div>
              <label htmlFor="email-address" className="text-sm font-semibold text-slate-700 dark:text-brand-100 block mb-1">
                E-mailadres
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-300">
                  <Mail className="h-5 w-5" />
                </div>
                <input
                  id="email-address"
                  name="email"
                  type="email"
                  required
                  aria-required="true"
                  aria-describedby={error ? "forgot-password-error" : undefined}
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="pl-10 block w-full rounded-2xl border border-slate-200 dark:border-brand-700/70 bg-white/60 dark:bg-brand-950/80 py-3 px-4 text-slate-900 dark:text-white placeholder-slate-400 dark:placeholder-brand-300/60 focus:outline-none focus:ring-2 focus:ring-brand-500 focus:border-transparent transition shadow-xs"
                  placeholder="naam@voorbeeld.be"
                />
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
                  'Herstellink versturen'
                )}
              </button>
            </div>

            <div className="text-center pt-2">
              <Link
                to="/login"
                className="inline-flex items-center text-sm font-medium text-brand-600 dark:text-brand-400 hover:text-brand-700 dark:hover:text-brand-300 transition group"
              >
                <ArrowLeft className="h-4 w-4 mr-1.5 transition-transform group-hover:-translate-x-1" />
                Terug naar inloggen
              </Link>
            </div>
          </form>
        )}
      </div>
    </div>
  );
};
