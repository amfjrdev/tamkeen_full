import React from 'react';
import type { TransactionActivity } from '../types';
import { Icons } from '../../../components/common/Icons';

interface ActivityFeedProps {
  activities: TransactionActivity[];
}

export const ActivityFeed: React.FC<ActivityFeedProps> = ({ activities }) => {
  return (
    <div className="bg-[#111111]/80 backdrop-blur-md border border-gray-800 rounded-xl p-6 shadow-xl">
      <h2 className="text-lg font-bold text-white mb-6 tracking-tight flex items-center gap-2">
        <Icons.activity className="w-5 h-5 text-indigo-400" />
        Transaction Activity
      </h2>
      <div className="space-y-4">
        {activities.map((item) => {
          let iconBg = 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20';
          let IconComponent = Icons.checkCircle;

          if (item.status === 'pending') {
            iconBg = 'bg-amber-500/10 text-amber-400 border border-amber-500/20';
            IconComponent = Icons.clock;
          } else if (item.status === 'failed') {
            iconBg = 'bg-rose-500/10 text-rose-400 border border-rose-500/20';
            IconComponent = Icons.xCircle;
          }

          return (
            <div
              key={item.id}
              className="bg-[#151515] border border-gray-800/80 rounded-xl p-4 flex items-center justify-between hover:border-gray-700/80 transition-all duration-200"
            >
              <div className="flex items-center gap-4">
                <div className={`w-10 h-10 rounded-full flex items-center justify-center ${iconBg}`}>
                  <IconComponent className="w-5 h-5" />
                </div>
                <div>
                  <p className="text-white text-sm font-semibold">
                    {item.user} <span className="text-gray-400 font-normal">purchased</span> {item.package}
                  </p>
                  <p className="text-gray-500 text-xs mt-0.5">
                    <span className="text-indigo-400 font-medium font-mono">{item.credits}</span> credits •{' '}
                    <span className="text-emerald-400 font-medium font-mono">${item.amount}</span>
                  </p>
                </div>
              </div>
              <span className="text-gray-500 text-xs font-mono">{item.time}</span>
            </div>
          );
        })}
      </div>
    </div>
  );
};
