import React from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';

interface DateNavigatorProps {
  value: string; // Format: YYYY-MM-DD
  onChange: (newDateStr: string) => void;
  onPrev: () => void;
  onNext: () => void;
  className?: string;
}

export const DateNavigator: React.FC<DateNavigatorProps> = ({
  value,
  onChange,
  onPrev,
  onNext,
  className = ''
}) => {
  return (
    <div className={`flex items-center justify-between sm:justify-start space-x-1.5 bg-slate-100 dark:bg-brand-950 p-1 rounded-xl border border-slate-200/60 dark:border-brand-800/60 transition-colors ${className}`}>
      <button
        type="button"
        onClick={onPrev}
        className="p-1.5 text-slate-600 dark:text-brand-300 hover:text-slate-800 dark:hover:text-white rounded-lg hover:bg-white dark:hover:bg-brand-900 transition cursor-pointer"
        title="Vorige"
      >
        <ChevronLeft className="h-4 w-4" />
      </button>
      <input
        type="date"
        value={value}
        onChange={(e) => {
          if (e.target.value) {
            onChange(e.target.value);
          }
        }}
        className="bg-transparent font-bold text-slate-700 dark:text-brand-100 text-xs focus:outline-none border-none cursor-pointer px-1 py-0.5"
      />
      <button
        type="button"
        onClick={onNext}
        className="p-1.5 text-slate-600 dark:text-brand-300 hover:text-slate-800 dark:hover:text-white rounded-lg hover:bg-white dark:hover:bg-brand-900 transition cursor-pointer"
        title="Volgende"
      >
        <ChevronRight className="h-4 w-4" />
      </button>
    </div>
  );
};
