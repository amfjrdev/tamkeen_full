import React from 'react';

export const BestPractices: React.FC = () => {
  return (
    <div className="bg-[#151035]/80 backdrop-blur-md border border-indigo-500/20 rounded-xl p-6 shadow-xl">
      <h2 className="text-lg font-bold text-white mb-6 tracking-tight">
        Package Pricing Best Practices
      </h2>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="flex gap-4">
          <div className="w-7 h-7 rounded-full bg-indigo-600 flex items-center justify-center text-white text-xs font-bold flex-shrink-0 shadow-sm mt-0.5">
            1
          </div>
          <div>
            <h3 className="text-white font-semibold text-sm mb-1">Value Tiers</h3>
            <p className="text-gray-400 text-xs leading-relaxed">
              Create clear value differences and discounts between packages to encourage upgrades.
            </p>
          </div>
        </div>
        
        <div className="flex gap-4">
          <div className="w-7 h-7 rounded-full bg-indigo-600 flex items-center justify-center text-white text-xs font-bold flex-shrink-0 shadow-sm mt-0.5">
            2
          </div>
          <div>
            <h3 className="text-white font-semibold text-sm mb-1">Highlight Popular</h3>
            <p className="text-gray-400 text-xs leading-relaxed">
              Mark your best-selling package clearly to guide user decision-making towards high margin plans.
            </p>
          </div>
        </div>

        <div className="flex gap-4">
          <div className="w-7 h-7 rounded-full bg-indigo-600 flex items-center justify-center text-white text-xs font-bold flex-shrink-0 shadow-sm mt-0.5">
            3
          </div>
          <div>
            <h3 className="text-white font-semibold text-sm mb-1">Bundle Benefits</h3>
            <p className="text-gray-400 text-xs leading-relaxed">
              Higher tiers should include exclusive benefits (boosts, badges) beyond just linear credit counts.
            </p>
          </div>
        </div>

        <div className="flex gap-4">
          <div className="w-7 h-7 rounded-full bg-indigo-600 flex items-center justify-center text-white text-xs font-bold flex-shrink-0 shadow-sm mt-0.5">
            4
          </div>
          <div>
            <h3 className="text-white font-semibold text-sm mb-1">Monitor Performance</h3>
            <p className="text-gray-400 text-xs leading-relaxed">
              Regularly review package conversion rates and adjust credit bounds or prices based on real sales data.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};
