import React from 'react';
import type { StatItem } from '../types';
import { TrendUpIcon, UsersIcon, ProvidersIcon, MessagingIcon } from '../../../components/common/Icons';

interface StatCardProps {
  stat: StatItem;
}

export const StatCard: React.FC<StatCardProps> = ({ stat }) => {
  const colorMap: Record<string, { bg: string; text: string; iconBg: string }> = {
    green: {
      bg: 'bg-emerald-500/10 border-emerald-500/20 hover:border-emerald-500/40',
      text: 'text-emerald-400',
      iconBg: 'bg-emerald-500 text-white',
    },
    blue: {
      bg: 'bg-sky-500/10 border-sky-500/20 hover:border-sky-500/40',
      text: 'text-sky-400',
      iconBg: 'bg-sky-500 text-white',
    },
    purple: {
      bg: 'bg-indigo-500/10 border-indigo-500/20 hover:border-indigo-500/40',
      text: 'text-indigo-400',
      iconBg: 'bg-indigo-500 text-white',
    },
    pink: {
      bg: 'bg-pink-500/10 border-pink-500/20 hover:border-pink-500/40',
      text: 'text-pink-400',
      iconBg: 'bg-pink-500 text-white',
    },
  };

  const iconMap: Record<string, React.ReactNode> = {
    revenue: <TrendUpIcon />,
    users: <UsersIcon />,
    providers: <ProvidersIcon />,
    messages: <MessagingIcon />,
  };

  const scheme = colorMap[stat.color] || colorMap.blue;

  return (
    <div className={`bg-[#111111] border rounded-xl p-5 hover:bg-[#151515] transition-all duration-300 ${scheme.bg}`}>
      <div className="flex items-start justify-between">
        <div>
          <p className="text-gray-400 text-sm font-medium mb-1">{stat.label}</p>
          <p className="text-white text-2xl lg:text-3xl font-bold tracking-tight">{stat.value}</p>
          <p className="text-emerald-400 text-xs mt-3 flex items-center gap-1 font-medium">
            <span className="inline-block transform translate-y-[-1px]">↑</span> 
            {stat.change} <span className="text-gray-500 font-normal">vs last week</span>
          </p>
        </div>
        <div className={`${scheme.iconBg} w-12 h-12 rounded-xl flex items-center justify-center shadow-lg transition-transform duration-300 hover:scale-105`}>
          {iconMap[stat.icon]}
        </div>
      </div>
    </div>
  );
};
