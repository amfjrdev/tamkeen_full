import React from 'react';
import type { DailyActivity } from '../types';

interface ActivityChartProps {
  data: DailyActivity[];
}

export const ActivityChart: React.FC<ActivityChartProps> = ({ data }) => {
  const width = 500;
  const height = 250;
  const padding = 45;
  const chartWidth = width - padding * 2;
  const chartHeight = height - padding * 2;
  const maxVal = Math.max(...data.map((d) => d.value), 800);

  // Generate polyline points
  const points = data
    .map((d, i) => {
      const x = padding + (i / (data.length - 1)) * chartWidth;
      const y = height - padding - (d.value / maxVal) * chartHeight;
      return `${x},${y}`;
    })
    .join(' ');

  const areaPath = `${points} L ${width - padding},${height - padding} L ${padding},${height - padding} Z`;

  return (
    <div className="w-full overflow-x-auto select-none">
      <div className="min-w-[400px]">
        <svg viewBox={`0 0 ${width} ${height}`} className="w-full">
          <defs>
            <linearGradient id="activityAreaGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#ec4899" stopOpacity="0.15" />
              <stop offset="100%" stopColor="#ec4899" stopOpacity="0" />
            </linearGradient>
          </defs>

          {/* Horizontal Grid lines */}
          {[0, 0.25, 0.5, 0.75, 1].map((ratio, i) => {
            const val = Math.round(maxVal * ratio);
            const y = height - padding - ratio * chartHeight;
            return (
              <g key={i}>
                <line
                  x1={padding}
                  y1={y}
                  x2={width - padding}
                  y2={y}
                  stroke="#374151"
                  strokeWidth={0.5}
                  strokeDasharray="4 4"
                />
                <text
                  x={padding - 10}
                  y={y + 3}
                  textAnchor="end"
                  fill="#9ca3af"
                  className="font-mono text-[9px] font-semibold"
                >
                  {val}
                </text>
              </g>
            );
          })}

          {/* X Axis Labels */}
          {data.map((d, i) => {
            const x = padding + (i / (data.length - 1)) * chartWidth;
            return (
              <text
                key={i}
                x={x}
                y={height - padding + 20}
                textAnchor="middle"
                fill="#9ca3af"
                className="font-semibold text-[9px]"
              >
                {d.time}
              </text>
            );
          })}

          {/* Fill Area under polyline */}
          <path d={areaPath} fill="url(#activityAreaGradient)" />

          {/* Line connecting points */}
          <polyline points={points} fill="none" stroke="#ec4899" strokeWidth="2.5" />

          {/* Circle markers at each point */}
          {data.map((d, i) => {
            const x = padding + (i / (data.length - 1)) * chartWidth;
            const y = height - padding - (d.value / maxVal) * chartHeight;
            return (
              <g key={i} className="group cursor-pointer">
                <circle
                  cx={x}
                  cy={y}
                  r="4.5"
                  fill="#ec4899"
                  className="transition-all duration-200 hover:r-6"
                />
                <circle
                  cx={x}
                  cy={y}
                  r="8"
                  fill="none"
                  stroke="#ec4899"
                  strokeWidth={1}
                  className="opacity-0 group-hover:opacity-60 transition-opacity duration-200"
                />
              </g>
            );
          })}
        </svg>
      </div>
    </div>
  );
};
