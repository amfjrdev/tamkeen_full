import React from 'react';
import type { WeeklyActivity } from '../types';

interface WeeklyActivityChartProps {
  data: WeeklyActivity;
}

export const WeeklyActivityChart: React.FC<WeeklyActivityChartProps> = ({ data }) => {
  const maxVal = 8000; // Adjusted based on max value in data (~6400) to fill chart nicely
  const width = 600;
  const height = 250;
  const padding = { top: 20, right: 20, bottom: 35, left: 45 };
  const chartW = width - padding.left - padding.right;
  const chartH = height - padding.top - padding.bottom;

  // Function to create standard line path
  const createPath = (values: number[]) => {
    return values
      .map((val, i) => {
        const x = padding.left + (i / (values.length - 1)) * chartW;
        const y = padding.top + chartH - (val / maxVal) * chartH;
        return `${i === 0 ? 'M' : 'L'} ${x} ${y}`;
      })
      .join(' ');
  };

  // Function to create area path (for gradient fills)
  const createAreaPath = (values: number[]) => {
    const linePath = createPath(values);
    if (!linePath) return '';
    const firstX = padding.left;
    const lastX = padding.left + chartW;
    const bottomY = padding.top + chartH;
    return `${linePath} L ${lastX} ${bottomY} L ${firstX} ${bottomY} Z`;
  };

  const yTicks = [0, 2000, 4000, 6000, 8000];

  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl p-5 lg:p-6 hover:border-gray-700 transition-colors">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between mb-6">
        <div>
          <h3 className="text-white text-lg font-semibold mb-1">Weekly Activity</h3>
          <p className="text-gray-400 text-sm">User engagement over the past week</p>
        </div>
        <div className="flex flex-wrap gap-4 mt-3 sm:mt-0">
          <div className="flex items-center gap-2">
            <span className="w-3 h-1.5 rounded-full bg-indigo-500"></span>
            <span className="text-gray-400 text-xs font-medium">Users</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="w-3 h-1.5 rounded-full bg-purple-500"></span>
            <span className="text-gray-400 text-xs font-medium">Requests</span>
          </div>
          <div className="flex items-center gap-2">
            <span className="w-3 h-1.5 rounded-full bg-pink-500"></span>
            <span className="text-gray-400 text-xs font-medium">Messages</span>
          </div>
        </div>
      </div>

      <div className="w-full overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <svg viewBox={`0 0 ${width} ${height}`} className="w-full min-w-[500px]" preserveAspectRatio="xMidYMid meet">
          <defs>
            <linearGradient id="gradientUsers" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#6366f1" stopOpacity="0.25" />
              <stop offset="100%" stopColor="#6366f1" stopOpacity="0.00" />
            </linearGradient>
            <linearGradient id="gradientRequests" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#8b5cf6" stopOpacity="0.2" />
              <stop offset="100%" stopColor="#8b5cf6" stopOpacity="0.00" />
            </linearGradient>
            <linearGradient id="gradientMessages" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stopColor="#ec4899" stopOpacity="0.2" />
              <stop offset="100%" stopColor="#ec4899" stopOpacity="0.00" />
            </linearGradient>
          </defs>

          {/* Grid lines */}
          {yTicks.map((tick) => {
            const y = padding.top + chartH - (tick / maxVal) * chartH;
            return (
              <g key={tick} className="opacity-40">
                <line 
                  x1={padding.left} 
                  y1={y} 
                  x2={width - padding.right} 
                  y2={y} 
                  stroke="#374151" 
                  strokeWidth="0.75" 
                  strokeDasharray="4 4" 
                />
                <text 
                  x={padding.left - 10} 
                  y={y + 3} 
                  textAnchor="end" 
                  fill="#9ca3af" 
                  fontSize="10" 
                  className="font-mono"
                >
                  {tick.toLocaleString()}
                </text>
              </g>
            );
          })}

          {/* X Axis Labels */}
          {data.labels.map((label: string, i: number) => {
            const x = padding.left + (i / (data.labels.length - 1)) * chartW;
            return (
              <text 
                key={label} 
                x={x} 
                y={height - 8} 
                textAnchor="middle" 
                fill="#9ca3af" 
                fontSize="10" 
                className="font-medium"
              >
                {label}
              </text>
            );
          })}

          {/* Area under lines */}
          <path d={createAreaPath(data.users)} fill="url(#gradientUsers)" />
          <path d={createAreaPath(data.requests)} fill="url(#gradientRequests)" />
          <path d={createAreaPath(data.messages)} fill="url(#gradientMessages)" />

          {/* Line paths */}
          <path d={createPath(data.users)} fill="none" stroke="#6366f1" strokeWidth="2.5" strokeLinecap="round" />
          <path d={createPath(data.requests)} fill="none" stroke="#8b5cf6" strokeWidth="2.5" strokeLinecap="round" />
          <path d={createPath(data.messages)} fill="none" stroke="#ec4899" strokeWidth="2.5" strokeLinecap="round" />

          {/* Message data point markers (circles) */}
          {data.messages.map((val: number, i: number) => {
            const x = padding.left + (i / (data.messages.length - 1)) * chartW;
            const y = padding.top + chartH - (val / maxVal) * chartH;
            return (
              <g key={i} className="hover:scale-125 transition-transform duration-200">
                <circle 
                  cx={x} 
                  cy={y} 
                  r="4" 
                  fill="#ec4899" 
                  stroke="#111111" 
                  strokeWidth="2" 
                />
              </g>
            );
          })}
        </svg>
      </div>
    </div>
  );
};
