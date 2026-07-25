import React from 'react';
import type { MessagingStat } from '../types';
import { Icons } from '../../../components/common/Icons';

interface MessagingStatCardProps {
  stat: MessagingStat;
}

export const MessagingStatCard: React.FC<MessagingStatCardProps> = ({ stat }) => {
  const colorMap: Record<string, { ring: string; iconBg: string; text: string }> = {
    green: {
      ring: 'border-emerald-500/20 hover:border-emerald-500/40 bg-emerald-500/5',
      iconBg: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/25',
      text: 'text-emerald-400',
    },
    blue: {
      ring: 'border-sky-500/20 hover:border-sky-500/40 bg-sky-500/5',
      iconBg: 'bg-sky-500/10 text-sky-400 border border-sky-500/25',
      text: 'text-sky-400',
    },
    red: {
      ring: 'border-rose-500/20 hover:border-rose-500/40 bg-rose-500/5',
      iconBg: 'bg-rose-500/10 text-rose-400 border border-rose-500/25',
      text: 'text-rose-400',
    },
    orange: {
      ring: 'border-amber-500/20 hover:border-amber-500/40 bg-amber-500/5',
      iconBg: 'bg-amber-500/10 text-amber-400 border border-amber-500/25',
      text: 'text-amber-400',
    },
  };

  const iconMap: Record<string, React.ReactNode> = {
    chat: <Icons.chat />,
    message: <Icons.messageSquare />,
    alert: <Icons.alert />,
    lock: <Icons.lockClosed />,
  };

  const scheme = colorMap[stat.color] || colorMap.blue;

  return (
    <div className={`border rounded-xl p-5 hover:bg-[#141414] transition-all duration-300 ${scheme.ring}`}>
      <div className="flex justify-between items-start">
        <div>
          <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider mb-2">{stat.label}</p>
          <p className="text-white text-3xl font-extrabold tracking-tight">{stat.value}</p>
        </div>
        <div className={`w-10 h-10 rounded-xl flex items-center justify-center shadow-lg transition-transform duration-300 hover:scale-105 ${scheme.iconBg}`}>
          {iconMap[stat.icon]}
        </div>
      </div>
      {stat.trend && (
        <div className="mt-4 flex items-center text-xs font-medium">
          <span className={`${stat.trend.startsWith('+') ? 'text-emerald-400' : stat.trend === '0' ? 'text-gray-400' : 'text-rose-400'} font-semibold font-mono`}>
            {stat.trend}
          </span>
          <span className="text-gray-500 ml-1.5 font-normal">vs last week</span>
        </div>
      )}
    </div>
  );
};
