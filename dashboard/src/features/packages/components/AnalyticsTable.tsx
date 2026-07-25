import React from 'react';
import type { PackageAnalytics } from '../types';

interface AnalyticsTableProps {
  analyticsData: PackageAnalytics[];
}

export const AnalyticsTable: React.FC<AnalyticsTableProps> = ({ analyticsData }) => {
  const getAvatarBg = (name: string) => {
    if (name.includes('Starter')) return 'bg-blue-500';
    if (name.includes('Professional')) return 'bg-indigo-500';
    if (name.includes('Premium')) return 'bg-pink-500';
    return 'bg-amber-500';
  };

  return (
    <div className="bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl overflow-hidden shadow-xl">
      <div className="p-6 border-b border-gray-800 bg-[#111111]/30">
        <h2 className="text-lg font-bold text-white tracking-tight">
          Package Performance Analytics
        </h2>
        <p className="text-gray-400 text-sm mt-0.5">
          Detailed sales metrics for each credit pricing tier
        </p>
      </div>

      <div className="overflow-x-auto">
        <table className="w-full min-w-[800px]">
          <thead className="bg-[#161616]/95 text-left border-b border-gray-800">
            <tr>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Package Name
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Credits
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Price
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Total Sales
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Revenue
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Conversion
              </th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">
                Status
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-800/60">
            {analyticsData.map((item) => {
              const themeColor = getAvatarBg(item.name);
              return (
                <tr key={item.id} className="hover:bg-gray-850/30 transition-colors">
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center gap-3">
                      <div className={`w-8 h-8 rounded-lg flex items-center justify-center text-white text-xs font-bold font-mono ${themeColor}`}>
                        {item.credits}
                      </div>
                      <div>
                        <p className="text-white font-semibold text-sm">{item.name}</p>
                        {item.tag && <p className="text-indigo-400 text-[10px] font-semibold mt-0.5">{item.tag}</p>}
                      </div>
                    </div>
                  </td>
                  <td className="px-6 py-4 text-indigo-400 font-bold text-sm font-mono whitespace-nowrap">
                    {item.credits} <span className="text-gray-500 text-xs font-normal">credits</span>
                  </td>
                  <td className="px-6 py-4 text-white text-sm font-mono whitespace-nowrap">
                    {item.price} DA
                  </td>
                  <td className="px-6 py-4 text-gray-300 text-sm font-mono whitespace-nowrap">
                    {item.sales.toLocaleString()} sales
                  </td>
                  <td className="px-6 py-4 text-emerald-400 font-extrabold text-sm font-mono whitespace-nowrap">
                    {item.revenue.toLocaleString()} DA
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center gap-3">
                      <div className="w-20 bg-gray-800 rounded-full h-1.5 overflow-hidden">
                        <div
                          className={`h-full rounded-full ${themeColor} transition-all duration-500`}
                          style={{ width: `${item.conversion}%` }}
                        ></div>
                      </div>
                      <span className="text-white text-xs font-semibold font-mono">{item.conversion}%</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-500 border border-emerald-500/20 capitalize">
                      {item.status}
                    </span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
};
