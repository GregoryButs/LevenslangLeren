import React from 'react';
import { HelpCircle } from 'lucide-react';

interface InfoTooltipProps {
  content: React.ReactNode;
  position?: 'top' | 'bottom';
}

export const InfoTooltip: React.FC<InfoTooltipProps> = ({ content, position = 'top' }) => {
  const isBottom = position === 'bottom';
  return (
    <div className="relative group inline-block cursor-help ml-2 align-middle select-none">
      <HelpCircle className="h-4 w-4 text-slate-400 hover:text-brand-500 transition-colors inline" />
      <div className={`absolute z-50 left-1/2 transform -translate-x-1/2 w-64 p-3 bg-slate-900 text-white text-[11px] font-normal leading-relaxed rounded-xl shadow-xl border border-slate-700 opacity-0 pointer-events-none group-hover:opacity-100 transition-opacity duration-200 ${
        isBottom ? 'top-full mt-2' : 'bottom-full mb-2'
      }`}>
        {content}
        <div className={`absolute left-1/2 transform -translate-x-1/2 border-[5px] border-solid border-transparent ${
          isBottom ? 'bottom-full border-b-slate-900' : 'top-full border-t-slate-900'
        }`}></div>
      </div>
    </div>
  );
};
