import React from 'react';
import { X, Shield, Lock, FileText, CheckCircle2 } from 'lucide-react';

interface CookiePolicyModalProps {
  onClose: () => void;
}

export const CookiePolicyModal: React.FC<CookiePolicyModalProps> = ({ onClose }) => {
  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/60 backdrop-blur-sm p-4 overflow-y-auto">
      <div className="bg-white dark:bg-brand-950 rounded-3xl max-w-3xl w-full p-6 sm:p-10 border border-slate-200 dark:border-brand-800 shadow-2xl space-y-6 my-8 max-h-[85vh] overflow-y-auto animate-fade-in text-slate-800 dark:text-brand-100">
        
        {/* Header */}
        <div className="flex items-center justify-between border-b border-slate-100 dark:border-brand-900 pb-5">
          <div className="flex items-center space-x-3">
            <div className="h-10 w-10 rounded-2xl bg-brand-100 dark:bg-brand-900 text-brand-600 dark:text-brand-400 flex items-center justify-center">
              <Shield className="h-5 w-5" />
            </div>
            <div>
              <h2 className="text-xl font-bold text-slate-900 dark:text-brand-50">Cookiebeleid & Privacy (AVG / GDPR)</h2>
              <p className="text-xs text-slate-500 dark:text-brand-400">Praktijk De Verstandhouding • Laatst bijgewerkt: juli 2026</p>
            </div>
          </div>
          <button 
            onClick={onClose}
            className="p-2 rounded-full text-slate-400 hover:text-slate-600 dark:hover:text-brand-200 transition"
          >
            <X className="h-6 w-6" />
          </button>
        </div>

        {/* Content Body */}
        <div className="space-y-6 text-sm leading-relaxed text-slate-600 dark:text-brand-200">
          
          <section className="space-y-2">
            <h3 className="text-base font-bold text-slate-900 dark:text-brand-50 flex items-center">
              <FileText className="h-4 w-4 mr-2 text-brand-600 dark:text-brand-400" /> 1. Algemeen & Verantwoordelijke
            </h3>
            <p>
              Dit Cookiebeleid is van toepassing op de website en het afsprakenbeheerportaal van <strong>Praktijk De Verstandhouding</strong> (gevestigd te Geraardsbergsestraat 68, 1570 Galmaarden, België, bereikbaar via <a href="mailto:inge@deverstandhouding.be" className="text-brand-600 dark:text-brand-400 underline">inge@deverstandhouding.be</a>).
            </p>
            <p>
              Wij hechten de hoogste waarde aan de bescherming van uw persoonsgegevens en uw medische privacy volgens de Europese Algemene Verordening Gegevensbescherming (AVG / GDPR).
            </p>
          </section>

          <section className="space-y-3">
            <h3 className="text-base font-bold text-slate-900 dark:text-brand-50 flex items-center">
              <Lock className="h-4 w-4 mr-2 text-brand-600 dark:text-brand-400" /> 2. Wat zijn cookies en lokale opslag?
            </h3>
            <p>
              Cookies zijn kleine tekstbestanden die door uw browser worden opgeslagen op uw computer of mobiel apparaat wanneer u onze website bezoekt. Daarnaast maken wij gebruik van <code>localStorage</code> om uw gekozen thema (donkere/lichte modus) en uw cookie-voorkeuren lokaal op uw apparaat te bewaren.
            </p>
          </section>

          <section className="space-y-3">
            <h3 className="text-base font-bold text-slate-900 dark:text-brand-50">3. Overzicht van gebruikte Cookies</h3>
            
            <div className="overflow-x-auto border border-slate-200 dark:border-brand-800/60 rounded-2xl">
              <table className="w-full text-left text-xs border-collapse">
                <thead>
                  <tr className="bg-slate-50 dark:bg-brand-900/60 border-b border-slate-200 dark:border-brand-800/60 font-bold text-slate-700 dark:text-brand-100">
                    <th className="p-3">Naam</th>
                    <th className="p-3">Type</th>
                    <th className="p-3">Doel</th>
                    <th className="p-3">Bewaartermijn</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 dark:divide-brand-900/60">
                  <tr>
                    <td className="p-3 font-mono text-brand-700 dark:text-brand-300 font-bold">.AspNetCore.Identity.Application</td>
                    <td className="p-3 font-semibold text-emerald-600 dark:text-emerald-400">Noodzakelijk</td>
                    <td className="p-3">Beveiligt en onthoudt uw ingelogde sessie in het patiëntenportaal.</td>
                    <td className="p-3">Sessie / 14 dagen</td>
                  </tr>
                  <tr>
                    <td className="p-3 font-mono text-brand-700 dark:text-brand-300 font-bold">cookie_consent_preferences</td>
                    <td className="p-3 font-semibold text-emerald-600 dark:text-emerald-400">Noodzakelijk</td>
                    <td className="p-3">Slaat uw gekozen cookie-toestemmingen op.</td>
                    <td className="p-3">1 jaar</td>
                  </tr>
                  <tr>
                    <td className="p-3 font-mono text-brand-700 dark:text-brand-300 font-bold">theme</td>
                    <td className="p-3 font-semibold text-emerald-600 dark:text-emerald-400">Noodzakelijk</td>
                    <td className="p-3">Onthoudt uw voorkeur voor Donkere of Lichte weergave.</td>
                    <td className="p-3">Onbeperkt</td>
                  </tr>
                  <tr>
                    <td className="p-3 font-mono text-brand-700 dark:text-brand-300 font-bold">_analytics_session</td>
                    <td className="p-3 font-semibold text-blue-600 dark:text-blue-400">Optioneel (Analyse)</td>
                    <td className="p-3">Anonieme bezoekersstatistieken om de website-ervaring te verbeteren.</td>
                    <td className="p-3">30 dagen</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </section>

          <section className="space-y-2">
            <h3 className="text-base font-bold text-slate-900 dark:text-brand-50 flex items-center">
              <CheckCircle2 className="h-4 w-4 mr-2 text-brand-600 dark:text-brand-400" /> 4. Uw Rechten onder de AVG / GDPR
            </h3>
            <p>
              U heeft op elk moment het recht om:
            </p>
            <ul className="list-disc pl-5 space-y-1 text-xs sm:text-sm">
              <li>Uw gegeven toestemming voor optionele cookies op elk gewenste moment **intrekken of te wijzigen**.</li>
              <li>Inzage te vragen in de persoonsgegevens die wij van u verwerken.</li>
              <li>Correctie of volledige verwijdering van uw patiëntenprofiel te verzoeken.</li>
            </ul>
          </section>

          <section className="space-y-2 pt-2">
            <h3 className="text-base font-bold text-slate-900 dark:text-brand-50">5. Contact voor Privacyfragen</h3>
            <p className="text-xs sm:text-sm">
              Voor vragen over ons privacy- of cookiebeleid kunt u contact opnemen met de praktijk via <a href="mailto:inge@deverstandhouding.be" className="text-brand-600 dark:text-brand-400 underline font-bold">inge@deverstandhouding.be</a>.
            </p>
          </section>

        </div>

        {/* Footer Actions */}
        <div className="pt-4 border-t border-slate-100 dark:border-brand-900 flex justify-end">
          <button
            type="button"
            onClick={onClose}
            className="px-6 py-2.5 bg-brand-600 hover:bg-brand-700 text-white font-bold text-sm rounded-xl transition shadow-md"
          >
            Sluiten
          </button>
        </div>

      </div>
    </div>
  );
};
