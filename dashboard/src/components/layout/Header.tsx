import React, { useState, useEffect } from 'react';
import { MenuIcon, CloseIcon, SunIcon, MoonIcon } from '../common/Icons';

interface HeaderProps {
  onMenuToggle: () => void;
  sidebarOpen: boolean;
}

export const Header: React.FC<HeaderProps> = ({ onMenuToggle, sidebarOpen }) => {
  const [isLight, setIsLight] = useState(() => {
    const saved = localStorage.getItem('theme');
    if (saved) return saved === 'light';
    return document.documentElement.classList.contains('light');
  });

  useEffect(() => {
    if (isLight) {
      document.documentElement.classList.add('light');
    } else {
      document.documentElement.classList.remove('light');
    }
  }, [isLight]);

  const toggleTheme = () => {
    const nextLight = !isLight;
    setIsLight(nextLight);
    localStorage.setItem('theme', nextLight ? 'light' : 'dark');
  };

  return (
    <header className="sticky top-0 z-30 bg-[#0a0a0a]/80 backdrop-blur-xl border-b border-gray-800 px-4 lg:px-6 py-3">
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3 flex-1">
          <button 
            className="lg:hidden text-gray-400 hover:text-white p-1 rounded-lg hover:bg-gray-800/50" 
            onClick={onMenuToggle}
            aria-label="Toggle Menu"
          >
            {sidebarOpen ? <CloseIcon /> : <MenuIcon />}
          </button>
        </div>
        <div className="flex items-center gap-3">
          <button 
            onClick={toggleTheme}
            className="text-gray-400 hover:text-white p-2 rounded-lg hover:bg-gray-800/50 transition-colors cursor-pointer"
            aria-label="Toggle Light/Dark Theme"
          >
            {isLight ? <MoonIcon /> : <SunIcon />}
          </button>
        </div>
      </div>
    </header>
  );
};
