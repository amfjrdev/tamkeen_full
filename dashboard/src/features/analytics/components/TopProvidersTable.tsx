import React from 'react';
import type { TopProvider } from '../types';
import { Icons } from '../../../components/common/Icons';

interface TopProvidersTableProps {
  providers: TopProvider[];
}

export const TopProvidersTable: React.FC<TopProvidersTableProps> = ({ providers }) => {
  return (
    <div className="bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl overflow-hidden shadow-xl">
      <div className="p-6 border-b border-gray-800 bg-[#111111]/30">
        <h2 className="text-lg font-bold text-white tracking-tight">Top Provider Rankings</h2>
        <p className="text-gray-400 text-sm mt-0.5">Best performing platform service providers</p>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[600px]">
          <thead className="bg-[#161616]/95 text-left border-b border-gray-800">
            <tr>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider w-24">Rank</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Provider</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Completions</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Rating</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Revenue</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-800/60">
            {providers.map((provider) => (
              <tr key={provider.rank} className="hover:bg-gray-850/30 transition-colors">
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="w-8 h-8 rounded-full bg-indigo-600/90 text-white text-xs font-extrabold flex items-center justify-center shadow-sm">
                    {provider.rank}
                  </div>
                </td>
                <td className="px-6 py-4 text-white font-semibold text-sm whitespace-nowrap">
                  {provider.name}
                </td>
                <td className="px-6 py-4 text-gray-300 text-sm font-mono whitespace-nowrap">
                  {provider.completions} tasks
                </td>
                <td className="px-6 py-4 whitespace-nowrap">
                  <div className="flex items-center gap-1 text-white font-mono text-sm font-semibold">
                    <span>{provider.rating.toFixed(1)}</span>
                    <Icons.star className="w-3.5 h-3.5 text-yellow-400 fill-current" />
                  </div>
                </td>
                <td className="px-6 py-4 text-emerald-400 font-extrabold text-sm font-mono whitespace-nowrap">
                  {provider.revenue.toLocaleString()} DA
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
