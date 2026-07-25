import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { StatCard } from '../features/dashboard/components/StatCard';
import { WeeklyActivityChart } from '../features/dashboard/components/WeeklyActivityChart';
import { ServiceCategoriesChart } from '../features/dashboard/components/ServiceCategoriesChart';
import { ConversionRateChart } from '../features/dashboard/components/ConversionRateChart';
import { RecentActivity } from '../features/dashboard/components/RecentActivity';
import { dashboardService } from '../features/dashboard/services/dashboardService';
import type { DashboardData } from '../features/dashboard/types';

interface DashboardPageProps {
  onNavigate?: (label: string) => void;
}

export const DashboardPage: React.FC<DashboardPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    const loadData = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await dashboardService.getDashboardData();
        setData(result);
      } catch (err) {
        console.error('Failed to load dashboard data', err);
        setError(err instanceof Error ? err.message : 'Failed to load dashboard details. Please try again.');
      } finally {
        setLoading(false);
      }
    };
    loadData();
  }, []);

  const handleRetry = () => {
    setError(null);
    setLoading(true);
    // Reload logic
    dashboardService.getDashboardData()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to load dashboard details. Please try again.');
        setLoading(false);
      });
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        {/* Modern glowing spinner */}
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading dashboard overview...</p>
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
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Dashboard</h3>
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

        <main className="p-4 lg:p-6 space-y-6">
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
              Dashboard Overview
            </h1>
            <p className="text-gray-400 text-sm">
              Monitor your platform's performance and activity
            </p>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
            {data.stats.map((stat, idx) => (
              <StatCard key={idx} stat={stat} />
            ))}
          </div>

          {/* Charts Grid 1 */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <WeeklyActivityChart data={data.weeklyActivity} />
            <ServiceCategoriesChart data={data.serviceCategories} />
          </div>

          {/* Charts Grid 2 */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <ConversionRateChart data={data.conversionRate} />
            <RecentActivity data={data.recentActivity} />
          </div>
        </main>
      </div>
    </div>
  );
};
