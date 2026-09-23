import React from 'react';

interface ServiceRequestStatCardProps {
  label: string;
  value: string | number;
  icon: React.ReactNode;
  color: 'orange' | 'green' | 'blue' | 'purple';
}

export const ServiceRequestStatCard: React.FC<ServiceRequestStatCardProps> = ({
  label,
  value,
  icon,
  color,
}) => {
  const colorMap = {
    orange: 'bg-orange-500/10 text-orange-400 border-orange-500/20',
    green: 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20',
    blue: 'bg-indigo-500/10 text-indigo-400 border-indigo-500/20',
    purple: 'bg-purple-500/10 text-purple-400 border-purple-500/20',
  };

  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl p-5 shadow-lg flex items-center justify-between">
      <div>
        <p className="text-gray-400 text-xs font-medium uppercase tracking-wider">{label}</p>
        <p className="text-white text-2xl font-bold mt-1.5">{value}</p>
      </div>
      <div className={`w-12 h-12 rounded-xl flex items-center justify-center border ${colorMap[color]}`}>
        {icon}
      </div>
    </div>
  );
};
