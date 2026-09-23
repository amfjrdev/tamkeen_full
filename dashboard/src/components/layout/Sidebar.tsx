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
          {(() => {
            const hasRequests = navItems.some(i => i.label.toLowerCase() === 'service requests');
            let items = [...navItems];
            if (!hasRequests) {
              const providersIndex = items.findIndex(i => i.label.toLowerCase() === 'providers');
              const reqItem = { label: 'Service Requests', icon: 'requests', active: false };
              if (providersIndex !== -1) {
                items.splice(providersIndex + 1, 0, reqItem);
              } else {
                items.push(reqItem);
              }
            }
            return items;
          })()
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

        <div className="absolute bottom-0 left-0 right-0 p-4 border-t border-gray-800 bg-[#0d0d0d] flex items-center justify-between">
          <div className="flex items-center gap-3 overflow-hidden">
            <div className="w-9 h-9 bg-indigo-600 rounded-full flex items-center justify-center text-white text-xs font-semibold flex-shrink-0">
              AD
            </div>
            <div className="overflow-hidden">
              <p className="text-white text-xs font-medium truncate">Admin User</p>
              <p className="text-gray-500 text-[10px] truncate">admin@platform.com</p>
            </div>
          </div>
          <button 
            onClick={() => onNavigate && onNavigate('Logout')}
            className="text-gray-400 hover:text-red-400 p-1.5 rounded-lg hover:bg-gray-800/50 transition-colors cursor-pointer"
            title="Se déconnecter"
          >
            <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
            </svg>
          </button>
        </div>
      </aside>
    </>
  );
};
