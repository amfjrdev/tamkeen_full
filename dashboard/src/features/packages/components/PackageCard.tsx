import React from 'react';
import type { PackageItem } from '../types';
import { Icons } from '../../../components/common/Icons';

interface PackageCardProps {
  pkg: PackageItem;
  onEdit: (pkg: PackageItem) => void;
  onDelete: (id: number) => void;
  onToggleStatus: (id: number) => void;
}

export const PackageCard: React.FC<PackageCardProps> = ({
  pkg,
  onEdit,
  onDelete,
  onToggleStatus,
}) => {
  const borderMap: Record<string, string> = {
    blue: 'border-t-blue-500',
    indigo: 'border-t-indigo-500',
    pink: 'border-t-pink-500',
    orange: 'border-t-orange-500',
    gray: 'border-t-gray-500',
  };

  const badgeColorMap: Record<string, string> = {
    blue: 'bg-blue-500',
    indigo: 'bg-indigo-500',
    pink: 'bg-pink-500',
    orange: 'bg-orange-500',
    gray: 'bg-gray-500',
  };

  const borderClass = borderMap[pkg.color] || 'border-t-gray-500';
  const badgeClass = badgeColorMap[pkg.color] || 'bg-gray-500';

  return (
    <div
      className={`bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl overflow-hidden flex flex-col hover:border-gray-700 transition-all duration-300 shadow-xl ${borderClass} border-t-4`}
    >
      <div className="p-6 flex-1 flex flex-col">
        {/* Package Header */}
        <div className="flex justify-between items-start mb-2 gap-2">
          <div>
            <h3 className="text-xl font-bold text-white tracking-tight">{pkg.name}</h3>
            {pkg.isPopular && (
              <span className="inline-block mt-1 px-2.5 py-0.5 bg-indigo-600/90 text-white text-[10px] font-extrabold uppercase tracking-wider rounded-full shadow-sm">
                Most Popular
              </span>
            )}
          </div>

          <button
            onClick={() => onToggleStatus(pkg.id)}
            title={pkg.status === 'inactive' ? 'Activate Package' : 'Deactivate Package'}
            className={`p-1.5 rounded-lg hover:bg-gray-800/80 transition-colors cursor-pointer ${
              pkg.status === 'inactive' ? 'text-rose-500' : 'text-emerald-500'
            }`}
          >
            {pkg.status === 'inactive' ? (
              <Icons.eyeOff className="w-4 h-4" />
            ) : (
              <Icons.eye className="w-4 h-4" />
            )}
          </button>
        </div>

        <p className="text-gray-400 text-sm mt-1 mb-6 min-h-[40px] leading-relaxed">
          {pkg.description}
        </p>

        {/* Pricing Metrics */}
        <div className="mb-6">
          <div className="flex items-baseline gap-1">
            <span className="text-4xl font-extrabold text-white font-mono tracking-tight">
              {pkg.price} <span className="text-xl font-bold">DA</span>
            </span>
            <span className="text-gray-400 text-xs font-semibold">/ {pkg.credits} credits</span>
          </div>
          <p className="text-gray-500 text-xs font-semibold mt-1 font-mono">
            {pkg.perCredit.toFixed(2)} DA per credit
          </p>
        </div>

        {/* Features Checklist */}
        <ul className="space-y-3 mb-8 flex-1">
          {pkg.features.map((feature, idx) => (
            <li key={idx} className="flex items-center text-sm text-gray-300">
              <span className={`w-1.5 h-1.5 rounded-full mr-2.5 ${badgeClass}`}></span>
              {feature}
            </li>
          ))}
        </ul>

        {/* Sales Performance Summary */}
        <div className="bg-[#161616]/80 border border-gray-850 rounded-lg p-4 mb-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-gray-500 text-xs font-semibold uppercase tracking-wider mb-0.5">Sales</p>
              <p className="text-white font-bold font-mono text-sm">{pkg.sales.toLocaleString()}</p>
            </div>
            <div>
              <p className="text-gray-500 text-xs font-semibold uppercase tracking-wider mb-0.5">Revenue</p>
              <p className="text-emerald-400 font-extrabold font-mono text-sm">
                {pkg.revenue.toLocaleString()} DA
              </p>
            </div>
          </div>
        </div>

        {/* Popularity slider bar */}
        <div>
          <div className="flex justify-between text-xs font-semibold mb-2">
            <span className="text-gray-400">Popularity</span>
            <span className="text-white font-mono">{pkg.popularity}%</span>
          </div>
          <div className="w-full bg-gray-800 rounded-full h-1.5">
            <div
              className={`h-1.5 rounded-full ${badgeClass} transition-all duration-500`}
              style={{ width: `${pkg.popularity}%` }}
            ></div>
          </div>
        </div>
      </div>

      {/* Package Card Actions */}
      <div className="p-4 border-t border-gray-800/80 flex gap-3 bg-[#111111]/30">
        <button
          onClick={() => onEdit(pkg)}
          className="flex-1 flex items-center justify-center gap-2 py-2 bg-[#1a1a1a] hover:bg-gray-800 border border-gray-700/60 text-white text-xs font-bold rounded-lg transition-all cursor-pointer active:scale-95"
        >
          <Icons.edit className="w-3.5 h-3.5" /> Edit
        </button>
        <button
          onClick={() => onDelete(pkg.id)}
          className="p-2 bg-[#1a1a1a] hover:bg-rose-500/10 hover:border-rose-500/30 hover:text-rose-500 text-gray-400 border border-gray-700/60 rounded-lg transition-all cursor-pointer active:scale-95"
        >
          <Icons.trash className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
};
