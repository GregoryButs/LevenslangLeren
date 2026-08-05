import React, { useState, useMemo, useEffect, useRef } from 'react';
import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getExpandedRowModel,
  ColumnDef,
  SortingState,
  ColumnFiltersState,
  VisibilityState,
  ExpandedState,
  flexRender,
  Table,
  Row,
  Column,
  HeaderGroup,
  Header,
  Cell
} from '@tanstack/react-table';
import { Patient } from '../../types';
import { getPatientDisplayName } from '../../utils/patientUtils';
import {
  ArrowUpDown, ArrowUp, ArrowDown, ChevronRight, ChevronDown,
  Search, SlidersHorizontal, Download, Merge, Edit2, Trash2,
  RefreshCw, Calendar, Link2, Link2Off, LayoutGrid, Table as TableIcon,
  GripVertical, ArrowLeft, ArrowRight, MoveHorizontal, RotateCcw
} from 'lucide-react';
import { InfoTooltip } from '../common/InfoTooltip';

interface PatientTableProps {
  data: Patient[];
  selectedPatientId: number | null;
  activeTab: 'active' | 'archived';
  onSelectPatient: (patient: Patient) => void;
  onEditPatient: (patient: Patient) => void;
  onDeactivatePatient: (id: number) => void;
  onReactivatePatient: (id: number) => void;
  onOpenMergeModal: (patients: [Patient, Patient]) => void;
}

const STORAGE_KEY_VISIBILITY = 'patient_table_visibility_v2';
const STORAGE_KEY_SIZING = 'patient_table_sizing_v2';
const STORAGE_KEY_ORDER = 'patient_table_order_v2';

const DEFAULT_COLUMN_ORDER = [
  'select',
  'volledigeNaam',
  'dossierNummer',
  'rijksregisternummer',
  'geboortedatum',
  'emotioneleStabiliteit',
  'noShowRisk',
  'isGekoppeld',
  'standaardTariefType',
  'actions'
];

