import React from 'react';
import type { ServiceCategory } from '../types';

interface ServiceCategoriesChartProps {
  data: ServiceCategory[];
}

export const ServiceCategoriesChart: React.FC<ServiceCategoriesChartProps> = ({ data }) => {
  const total = data.reduce((sum, item) => sum + item.value, 0);
  const radius = 75;
  const strokeWidth = 22;
  const circumference = 2 * Math.PI * radius;
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl p-5 lg:p-6 hover:border-gray-700 transition-colors flex flex-col justify-between">
      <div>
        <h3 className="text-white text-lg font-semibold mb-1">Service Categories</h3>
        <p className="text-gray-400 text-sm mb-6">Distribution by category</p>
      </div>

      <div className="flex flex-col items-center justify-center my-auto">
        <div className="relative w-[200px] h-[200px]">
          <svg width="200" height="200" viewBox="0 0 200 200" className="transform -rotate-90">
            {data.map((item, idx) => {
              const dashLength = (item.value / total) * circumference;
              const dashGap = circumference - dashLength;
              const offset = data.slice(0, idx).reduce((sum, prev) => sum + (prev.value / total) * circumference, 0);
              return (
                <circle
                  key={idx}
                  cx="100"
                  cy="100"
                  r={radius}
                  fill="none"
                  stroke={item.color}
                  strokeWidth={strokeWidth}
                  strokeDasharray={`${dashLength} ${dashGap}`}
                  strokeDashoffset={-offset}
                  strokeLinecap="round"
                  className="transition-all duration-500 ease-out cursor-pointer hover:opacity-85"
                  style={{
                    transformOrigin: '50% 50%',
                  }}
                />
              );
            })}
          </svg>
          {/* Inner Text for Donut Chart */}
          <div className="absolute inset-0 flex flex-col items-center justify-center pointer-events-none">
            <span className="text-white text-3xl font-bold tracking-tight">{total}%</span>
            <span className="text-gray-400 text-xs font-medium">Assigned</span>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-x-6 gap-y-2.5 mt-8 w-full">
          {data.map((item, idx) => (
            <div key={idx} className="flex items-center gap-2 px-1">
              <div 
                className="w-2.5 h-2.5 rounded-full shrink-0" 
                style={{ backgroundColor: item.color }} 
              />
              <span className="text-gray-400 text-xs font-medium truncate">{item.name}</span>
              <span className="text-gray-500 text-xs ml-auto font-mono">{item.value}%</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};
