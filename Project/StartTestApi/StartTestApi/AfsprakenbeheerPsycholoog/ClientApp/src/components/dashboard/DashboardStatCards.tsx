import React from 'react';
import { Clock, Calendar, Users } from 'lucide-react';
import { DashboardData } from '../../types';

interface DashboardStatCardsProps {
  data: DashboardData | null;
}

export const DashboardStatCards: React.FC<DashboardStatCardsProps> = ({ data }) => {
  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
      <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm flex items-center space-x-4 transition-colors">
        <div className="h-12 w-12 rounded-2xl bg-brand-50 dark:bg-brand-800 flex items-center justify-center text-brand-600 dark:text-brand-300">
          <Clock className="h-6 w-6" />
        </div>
        <div>
          <p className="text-sm font-semibold text-slate-500 dark:text-brand-300">Afspraken vandaag</p>
          <p className="text-2xl font-bold text-slate-800 dark:text-brand-50">{data?.aantalAfsprakenVandaag ?? 0}</p>
        </div>
      </div>

      <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm flex items-center space-x-4 transition-colors">
        <div className="h-12 w-12 rounded-2xl bg-emerald-50 dark:bg-emerald-950 flex items-center justify-center text-emerald-600 dark:text-emerald-400">
          <Calendar className="h-6 w-6" />
        </div>
        <div>
          <p className="text-sm font-semibold text-slate-500 dark:text-brand-300">Afspraken deze week</p>
          <p className="text-2xl font-bold text-slate-800 dark:text-brand-50">{data?.aantalAfsprakenDezeWeek ?? 0}</p>
        </div>
      </div>

      <div className="bg-white dark:bg-brand-900 p-6 rounded-3xl border border-slate-100 dark:border-brand-800/40 shadow-sm flex items-center space-x-4 transition-colors">
        <div className="h-12 w-12 rounded-2xl bg-blue-50 dark:bg-blue-950 flex items-center justify-center text-blue-600 dark:text-blue-400">
          <Users className="h-6 w-6" />
        </div>
        <div>
          <p className="text-sm font-semibold text-slate-500 dark:text-brand-300">Totaal patiënten</p>
          <p className="text-2xl font-bold text-slate-800 dark:text-brand-50">{data?.aantalPatienten ?? 0}</p>
        </div>
      </div>
    </div>
  );
};
