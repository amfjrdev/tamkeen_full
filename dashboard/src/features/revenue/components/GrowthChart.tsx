import React from 'react';
import type { GrowthItem } from '../types';

interface GrowthChartProps {
  data: GrowthItem[];
}

export const GrowthChart: React.FC<GrowthChartProps> = ({ data }) => {
  const width = 800;
  const height = 300;
  const padding = 40;
  const chartWidth = width - padding * 2;
  const chartHeight = height - padding * 2;
  
  const maxVal = 180000;
  
  // Calculate points for actual revenue line
  // Note: Only plot months where revenue > 0 (to avoid drawing zero for future months)
  const actualData = data.filter(d => d.revenue > 0);
  const points = actualData.map((d, i) => {
    const x = padding + (i / (data.length - 1)) * chartWidth;
    const y = height - padding - (d.revenue / maxVal) * chartHeight;
    return `${x},${y}`;
  }).join(' ');

  // Calculate points for forecast revenue line
  const forecastPoints = data.map((d, i) => {
    const x = padding + (i / (data.length - 1)) * chartWidth;
    const y = height - padding - (d.forecast / maxVal) * chartHeight;
    return `${x},${y}`;
  }).join(' ');

  // SVG Area polyfill points
  const firstX = padding;
  const bottomY = height - padding;
  const lastActualIndex = actualData.length - 1;
  const lastX = padding + (lastActualIndex / (data.length - 1)) * chartWidth;

  const yTicks = [0, 45000, 90000, 135000, 180000];

  return (
    <div className="w-full overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
      <svg viewBox={`0 0 ${width} ${height}`} className="w-full min-w-[700px]" preserveAspectRatio="xMidYMid meet">
        <defs>
          <linearGradient id="areaGradientRevenue" x1="0" y1="0" x2="0" y2="1">
            <stop offset="0%" stopColor="#6366f1" stopOpacity="0.3" />
            <stop offset="100%" stopColor="#6366f1" stopOpacity="0" />
          </linearGradient>
        </defs>

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
                y={y + 3.5} 
                textAnchor="end" 
                fill="#9ca3af" 
                fontSize="10"
                className="font-mono font-semibold"
              >
                ${(val / 1000).toFixed(0)}k
              </text>
            </g>
          );
        })}

        {/* X Axis Labels */}
        {data.map((d, i) => (
          <text 
            key={i} 
            x={padding + (i / (data.length - 1)) * chartWidth} 
            y={height - 10} 
            textAnchor="middle" 
            fill="#9ca3af" 
            fontSize="10"
            className="font-semibold"
          >
            {d.month}
          </text>
        ))}

        {/* Area Fill for actual revenue */}
        {actualData.length > 0 && (
          <polygon 
            points={`${firstX},${bottomY} ${points} ${lastX},${bottomY}`} 
            fill="url(#areaGradientRevenue)" 
          />
        )}

        {/* Forecast Line (Dashed) */}
        <polyline 
          points={forecastPoints} 
          fill="none" 
          stroke="#8b5cf6" 
          strokeWidth="2.5" 
          strokeDasharray="5 5" 
          strokeLinecap="round"
          className="opacity-70"
        />

        {/* Actual Revenue Line */}
        {actualData.length > 0 && (
          <polyline 
            points={points} 
            fill="none" 
            stroke="#6366f1" 
            strokeWidth="3" 
            strokeLinecap="round"
          />
        )}

        {/* March Highlight Line */}
        <line 
          x1={padding + (2 / (data.length - 1)) * chartWidth} 
          y1={padding} 
          x2={padding + (2 / (data.length - 1)) * chartWidth} 
          y2={height - padding} 
          stroke="#4b5563" 
          strokeDasharray="2 2" 
          className="opacity-50"
        />
        
        {/* Highlight Dot for actual data point */}
        {actualData.length > 2 && (
          <circle 
            cx={padding + (2 / (data.length - 1)) * chartWidth} 
            cy={height - padding - (data[2].revenue / maxVal) * chartHeight} 
            r="4.5" 
            fill="white" 
            stroke="#6366f1"
            strokeWidth="2.5"
            className="shadow-lg"
          />
        )}
      </svg>
    </div>
  );
};
