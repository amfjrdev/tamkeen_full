import React from 'react';
import { Icons, DashboardIcon } from '../common/Icons';
import type { IconName } from '../common/Icons';
import type { NavItem } from '../../features/dashboard/types';

interface SidebarProps {
  navItems: NavItem[];
  isOpen: boolean;
  onClose: () => void;
  onNavigate?: (label: string) => void;
}

export const Sidebar: React.FC<SidebarProps> = ({ navItems, isOpen, onClose, onNavigate }) => {
  const getIcon = (iconName: string) => {
    const IconComponent = Icons[iconName as IconName];
    return IconComponent ? <IconComponent /> : null;
  };

  return (
    <>
      {isOpen && (
        <div 
          className="fixed inset-0 bg-black/50 z-40 lg:hidden transition-opacity duration-300" 
          onClick={onClose} 
        />
      )}
      <aside 
        className={`fixed top-0 left-0 h-full w-64 bg-[#0d0d0d] border-r border-gray-800 z-50 transform transition-transform duration-300 ease-in-out lg:translate-x-0 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
      >
        <div className="flex items-center gap-3 px-6 py-5 border-b border-gray-800">
          <div className="w-8 h-8 bg-indigo-600 rounded-lg flex items-center justify-center text-white">
            <DashboardIcon />
          </div>
          <span className="text-white font-semibold text-lg">Admin Panel</span>
        </div>

        <nav className="px-3 py-4 space-y-1 overflow-y-auto h-[calc(100vh-140px)]">
          {navItems
            .filter((item) => item.label.toLowerCase() !== 'settings')
            .map((item, idx) => (
              <button
              key={idx}
              onClick={() => onNavigate && onNavigate(item.label)}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                item.active
                  ? 'bg-indigo-600/20 text-indigo-400'
                  : 'text-gray-400 hover:text-white hover:bg-gray-800/50'
              }`}
            >
              {getIcon(item.icon)}
              {item.label}
            </button>
          ))}
        </nav>

        <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-gray-800 bg-[#0d0d0d]">
          <div className="flex items-center gap-3">
            <div className="w-10 h-10 bg-indigo-600 rounded-full flex items-center justify-center text-white text-sm font-semibold">
              AD
            </div>
            <div className="overflow-hidden">
              <p className="text-white text-sm font-medium truncate">Admin User</p>
              <p className="text-gray-500 text-xs truncate">admin@platform.com</p>
            </div>
          </div>
        </div>
      </aside>
    </>
  );
};
