import React from 'react';
import type { Category } from '../types';

interface CategoryTableProps {
  categories: Category[];
}

export const CategoryTable: React.FC<CategoryTableProps> = ({ categories }) => {
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl">
      <div className="p-6 border-b border-gray-800">
        <h2 className="text-lg font-semibold text-white">Category Statistics</h2>
        <p className="text-gray-400 text-sm">Detailed usage metrics and performance indexes</p>
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
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {categories.map((cat) => (
              <tr key={cat.id} className="hover:bg-gray-800/20 transition-colors">
                <td className="px-6 py-4">
                  <div className="flex items-center gap-3">
                    <div className={`w-8 h-8 rounded-lg ${cat.color} flex items-center justify-center shrink-0 shadow`}>
                       {/* Simplified icon rendering representation for table */}
                       <div className="w-3.5 h-3.5 bg-white/20 rounded-sm"></div> 
                    </div>
                    <span className="text-white font-semibold text-sm">{cat.name}</span>
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
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
