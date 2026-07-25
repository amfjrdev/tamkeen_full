import React from 'react';
import type { Provider } from '../types';
import { ProviderRow } from './ProviderRow';

interface ProvidersTableProps {
  providers: Provider[];
}

export const ProvidersTable: React.FC<ProvidersTableProps> = ({ providers }) => {
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl">
      <div className="overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <table className="w-full min-w-[1100px] border-collapse">
          <thead className="bg-[#161616] border-b border-gray-800">
            <tr>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Provider</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Category</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Status</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Performance</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Credits</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Total Spent</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Connect Permission</th>
              <th className="text-right text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {providers.length === 0 ? (
              <tr>
                <td colSpan={8} className="px-6 py-10 text-center text-gray-500 text-sm font-medium">
                  No providers found matching your filters.
                </td>
              </tr>
            ) : (
              providers.map((provider) => (
                <ProviderRow key={provider.id} provider={provider} />
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
