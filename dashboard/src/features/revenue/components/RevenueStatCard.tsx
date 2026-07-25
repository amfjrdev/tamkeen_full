import React from 'react';
import type { RevenueStat } from '../types';
import { Icons } from '../../../components/common/Icons';

interface RevenueStatCardProps {
  stat: RevenueStat;
}

export const RevenueStatCard: React.FC<RevenueStatCardProps> = ({ stat }) => {
  const colorMap: Record<string, { bg: string; iconBg: string; text: string }> = {
    green: {
      bg: 'bg-emerald-500/5 border-emerald-500/20 hover:border-emerald-500/40',
      iconBg: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/25',
      text: 'text-emerald-400',
    },
    blue: {
      bg: 'bg-sky-500/5 border-sky-500/20 hover:border-sky-500/40',
      iconBg: 'bg-sky-500/10 text-sky-400 border border-sky-500/25',
      text: 'text-sky-400',
    },
    purple: {
      bg: 'bg-indigo-500/5 border-indigo-500/20 hover:border-indigo-500/40',
      iconBg: 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/25',
      text: 'text-indigo-400',
    },
    pink: {
      bg: 'bg-pink-500/5 border-pink-500/20 hover:border-pink-500/40',
      iconBg: 'bg-pink-500/10 text-pink-400 border border-pink-500/25',
      text: 'text-pink-400',
    },
    orange: {
      bg: 'bg-orange-500/5 border-orange-500/20 hover:border-orange-500/40',
      iconBg: 'bg-orange-500/10 text-orange-400 border border-orange-500/25',
      text: 'text-orange-400',
    },
    yellow: {
      bg: 'bg-amber-500/5 border-amber-500/20 hover:border-amber-500/40',
      iconBg: 'bg-amber-500/10 text-amber-400 border border-amber-500/25',
      text: 'text-amber-400',
    },
  };

  const iconMap: Record<string, React.ReactNode> = {
    dollar: <Icons.dollar />,
    'chart-up': <Icons.chartUp />,
    'credit-card': <Icons.creditCard />,
    'check-circle': <Icons.checkCircle className="w-6 h-6 text-emerald-500" />,
    'alert-circle': <Icons.alertCircle className="w-6 h-6 text-rose-500" />,
    clock: <Icons.clock className="w-6 h-6 text-amber-500" />,
  };

  const scheme = colorMap[stat.color] || colorMap.blue;

  return (
    <div className={`bg-[#111111] border rounded-xl p-5 hover:bg-[#141414] transition-all duration-300 ${scheme.bg} flex flex-col justify-between h-full`}>
      <div className="flex justify-between items-start mb-5.5">
        <div className={`w-10 h-10 rounded-xl flex items-center justify-center shadow-lg ${scheme.iconBg}`}>
          {iconMap[stat.icon]}
        </div>
        <span className={`text-[10px] font-bold flex items-center gap-0.5 px-1.5 py-0.5 rounded bg-black/35 border border-gray-800/40 ${
          stat.negative ? 'text-rose-400' : 'text-emerald-400'
        }`}>
          {stat.negative ? <Icons.trendDown className="w-2.5 h-2.5" /> : <Icons.trendUp className="w-2.5 h-2.5" />}
          {stat.trend}
        </span>
      </div>
      <div>
        <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider mb-1.5">{stat.label}</p>
        <p className="text-white text-2xl lg:text-3xl font-extrabold tracking-tight font-mono">{stat.value}</p>
      </div>
    </div>
  );
};
