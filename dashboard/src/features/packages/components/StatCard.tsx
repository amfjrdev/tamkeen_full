import React from 'react';
import type { PackageStat } from '../types';
import { Icons } from '../../../components/common/Icons';

interface StatCardProps {
  stat: PackageStat;
}

export const StatCard: React.FC<StatCardProps> = ({ stat }) => {
  const colorMap: Record<string, string> = {
    blue: 'bg-blue-500/10 text-blue-400 border border-blue-500/25',
    purple: 'bg-indigo-500/10 text-indigo-400 border border-indigo-500/25',
    pink: 'bg-pink-500/10 text-pink-400 border border-pink-500/25',
    green: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/25',
  };

  const iconMap: Record<string, React.ComponentType<React.SVGProps<SVGSVGElement>>> = {
    dollar: Icons.dollar,
    users: Icons.users,
    trendingUp: Icons.trendUp, // trendUp mapped from trendingUp
  };

  const IconComponent = iconMap[stat.icon] || Icons.trendUp;
  const styleClass = colorMap[stat.color] || colorMap.blue;

  return (
    <div className="bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl p-5 hover:bg-[#151515] hover:border-gray-700/80 transition-all duration-300 shadow-lg group">
      <div className={`w-10 h-10 rounded-lg ${styleClass} flex items-center justify-center mb-4 transition-transform group-hover:scale-105 duration-300`}>
        <IconComponent className="w-5 h-5" />
      </div>
      <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider mb-1">
        {stat.label}
      </p>
      <p className="text-white text-2xl font-extrabold tracking-tight font-mono">
        {stat.value}
      </p>
    </div>
  );
};
