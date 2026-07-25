import React from 'react';
import type { GrowthTrend } from '../types';

interface GrowthChartProps {
  data: GrowthTrend[];
}

export const GrowthChart: React.FC<GrowthChartProps> = ({ data }) => {
  const width = 800;
  const height = 300;
  const padding = 45;
  const chartWidth = width - padding * 2;
  const chartHeight = height - padding * 2;

  // Find max value in datasets to scale SVG paths correctly
  const maxVal = Math.max(...data.map((d) => Math.max(d.users, d.providers, d.requests)), 100);

  // Generate coordinate points for users (Indigo), providers (Amber), and requests (Pink)
  const getPoints = (key: 'users' | 'providers' | 'requests') => {
    return data.map((d, i) => {
      const x = padding + (i / (data.length - 1)) * chartWidth;
      const y = height - padding - (d[key] / maxVal) * chartHeight;
      return `${x},${y}`;
    }).join(' ');
  };

  const usersPoints = getPoints('users');
  const providersPoints = getPoints('providers');
  const requestsPoints = getPoints('requests');

  // Area path for requests gradient fill
  const requestsAreaPath = `${requestsPoints} L ${width - padding},${height - padding} L ${padding},${height - padding} Z`;

  return (
    <div className="w-full overflow-x-auto select-none">
      <div className="min-w-[600px]">
        <svg viewBox={`0 0 ${width} ${height}`} className="w-full">
          <defs>
            <linearGradient id="requestsAreaGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#ec4899" stopOpacity="0.15" />
              <stop offset="100%" stopColor="#ec4899" stopOpacity="0" />
            </linearGradient>
            <linearGradient id="usersAreaGradient" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#6366f1" stopOpacity="0.1" />
              <stop offset="100%" stopColor="#6366f1" stopOpacity="0" />
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
                  x={padding - 12}
                  y={y + 3}
                  textAnchor="end"
                  fill="#9ca3af"
                  className="font-mono text-[10px] font-semibold"
                >
                  {val.toLocaleString()}
                </text>
              </g>
            );
          })}

          {/* Month labels along X-axis */}
          {data.map((d, i) => {
            const x = padding + (i / (data.length - 1)) * chartWidth;
            return (
              <text
                key={i}
                x={x}
                y={height - padding + 20}
                textAnchor="middle"
                fill="#9ca3af"
                className="font-semibold text-[10px]"
              >
                {d.month}
              </text>
            );
          })}

          {/* Gradient Areas */}
          <path d={requestsAreaPath} fill="url(#requestsAreaGradient)" />

          {/* Lines */}
          <polyline points={usersPoints} fill="none" stroke="#6366f1" strokeWidth="2.5" />
          <polyline points={providersPoints} fill="none" stroke="#f59e0b" strokeWidth="2.5" />
          <polyline points={requestsPoints} fill="none" stroke="#ec4899" strokeWidth="2.5" />

          {/* Dot Highlights */}
          {data.map((d, i) => {
            const x = padding + (i / (data.length - 1)) * chartWidth;
            return (
              <g key={i}>
                <circle cx={x} cy={height - padding - (d.users / maxVal) * chartHeight} r="4" fill="#6366f1" />
                <circle cx={x} cy={height - padding - (d.providers / maxVal) * chartHeight} r="4" fill="#f59e0b" />
                <circle cx={x} cy={height - padding - (d.requests / maxVal) * chartHeight} r="4" fill="#ec4899" />
              </g>
            );
          })}
        </svg>
      </div>

      {/* Chart Legend */}
      <div className="flex justify-center gap-6 mt-4">
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-indigo-500 shadow-sm" />
          <span className="text-gray-400 text-xs font-semibold">Users</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-amber-500 shadow-sm" />
          <span className="text-gray-400 text-xs font-semibold">Providers</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-3 h-3 rounded-full bg-pink-500 shadow-sm" />
          <span className="text-gray-400 text-xs font-semibold">Requests</span>
        </div>
      </div>
    </div>
  );
};
