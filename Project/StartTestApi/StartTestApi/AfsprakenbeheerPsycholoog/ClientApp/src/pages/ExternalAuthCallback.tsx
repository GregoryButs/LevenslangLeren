import React, { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { authApi } from '../services/api';
import { User } from '../types';
import { Loader2 } from 'lucide-react';

interface ExternalAuthCallbackProps {
  setUser: (user: User | null) => void;
}

export const ExternalAuthCallback: React.FC<ExternalAuthCallbackProps> = ({ setUser }) => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  useEffect(() => {
    const status = searchParams.get('status');
    const message = searchParams.get('message');

    if (status === 'success') {
      authApi.me()
        .then((user) => {
          setUser(user);
          localStorage.setItem('user', JSON.stringify(user));
          if (user.isPsycholoog) {
            navigate('/dashboard', { replace: true });
          } else {
            navigate('/portal', { replace: true });
          }
        })
        .catch((err) => {
          console.error('Fout bij ophalen gebruikersprofiel via me():', err);
          navigate('/login?error=' + encodeURIComponent('Inloggen is gelukt, maar uw gebruikersprofiel kon niet worden geladen.'), { replace: true });
        });
    } else {
      const errorMsg = message || 'Inloggen via de externe provider is mislukt of geannuleerd.';
      navigate('/login?error=' + encodeURIComponent(errorMsg), { replace: true });
    }
  }, [searchParams, navigate, setUser]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 dark:bg-brand-950 text-slate-800 dark:text-brand-100 p-4 transition-colors duration-300">
      <div className="max-w-md w-full bg-white/70 dark:bg-brand-900/80 backdrop-blur-md p-8 rounded-3xl shadow-xl border border-white/40 dark:border-brand-800/40 text-center space-y-6">
        <Loader2 className="h-12 w-12 animate-spin text-brand-600 dark:text-brand-400 mx-auto" />
        <div className="space-y-2">
          <h2 className="text-xl font-bold text-slate-800 dark:text-white">Authenticatie verifiëren</h2>
          <p className="text-sm text-slate-500 dark:text-brand-300">
            Een moment geduld alstublieft, uw inloggegevens via de gekozen inlogprovider worden gecontroleerd...
          </p>
        </div>
      </div>
    </div>
  );
};
