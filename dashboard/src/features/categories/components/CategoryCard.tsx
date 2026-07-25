import React from 'react';
import type { Category } from '../types';
import { Icons } from '../../../components/common/Icons';

interface CategoryCardProps {
  category: Category;
}

export const CategoryCard: React.FC<CategoryCardProps> = ({ category }) => {
  const getIcon = (iconName: string) => {
    // Dynamic mapping
    const IconComponent = Icons[iconName as keyof typeof Icons];
    return IconComponent ? <IconComponent /> : null;
  };

  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl p-6 hover:border-indigo-500/40 hover:bg-[#141414] transition-all duration-300 flex flex-col shadow-lg group">
      <div className={`w-12 h-12 rounded-xl ${category.color} flex items-center justify-center mb-4.5 shadow-md shadow-black/30 transition-transform duration-300 group-hover:scale-105`}>
        {getIcon(category.icon)}
      </div>
      <h3 className="text-white text-lg font-bold mb-5 tracking-tight group-hover:text-indigo-400 transition-colors">
        {category.name}
      </h3>
      
      <div className="grid grid-cols-2 gap-4 mb-6">
        <div className="bg-black/20 p-2.5 rounded-lg border border-gray-900/60">
          <p className="text-white text-2xl font-extrabold font-mono tracking-tight">{category.providers}</p>
          <p className="text-gray-500 text-[10px] font-bold uppercase tracking-wider mt-0.5">Providers</p>
        </div>
        <div className="bg-black/20 p-2.5 rounded-lg border border-gray-900/60">
          <p className="text-white text-2xl font-extrabold font-mono tracking-tight">{category.requests}</p>
          <p className="text-gray-500 text-[10px] font-bold uppercase tracking-wider mt-0.5">Requests</p>
        </div>
      </div>

      <div className="mt-auto pt-4 border-t border-gray-850 flex items-center justify-between">
        <span className="text-gray-400 text-sm font-medium">Avg. Rating</span>
        <div className="flex items-center gap-1.5 text-white text-sm font-bold font-mono">
          {category.rating} 
          <Icons.star className="w-4 h-4 text-yellow-400" />
        </div>
      </div>
    </div>
  );
};
