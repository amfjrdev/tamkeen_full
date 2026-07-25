import React from 'react';
import type { SalesTrendItem } from '../types';

interface SalesTrendChartProps {
  data: SalesTrendItem[];
}

export const SalesTrendChart: React.FC<SalesTrendChartProps> = ({ data }) => {
  const width = 800;
  const height = 300;
  const padding = 40;
  const chartWidth = width - padding * 2;
  const chartHeight = height - padding * 2;
  const maxVal = 500; // Adjusted based on max value in salesTrend (~400) to fill chart nicely
  const barGroupWidth = chartWidth / data.length;
  const barWidth = (barGroupWidth * 0.6) / 4;
  const gap = (barGroupWidth * 0.4) / 5;

  const colors = ['#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b']; // starter, professional, premium, enterprise
  const yTicks = [0, 125, 250, 375, 500];

  return (
    <div className="w-full overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
      <svg viewBox={`0 0 ${width} ${height}`} className="w-full min-w-[700px]" preserveAspectRatio="xMidYMid meet">
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
                {val}
              </text>
            </g>
          );
        })}

        {/* Grouped Bars */}
        {data.map((d, i) => {
          const xStart = padding + i * barGroupWidth;
          const keys = ['starter', 'professional', 'premium', 'enterprise'];
          
          return (
            <g key={i}>
              {keys.map((key, j) => {
                const val = d[key] as number;
                const h = (val / maxVal) * chartHeight;
                const x = xStart + gap + j * (barWidth + gap);
                const y = height - padding - h;
                return (
                  <rect 
                    key={j} 
                    x={x} 
                    y={y} 
                    width={barWidth} 
                    height={h} 
                    fill={colors[j]} 
                    rx="2"
                    className="transition-all duration-300 hover:brightness-110 cursor-pointer"
                  />
                );
              })}
              <text 
                x={xStart + barGroupWidth / 2} 
                y={height - 15} 
                textAnchor="middle" 
                fill="#9ca3af" 
                fontSize="10"
                className="font-semibold"
              >
                {d.month}
              </text>
            </g>
          );
        })}
      </svg>
    </div>
  );
};
