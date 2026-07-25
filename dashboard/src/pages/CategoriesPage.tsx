import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { CategoryCard } from '../features/categories/components/CategoryCard';
import { CategoryTable } from '../features/categories/components/CategoryTable';
import { categoryService } from '../features/categories/services/categoryService';
import type { CategoriesData } from '../features/categories/types';
import { Icons } from '../components/common/Icons';

interface CategoriesPageProps {
  onNavigate?: (label: string) => void;
}

export const CategoriesPage: React.FC<CategoriesPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<CategoriesData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    const loadCategoriesData = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await categoryService.getCategoriesData();
        setData(result);
      } catch (err) {
        console.error('Failed to load categories', err);
        setError(err instanceof Error ? err.message : 'Failed to retrieve service categories listings.');
      } finally {
        setLoading(false);
      }
    };
    loadCategoriesData();
  }, []);

  const handleRetry = () => {
    setLoading(true);
    setError(null);
    categoryService.getCategoriesData()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to retrieve service categories listings.');
        setLoading(false);
      });
  };

  // Add Category local state handler to make the app interactive and production-grade
  const handleAddCategory = async () => {
    if (!data) return;
    
    const promptName = prompt('Enter the name of the new service category:');
    if (!promptName || promptName.trim() === '') return;
    const promptDesc = prompt('Enter description for the new category (optional):') || '';

    try {
      setLoading(true);
      await categoryService.createCategory({ name: promptName.trim(), description: promptDesc.trim() });
      const result = await categoryService.getCategoriesData();
      setData(result);
    } catch (err) {
      console.error(err);
      alert(err instanceof Error ? err.message : 'Failed to create category');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading service categories database...</p>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center p-6 text-center">
        <div className="w-16 h-16 rounded-2xl bg-red-500/10 border border-red-500/20 flex items-center justify-center text-red-500 mb-4 shadow-lg shadow-red-500/5">
          <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Categories</h3>
        <p className="text-gray-400 text-sm max-w-md mb-6">{error}</p>
        <button
          onClick={handleRetry}
          className="bg-indigo-600 hover:bg-indigo-700 active:scale-95 text-white font-semibold text-sm px-6 py-2.5 rounded-lg transition-all shadow-lg shadow-indigo-600/15"
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#0a0a0a] text-white">
      <Sidebar 
        navItems={data.navItems} 
        isOpen={sidebarOpen} 
        onClose={() => setSidebarOpen(false)} 
        onNavigate={onNavigate}
      />

      <div className="lg:ml-64 transition-all duration-300">
        <Header 
          onMenuToggle={() => setSidebarOpen(!sidebarOpen)} 
          sidebarOpen={sidebarOpen}
        />

        <main className="p-4 lg:p-6 space-y-8">
          {/* Page Header */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
                Categories Management
              </h1>
              <p className="text-gray-400 text-sm">
                Organize, edit, and audit platform service categories
              </p>
            </div>
            
            <button 
              onClick={handleAddCategory}
              className="flex items-center justify-center gap-2 px-4.5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold active:scale-95 transition-all shadow-lg shadow-indigo-500/25 cursor-pointer"
            >
              <Icons.plus className="w-4 h-4" />
              Add Category
            </button>
          </div>

          {/* Summary Stats Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {data.summaryStats.map((stat, idx) => (
              <div key={idx} className="bg-[#111111] border border-gray-800 rounded-xl p-5 hover:bg-[#151515] transition-colors duration-200">
                <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider mb-1.5">{stat.label}</p>
                <p className="text-white text-3xl font-extrabold tracking-tight font-mono">{stat.value}</p>
              </div>
            ))}
          </div>

          {/* Category Cards Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {data.categories.map((cat) => (
              <CategoryCard key={cat.id} category={cat} />
            ))}
          </div>

          {/* Statistics Table */}
          <CategoryTable categories={data.categories} />
        </main>
      </div>
    </div>
  );
};
