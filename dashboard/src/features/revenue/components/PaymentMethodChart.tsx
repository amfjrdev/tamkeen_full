import React from 'react';
import type { PaymentMethod } from '../types';

interface PaymentMethodChartProps {
  data: PaymentMethod[];
}

export const PaymentMethodChart: React.FC<PaymentMethodChartProps> = ({ data }) => {
  const radius = 65;
  const circumference = 2 * Math.PI * radius;
  return (
    <div className="flex flex-col items-center justify-center py-4">
      <svg width="340" height="220" viewBox="0 0 340 220" className="mx-auto">
        <g transform="translate(170, 110)">
          {data.map((item, i) => {
            const strokeDasharray = `${(item.value / 100) * circumference} ${circumference}`;
            const strokeDashoffset = -data.slice(0, i).reduce((sum, prev) => sum + (prev.value / 100) * circumference, 0);
            return (
              <circle
                key={i}
                r={radius}
                fill="transparent"
                stroke={item.color}
                strokeWidth="20"
                strokeDasharray={strokeDasharray}
                strokeDashoffset={strokeDashoffset}
                strokeLinecap="round"
                transform="rotate(-90)"
                className="transition-all duration-500 ease-out hover:opacity-80 cursor-pointer"
                style={{
                  transformOrigin: '0 0',
                }}
              />
            );
          })}
          
          {/* Labels & Lines pointing outwards */}
          {data.map((item, i) => {
             const angle = (data.slice(0, i).reduce((acc, curr) => acc + curr.value, 0) + item.value / 2) * 3.6 - 90;
             const rad = angle * (Math.PI / 180);
             const labelRadius = radius + 25;
             const x = Math.cos(rad) * labelRadius;
             const y = Math.sin(rad) * labelRadius;
             
             // Dynamic text-anchor based on label x position
             const textAnchor = x > 0 ? 'start' : 'end';
             const textOffset = x > 0 ? 8 : -8;

             return (
               <g key={`label-${i}`} className="opacity-90">
                 {/* Connect line */}
                 <line 
                   x1={Math.cos(rad) * (radius + 10)} 
                   y1={Math.sin(rad) * (radius + 10)} 
                   x2={x} 
                   y2={y} 
                   stroke={item.color} 
                   strokeWidth="1" 
                   strokeDasharray="2 2"
                 />
                 {/* Text detail */}
                 <text 
                   x={x + textOffset} 
                   y={y + 4} 
                   textAnchor={textAnchor} 
                   fill={item.color} 
                   fontSize="10" 
                   className="font-bold tracking-tight font-mono"
                 >
                   {item.name}: {item.value}%
                 </text>
               </g>
             );
          })}
        </g>
      </svg>
    </div>
  );
};
