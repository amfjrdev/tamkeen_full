import React from 'react';
import type { Category } from '../types';
import { Icons } from '../../../components/common/Icons';

interface CategoryCardProps {
  category: Category;
  onDelete?: (id: string | number, name: string) => void;
  isDeleting?: boolean;
}

export const CategoryCard: React.FC<CategoryCardProps> = ({ 
  category, 
  onDelete, 
  isDeleting = false 
}) => {
  const getIcon = (iconName: string) => {
    const IconComponent = Icons[iconName as keyof typeof Icons];
    return IconComponent ? <IconComponent /> : null;
  };

  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl p-6 hover:border-indigo-500/40 hover:bg-[#141414] transition-all duration-300 flex flex-col shadow-lg group relative">
      <div className="flex items-center justify-between mb-4.5">
        <div className={`w-12 h-12 rounded-xl ${category.color} flex items-center justify-center shadow-md shadow-black/30 transition-transform duration-300 group-hover:scale-105`}>
          {getIcon(category.icon)}
        </div>
        
        {onDelete && (
          <button
            onClick={() => onDelete(category.id, category.name)}
            disabled={isDeleting}
            title={`Delete ${category.name}`}
            aria-label={`Delete category ${category.name}`}
            className="text-gray-500 hover:text-rose-500 hover:bg-rose-500/10 p-2 rounded-lg border border-transparent hover:border-rose-500/20 transition-all duration-200 active:scale-95 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
          >
            <Icons.trash className="w-4 h-4" />
          </button>
        )}
      </div>

      <h3 className="text-white text-lg font-bold mb-1 tracking-tight group-hover:text-indigo-400 transition-colors">
        {category.name}
      </h3>
      {category.description && (
        <p className="text-gray-400 text-xs line-clamp-2 mb-4">
          {category.description}
        </p>
      )}
      {!category.description && <div className="mb-4" />}
      
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
