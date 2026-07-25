import React from 'react';
import type { CategoryPerformance } from '../types';

interface CategoryChartProps {
  data: CategoryPerformance[];
}

export const CategoryChart: React.FC<CategoryChartProps> = ({ data }) => {
  const maxVal = Math.max(...data.map((d) => Math.max(d.requests, d.revenue)), 60000);

  return (
    <div className="w-full overflow-x-auto select-none">
      <div className="min-w-[400px]">
        <svg viewBox="0 0 600 300" className="w-full">
          {/* Horizontal grid lines */}
          {[0, 0.25, 0.5, 0.75, 1].map((ratio, i) => {
            const val = Math.round(maxVal * ratio);
            const y = 240 - ratio * 200;
            return (
              <g key={i}>
                <line
                  x1={140}
                  y1={y}
                  x2={560}
                  y2={y}
                  stroke="#374151"
                  strokeWidth={0.5}
                  strokeDasharray="4 4"
                />
                <text
                  x={130}
                  y={y + 3}
                  textAnchor="end"
                  fill="#9ca3af"
                  className="font-mono text-[9px] font-semibold"
                >
                  {val.toLocaleString()}
                </text>
              </g>
            );
          })}

          {/* Grouped vertical bars for each category */}
          {data.map((d, i) => {
            const x = 160 + i * 80;
            const barWidth = 16;
            
            // Scaled heights
            const hReq = (d.requests / maxVal) * 200;
            const hRev = (d.revenue / maxVal) * 200;

            const yReq = 240 - hReq;
            const yRev = 240 - hRev;

            return (
              <g key={i}>
                {/* Requests Bar (Indigo) */}
                <rect
                  x={x}
                  y={yReq}
                  width={barWidth}
                  height={hReq}
                  fill="#6366f1"
                  rx={2}
                  className="transition-all duration-300 hover:fill-indigo-400"
                />
                {/* Revenue Bar (Purple) */}
                <rect
                  x={x + barWidth + 4}
                  y={yRev}
                  width={barWidth}
                  height={hRev}
                  fill="#a855f7"
                  rx={2}
                  className="transition-all duration-300 hover:fill-purple-400"
                />
                {/* Category label */}
                <text
                  x={x + barWidth}
                  y={260}
                  textAnchor="middle"
                  fill="#9ca3af"
                  className="text-[9px] font-semibold"
                >
                  {d.name.split(' ')[0]}
                </text>
              </g>
            );
          })}
        </svg>
      </div>

      {/* Legend */}
      <div className="flex justify-center gap-6 mt-4">
        <div className="flex items-center gap-2">
          <div className="w-3.5 h-2 bg-indigo-500 rounded-sm shadow-sm" />
          <span className="text-gray-400 text-xs font-semibold">Requests</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3.5 h-2 bg-purple-500 rounded-sm shadow-sm" />
          <span className="text-gray-400 text-xs font-semibold">Revenue ($)</span>
        </div>
      </div>
    </div>
  );
};
