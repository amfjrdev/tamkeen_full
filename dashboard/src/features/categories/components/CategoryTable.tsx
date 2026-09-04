import React from 'react';
import type { Category } from '../types';
import { Icons } from '../../../components/common/Icons';

interface CategoryTableProps {
  categories: Category[];
  onDelete?: (id: string | number, name: string) => void;
  deletingId?: string | number | null;
}

export const CategoryTable: React.FC<CategoryTableProps> = ({ 
  categories, 
  onDelete,
  deletingId = null 
}) => {
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl">
      <div className="p-6 border-b border-gray-800 flex items-center justify-between">
        <div>
          <h2 className="text-lg font-semibold text-white">Category Statistics</h2>
          <p className="text-gray-400 text-sm">Detailed usage metrics and performance indexes</p>
        </div>
      </div>
      <div className="overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <table className="w-full min-w-[800px] border-collapse">
          <thead className="bg-[#161616] text-left border-b border-gray-800">
            <tr>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Category</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Providers</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Requests</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Completion Rate</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Growth</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {categories.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-10 text-center text-gray-500 text-sm font-medium">
                  No service categories found.
                </td>
              </tr>
            ) : (
              categories.map((cat) => (
                <tr key={cat.id} className="hover:bg-gray-800/20 transition-colors">
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3">
                      <div className={`w-8 h-8 rounded-lg ${cat.color} flex items-center justify-center shrink-0 shadow`}>
                        <div className="w-3.5 h-3.5 bg-white/20 rounded-sm" />
                      </div>
                      <div>
                        <span className="text-white font-semibold text-sm block">{cat.name}</span>
                        {cat.description && (
                          <span className="text-gray-500 text-xs truncate max-w-[200px] block">{cat.description}</span>
                        )}
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-gray-300 font-semibold font-mono">{cat.providers}</td>
                  <td className="px-6 py-4 text-sm text-gray-300 font-semibold font-mono">{cat.requests}</td>
                  <td className="px-6 py-4">
                    <div className="flex items-center gap-3.5">
                      <div className="flex-1 h-2 bg-gray-800/50 rounded-full overflow-hidden max-w-[100px] border border-gray-800">
                        <div 
                          className="h-full bg-gradient-to-r from-indigo-600 to-purple-500 rounded-full" 
                          style={{ width: `${cat.completionRate}%` }}
                        />
                      </div>
                      <span className="text-white text-xs font-bold font-mono">{cat.completionRate}%</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-sm text-emerald-400 font-bold font-mono">{cat.growth}</td>
                  <td className="px-6 py-4 text-right">
                    {onDelete && (
                      <button
                        onClick={() => onDelete(cat.id, cat.name)}
                        disabled={deletingId === cat.id}
                        title={`Delete ${cat.name}`}
                        aria-label={`Delete category ${cat.name}`}
                        className="text-gray-500 hover:text-rose-500 hover:bg-rose-500/10 p-2 rounded-lg border border-transparent hover:border-rose-500/20 transition-all duration-200 active:scale-95 cursor-pointer disabled:opacity-40"
                      >
                        <Icons.trash className="w-4 h-4" />
                      </button>
                    )}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
