import React from 'react';
import type { DailyRevenueItem } from '../types';

interface DailyRevenueChartProps {
  data: DailyRevenueItem[];
}

export const DailyRevenueChart: React.FC<DailyRevenueChartProps> = ({ data }) => {
  const width = 500;
  const height = 250;
  const padding = 40;
  const chartWidth = width - padding * 2;
  const chartHeight = height - padding * 2;
  const maxVal = 16000;
  const barWidth = (chartWidth / data.length) * 0.55;
  const gap = (chartWidth / data.length) * 0.45;

  const yTicks = [0, 4000, 8000, 12000, 16000];

  return (
    <div className="w-full overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
      <svg viewBox={`0 0 ${width} ${height}`} className="w-full min-w-[380px]" preserveAspectRatio="xMidYMid meet">
        {/* Grid lines */}
        {yTicks.map((val, i) => {
          const y = height - padding - (val / maxVal) * chartHeight;
          return (
            <g key={i} className="opacity-30">
              <line 
                x1={padding} 
                y1={y} 
                x2={width - padding} 
                y2={y} 
                stroke="#374151" 
                strokeWidth="0.75"
                strokeDasharray="4 4" 
              />
              <text 
                x={padding - 10} 
                y={y + 3} 
                textAnchor="end" 
                fill="#9ca3af" 
                fontSize="9"
                className="font-mono font-semibold"
              >
                ${val.toLocaleString()}
              </text>
            </g>
          );
        })}

        {/* Bars */}
        {data.map((d, i) => {
          const x = padding + (i * (barWidth + gap)) + gap / 2;
          const h = (d.value / maxVal) * chartHeight;
          const y = height - padding - h;
          return (
            <g key={i} className="group cursor-pointer">
              {/* Tooltip on hover */}
              <text
                x={x + barWidth / 2}
                y={y - 8}
                textAnchor="middle"
                fill="white"
                fontSize="9"
                className="opacity-0 group-hover:opacity-100 transition-opacity duration-200 font-bold font-mono bg-black"
              >
                ${d.value.toLocaleString()}
              </text>

              {/* Bar rectangle with gradient-like look */}
              <rect 
                x={x} 
                y={y} 
                width={barWidth} 
                height={h} 
                fill="url(#barGradient)" 
                rx="4" 
                className="transition-all duration-300 group-hover:brightness-125"
              />
              <text 
                x={x + barWidth / 2} 
                y={height - 15} 
                textAnchor="middle" 
                fill="#9ca3af" 
                fontSize="10"
                className="font-medium"
              >
                {d.day}
              </text>
            </g>
          );
        })}

        {/* Gradients */}
        <defs>
          <linearGradient id="barGradient" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor="#818cf8" />
            <stop offset="100%" stopColor="#4f46e5" />
          </linearGradient>
        </defs>
      </svg>
    </div>
  );
};
