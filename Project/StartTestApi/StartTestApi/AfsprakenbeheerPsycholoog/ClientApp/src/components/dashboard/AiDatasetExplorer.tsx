import React from 'react';
import { Database, Search, Loader2 } from 'lucide-react';
import { InfoTooltip } from '../common/InfoTooltip';

interface AiDatasetExplorerProps {
  syntheticPatients: any[];
  synTotal: number;
  synPage: number;
  synPageSize: number;
  synSearch: string;
  synLoading: boolean;
  onSearchChange: (search: string) => void;
  onPageChange: (page: number) => void;
}

export const AiDatasetExplorer: React.FC<AiDatasetExplorerProps> = ({
  syntheticPatients,
  synTotal,
  synPage,
  synPageSize,
  synSearch,
  synLoading,
  onSearchChange,
  onPageChange,
}) => {
  const totalPages = Math.ceil(synTotal / synPageSize);

  return (
    <div className="bg-white rounded-3xl border border-slate-100 shadow-sm p-6 space-y-6">
      <div className="flex justify-between items-center border-b border-slate-100 pb-4">
        <div>
          <h2 className="text-lg font-bold text-slate-800 flex items-center space-x-2">
            <Database className="h-5 w-5 text-brand-600" />
            <span>Dataset Explorer (5000 Synthetische Cliënten)</span>
            <InfoTooltip content="Toont de geanonimiseerde dataset van 5.000 gesimuleerde patiëntenprofielen. Deze dataset wordt gebruikt om de machine learning en regression-modellen te trainen." />
          </h2>
          <p className="text-slate-500 text-xs mt-1">Blader live door de geanonimiseerde profielen gegenereerd door de Data-Strategist.</p>
        </div>

        {/* Search Input */}
        <div className="flex items-center space-x-2 bg-slate-50 border border-slate-200 py-1.5 px-3 rounded-xl w-64 focus-within:ring-2 focus-within:ring-brand-500">
          <Search className="h-4 w-4 text-slate-400" />
          <input
            type="text"
            placeholder="Zoeken op naam of ID..."
            value={synSearch}
            onChange={(e) => onSearchChange(e.target.value)}
            className="bg-transparent text-xs text-slate-700 w-full focus:outline-none"
          />
        </div>
      </div>

      {synLoading ? (
        <div className="flex justify-center items-center py-16">
          <Loader2 className="animate-spin h-8 w-8 text-brand-600" />
        </div>
      ) : syntheticPatients.length > 0 ? (
        <div className="space-y-4">
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-slate-100 text-slate-400 text-xs font-semibold uppercase">
                  <th className="py-3 px-4">Client ID</th>
                  <th className="py-3 px-4">Naam</th>
                  <th className="py-3 px-4">Leeftijd</th>
                  <th className="py-3 px-4">Tussenpoos (Gap)</th>
                  <th className="py-3 px-4">Behandeling</th>
                  <th className="py-3 px-4">Sessies</th>
                  <th className="py-3 px-4 text-center">
                    <span className="inline-block align-middle">No-Show Risico</span>
                    <InfoTooltip position="bottom" content="De door het ML-regressiemodel berekende no-show kans (0-100%) voor dit gesimuleerde profiel." />
                  </th>
                  <th className="py-3 px-4 text-center">
                    <span className="inline-block align-middle">No-Show Status</span>
                    <InfoTooltip position="bottom" content="De uiteindelijke, werkelijke uitkomst van de afspraak (Aanwezig of No-Show) in de dataset." />
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 text-sm">
                {syntheticPatients.map((p: any) => (
                  <tr key={p.clientId} className="hover:bg-slate-50/50">
                    <td className="py-4 px-4 font-mono font-semibold text-xs text-slate-500">{p.clientId}</td>
                    <td className="py-4 px-4 font-bold text-slate-800">{p.name}</td>
                    <td className="py-4 px-4 text-slate-600">{p.age} jr</td>
                    <td className="py-4 px-4 text-slate-600">{p.lastSessionGap} dagen</td>
                    <td className="py-4 px-4">
                      <span className="capitalize px-2 py-0.5 rounded-full text-xs font-semibold bg-brand-50 text-brand-700">
                        {p.treatmentType}
                      </span>
                    </td>
                    <td className="py-4 px-4 text-slate-600">{p.sessionsCompleted}</td>
                    <td className="py-4 px-4 text-center font-semibold text-slate-750">{(p.noShowProbability * 100).toFixed(1)}%</td>
                    <td className="py-4 px-4 text-center">
                      <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-bold ${
                        p.noShow === 1 ? 'bg-red-100 text-red-800' : 'bg-emerald-100 text-emerald-800'
                      }`}>
                        {p.noShow === 1 ? 'No-Show' : 'Aanwezig'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Pagination Controls */}
          <div className="flex justify-between items-center pt-4 border-t border-slate-100 text-xs text-slate-500">
            <span>
              Tonen van <strong>{((synPage - 1) * synPageSize) + 1}</strong> tot <strong>{Math.min(synPage * synPageSize, synTotal)}</strong> van de <strong>{synTotal}</strong> records
            </span>
            <div className="flex space-x-2">
              <button
                onClick={() => onPageChange(Math.max(1, synPage - 1))}
                disabled={synPage === 1}
                className="py-1.5 px-3 rounded-lg border border-slate-200 hover:bg-slate-50 disabled:opacity-50 disabled:hover:bg-transparent font-semibold transition cursor-pointer"
              >
                Vorige
              </button>
              <span className="py-1.5 px-3 bg-slate-50 border border-slate-200 rounded-lg text-slate-700 font-bold">
                Pagina {synPage} van {totalPages || 1}
              </span>
              <button
                onClick={() => onPageChange(Math.min(totalPages, synPage + 1))}
                disabled={synPage >= totalPages}
                className="py-1.5 px-3 rounded-lg border border-slate-200 hover:bg-slate-50 disabled:opacity-50 disabled:hover:bg-transparent font-semibold transition cursor-pointer"
              >
                Volgende
              </button>
            </div>
          </div>
        </div>
      ) : (
        <div className="text-center py-12 text-slate-400">Geen gesimuleerde data gevonden voor uw zoekopdracht.</div>
      )}
    </div>
  );
};
