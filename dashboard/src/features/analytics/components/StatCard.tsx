import React from 'react';
import type { AnalyticsStat } from '../types';
import { Icons } from '../../../components/common/Icons';

interface StatCardProps {
  stat: AnalyticsStat;
}

export const StatCard: React.FC<StatCardProps> = ({ stat }) => {
  const colorMap: Record<string, string> = {
    green: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20',
    blue: 'bg-blue-500/10 text-blue-400 border border-blue-500/20',
    orange: 'bg-amber-500/10 text-amber-400 border border-amber-500/20',
    purple: 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/20',
  };

  const iconMap: Record<string, React.ComponentType<React.SVGProps<SVGSVGElement>>> = {
    dollar: Icons.dollar,
    users: Icons.users,
    clock: Icons.clock,
    trendingUp: Icons.trendUp,
  };

  const IconComponent = iconMap[stat.icon] || Icons.trendUp;
  const styleClass = colorMap[stat.color] || colorMap.blue;

  return (
    <div className="bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl p-5 hover:bg-[#151515] transition-all duration-300 shadow-lg group">
      <div className="flex justify-between items-start">
        <div className="space-y-2">
          <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider">
            {stat.label}
          </p>
          <p className="text-white text-3xl font-extrabold tracking-tight font-mono">
            {stat.value}
          </p>
          <p className={`text-xs font-semibold flex items-center gap-1 ${stat.positive ? 'text-emerald-400' : 'text-rose-400'}`}>
            <span>{stat.trend}</span>
            <span className="text-gray-500 font-normal">{stat.trendLabel}</span>
          </p>
        </div>
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center ${styleClass} transition-transform group-hover:scale-105 duration-300`}>
          <IconComponent className="w-6 h-6" />
        </div>
      </div>
    </div>
  );
};