export const PatientTable: React.FC<PatientTableProps> = ({
  data,
  selectedPatientId,
  activeTab,
  onSelectPatient,
  onEditPatient,
  onDeactivatePatient,
  onReactivatePatient,
  onOpenMergeModal
}) => {
  const [sorting, setSorting] = useState<SortingState>([
    { id: 'volledigeNaam', desc: false }
  ]);
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([]);
  const [globalFilter, setGlobalFilter] = useState('');
  const [rowSelection, setRowSelection] = useState<{ [key: string]: boolean }>({});
  const [expanded, setExpanded] = useState<ExpandedState>({});
  const [showVisibilityMenu, setShowVisibilityMenu] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  // Persistence: Column Visibility
  const [columnVisibility, setColumnVisibility] = useState<VisibilityState>(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY_VISIBILITY);
      return saved ? JSON.parse(saved) : {};
    } catch {
      return {};
    }
  });

  // Persistence: Column Sizing
  const [columnSizing, setColumnSizing] = useState<Record<string, number>>(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY_SIZING);
      return saved ? JSON.parse(saved) : {};
    } catch {
      return {};
    }
  });

  // Persistence: Column Order
  const [columnOrder, setColumnOrder] = useState<string[]>(() => {
    try {
      const saved = localStorage.getItem(STORAGE_KEY_ORDER);
      if (saved) {
        const parsed = JSON.parse(saved) as string[];
        // Zorg dat 'rijksregisternummer' niet achter 'actions' belandt
        if (!parsed.includes('rijksregisternummer')) {
          const idx = parsed.indexOf('dossierNummer');
          if (idx !== -1) {
            parsed.splice(idx + 1, 0, 'rijksregisternummer');
          } else {
            parsed.push('rijksregisternummer');
          }
        }
        // Garandeer dat 'standaardTariefType' voor 'actions' staat en 'actions' als allerlaatste
        const filtered = parsed.filter(c => c !== 'actions' && c !== 'standaardTariefType');
        filtered.push('standaardTariefType');
        filtered.push('actions');
        return filtered;
      }
      return DEFAULT_COLUMN_ORDER;
    } catch {
      return DEFAULT_COLUMN_ORDER;
    }
  });

  // Save layout state to localStorage
  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY_VISIBILITY, JSON.stringify(columnVisibility));
    } catch {}
  }, [columnVisibility]);

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY_SIZING, JSON.stringify(columnSizing));
    } catch {}
  }, [columnSizing]);

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY_ORDER, JSON.stringify(columnOrder));
    } catch {}
  }, [columnOrder]);

  // Auto-close Kolommen dropdown whenever selectedPatientId changes
  useEffect(() => {
    setShowVisibilityMenu(false);
  }, [selectedPatientId]);

  // Click Outside Listener to Auto-close "Kolommen" Dropdown Popup
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(event.target as Node)) {
        setShowVisibilityMenu(false);
      }
    };
    if (showVisibilityMenu) {
      document.addEventListener('mousedown', handleClickOutside);
    }
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [showVisibilityMenu]);

  // View Mode state: Automatically defaults to 'cards' on mobile (< 640px), but allows user override
  const [viewMode, setViewMode] = useState<'table' | 'cards'>(() => {
    return typeof window !== 'undefined' && window.innerWidth < 640 ? 'cards' : 'table';
  });
  const [userOverrodeViewMode, setUserOverrodeViewMode] = useState(false);

  useEffect(() => {
    const handleResize = () => {
      if (!userOverrodeViewMode) {
        setViewMode(window.innerWidth < 640 ? 'cards' : 'table');
      }
    };
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [userOverrodeViewMode]);

  const handleToggleViewMode = (mode: 'table' | 'cards') => {
    setViewMode(mode);
    setUserOverrodeViewMode(true);
  };

  const [draggedColumnId, setDraggedColumnId] = useState<string | null>(null);

  // Helper calculation for AI No-Show Risk
  const getNoShowRisk = (patient: Patient) => {
    if (!patient) return { probability: 0.10, category: 'Low' };
    const dateObj = patient.geboortedatum ? new Date(patient.geboortedatum) : null;
    const birthYear = dateObj && !isNaN(dateObj.getTime()) ? dateObj.getFullYear() : 1990;
    const age = new Date().getFullYear() - birthYear;
    const completedSessions = patient.afspraken?.filter(a => a.status === 'Voltooid').length || 0;

    let lastSessionGap = 7;
    const completed = patient.afspraken?.filter(a => a.status === 'Voltooid') || [];
    if (completed.length > 0) {
      const dates = completed.map(a => new Date(a.starttijd).getTime());
      const lastDate = Math.max(...dates);
      lastSessionGap = Math.floor((new Date().getTime() - lastDate) / (1000 * 60 * 60 * 24));
      if (lastSessionGap < 0) lastSessionGap = 0;
    }

    const latestType = patient.afspraken && patient.afspraken.length > 0
      ? patient.afspraken[patient.afspraken.length - 1].afspraakTypeNaam
      : 'Therapie';

    let prob = 0.10;
    if (lastSessionGap > 21) prob += 0.30;
    if (latestType?.toLowerCase() === 'depressie') prob += 0.20;
    if (age > 60) prob += 0.05;
    prob -= 0.005 * completedSessions;

    prob = Math.max(0.02, Math.min(0.95, prob));

    let category = 'Low';
    if (prob > 0.50) category = 'High';
    else if (prob > 0.25) category = 'Medium';

    return { probability: prob, category };
  };

  // Reset Table Layout to Defaults
  const handleResetLayout = () => {
    setColumnVisibility({});
    setColumnSizing({});
    setColumnOrder(DEFAULT_COLUMN_ORDER);
    try {
      localStorage.removeItem(STORAGE_KEY_VISIBILITY);
      localStorage.removeItem(STORAGE_KEY_SIZING);
      localStorage.removeItem(STORAGE_KEY_ORDER);
    } catch {}
  };

  // Define Columns with Sorting for AI Risk & Account Status
  const columns = useMemo<ColumnDef<Patient>[]>(
    () => [
      // Expand Chevron & Selection Checkbox Column
      {
        id: 'select',
        size: 60,
        minSize: 50,
        maxSize: 80,
        header: ({ table }: { table: Table<Patient> }) => (
          <div className="flex items-center space-x-2">
            <input
              type="checkbox"
              checked={table.getIsAllPageRowsSelected()}
              onChange={table.getToggleAllPageRowsSelectedHandler()}
              className="rounded border-slate-300 dark:border-brand-700 text-brand-600 focus:ring-brand-500 h-4 w-4 accent-brand-600 cursor-pointer"
              title="Selecteer pagina"
            />
          </div>
        ),
        cell: ({ row }: { row: Row<Patient> }) => (
          <div className="flex items-center space-x-2">
            <input
              type="checkbox"
              checked={row.getIsSelected()}
              onChange={row.getToggleSelectedHandler()}
              onClick={(e) => e.stopPropagation()}
              className="rounded border-slate-300 dark:border-brand-700 text-brand-600 focus:ring-brand-500 h-4 w-4 accent-brand-600 cursor-pointer"
            />
            <button
              onClick={(e) => {
                e.stopPropagation();
                row.toggleExpanded();
              }}
              className="p-1 hover:bg-slate-200 dark:hover:bg-brand-800 rounded-md text-slate-400 dark:text-brand-400 hover:text-slate-700 dark:hover:text-white transition"
              title="Afspraken inzien"
            >
              {row.getIsExpanded() ? (
                <ChevronDown className="h-4 w-4 text-brand-600 dark:text-brand-400" />
              ) : (
                <ChevronRight className="h-4 w-4" />
              )}
            </button>
          </div>
        ),
        enableSorting: false,
        enableHiding: false,
        enableResizing: false
      },

      // Volledige Naam & E-mail
      {
        accessorKey: 'volledigeNaam',
        size: 220,
        minSize: 130,
        maxSize: 500,
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>Patiënt & E-mail</span>
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => {
          const patient = row.original;
          return (
            <div className="min-w-0 overflow-hidden">
              <div className="flex items-center space-x-1.5">
                <span className="font-bold text-slate-800 dark:text-white truncate">
                  {getPatientDisplayName(patient)}
                </span>
                {(patient.standaardTariefType === 'ELP' || (patient as any).StandaardTariefType === 'ELP' || (patient as any).standaardTariefType === 1) && (
                  <span className="px-1.5 py-0.2 bg-brand-100 dark:bg-brand-950 text-brand-700 dark:text-brand-300 rounded text-[10px] font-extrabold border border-brand-200 dark:border-brand-800 shrink-0">
                    ELP
                  </span>
                )}
              </div>
              <span className="text-xs text-slate-500 dark:text-brand-300 block truncate">
                {patient.email}
              </span>
            </div>
          );
        }
      },

      // Dossiernummer
      {
        accessorKey: 'dossierNummer',
        size: 120,
        minSize: 80,
        maxSize: 220,
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>Dossier #</span>
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => (
          <span className="font-mono text-xs bg-slate-100 dark:bg-brand-950 px-2 py-0.5 rounded-full text-slate-700 dark:text-brand-200 border border-slate-200 dark:border-brand-800 truncate inline-block">
            {row.original.dossierNummer || 'DOS-N/A'}
          </span>
        )
      },

      // Rijksregisternummer
      {
        accessorKey: 'rijksregisternummer',
        size: 140,
        minSize: 100,
        maxSize: 220,
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>Rijksregisternr</span>
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => (
          <span className="font-mono text-xs text-slate-700 dark:text-brand-200 truncate inline-block">
            {row.original.rijksregisternummer || '—'}
          </span>
        )
      },

      // Geboortedatum / Leeftijd
      {
        accessorKey: 'geboortedatum',
        size: 140,
        minSize: 90,
        maxSize: 240,
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>Geboortedatum</span>
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => {
          if (!row.original.geboortedatum) return <span className="text-slate-400">—</span>;
          const date = new Date(row.original.geboortedatum);
          if (isNaN(date.getTime())) return <span className="text-slate-400">—</span>;
          const age = new Date().getFullYear() - date.getFullYear();
          return (
            <div className="text-xs truncate">
              <span className="text-slate-800 dark:text-brand-100 font-semibold">{date.toLocaleDateString('nl-NL')}</span>
              <span className="text-slate-400 dark:text-brand-400 ml-1">({age} jr)</span>
            </div>
          );
        }
      },

      // Emotionele Stabiliteit
      {
        accessorKey: 'emotioneleStabiliteit',
        size: 110,
        minSize: 80,
        maxSize: 200,
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>Stabiliteit</span>
            <InfoTooltip content="Klinische score ingevoerd door de therapeut (1.0 - 10.0)" />
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0 ml-1" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0 ml-1" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0 ml-1" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => {
          const val = row.original.emotioneleStabiliteit;
          return (
            <span className="text-xs font-bold text-slate-700 dark:text-brand-200 truncate block">
              {val !== null && val !== undefined ? `${val.toFixed(1)} / 10.0` : '5.5 (Std)'}
            </span>
          );
        }
      },

      // AI No-Show Risico (Sorteerbaar)
      {
        id: 'noShowRisk',
        size: 120,
        minSize: 80,
        maxSize: 200,
        accessorFn: (row) => getNoShowRisk(row).probability,
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>AI Risk</span>
            <InfoTooltip content="Berekend no-show risico voor de eerstvolgende sessie. Klik om te sorteren." />
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => {
          const risk = getNoShowRisk(row.original);
          return (
            <span className={`inline-block px-2 py-0.5 rounded-full text-[11px] font-bold truncate ${
              risk.category === 'High' ? 'bg-red-100 dark:bg-red-950/80 text-red-800 dark:text-red-300 border border-red-200 dark:border-red-800' :
              risk.category === 'Medium' ? 'bg-amber-100 dark:bg-amber-950/80 text-amber-800 dark:text-amber-300 border border-amber-200 dark:border-amber-800' :
              'bg-emerald-100 dark:bg-emerald-950/80 text-emerald-800 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800'
            }`}>
              {(risk.probability * 100).toFixed(0)}% ({risk.category})
            </span>
          );
        }
      },

      // Account Koppelstatus (Sorteerbaar & Filterbaar)
      {
        accessorKey: 'isGekoppeld',
        size: 130,
        minSize: 90,
        maxSize: 220,
        filterFn: (row, columnId, filterValue) => {
          if (filterValue === undefined || filterValue === null || filterValue === 'all') return true;
          const boolValue = filterValue === 'true' || filterValue === true;
          return !!row.getValue(columnId) === boolValue;
        },
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>Account</span>
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => (
          <span className={`inline-flex items-center space-x-1 px-2 py-0.5 rounded-full text-[11px] font-semibold truncate ${
            row.original.isGekoppeld
              ? 'bg-brand-50 dark:bg-brand-950 text-brand-700 dark:text-brand-300 border border-brand-200 dark:border-brand-800'
              : 'bg-slate-100 dark:bg-slate-800 text-slate-500 dark:text-slate-400'
          }`}>
            {row.original.isGekoppeld ? (
              <>
                <Link2 className="h-3 w-3 shrink-0" />
                <span className="truncate">Gekoppeld</span>
              </>
            ) : (
              <>
                <Link2Off className="h-3 w-3 shrink-0" />
                <span className="truncate">Geen account</span>
              </>
            )}
          </span>
        )
      },

      // Standaard Tarieftype (ELP / Regulier) - Verplaatst naar net vóór Acties
      {
        accessorKey: 'standaardTariefType',
        size: 130,
        minSize: 90,
        maxSize: 180,
        filterFn: (row, _columnId, filterValue) => {
          if (!filterValue || filterValue === 'all') return true;
          const val = row.original.standaardTariefType || (row.original as any).StandaardTariefType || 'Regulier';
          return val === filterValue;
        },
        header: ({ column }: { column: Column<Patient, unknown> }) => (
          <button
            onClick={() => column.toggleSorting(column.getIsSorted() === 'asc')}
            className="flex items-center space-x-1 text-slate-700 dark:text-brand-100 hover:text-brand-600 dark:hover:text-brand-300 transition font-bold truncate"
          >
            <span>Tarief</span>
            {column.getIsSorted() === 'asc' ? (
              <ArrowUp className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : column.getIsSorted() === 'desc' ? (
              <ArrowDown className="h-3.5 w-3.5 text-brand-600 shrink-0" />
            ) : (
              <ArrowUpDown className="h-3.5 w-3.5 opacity-40 shrink-0" />
            )}
          </button>
        ),
        cell: ({ row }: { row: Row<Patient> }) => {
          const type = row.original.standaardTariefType || (row.original as any).StandaardTariefType || 'Regulier';
          return (
            <span className={`inline-block px-2 py-0.5 rounded-full text-[11px] font-bold truncate ${
              type === 'ELP' 
                ? 'bg-brand-100 dark:bg-brand-950 text-brand-800 dark:text-brand-300 border border-brand-200 dark:border-brand-800' 
                : 'bg-slate-100 dark:bg-slate-800 text-slate-600 dark:text-slate-400'
            }`}>
              {type === 'ELP' ? 'ELP' : 'Regulier'}
            </span>
          );
        }
      },

      // Sticky Quick Actions Column
      {
        id: 'actions',
        size: 110,
        minSize: 90,
        maxSize: 150,
        header: () => <span className="font-bold text-right block pr-2 text-slate-700 dark:text-brand-100">Acties</span>,
        cell: ({ row }: { row: Row<Patient> }) => {
          const patient = row.original;
          return (
            <div className="flex items-center justify-end space-x-1.5" onClick={(e) => e.stopPropagation()}>
              <button
                onClick={() => onEditPatient(patient)}
                className="p-1.5 bg-slate-100 dark:bg-brand-950 hover:bg-brand-100 dark:hover:bg-brand-800 text-slate-600 dark:text-brand-200 rounded-lg transition"
                title="Bewerken"
              >
                <Edit2 className="h-3.5 w-3.5" />
              </button>
              {activeTab === 'active' ? (
                <button
                  onClick={() => onDeactivatePatient(patient.id)}
                  className="p-1.5 bg-red-50 dark:bg-red-950/60 hover:bg-red-100 text-red-600 dark:text-red-300 rounded-lg transition"
                  title="Deactiveren"
                >
                  <Trash2 className="h-3.5 w-3.5" />
                </button>
              ) : (
                <button
                  onClick={() => onReactivatePatient(patient.id)}
                  className="p-1.5 bg-emerald-50 dark:bg-emerald-950/60 hover:bg-emerald-100 text-emerald-700 dark:text-emerald-300 rounded-lg transition"
                  title="Heractiveren"
                >
                  <RefreshCw className="h-3.5 w-3.5" />
                </button>
              )}
            </div>
          );
        },
        enableSorting: false,
        enableHiding: false,
        enableResizing: false
      }
    ],
    [activeTab, onEditPatient, onDeactivatePatient, onReactivatePatient]
  );

  // Initialize TanStack Table instance
  const table = useReactTable({
    data,
    columns,
    columnResizeMode: 'onChange',
    enableColumnResizing: true,
    state: {
      sorting,
      columnFilters,
      globalFilter,
      columnVisibility,
      columnSizing,
      rowSelection,
      expanded,
      columnOrder
    },
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onGlobalFilterChange: setGlobalFilter,
    onColumnVisibilityChange: setColumnVisibility,
    onColumnSizingChange: setColumnSizing,
    onRowSelectionChange: setRowSelection,
    onExpandedChange: setExpanded,
    onColumnOrderChange: setColumnOrder,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    enableRowSelection: true,
    initialState: {
      pagination: {
        pageSize: 10
      }
    }
  });

  // Drag & Drop Handlers for Column Reordering
  const handleDragStart = (e: React.DragEvent, columnId: string) => {
    if (columnId === 'select' || columnId === 'actions') return;
    e.dataTransfer.setData('text/plain', columnId);
    setDraggedColumnId(columnId);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDrop = (e: React.DragEvent, targetColumnId: string) => {
    e.preventDefault();
    if (targetColumnId === 'select' || targetColumnId === 'actions') return;
    const sourceColumnId = e.dataTransfer.getData('text/plain');

    if (sourceColumnId && sourceColumnId !== targetColumnId) {
      setColumnOrder((prevOrder) => {
        const newOrder = [...prevOrder];
        const sourceIndex = newOrder.indexOf(sourceColumnId);
        const targetIndex = newOrder.indexOf(targetColumnId);
        if (sourceIndex !== -1 && targetIndex !== -1) {
          newOrder.splice(sourceIndex, 1);
          newOrder.splice(targetIndex, 0, sourceColumnId);
        }
        return newOrder;
      });
    }
    setDraggedColumnId(null);
  };

  // Move Column Up/Down for Mobile Touch
  const moveColumn = (columnId: string, direction: 'left' | 'right') => {
    setColumnOrder((prevOrder) => {
      const idx = prevOrder.indexOf(columnId);
      if (idx === -1) return prevOrder;
      const targetIdx = direction === 'left' ? idx - 1 : idx + 1;
      if (targetIdx <= 0 || targetIdx >= prevOrder.length - 1) return prevOrder;
      const newOrder = [...prevOrder];
      const temp = newOrder[idx];
      newOrder[idx] = newOrder[targetIdx];
      newOrder[targetIdx] = temp;
      return newOrder;
    });
  };

  // Selected Patients for Merge
  const selectedRows = table.getSelectedRowModel().rows;
  const canMerge = selectedRows.length === 2;

  const handleTriggerMerge = () => {
    if (canMerge) {
      onOpenMergeModal([selectedRows[0].original, selectedRows[1].original]);
    }
  };

  // CSV Export Handler
  const handleExportCSV = () => {
    const rowsToExport = table.getFilteredRowModel().rows.map((r: Row<Patient>) => r.original);
    if (rowsToExport.length === 0) return;

    const headers = ['ID', 'Voornaam', 'Achternaam', 'Geboortedatum', 'Email', 'SecundairEmail', 'Telefoon', 'Dossiernummer', 'EmotioneleStabiliteit', 'IsActief'];
    const csvContent = [
      headers.join(','),
      ...rowsToExport.map((p: Patient) => [
        p.id,
        `"${p.voornaam || ''}"`,
        `"${p.achternaam || ''}"`,
        `"${p.geboortedatum || ''}"`,
        `"${p.email || ''}"`,
        `"${p.secundairEmail || ''}"`,
        `"${p.telefoonnummer || ''}"`,
        `"${p.dossierNummer || ''}"`,
        p.emotioneleStabiliteit ?? '',
        p.isActief
      ].join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', `patienten_export_${activeTab}_${new Date().toISOString().split('T')[0]}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  };

  return (
    <div className="space-y-4">
      {/* Top Toolbar: Search, Filter, View Mode Toggle, Column Visibility Dropdown, Export */}
      <div className="bg-white dark:bg-brand-900 p-4 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm space-y-3 transition-colors">
        <div className="flex flex-col gap-3">
          {/* Global Search Bar */}
          <div className="relative w-full">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-slate-400 dark:text-brand-400">
              <Search className="h-4 w-4" />
            </div>
            <input
              type="text"
              value={globalFilter ?? ''}
              onChange={(e) => setGlobalFilter(e.target.value)}
              placeholder="Zoek patiënten op naam, email of dossiernummer..."
              className="pl-9 w-full rounded-2xl border border-slate-200 dark:border-brand-800 bg-slate-50/50 dark:bg-brand-950/60 py-2 px-4 text-slate-800 dark:text-white placeholder-slate-400 dark:placeholder-brand-400 focus:outline-none focus:ring-2 focus:ring-brand-500 text-xs sm:text-sm"
            />
          </div>

          {/* Account Filter & Action Buttons Row (Clean Container Wrapping) */}
          <div className="flex flex-wrap items-center justify-between gap-2">
            <div className="flex flex-wrap items-center gap-2">
              {/* Account Status Filter Dropdown */}
              <select
                value={
                  table.getColumn('isGekoppeld')?.getFilterValue() === undefined
                    ? 'all'
                    : String(table.getColumn('isGekoppeld')?.getFilterValue())
                }
                onChange={(e) => {
                  const val = e.target.value;
                  table.getColumn('isGekoppeld')?.setFilterValue(val === 'all' ? undefined : val);
                }}
                className="rounded-2xl border border-slate-200 dark:border-brand-800 bg-slate-50/50 dark:bg-brand-950/60 py-1.5 px-3 text-slate-700 dark:text-brand-200 text-xs font-semibold focus:outline-none focus:ring-2 focus:ring-brand-500 cursor-pointer"
              >
                <option value="all">Alle Accounts</option>
                <option value="true">🔗 Gekoppeld</option>
                <option value="false">❌ Geen account</option>
              </select>

              {/* ELP Tarief Filter Dropdown */}
              <select
                value={
                  table.getColumn('standaardTariefType')?.getFilterValue() === undefined
                    ? 'all'
                    : String(table.getColumn('standaardTariefType')?.getFilterValue())
                }
                onChange={(e) => {
                  const val = e.target.value;
                  table.getColumn('standaardTariefType')?.setFilterValue(val === 'all' ? undefined : val);
                }}
                className="rounded-2xl border border-slate-200 dark:border-brand-800 bg-slate-50/50 dark:bg-brand-950/60 py-1.5 px-3 text-slate-700 dark:text-brand-200 text-xs font-semibold focus:outline-none focus:ring-2 focus:ring-brand-500 cursor-pointer"
              >
                <option value="all">Alle Tarieven</option>
                <option value="ELP">🏥 Alleen ELP Patiënten</option>
                <option value="Regulier">💼 Alleen Reguliere Patiënten</option>
              </select>

              {/* View Mode Toggle (Mobile / Desktop) */}
              <div className="flex bg-slate-100 dark:bg-brand-950 p-1 rounded-2xl border border-slate-200 dark:border-brand-800">
                <button
                  onClick={() => handleToggleViewMode('table')}
                  className={`p-1.5 rounded-xl transition ${
                    viewMode === 'table'
                      ? 'bg-white dark:bg-brand-800 text-brand-600 dark:text-white shadow-xs font-bold'
                      : 'text-slate-400 hover:text-slate-600 dark:hover:text-brand-200'
                  }`}
                  title="Tabelweergave"
                >
                  <TableIcon className="h-4 w-4" />
                </button>
                <button
                  onClick={() => handleToggleViewMode('cards')}
                  className={`p-1.5 rounded-xl transition ${
                    viewMode === 'cards'
                      ? 'bg-white dark:bg-brand-800 text-brand-600 dark:text-white shadow-xs font-bold'
                      : 'text-slate-400 hover:text-slate-600 dark:hover:text-brand-200'
                  }`}
                  title="Kaartweergave (Mobiel geoptimaliseerd)"
                >
                  <LayoutGrid className="h-4 w-4" />
                </button>
              </div>
            </div>

            <div className="flex items-center space-x-2">
              {/* Column Visibility, Reordering & Resizing Dropdown Menu (With Click-Outside Ref) */}
              <div className="relative" ref={menuRef}>
                <button
                  onClick={() => setShowVisibilityMenu(!showVisibilityMenu)}
                  className="flex items-center space-x-1.5 bg-slate-50 dark:bg-brand-950 hover:bg-slate-100 dark:hover:bg-brand-800 text-slate-700 dark:text-brand-200 border border-slate-200 dark:border-brand-800 py-1.5 px-3 rounded-2xl text-xs font-semibold transition"
                >
                  <SlidersHorizontal className="h-3.5 w-3.5" />
                  <span>Kolommen</span>
                </button>

                {showVisibilityMenu && (
                  <div className="absolute right-0 mt-2 w-72 max-w-[280px] bg-white dark:bg-brand-900 rounded-2xl shadow-2xl border border-slate-200 dark:border-brand-800 p-3 z-50 space-y-2 text-xs">
                    <div className="flex justify-between items-center pb-1 border-b border-slate-100 dark:border-brand-800">
                      <span className="font-bold text-slate-400 uppercase tracking-wider text-[10px]">Kolommen & Lay-out</span>
                      <button
                        onClick={handleResetLayout}
                        className="flex items-center space-x-1 text-[10px] text-brand-600 dark:text-brand-400 hover:underline font-semibold"
                      >
                        <RotateCcw className="h-2.5 w-2.5" />
                        <span>Reset naar standaard</span>
                      </button>
                    </div>
                    <div className="max-h-72 overflow-y-auto space-y-2 pr-1">
                      {table.getAllLeafColumns().map((column: Column<Patient, unknown>) => {
                        if (column.id === 'select' || column.id === 'actions') return null;
                        const colLabel =
                          column.id === 'volledigeNaam' ? 'Naam & Email' :
                          column.id === 'dossierNummer' ? 'Dossiernummer' :
                          column.id === 'geboortedatum' ? 'Geboortedatum' :
                          column.id === 'emotioneleStabiliteit' ? 'Stabiliteit' :
                          column.id === 'noShowRisk' ? 'AI Risk' :
                          column.id === 'isGekoppeld' ? 'Account Status' :
                          column.id === 'standaardTariefType' ? 'Tarief' : column.id;

                        return (
                          <div key={column.id} className="p-2 rounded-xl border border-slate-100 dark:border-brand-800/40 bg-slate-50/60 dark:bg-brand-950/60 space-y-1.5">
                            <div className="flex items-center justify-between">
                              <label className="flex items-center space-x-2 cursor-pointer min-w-0 flex-1">
                                <input
                                  type="checkbox"
                                  checked={column.getIsVisible()}
                                  onChange={column.getToggleVisibilityHandler()}
                                  className="accent-brand-600 rounded"
                                />
                                <span className="text-slate-700 dark:text-brand-200 font-bold truncate">{colLabel}</span>
                              </label>
                              <div className="flex items-center space-x-1 flex-shrink-0">
                                <button
                                  onClick={() => moveColumn(column.id, 'left')}
                                  className="p-1 text-slate-400 hover:text-brand-600 hover:bg-slate-200 dark:hover:bg-brand-800 rounded"
                                  title="Naar links verplaatsen"
                                >
                                  <ArrowLeft className="h-3 w-3" />
                                </button>
                                <button
                                  onClick={() => moveColumn(column.id, 'right')}
                                  className="p-1 text-slate-400 hover:text-brand-600 hover:bg-slate-200 dark:hover:bg-brand-800 rounded"
                                  title="Naar rechts verplaatsen"
                                >
                                  <ArrowRight className="h-3 w-3" />
                                </button>
                              </div>
                            </div>

                            {/* Column Width Slider */}
                            <div className="flex items-center space-x-2 pt-1 border-t border-slate-200/50 dark:border-brand-800/40 text-[10px] text-slate-500">
                              <MoveHorizontal className="h-3 w-3 text-slate-400 shrink-0" />
                              <span>Breedte: {column.getSize()}px</span>
                              <input
                                type="range"
                                min={column.columnDef.minSize || 80}
                                max={column.columnDef.maxSize || 450}
                                value={column.getSize()}
                                onChange={(e) => {
                                  const newSize = Number(e.target.value);
                                  table.setColumnSizing((prev) => ({
                                    ...prev,
                                    [column.id]: newSize
                                  }));
                                }}
                                className="w-full accent-brand-600 cursor-pointer h-1 bg-slate-200 dark:bg-brand-800 rounded-lg"
                              />
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                )}
              </div>

              {/* CSV Export Button */}
              <button
                onClick={handleExportCSV}
                className="flex items-center space-x-1.5 bg-slate-50 dark:bg-brand-950 hover:bg-slate-100 dark:hover:bg-brand-800 text-slate-700 dark:text-brand-200 border border-slate-200 dark:border-brand-800 py-1.5 px-3 rounded-2xl text-xs font-semibold transition"
                title="Exporteer gefilterde patiënten naar CSV"
              >
                <Download className="h-3.5 w-3.5" />
                <span className="hidden sm:inline">Export CSV</span>
              </button>
            </div>
          </div>
        </div>

        {/* Selected Merge Banner */}
        {selectedRows.length > 0 && (
          <div className="flex items-center justify-between bg-brand-50 dark:bg-brand-950/80 p-3 rounded-2xl border border-brand-200 dark:border-brand-800 text-xs animate-in fade-in duration-150">
            <span className="text-brand-900 dark:text-brand-200 font-semibold">
              {selectedRows.length} patiënt{selectedRows.length > 1 ? 'en' : ''} geselecteerd
            </span>
            {canMerge ? (
              <button
                onClick={handleTriggerMerge}
                className="flex items-center space-x-1.5 bg-brand-600 hover:bg-brand-700 text-white font-bold py-1.5 px-4 rounded-xl shadow-md transition"
              >
                <Merge className="h-4 w-4" />
                <span>Patiënten Samenvoegen / Mergen</span>
              </button>
            ) : (
              <span className="text-slate-400 dark:text-brand-400 italic">
                (Selecteer exact 2 patiënten om te mergen)
              </span>
            )}
          </div>
        )}
      </div>

      {/* Main Table View vs Mobile Card View */}
      {viewMode === 'table' ? (
        <div className="bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm overflow-hidden transition-colors">
          <div className="overflow-x-auto relative w-full">
            <table
              className="w-full min-w-full text-left border-collapse text-xs sm:text-sm table-fixed"
              style={{ width: table.getTotalSize() }}
            >
              <thead>
                {table.getHeaderGroups().map((headerGroup: HeaderGroup<Patient>) => (
                  <tr key={headerGroup.id} className="bg-slate-100/90 dark:bg-brand-950 border-b border-slate-200 dark:border-brand-800 text-slate-700 dark:text-brand-100 font-bold uppercase tracking-wider text-[11px]">
                    {headerGroup.headers.map((header: Header<Patient, unknown>) => {
                      const isActions = header.id === 'actions';
                      const isSelect = header.id === 'select';
                      const isReorderable = !isActions && !isSelect;

                      return (
                        <th
                          key={header.id}
                          style={{ width: header.getSize() }}
                          className={`py-3 px-4 relative group select-none transition-colors overflow-hidden ${
                            draggedColumnId === header.id ? 'opacity-40 bg-brand-100 dark:bg-brand-800' : ''
                          } ${
                            isActions
                              ? 'sticky right-0 z-20 bg-slate-100 dark:bg-brand-950 border-l border-slate-200 dark:border-brand-800 shadow-[-4px_0_12px_rgba(0,0,0,0.1)]'
                              : ''
                          }`}
                        >
                          <div className="flex items-center space-x-1 pr-2">
                            {isReorderable && (
                              <span
                                draggable
                                onDragStart={(e) => handleDragStart(e, header.id)}
                                onDragOver={handleDragOver}
                                onDrop={(e) => handleDrop(e, header.id)}
                                className="cursor-grab active:cursor-grabbing p-0.5 hover:bg-slate-200 dark:hover:bg-brand-800 rounded shrink-0"
                                title="Sleep om kolom te verplaatsen"
                              >
                                <GripVertical className="h-3 w-3 text-slate-400 dark:text-brand-400" />
                              </span>
                            )}
                            <div className="flex-1 min-w-0 truncate">
                              {header.isPlaceholder
                                ? null
                                : flexRender(header.column.columnDef.header, header.getContext())}
                            </div>
                          </div>

                          {/* Interactive Column Resize Handle */}
                          {header.column.getCanResize() && (
                            <div
                              onMouseDown={(e) => {
                                e.stopPropagation();
                                header.getResizeHandler()(e);
                              }}
                              onTouchStart={(e) => {
                                e.stopPropagation();
                                header.getResizeHandler()(e);
                              }}
                              className={`absolute right-0 top-0 h-full w-4 cursor-col-resize select-none touch-none z-30 flex items-center justify-center group/resizer ${
                                header.column.getIsResizing() ? 'bg-brand-500/20' : ''
                              }`}
                              onClick={(e) => e.stopPropagation()}
                              title="Sleep rechterrand om kolombreedte aan te passen"
                            >
                              <div className={`w-1 h-3/4 rounded-full transition ${
                                header.column.getIsResizing() ? 'bg-brand-600' : 'bg-slate-300 dark:bg-brand-700 opacity-0 group-hover/resizer:opacity-100 hover:bg-brand-500'
                              }`} />
                            </div>
                          )}
                        </th>
                      );
                    })}
                  </tr>
                ))}
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-brand-800/40 text-slate-800 dark:text-brand-100">
                {table.getRowModel().rows.length > 0 ? (
                  table.getRowModel().rows.map((row: Row<Patient>) => {
                    const isSelected = selectedPatientId === row.original.id;
                    return (
                      <React.Fragment key={row.id}>
                        <tr
                          onClick={() => {
                            setShowVisibilityMenu(false);
                            onSelectPatient(row.original);
                          }}
                          className={`cursor-pointer transition-colors group ${
                            isSelected
                              ? 'bg-brand-50/60 dark:bg-brand-800/60'
                              : 'hover:bg-slate-50/70 dark:hover:bg-brand-950/50'
                          }`}
                        >
                          {row.getVisibleCells().map((cell: Cell<Patient, unknown>) => {
                            const isActions = cell.column.id === 'actions';
                            return (
                              <td
                                key={cell.id}
                                style={{ width: cell.column.getSize() }}
                                className={`py-3 px-4 align-middle overflow-hidden ${
                                  isActions
                                    ? `sticky right-0 z-10 border-l border-slate-100 dark:border-brand-800/40 shadow-[-4px_0_12px_rgba(0,0,0,0.08)] ${
                                        isSelected
                                          ? 'bg-brand-50 dark:bg-brand-800'
                                          : 'bg-white dark:bg-brand-900 group-hover:bg-slate-50 dark:group-hover:bg-brand-950'
                                      }`
                                      : ''
                                }`}
                              >
                                {flexRender(cell.column.columnDef.cell, cell.getContext())}
                              </td>
                            );
                          })}
                        </tr>

                        {/* Row Expansion: Nested Appointments Sub-table */}
                        {row.getIsExpanded() && (
                          <tr className="bg-slate-50/80 dark:bg-brand-950/70">
                            <td colSpan={columns.length} className="p-4 border-t border-b border-slate-200 dark:border-brand-800">
                              <div className="space-y-2 max-w-4xl">
                                <h5 className="font-bold text-xs text-slate-700 dark:text-brand-200 flex items-center space-x-1.5">
                                  <Calendar className="h-3.5 w-3.5 text-brand-600 dark:text-brand-400" />
                                  <span>Afsprakenhistorie voor {getPatientDisplayName(row.original)}</span>
                                </h5>
                                {row.original.afspraken && row.original.afspraken.length > 0 ? (
                                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs">
                                    {row.original.afspraken.map((appt: any) => {
                                      const start = new Date(appt.starttijd);
                                      return (
                                        <div key={appt.id} className="p-2.5 bg-white dark:bg-brand-900 rounded-xl border border-slate-200 dark:border-brand-800 flex justify-between items-center">
                                          <div>
                                            <span className="font-semibold text-slate-800 dark:text-white block">{appt.afspraakTypeNaam}</span>
                                            <span className="text-[11px] text-slate-500 dark:text-brand-300">
                                              {start.toLocaleDateString('nl-NL')} om {start.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                                            </span>
                                        </div>
                                        <span
                                          className="py-0.5 px-2 rounded-full text-[10px] font-bold text-white uppercase shadow-xs"
                                          style={{ backgroundColor: appt.kleurcode || '#478d96' }}
                                        >
                                          {appt.status}
                                        </span>
                                      </div>
                                    );
                                  })}
                                </div>
                                ) : (
                                  <p className="text-xs text-slate-400 dark:text-brand-400 italic">Geen afspraken gepland of gevonden.</p>
                                )}
                              </div>
                            </td>
                          </tr>
                        )}
                      </React.Fragment>
                    );
                  })
                ) : (
                  <tr>
                    <td colSpan={columns.length} className="text-center py-12 text-slate-400 dark:text-brand-400">
                      Geen patiënten gevonden die voldoen aan de zoekcriteria.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>

          {/* Pagination Bar */}
          <div className="p-4 border-t border-slate-100 dark:border-brand-800/40 bg-slate-50/50 dark:bg-brand-950/50 flex flex-col sm:flex-row justify-between items-center gap-3 text-xs">
            <div className="text-slate-500 dark:text-brand-300 font-medium">
              Pagina <strong className="text-slate-800 dark:text-white">{table.getState().pagination.pageIndex + 1}</strong> van{' '}
              <strong className="text-slate-800 dark:text-white">{table.getPageCount() || 1}</strong> ({table.getFilteredRowModel().rows.length} resultaten)
            </div>

            <div className="flex items-center space-x-3">
              {/* Page Size Selector */}
              <div className="flex items-center space-x-1.5">
                  <span className="text-slate-400">Per pagina:</span>
                  <select
                    value={table.getState().pagination.pageSize}
                    onChange={(e) => table.setPageSize(Number(e.target.value))}
                    className="bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-800 rounded-xl px-2 py-1 text-slate-800 dark:text-white font-semibold focus:outline-none cursor-pointer"
                  >
                    {[10, 25, 50, 100].map((pageSize) => (
                      <option key={pageSize} value={pageSize}>
                        {pageSize}
                      </option>
                    ))}
                </select>
              </div>

              {/* Pagination Controls */}
              <div className="flex space-x-1">
                <button
                  onClick={() => table.previousPage()}
                  disabled={!table.getCanPreviousPage()}
                  className="py-1 px-3 bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-800 rounded-xl font-semibold disabled:opacity-40 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-brand-800 transition cursor-pointer"
                >
                  Vorige
                </button>
                <button
                  onClick={() => table.nextPage()}
                  disabled={!table.getCanNextPage()}
                  className="py-1 px-3 bg-white dark:bg-brand-900 border border-slate-200 dark:border-brand-800 rounded-xl font-semibold disabled:opacity-40 disabled:cursor-not-allowed hover:bg-slate-50 dark:hover:bg-brand-800 transition cursor-pointer"
                >
                  Volgende
                </button>
              </div>
            </div>
          </div>
        </div>
      ) : (
        /* Mobile Optimized Cards View Mode */
        <div className="space-y-3">
          {table.getRowModel().rows.length > 0 ? (
            table.getRowModel().rows.map((row: Row<Patient>) => {
              const p = row.original;
              const isSelected = selectedPatientId === p.id;
              const risk = getNoShowRisk(p);

              return (
                <div
                  key={p.id}
                  onClick={() => {
                    setShowVisibilityMenu(false);
                    onSelectPatient(p);
                  }}
                  className={`p-4 rounded-3xl border transition cursor-pointer space-y-3 ${
                    isSelected
                      ? 'bg-brand-50/60 dark:bg-brand-800/60 border-brand-300 dark:border-brand-700 shadow-md'
                      : 'bg-white dark:bg-brand-900 border-slate-100 dark:border-brand-800/40 hover:border-slate-200 dark:hover:border-brand-700 shadow-sm'
                  }`}
                >
                  <div className="flex justify-between items-start gap-2">
                    <div className="flex items-start space-x-3 min-w-0 flex-1">
                      <input
                        type="checkbox"
                        checked={row.getIsSelected()}
                        onChange={row.getToggleSelectedHandler()}
                        onClick={(e) => e.stopPropagation()}
                        className="mt-1 rounded border-slate-300 dark:border-brand-700 text-brand-600 focus:ring-brand-500 h-4 w-4 accent-brand-600 cursor-pointer"
                      />
                      <div className="min-w-0 flex-1">
                        <h4 className="font-bold text-slate-800 dark:text-white truncate text-base">{getPatientDisplayName(p)}</h4>
                        <p className="text-xs text-slate-500 dark:text-brand-300 truncate">{p.email}</p>
                        <div className="flex flex-wrap gap-1.5 pt-1.5">
                          {p.dossierNummer && (
                            <span className="text-[10px] bg-slate-100 dark:bg-brand-950 text-slate-600 dark:text-brand-200 px-2 py-0.5 rounded-full font-mono">
                              {p.dossierNummer}
                            </span>
                          )}
                          <span className={`text-[10px] px-2 py-0.5 rounded-full font-bold ${
                            risk.category === 'High' ? 'bg-red-100 text-red-800 dark:bg-red-950 dark:text-red-300' :
                            risk.category === 'Medium' ? 'bg-amber-100 text-amber-800 dark:bg-amber-950 dark:text-amber-300' :
                            'bg-emerald-100 text-emerald-800 dark:bg-emerald-950 dark:text-emerald-300'
                          }`}>
                            Risk: {risk.category}
                          </span>
                        </div>
                      </div>
                    </div>

                    {/* Mobile Card Action Buttons */}
                    <div className="flex items-center space-x-1 shrink-0" onClick={(e) => e.stopPropagation()}>
                      <button
                        onClick={() => onEditPatient(p)}
                        className="p-2 bg-slate-100 dark:bg-brand-950 hover:bg-brand-100 dark:hover:bg-brand-800 text-slate-600 dark:text-brand-200 rounded-xl transition"
                        title="Bewerken"
                      >
                        <Edit2 className="h-4 w-4" />
                      </button>
                      {activeTab === 'active' ? (
                        <button
                          onClick={() => onDeactivatePatient(p.id)}
                          className="p-2 bg-red-50 dark:bg-red-950/60 hover:bg-red-100 text-red-600 dark:text-red-300 rounded-xl transition"
                          title="Deactiveren"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      ) : (
                        <button
                          onClick={() => onReactivatePatient(p.id)}
                          className="p-2 bg-emerald-50 dark:bg-emerald-950/60 hover:bg-emerald-100 text-emerald-700 dark:text-emerald-300 rounded-xl transition"
                          title="Heractiveren"
                        >
                          <RefreshCw className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                  </div>
                </div>
              );
          })
        ) : (
          <div className="text-center py-12 bg-white dark:bg-brand-900 rounded-3xl border border-slate-100 text-slate-400">
            Geen patiënten gevonden.
          </div>
        )}
      </div>
    )}
  </div>
  );
};