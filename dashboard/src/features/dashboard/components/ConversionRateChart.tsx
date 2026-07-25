import React from 'react';
import type { ConversionRateItem } from '../types';

interface ConversionRateChartProps {
  data: ConversionRateItem[];
}

export const ConversionRateChart: React.FC<ConversionRateChartProps> = ({ data }) => {
  const maxVal = 100;

  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl p-5 lg:p-6 hover:border-gray-700 transition-colors">
      <h3 className="text-white text-lg font-semibold mb-1">Conversion Rate</h3>
      <p className="text-gray-400 text-sm mb-8">Client to provider connections</p>
      
      <div className="flex items-end justify-between gap-4 h-48 px-2 relative">
        {data.map((item, idx) => (
          <div key={idx} className="flex flex-col items-center flex-1 h-full justify-end group">
            {/* Value Tooltip on Hover */}
            <span className="opacity-0 group-hover:opacity-100 transition-opacity bg-indigo-600 text-white text-[10px] font-bold px-1.5 py-0.5 rounded absolute mb-2 translate-y-[-180px] pointer-events-none font-mono">
              {item.value}%
            </span>
            
            {/* Bar Container */}
            <div className="w-full bg-gray-800/20 rounded-t-lg h-full flex items-end overflow-hidden">
              <div
                className="w-full bg-gradient-to-t from-indigo-600 to-purple-500 rounded-t-lg transition-all duration-700 ease-out group-hover:brightness-110 shadow-lg shadow-indigo-500/10 cursor-pointer"
                style={{ height: `${(item.value / maxVal) * 100}%` }}
              />
            </div>
            
            <span className="text-gray-400 text-xs mt-3 font-medium">{item.month}</span>
          </div>
        ))}
      </div>
      
      <div className="flex items-center justify-between px-2 mt-4 pt-3 border-t border-gray-800/50">
        <span className="text-gray-500 text-[10px] font-semibold font-mono">0%</span>
        <span className="text-gray-500 text-[10px] font-semibold font-mono">25%</span>
        <span className="text-gray-500 text-[10px] font-semibold font-mono">50%</span>
        <span className="text-gray-500 text-[10px] font-semibold font-mono">75%</span>
        <span className="text-gray-500 text-[10px] font-semibold font-mono">100%</span>
      </div>
    </div>
  );
};
