import React from 'react';
import type { ReportedMessage } from '../types';
import { Icons } from '../../../components/common/Icons';

interface ReportedMessageCardProps {
  report: ReportedMessage;
  onResolve?: (id: number) => void;
  onTakeAction?: (id: number) => void;
}

export const ReportedMessageCard: React.FC<ReportedMessageCardProps> = ({ report, onResolve, onTakeAction }) => {
  return (
    <div className="bg-[#1a1a1a] border border-gray-800 rounded-xl p-5 hover:border-gray-700 transition-all duration-300 shadow-lg">
      <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 mb-4.5">
        <div className="flex flex-wrap items-center gap-2 text-sm">
          <span className="text-white font-semibold">{report.participant1}</span>
          <span className="text-indigo-400 opacity-80" aria-hidden="true">
            <Icons.exchange className="w-3.5 h-3.5 mx-0.5" />
          </span>
          <span className="text-white font-semibold">{report.participant2}</span>
          <span className="text-gray-500 text-xs font-mono font-semibold ml-2.5">
            {report.time}
          </span>
        </div>
        
        <div className="flex items-center gap-2.5 shrink-0">
          <button 
            onClick={() => onResolve && onResolve(report.id)}
            className="px-3.5 py-1.5 bg-emerald-500/10 text-emerald-400 border border-emerald-500/20 rounded-lg text-xs font-bold hover:bg-emerald-500/20 active:scale-95 transition-all cursor-pointer"
          >
            Resolve Dispute
          </button>
          <button 
            onClick={() => onTakeAction && onTakeAction(report.id)}
            className="px-3.5 py-1.5 bg-rose-500/10 text-rose-400 border border-rose-500/20 rounded-lg text-xs font-bold hover:bg-rose-500/20 active:scale-95 transition-all cursor-pointer"
          >
            Take Moderation Action
          </button>
        </div>
      </div>
      
      <div className="bg-black/45 rounded-xl p-4 mb-4 border border-gray-800/60 shadow-inner">
        <p className="text-gray-300 text-sm italic font-medium leading-relaxed">
          {report.content}
        </p>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3 text-xs border-t border-gray-850 pt-3">
        <span className="text-gray-400 font-medium">
          Flagged by: <span className="text-indigo-400 font-semibold">{report.reportedBy}</span>
        </span>
        <span className="px-2.5 py-0.5 rounded bg-rose-500/15 text-rose-400 border border-rose-500/25 font-semibold capitalize tracking-wide shadow-sm">
          Reason: {report.reason}
        </span>
      </div>
    </div>
  );
};
