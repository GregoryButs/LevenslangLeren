import React, { useState, useEffect, useMemo } from 'react';
import { elpApi } from '../services/api';
import { ElpMaandoverzicht } from '../types';
import { 
  Calendar, CheckCircle2, Clock, AlertTriangle, Copy, Check, 
  Download, Search, Loader2, ChevronLeft, ChevronRight,
  ShieldAlert, FileSpreadsheet
} from 'lucide-react';

const MAANDEN = [
  'Januari', 'Februari', 'Maart', 'April', 'Mei', 'Juni',
  'Juli', 'Augustus', 'September', 'Oktober', 'November', 'December'
];

export const ElpMaandafsluiting: React.FC = () => {
  const now = new Date();
  const [jaar, setJaar] = useState(now.getFullYear());
  const [maand, setMaand] = useState(now.getMonth() + 1); // 1-12
  const [data, setData] = useState<ElpMaandoverzicht | null>(null);
  const [loading, setLoading] = useState(true);
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<'alle' | 'te_verwerken' | 'verwerkt'>('alle');
  const [copiedId, setCopiedId] = useState<number | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const fetchMaandoverzicht = async () => {
    setLoading(true);
    try {
      const res = await elpApi.getMaandoverzicht(jaar, maand);
      setData(res);
      setSelectedIds([]);
    } catch (err) {
      console.error('Fout bij laden ELP maandoverzicht:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMaandoverzicht();
  }, [jaar, maand]);

  const handlePrevMonth = () => {
    if (maand === 1) {
      setMaand(12);
      setJaar(jaar - 1);
    } else {
      setMaand(maand - 1);
    }
  };

  const handleNextMonth = () => {
    if (maand === 12) {
      setMaand(1);
      setJaar(jaar + 1);
    } else {
      setMaand(maand + 1);
    }
  };

  const handleCopyRijksregister = (id: number, rr: string | null) => {
    if (!rr) return;
    const cleanNumber = rr.replace(/[^0-9]/g, '');
    navigator.clipboard.writeText(cleanNumber);
    setCopiedId(id);
    setTimeout(() => setCopiedId(null), 2000);
  };

  const handleToggleSingleStatus = async (id: number) => {
    try {
      await elpApi.toggleStatus(id);
      await fetchMaandoverzicht();
    } catch (err) {
      console.error(err);
    }
  };

  const handleBulkMarkeerVerwerkt = async () => {
    if (selectedIds.length === 0) return;
    setActionLoading(true);
    try {
      await elpApi.markeerVerwerkt(selectedIds);
      await fetchMaandoverzicht();
    } catch (err) {
      console.error(err);
    } finally {
      setActionLoading(false);
    }
  };

  const handleBulkMarkeerTeVerwerken = async () => {
    if (selectedIds.length === 0) return;
    setActionLoading(true);
    try {
      await elpApi.markeerTeVerwerken(selectedIds);
      await fetchMaandoverzicht();
    } catch (err) {
      console.error(err);
    } finally {
      setActionLoading(false);
    }
  };

  // Export naar CSV
  const handleExportCsv = () => {
    if (!data || data.afspraken.length === 0) return;

    const headers = ['Datum', 'Starttijd', 'Eindtijd', 'Patient', 'Dossiernummer', 'Rijksregisternummer', 'ELP Type', 'Traject Teller', 'Status'];
    const rows = data.afspraken.map(item => {
      const dt = new Date(item.starttijd);
      const datumStr = dt.toLocaleDateString('nl-BE');
      const startTijdStr = dt.toLocaleTimeString('nl-BE', { hour: '2-digit', minute: '2-digit' });
      const eindTijdStr = new Date(item.eindtijd).toLocaleTimeString('nl-BE', { hour: '2-digit', minute: '2-digit' });
      
      return [
        `"${datumStr}"`,
        `"${startTijdStr}"`,
        `"${eindTijdStr}"`,
        `"${item.patientNaam.replace(/"/g, '""')}"`,
        `"${item.dossierNummer}"`,
        `"${item.rijksregisternummer || ''}"`,
        `"${item.elpType}"`,
        `"${item.elpSessieTeller}"`,
        `"${item.elpStatus === 'Verwerkt' ? 'Verwerkt' : 'Te verwerken'}"`
      ].join(';');
    });

    const csvContent = 'data:text/csv;charset=utf-8,\uFEFF' + [headers.join(';'), ...rows].join('\n');
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement('a');
    link.setAttribute('href', encodedUri);
    link.setAttribute('download', `ELP_Maandafsluiting_${MAANDEN[maand - 1]}_${jaar}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  // Gefilterde afspraken
  const gefilterdeAfspraken = useMemo(() => {
    if (!data) return [];
    return data.afspraken.filter(item => {
      // Search filter
      const matchesSearch = 
        item.patientNaam.toLowerCase().includes(searchTerm.toLowerCase()) ||
        item.dossierNummer.toLowerCase().includes(searchTerm.toLowerCase()) ||
        (item.rijksregisternummer && item.rijksregisternummer.includes(searchTerm));

      // Status filter
      const matchesStatus = 
        statusFilter === 'alle' ? true :
        statusFilter === 'te_verwerken' ? item.elpStatus === 'TeVerwerken' :
        item.elpStatus === 'Verwerkt';

      return matchesSearch && matchesStatus;
    });
  }, [data, searchTerm, statusFilter]);

  const allSelected = gefilterdeAfspraken.length > 0 && gefilterdeAfspraken.every(item => selectedIds.includes(item.id));

  const toggleSelectAll = () => {
    if (allSelected) {
      setSelectedIds([]);
    } else {
      setSelectedIds(gefilterdeAfspraken.map(item => item.id));
    }
  };

  const toggleSelectRow = (id: number) => {
    if (selectedIds.includes(id)) {
      setSelectedIds(selectedIds.filter(i => i !== id));
    } else {
      setSelectedIds([...selectedIds, id]);
    }
  };

  return (
    <div className="p-6 sm:p-8 max-w-7xl mx-auto space-y-8 min-h-screen pb-24">
      {/* Header & Maand Selectie */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-xs">
        <div>
          <div className="flex items-center space-x-2 text-brand-600 dark:text-brand-400 font-bold text-xs uppercase tracking-wider mb-1">
            <FileSpreadsheet className="h-4 w-4" />
            <span>Belgische Zorgcontext (Eerstelijnspsychologie)</span>
          </div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-800 dark:text-white tracking-tight">
            ELP Maandafsluiting
          </h1>
          <p className="text-xs sm:text-sm text-slate-500 dark:text-brand-300 mt-1">
            Verwerk alle ELP-sessies van de maand efficiënt voor registratie in het eHealth/ELP-portaal.
          </p>
        </div>

        {/* Maand & Jaar Selector */}
        <div className="flex items-center space-x-3 bg-slate-50 dark:bg-brand-950 p-2 rounded-2xl border border-slate-200/60 dark:border-brand-800/60 self-start md:self-auto">
          <button
            onClick={handlePrevMonth}
            className="p-2 rounded-xl text-slate-600 dark:text-brand-300 hover:bg-white dark:hover:bg-brand-900 transition shadow-2xs"
            title="Vorige maand"
          >
            <ChevronLeft className="h-5 w-5" />
          </button>
          
          <div className="flex items-center space-x-2 px-3 py-1 font-bold text-slate-800 dark:text-white text-sm sm:text-base">
            <Calendar className="h-4 w-4 text-brand-600 dark:text-brand-400 shrink-0" />
            <span>{MAANDEN[maand - 1]} {jaar}</span>
          </div>

          <button
            onClick={handleNextMonth}
            className="p-2 rounded-xl text-slate-600 dark:text-brand-300 hover:bg-white dark:hover:bg-brand-900 transition shadow-2xs"
            title="Volgende maand"
          >
            <ChevronRight className="h-5 w-5" />
          </button>
        </div>
      </div>

      {/* Statistieken Kaarten */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white dark:bg-brand-900 p-5 rounded-2xl border border-slate-100 dark:border-brand-800/40 shadow-xs flex items-center space-x-4">
          <div className="h-12 w-12 rounded-xl bg-brand-50 dark:bg-brand-950 text-brand-600 dark:text-brand-400 flex items-center justify-center shrink-0">
            <Calendar className="h-6 w-6" />
          </div>
          <div>
            <p className="text-xs font-bold text-slate-400 dark:text-brand-400 uppercase tracking-wider">Totaal ELP Sessies</p>
            <p className="text-2xl font-extrabold text-slate-800 dark:text-white">{loading ? '...' : data?.totaalElpSessies ?? 0}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-brand-900 p-5 rounded-2xl border border-slate-100 dark:border-brand-800/40 shadow-xs flex items-center space-x-4">
          <div className="h-12 w-12 rounded-xl bg-amber-50 dark:bg-amber-950/60 text-amber-600 dark:text-amber-400 flex items-center justify-center shrink-0">
            <Clock className="h-6 w-6" />
          </div>
          <div>
            <p className="text-xs font-bold text-slate-400 dark:text-brand-400 uppercase tracking-wider">Te Verwerken</p>
            <p className="text-2xl font-extrabold text-amber-600 dark:text-amber-400">{loading ? '...' : data?.totaalTeVerwerken ?? 0}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-brand-900 p-5 rounded-2xl border border-slate-100 dark:border-brand-800/40 shadow-xs flex items-center space-x-4">
          <div className="h-12 w-12 rounded-xl bg-emerald-50 dark:bg-emerald-950/60 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shrink-0">
            <CheckCircle2 className="h-6 w-6" />
          </div>
          <div>
            <p className="text-xs font-bold text-slate-400 dark:text-brand-400 uppercase tracking-wider">Reeds Verwerkt</p>
            <p className="text-2xl font-extrabold text-emerald-600 dark:text-emerald-400">{loading ? '...' : data?.totaalVerwerkt ?? 0}</p>
          </div>
        </div>

        <div className="bg-white dark:bg-brand-900 p-5 rounded-2xl border border-slate-100 dark:border-brand-800/40 shadow-xs flex items-center space-x-4">
          <div className={`h-12 w-12 rounded-xl flex items-center justify-center shrink-0 ${
            (data?.ontbrekendeRijksregisternummers ?? 0) > 0 
              ? 'bg-rose-50 dark:bg-rose-950/60 text-rose-600 dark:text-rose-400' 
              : 'bg-slate-50 dark:bg-brand-950 text-slate-400 dark:text-brand-400'
          }`}>
            <AlertTriangle className="h-6 w-6" />
          </div>
          <div>
            <p className="text-xs font-bold text-slate-400 dark:text-brand-400 uppercase tracking-wider">Rijksregisternr Ontbreekt</p>
            <p className={`text-2xl font-extrabold ${(data?.ontbrekendeRijksregisternummers ?? 0) > 0 ? 'text-rose-600 dark:text-rose-400' : 'text-slate-800 dark:text-white'}`}>
              {loading ? '...' : data?.ontbrekendeRijksregisternummers ?? 0}
            </p>
          </div>
        </div>
      </div>

      {/* Actiebalk & Filters */}
      <div className="bg-white dark:bg-brand-900 p-4 sm:p-5 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-xs space-y-4">
        <div className="flex flex-col lg:flex-row items-stretch lg:items-center justify-between gap-4">
          {/* Zoekbalk */}
          <div className="relative flex-1 max-w-md">
            <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-slate-400 dark:text-brand-400" />
            <input
              type="text"
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              placeholder="Zoek patiënt, dossiernummer of rijksregisternr..."
              className="w-full pl-10 pr-4 py-2.5 bg-slate-50 dark:bg-brand-950 border border-slate-200 dark:border-brand-800 rounded-2xl text-xs sm:text-sm text-slate-800 dark:text-white font-medium focus:ring-2 focus:ring-brand-500 outline-none"
            />
          </div>

          {/* Status Tabs */}
          <div className="flex items-center space-x-1 bg-slate-50 dark:bg-brand-950 p-1.5 rounded-2xl border border-slate-200/60 dark:border-brand-800/60">
            <button
              onClick={() => setStatusFilter('alle')}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold transition ${
                statusFilter === 'alle' 
                  ? 'bg-white dark:bg-brand-900 text-slate-800 dark:text-white shadow-xs' 
                  : 'text-slate-500 dark:text-brand-300 hover:text-slate-700'
              }`}
            >
              Alle ({data?.totaalElpSessies ?? 0})
            </button>
            <button
              onClick={() => setStatusFilter('te_verwerken')}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold transition ${
                statusFilter === 'te_verwerken' 
                  ? 'bg-amber-500 text-white shadow-xs' 
                  : 'text-amber-600 dark:text-amber-400 hover:bg-amber-50 dark:hover:bg-amber-950/40'
              }`}
            >
              Nog te verwerken ({data?.totaalTeVerwerken ?? 0})
            </button>
            <button
              onClick={() => setStatusFilter('verwerkt')}
              className={`px-3 py-1.5 rounded-xl text-xs font-bold transition ${
                statusFilter === 'verwerkt' 
                  ? 'bg-emerald-600 text-white shadow-xs' 
                  : 'text-emerald-600 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-950/40'
              }`}
            >
              Verwerkt ({data?.totaalVerwerkt ?? 0})
            </button>
          </div>

          {/* Export CSV Knop */}
          <button
            onClick={handleExportCsv}
            disabled={!data || data.afspraken.length === 0}
            className="flex items-center justify-center space-x-2 px-4 py-2.5 rounded-2xl text-xs sm:text-sm font-bold text-slate-700 dark:text-brand-200 bg-slate-100 dark:bg-brand-950 hover:bg-slate-200 dark:hover:bg-brand-800 border border-slate-200 dark:border-brand-800 transition disabled:opacity-40"
          >
            <Download className="h-4 w-4 text-brand-600 dark:text-brand-400" />
            <span>Exporteer CSV</span>
          </button>
        </div>

        {/* Bulk Actions Bar */}
        {selectedIds.length > 0 && (
          <div className="flex flex-wrap items-center justify-between gap-3 p-3 bg-brand-50 dark:bg-brand-950/80 border border-brand-200 dark:border-brand-800 rounded-2xl animate-fade-in">
            <span className="text-xs font-bold text-brand-800 dark:text-brand-200">
              {selectedIds.length} afspraken geselecteerd
            </span>
            <div className="flex items-center space-x-2">
              <button
                onClick={handleBulkMarkeerVerwerkt}
                disabled={actionLoading}
                className="flex items-center space-x-1.5 px-3 py-1.5 bg-emerald-600 hover:bg-emerald-700 text-white text-xs font-bold rounded-xl transition shadow-xs disabled:opacity-50"
              >
                {actionLoading ? <Loader2 className="h-3.5 w-3.5 animate-spin" /> : <CheckCircle2 className="h-3.5 w-3.5" />}
                <span>Markeer als verwerkt</span>
              </button>
              <button
                onClick={handleBulkMarkeerTeVerwerken}
                disabled={actionLoading}
                className="flex items-center space-x-1.5 px-3 py-1.5 bg-amber-500 hover:bg-amber-600 text-white text-xs font-bold rounded-xl transition shadow-xs disabled:opacity-50"
              >
                <Clock className="h-3.5 w-3.5" />
                <span>Markeer als te verwerken</span>
              </button>
            </div>
          </div>
        )}
      </div>

      {/* Data Grid Tabel */}
      <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-xs overflow-hidden">
        {loading ? (
          <div className="p-12 flex flex-col items-center justify-center space-y-3">
            <Loader2 className="h-8 w-8 text-brand-600 dark:text-brand-400 animate-spin" />
            <p className="text-xs font-semibold text-slate-400 dark:text-brand-400">ELP Maandoverzicht laden...</p>
          </div>
        ) : gefilterdeAfspraken.length === 0 ? (
          <div className="p-12 text-center space-y-3">
            <Calendar className="h-10 w-10 text-slate-300 dark:text-brand-600 mx-auto" />
            <p className="text-sm font-bold text-slate-700 dark:text-brand-200">Geen ELP-afspraken gevonden</p>
            <p className="text-xs text-slate-400 dark:text-brand-400 max-w-sm mx-auto">
              Er zijn geen afspraken met tarieftype 'ELP' gevonden voor {MAANDEN[maand - 1]} {jaar}.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left border-collapse">
              <thead>
                <tr className="border-b border-slate-100 dark:border-brand-800/40 bg-slate-50/50 dark:bg-brand-950/40 text-[11px] font-bold uppercase tracking-wider text-slate-500 dark:text-brand-300">
                  <th className="py-4 px-4 w-10 text-center">
                    <input
                      type="checkbox"
                      checked={allSelected}
                      onChange={toggleSelectAll}
                      className="rounded border-slate-300 dark:border-brand-800 text-brand-600 focus:ring-brand-500"
                    />
                  </th>
                  <th className="py-4 px-4">Datum & Tijd</th>
                  <th className="py-4 px-4">Patiënt & Dossier #</th>
                  <th className="py-4 px-4">Rijksregisternummer</th>
                  <th className="py-4 px-4">Type ELP</th>
                  <th className="py-4 px-4">Traject Teller</th>
                  <th className="py-4 px-4 text-center">Status Ingegeven</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-brand-800/30 text-xs font-medium text-slate-700 dark:text-brand-100">
                {gefilterdeAfspraken.map((item) => {
                  const dt = new Date(item.starttijd);
                  const isSelected = selectedIds.includes(item.id);
                  const isCopied = copiedId === item.id;

                  return (
                    <tr 
                      key={item.id} 
                      className={`hover:bg-slate-50/80 dark:hover:bg-brand-800/30 transition ${
                        isSelected ? 'bg-brand-50/40 dark:bg-brand-950/60' : ''
                      }`}
                    >
                      <td className="py-3.5 px-4 text-center">
                        <input
                          type="checkbox"
                          checked={isSelected}
                          onChange={() => toggleSelectRow(item.id)}
                          className="rounded border-slate-300 dark:border-brand-800 text-brand-600 focus:ring-brand-500"
                        />
                      </td>

                      {/* Datum & Tijd */}
                      <td className="py-3.5 px-4 whitespace-nowrap">
                        <p className="font-bold text-slate-800 dark:text-white">
                          {dt.toLocaleDateString('nl-BE', { weekday: 'short', day: 'numeric', month: 'short' })}
                        </p>
                        <p className="text-[11px] text-slate-400 dark:text-brand-400">
                          {dt.toLocaleTimeString('nl-BE', { hour: '2-digit', minute: '2-digit' })} - {new Date(item.eindtijd).toLocaleTimeString('nl-BE', { hour: '2-digit', minute: '2-digit' })}
                        </p>
                      </td>

                      {/* Patiënt & Dossier # */}
                      <td className="py-3.5 px-4 whitespace-nowrap">
                        <p className="font-bold text-slate-800 dark:text-white">{item.patientNaam}</p>
                        <span className="inline-block text-[10px] font-mono bg-slate-100 dark:bg-brand-950 px-2 py-0.5 rounded-md text-slate-500 dark:text-brand-300">
                          {item.dossierNummer}
                        </span>
                      </td>

                      {/* Rijksregisternummer met 1-klik Kopieerknop */}
                      <td className="py-3.5 px-4 whitespace-nowrap">
                        {item.heeftRijksregisternummer ? (
                          <div className="flex items-center space-x-2">
                            <span className="font-mono text-xs font-semibold text-slate-800 dark:text-brand-100 bg-slate-100 dark:bg-brand-950 px-2.5 py-1 rounded-xl border border-slate-200 dark:border-brand-800">
                              {item.rijksregisternummer}
                            </span>
                            <button
                              onClick={() => handleCopyRijksregister(item.id, item.rijksregisternummer)}
                              className={`p-1.5 rounded-xl border transition flex items-center space-x-1 ${
                                isCopied 
                                  ? 'bg-emerald-100 dark:bg-emerald-950 text-emerald-700 dark:text-emerald-300 border-emerald-300' 
                                  : 'bg-white dark:bg-brand-900 border-slate-200 dark:border-brand-800 text-slate-500 dark:text-brand-300 hover:text-brand-600 dark:hover:text-brand-200'
                              }`}
                              title="Kopieer Rijksregisternummer naar klembord"
                            >
                              {isCopied ? (
                                <>
                                  <Check className="h-3.5 w-3.5 text-emerald-600" />
                                  <span className="text-[10px] font-bold">Klaar!</span>
                                </>
                              ) : (
                                <>
                                  <Copy className="h-3.5 w-3.5" />
                                </>
                              )}
                            </button>
                          </div>
                        ) : (
                          <span className="inline-flex items-center space-x-1 px-2.5 py-1 rounded-xl text-[11px] font-bold bg-rose-50 dark:bg-rose-950/60 text-rose-700 dark:text-rose-300 border border-rose-200 dark:border-rose-800/60">
                            <ShieldAlert className="h-3.5 w-3.5 shrink-0" />
                            <span>Ontbreekt</span>
                          </span>
                        )}
                      </td>

                      {/* Type ELP */}
                      <td className="py-3.5 px-4 whitespace-nowrap">
                        <span className="px-2.5 py-1 bg-slate-100 dark:bg-brand-950 text-slate-700 dark:text-brand-300 rounded-xl text-xs font-semibold">
                          {item.elpType}
                        </span>
                      </td>

                      {/* Traject Teller */}
                      <td className="py-3.5 px-4 whitespace-nowrap">
                        <span className="px-2.5 py-1 bg-brand-50 dark:bg-brand-950 text-brand-700 dark:text-brand-300 border border-brand-200 dark:border-brand-800 rounded-xl text-xs font-bold">
                          {item.elpSessieTeller}
                        </span>
                      </td>

                      {/* Status Toggle Switch */}
                      <td className="py-3.5 px-4 whitespace-nowrap text-center">
                        <button
                          onClick={() => handleToggleSingleStatus(item.id)}
                          className={`inline-flex items-center space-x-1.5 px-3 py-1 rounded-full text-xs font-bold transition shadow-2xs ${
                            item.elpStatus === 'Verwerkt'
                              ? 'bg-emerald-100 dark:bg-emerald-950 text-emerald-800 dark:text-emerald-300 border border-emerald-300 dark:border-emerald-800 hover:bg-emerald-200'
                              : 'bg-amber-100 dark:bg-amber-950 text-amber-800 dark:text-amber-300 border border-amber-300 dark:border-amber-800 hover:bg-amber-200'
                          }`}
                        >
                          {item.elpStatus === 'Verwerkt' ? (
                            <>
                              <CheckCircle2 className="h-3.5 w-3.5 text-emerald-600 dark:text-emerald-400" />
                              <span>Verwerkt</span>
                            </>
                          ) : (
                            <>
                              <Clock className="h-3.5 w-3.5 text-amber-600 dark:text-amber-400" />
                              <span>Te verwerken</span>
                            </>
                          )}
                        </button>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
