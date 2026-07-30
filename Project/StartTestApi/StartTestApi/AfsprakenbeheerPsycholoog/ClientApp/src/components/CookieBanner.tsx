import React, { useState, useEffect } from 'react';
import { ShieldCheck, Cookie, Settings, X, Lock, Info } from 'lucide-react';
import { CookiePolicyModal } from './CookiePolicyModal';

export interface CookiePreferences {
  necessary: boolean;
  analytics: boolean;
  marketing: boolean;
  savedAt: string;
}

const DEFAULT_PREFERENCES: CookiePreferences = {
  necessary: true,
  analytics: false,
  marketing: false,
  savedAt: ''
};

export const CookieBanner: React.FC = () => {
  const [isVisible, setIsVisible] = useState(false);
  const [showPreferencesModal, setShowPreferencesModal] = useState(false);
  const [showPolicyModal, setShowPolicyModal] = useState(false);
  const [preferences, setPreferences] = useState<CookiePreferences>(DEFAULT_PREFERENCES);

  useEffect(() => {
    const savedConsent = localStorage.getItem('cookie_consent_preferences');
    if (!savedConsent) {
      setIsVisible(true);
    } else {
      try {
        setPreferences(JSON.parse(savedConsent));
      } catch {
        setIsVisible(true);
      }
    }

    // Listen for custom trigger to re-open settings from footer
    const handleOpenSettings = () => {
      setIsVisible(true);
      setShowPreferencesModal(true);
    };
    window.addEventListener('open-cookie-settings', handleOpenSettings);
    return () => window.removeEventListener('open-cookie-settings', handleOpenSettings);
  }, []);

  const saveConsent = (prefs: CookiePreferences) => {
    const updated = { ...prefs, savedAt: new Date().toISOString() };
    localStorage.setItem('cookie_consent_preferences', JSON.stringify(updated));
    setPreferences(updated);
    setIsVisible(false);
    setShowPreferencesModal(false);
  };

  const handleAcceptAll = () => {
    saveConsent({
      necessary: true,
      analytics: true,
      marketing: true,
      savedAt: ''
    });
  };

  const handleAcceptNecessaryOnly = () => {
    saveConsent({
      necessary: true,
      analytics: false,
      marketing: false,
      savedAt: ''
    });
  };

  if (!isVisible && !showPreferencesModal && !showPolicyModal) {
    return null;
  }

  return (
    <>
      {/* Main Cookie Consent Banner (Sticky at bottom) */}
      {isVisible && !showPreferencesModal && (
        <div className="fixed bottom-0 inset-x-0 z-50 p-4 sm:p-6 bg-white/95 dark:bg-brand-950/95 backdrop-blur-lg border-t border-brand-100 dark:border-brand-800 shadow-2xl transition-all animate-slide-up">
          <div className="max-w-7xl mx-auto flex flex-col lg:flex-row items-start lg:items-center justify-between gap-6">
            
            {/* Banner Text & Icon */}
            <div className="flex items-start space-x-4 max-w-3xl">
              <div className="h-12 w-12 rounded-2xl bg-brand-100 dark:bg-brand-900 text-brand-700 dark:text-brand-300 flex items-center justify-center shrink-0 mt-0.5">
                <Cookie className="h-6 w-6" />
              </div>
              <div className="space-y-1">
                <div className="flex items-center space-x-2">
                  <h3 className="text-base font-bold text-slate-900 dark:text-brand-50">
                    Cookie- & Privacyinstellingen (AVG / GDPR)
                  </h3>
                  <span className="inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold bg-brand-100 dark:bg-brand-900 text-brand-700 dark:text-brand-300">
                    <ShieldCheck className="h-3 w-3 mr-1" /> Privacy Borging
                  </span>
                </div>
                <p className="text-xs sm:text-sm text-slate-600 dark:text-brand-300 leading-relaxed">
                  Praktijk <strong>De Verstandhouding</strong> gebruikt functionele cookies om de website en het afsprakenportaal veilig te laten werken. Met uw toestemming gebruiken we optionele cookies voor geanonimiseerde analyse. 
                  Lees meer in ons{' '}
                  <button 
                    onClick={() => setShowPolicyModal(true)} 
                    className="text-brand-600 dark:text-brand-400 font-bold underline hover:text-brand-700"
                  >
                    Cookiebeleid
                  </button>.
                </p>
              </div>
            </div>

            {/* Buttons (Equal weight according to GDPR directives) */}
            <div className="flex flex-wrap sm:flex-nowrap items-center gap-3 w-full lg:w-auto shrink-0 justify-end">
              <button
                type="button"
                onClick={handleAcceptNecessaryOnly}
                className="w-full sm:w-auto px-5 py-2.5 rounded-xl border border-slate-200 dark:border-brand-800 text-slate-700 dark:text-brand-200 hover:bg-slate-100 dark:hover:bg-brand-900 text-xs sm:text-sm font-semibold transition"
              >
                Alleen Noodzakelijk
              </button>

              <button
                type="button"
                onClick={() => setShowPreferencesModal(true)}
                className="w-full sm:w-auto px-4 py-2.5 rounded-xl border border-brand-200 dark:border-brand-800 text-brand-700 dark:text-brand-300 hover:bg-brand-50 dark:hover:bg-brand-900/60 text-xs sm:text-sm font-semibold flex items-center justify-center transition"
              >
                <Settings className="h-4 w-4 mr-1.5" />
                Voorkeuren
              </button>

              <button
                type="button"
                onClick={handleAcceptAll}
                className="w-full sm:w-auto px-6 py-2.5 rounded-xl bg-brand-600 hover:bg-brand-700 text-white text-xs sm:text-sm font-bold transition shadow-lg shadow-brand-500/20"
              >
                Alles Accepteren
              </button>
            </div>

          </div>
        </div>
      )}

      {/* Preferences Customization Modal */}
      {showPreferencesModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4">
          <div className="bg-white dark:bg-brand-950 rounded-3xl max-w-xl w-full p-6 sm:p-8 border border-slate-200 dark:border-brand-800 shadow-2xl space-y-6 max-h-[90vh] overflow-y-auto animate-fade-in">
            
            {/* Modal Header */}
            <div className="flex items-center justify-between border-b border-slate-100 dark:border-brand-900 pb-4">
              <div className="flex items-center space-x-3">
                <Settings className="h-6 w-6 text-brand-600 dark:text-brand-400" />
                <h3 className="text-lg font-bold text-slate-900 dark:text-brand-50">Cookie Voorkeuren Beheren</h3>
              </div>
              <button 
                onClick={() => setShowPreferencesModal(false)}
                className="p-1 rounded-full text-slate-400 hover:text-slate-600 dark:hover:text-brand-200"
              >
                <X className="h-5 w-5" />
              </button>
            </div>

            <p className="text-xs sm:text-sm text-slate-600 dark:text-brand-300">
              U kunt hieronder aangeven welke categorieën cookies u wilt toestaan. Uw voorkeur kan op elk moment weer worden gewijzigd via de link in de voettekst.
            </p>

            {/* Preference Categories */}
            <div className="space-y-4">
              
              {/* Category 1: Noodzakelijk (Altijd Actief) */}
              <div className="p-4 rounded-2xl bg-slate-50 dark:bg-brand-900/50 border border-slate-200 dark:border-brand-800/60 space-y-2">
                <div className="flex items-center justify-between">
                  <div className="flex items-center space-x-2">
                    <Lock className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
                    <span className="font-bold text-sm text-slate-800 dark:text-brand-100">Noodzakelijk & Functioneel</span>
                  </div>
                  <span className="text-[11px] font-bold uppercase tracking-wider px-2.5 py-1 rounded-full bg-emerald-100 dark:bg-emerald-950 text-emerald-700 dark:text-emerald-300">
                    Altijd Actief
                  </span>
                </div>
                <p className="text-xs text-slate-500 dark:text-brand-400 leading-relaxed">
                  Vereist voor het inloggen, afspraken boeken, beveiligen van formulieren en het bewaren van uw sessie. Deze kunnen niet worden uitgeschakeld.
                </p>
              </div>

              {/* Category 2: Analytisch */}
              <div className="p-4 rounded-2xl bg-white dark:bg-brand-900/20 border border-slate-200 dark:border-brand-800 space-y-2">
                <div className="flex items-center justify-between">
                  <span className="font-bold text-sm text-slate-800 dark:text-brand-100">Analytische Cookies</span>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input 
                      type="checkbox" 
                      checked={preferences.analytics}
                      onChange={(e) => setPreferences({ ...preferences, analytics: e.target.checked })}
                      className="sr-only peer"
                    />
                    <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-brand-800 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-brand-600"></div>
                  </label>
                </div>
                <p className="text-xs text-slate-500 dark:text-brand-400 leading-relaxed">
                  Helpt ons te begrijpen hoe bezoekers de website gebruiken (geanonimiseerde statistieken zonder persoonsgegevens).
                </p>
              </div>

              {/* Category 3: Marketing / Externe Integraties */}
              <div className="p-4 rounded-2xl bg-white dark:bg-brand-900/20 border border-slate-200 dark:border-brand-800 space-y-2">
                <div className="flex items-center justify-between">
                  <span className="font-bold text-sm text-slate-800 dark:text-brand-100">Marketing & Integraties</span>
                  <label className="relative inline-flex items-center cursor-pointer">
                    <input 
                      type="checkbox" 
                      checked={preferences.marketing}
                      onChange={(e) => setPreferences({ ...preferences, marketing: e.target.checked })}
                      className="sr-only peer"
                    />
                    <div className="w-11 h-6 bg-slate-200 peer-focus:outline-none rounded-full peer dark:bg-brand-800 peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-brand-600"></div>
                  </label>
                </div>
                <p className="text-xs text-slate-500 dark:text-brand-400 leading-relaxed">
                  Gebruikt voor eventuele embedded kaarten (zoals Google Maps) of externe media-integraties.
                </p>
              </div>

            </div>

            {/* Modal Actions */}
            <div className="flex items-center justify-between pt-4 border-t border-slate-100 dark:border-brand-900">
              <button
                type="button"
                onClick={() => setShowPolicyModal(true)}
                className="text-xs font-semibold text-brand-600 dark:text-brand-400 hover:underline flex items-center"
              >
                <Info className="h-3.5 w-3.5 mr-1" /> Bekijk Cookiebeleid
              </button>

              <button
                type="button"
                onClick={() => saveConsent(preferences)}
                className="px-6 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-xs sm:text-sm rounded-xl transition shadow-md"
              >
                Voorkeuren Opslaan
              </button>
            </div>

          </div>
        </div>
      )}

      {/* Full Cookie Policy Modal */}
      {showPolicyModal && (
        <CookiePolicyModal onClose={() => setShowPolicyModal(false)} />
      )}
    </>
  );
};
