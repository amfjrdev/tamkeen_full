import React from 'react';
import type { RecentActivityItem } from '../types';
import { ClockIcon, CheckIcon, PulseIcon, ChartPulseIcon } from '../../../components/common/Icons';

interface RecentActivityProps {
  data: RecentActivityItem[];
}

export const RecentActivity: React.FC<RecentActivityProps> = ({ data }) => {
  const getIcon = (type: string) => {
    switch (type) {
      case 'pending':
        return (
          <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center border border-amber-500/20 shrink-0">
            <span className="text-amber-400"><ClockIcon /></span>
          </div>
        );
      case 'success':
        return (
          <div className="w-10 h-10 rounded-xl bg-emerald-500/10 flex items-center justify-center border border-emerald-500/20 shrink-0">
            <span className="text-emerald-400"><CheckIcon /></span>
          </div>
        );
      case 'info':
        return (
          <div className="w-10 h-10 rounded-xl bg-sky-500/10 flex items-center justify-center border border-sky-500/20 shrink-0">
            <span className="text-sky-400"><PulseIcon /></span>
          </div>
        );
      default:
        return null;
    }
  };

  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl p-5 lg:p-6 hover:border-gray-700 transition-colors flex flex-col justify-between">
      <div>
        <div className="flex items-center justify-between mb-1">
          <h3 className="text-white text-lg font-semibold">Recent Activity</h3>
          <ChartPulseIcon />
        </div>
        <p className="text-gray-400 text-sm mb-6">Latest platform events</p>
      </div>
      
      <div className="space-y-3.5 my-auto">
        {data.map((item) => (
          <div 
            key={item.id} 
            className="bg-[#161616] border border-gray-800/60 rounded-xl p-4 flex items-center justify-between hover:border-gray-700 transition-all duration-300"
          >
            <div className="flex items-center gap-3.5 min-w-0">
              {getIcon(item.type)}
              <div className="min-w-0">
                <p className="text-white text-sm font-semibold truncate">{item.name}</p>
                <p className="text-gray-400 text-xs truncate mt-0.5">{item.description}</p>
              </div>
            </div>
            <span className="text-gray-500 text-[10px] font-semibold whitespace-nowrap ml-4 shrink-0 font-mono">
              {item.time}
            </span>
          </div>
        ))}
      </div>
    </div>
  );
};
