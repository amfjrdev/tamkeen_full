import React from 'react';
import type { TopProviderItem } from '../types';

interface TopProvidersTableProps {
  providers: TopProviderItem[];
}

export const TopProvidersTable: React.FC<TopProvidersTableProps> = ({ providers }) => {
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl">
      <div className="p-6 border-b border-gray-800">
        <h2 className="text-lg font-semibold text-white">Top Spending Providers</h2>
        <p className="text-gray-400 text-sm">Highest revenue contributors based on purchased packages</p>
      </div>
      <div className="overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <table className="w-full min-w-[600px] border-collapse">
          <thead className="bg-[#161616] text-left border-b border-gray-800">
            <tr>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Rank</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Provider</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Total Spent</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Credits Purchased</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Packages Bought</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {providers.map((provider) => (
              <tr key={provider.rank} className="hover:bg-gray-800/20 transition-colors">
                <td className="px-6 py-4">
                  <div className="w-8 h-8 rounded-lg bg-indigo-600/10 border border-indigo-500/20 flex items-center justify-center text-indigo-400 text-sm font-extrabold font-mono">
                    #{provider.rank}
                  </div>
                </td>
                <td className="px-6 py-4 text-white font-semibold text-sm">{provider.name}</td>
                <td className="px-6 py-4 text-emerald-400 font-bold font-mono text-sm">
                  {provider.spent.toLocaleString()} DA
                </td>
                <td className="px-6 py-4 text-gray-300 font-semibold font-mono text-sm">
                  {provider.credits.toLocaleString()} credits
                </td>
                <td className="px-6 py-4 text-gray-300 font-semibold font-mono text-sm">
                  {provider.packages} packages
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
