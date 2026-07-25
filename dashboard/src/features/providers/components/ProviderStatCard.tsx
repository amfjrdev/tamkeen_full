import React from 'react';
import type { ProviderStat } from '../types';
import { Icons } from '../../../components/common/Icons';

interface ProviderStatCardProps {
  stat: ProviderStat;
}

export const ProviderStatCard: React.FC<ProviderStatCardProps> = ({ stat }) => {
  const colorMap: Record<string, { cardBorder: string; iconBg: string; textColor: string }> = {
    orange: {
      cardBorder: 'border-orange-500/20 hover:border-orange-500/40 bg-orange-500/5',
      iconBg: 'bg-orange-500/10 text-orange-400 border border-orange-500/25',
      textColor: 'text-orange-400',
    },
    green: {
      cardBorder: 'border-emerald-500/20 hover:border-emerald-500/40 bg-emerald-500/5',
      iconBg: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/25',
      textColor: 'text-emerald-400',
    },
    yellow: {
      cardBorder: 'border-amber-500/20 hover:border-amber-500/40 bg-amber-500/5',
      iconBg: 'bg-amber-500/10 text-amber-400 border border-amber-500/25',
      textColor: 'text-amber-400',
    },
  };

  const iconMap: Record<string, React.ReactNode> = {
    pending: <Icons.clockPending />,
    active: <Icons.checkActive />,
    rating: <Icons.starIcon />,
  };

  const scheme = colorMap[stat.color] || colorMap.green;

  return (
    <div className={`border rounded-xl p-5 hover:bg-[#141414] transition-all duration-300 ${scheme.cardBorder}`}>
      <div className="flex items-start justify-between">
        <div>
          <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider mb-2">{stat.label}</p>
          <p className="text-white text-3xl font-extrabold tracking-tight">{stat.value}</p>
        </div>
        <div className={`w-12 h-12 rounded-xl flex items-center justify-center shadow-lg transition-transform duration-300 hover:scale-105 ${scheme.iconBg}`}>
          {iconMap[stat.icon]}
        </div>
      </div>
    </div>
  );
};
