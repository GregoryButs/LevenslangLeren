import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { authApi } from '../services/api';
import { User } from '../types';
import { Lock, Mail, UserIcon, Loader2, CheckCircle2, Sun, Moon } from 'lucide-react';
import { extractErrorMessage } from '../utils/errorUtils';
import { isValidEmail } from '../utils/validationUtils';

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

  const handleExternalLogin = (provider: 'Google' | 'Microsoft' | 'Facebook' | 'Apple') => {
    setError(null);
    const returnUrl = encodeURIComponent(`${window.location.origin}/external-auth-callback`);
    window.location.href = `/api/auth/external-login?provider=${provider}&returnUrl=${returnUrl}`;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!isValidEmail(email)) {
      setError('Voer een geldig e-mailadres in.');
      return;
    }

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
        const user = await authApi.login(email, password);
        setUser(user);
        localStorage.setItem('user', JSON.stringify(user));
        navigate('/portal');
      }
    } catch (err: any) {
      console.error(err);
      setError(extractErrorMessage(err, 'Registreren mislukt. Mogelijk bestaat dit e-mailadres al.'));
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

            {/* Scheidingslijn Social Login */}
            <div className="relative my-6">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-slate-200 dark:border-brand-800/80"></div>
              </div>
              <div className="relative flex justify-center text-xs uppercase">
                <span className="bg-white/80 dark:bg-brand-900/90 px-3 text-slate-400 dark:text-brand-400 font-medium tracking-wider">
                  Of registreer snel via
                </span>
              </div>
            </div>

            {/* Grid van 4 Social Login Knoppen */}
            <div className="grid grid-cols-2 gap-3">
              {/* Google */}
              <button
                type="button"
                aria-label="Registreren met Google"
                onClick={() => handleExternalLogin('Google')}
                className="flex items-center justify-center py-2.5 px-3 border border-slate-200 dark:border-brand-800 rounded-2xl bg-white/80 dark:bg-brand-950/60 hover:bg-slate-50 dark:hover:bg-brand-800/60 text-slate-700 dark:text-brand-100 font-medium text-xs sm:text-sm transition-all duration-200 hover:shadow-md active:scale-95 cursor-pointer"
              >
                <svg className="h-4 w-4 sm:h-5 sm:w-5 mr-2 shrink-0" viewBox="0 0 24 24">
                  <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
                  <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
                  <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" />
                  <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" />
                </svg>
                Google
              </button>

              {/* Microsoft */}
              <button
                type="button"
                aria-label="Registreren met Microsoft"
                onClick={() => handleExternalLogin('Microsoft')}
                className="flex items-center justify-center py-2.5 px-3 border border-slate-200 dark:border-brand-800 rounded-2xl bg-white/80 dark:bg-brand-950/60 hover:bg-slate-50 dark:hover:bg-brand-800/60 text-slate-700 dark:text-brand-100 font-medium text-xs sm:text-sm transition-all duration-200 hover:shadow-md active:scale-95 cursor-pointer"
              >
                <svg className="h-4 w-4 sm:h-5 sm:w-5 mr-2 shrink-0" viewBox="0 0 23 23">
                  <path fill="#f35022" d="M0 0h11v11H0z" />
                  <path fill="#80bb0a" d="M12 0h11v11H12z" />
                  <path fill="#00a1f1" d="M0 12h11v11H0z" />
                  <path fill="#ffb900" d="M12 12h11v11H12z" />
                </svg>
                Microsoft
              </button>

              {/* Facebook */}
              <button
                type="button"
                aria-label="Registreren met Facebook"
                onClick={() => handleExternalLogin('Facebook')}
                className="flex items-center justify-center py-2.5 px-3 border border-slate-200 dark:border-brand-800 rounded-2xl bg-white/80 dark:bg-brand-950/60 hover:bg-slate-50 dark:hover:bg-brand-800/60 text-slate-700 dark:text-brand-100 font-medium text-xs sm:text-sm transition-all duration-200 hover:shadow-md active:scale-95 cursor-pointer"
              >
                <svg className="h-4 w-4 sm:h-5 sm:w-5 mr-2 shrink-0" fill="#1877F2" viewBox="0 0 24 24">
                  <path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z"/>
                </svg>
                Facebook
              </button>

              {/* Apple */}
              <button
                type="button"
                aria-label="Registreren met Apple"
                onClick={() => handleExternalLogin('Apple')}
                className="flex items-center justify-center py-2.5 px-3 border border-slate-200 dark:border-brand-800 rounded-2xl bg-white/80 dark:bg-brand-950/60 hover:bg-slate-50 dark:hover:bg-brand-800/60 text-slate-700 dark:text-brand-100 font-medium text-xs sm:text-sm transition-all duration-200 hover:shadow-md active:scale-95 cursor-pointer"
              >
                <svg className="h-4 w-4 sm:h-5 sm:w-5 mr-2 shrink-0 fill-current text-slate-900 dark:text-white" viewBox="0 0 170 170">
                  <path d="M150.37 130.25c-2.45 5.66-5.35 10.87-8.71 15.66-4.58 6.53-8.33 11.05-11.22 13.56-4.48 4.12-9.28 6.23-14.42 6.35-3.69 0-8.14-1.05-13.32-3.18-5.19-2.12-9.97-3.17-14.34-3.17-4.58 0-9.49 1.05-14.75 3.17-5.26 2.13-9.5 3.24-12.74 3.35-4.34.13-9.16-1.9-14.48-6.08-3.32-2.67-7.23-7.3-11.73-13.88-6.19-9.06-11.05-19.14-14.58-30.24-3.53-11.1-5.3-21.72-5.3-31.86 0-14.46 3.65-26.24 10.95-35.34 7.3-9.1 16.5-13.74 27.6-13.92 4.97.13 10.37 1.34 16.2 3.64 5.83 2.3 9.87 3.51 12.11 3.64 2.24 0 6.47-1.34 12.68-4.02 6.21-2.68 11.73-3.9 16.56-3.64 12.61.76 22.56 5.56 29.87 14.38-11.27 6.83-16.79 16.32-16.57 28.47.22 9.61 3.93 17.59 11.13 23.94 7.2 6.35 15.93 9.82 26.2 10.4-2.45 7.42-5.71 14.8-9.78 22.14zM119.22 31.42c0-7.07 2.53-13.94 7.6-20.61 5.07-6.67 11.51-10.8 19.32-12.4 1.04 8.04-1.33 15.34-7.1 21.9-5.77 6.56-12.4 10.5-19.82 11.11z"/>
                </svg>
                Apple
              </button>
            </div>

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
