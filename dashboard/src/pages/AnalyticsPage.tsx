import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { analyticsService } from '../features/analytics/services/analyticsService';
import { StatCard } from '../features/analytics/components/StatCard';
import { GrowthChart } from '../features/analytics/components/GrowthChart';
import { CategoryChart } from '../features/analytics/components/CategoryChart';
import { ActivityChart } from '../features/analytics/components/ActivityChart';
import { TopProvidersTable } from '../features/analytics/components/TopProvidersTable';
import type { AnalyticsData } from '../features/analytics/types';

interface AnalyticsPageProps {
  onNavigate?: (label: string) => void;
}

export const AnalyticsPage: React.FC<AnalyticsPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<AnalyticsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await analyticsService.getAnalyticsData();
        setData(result);
      } catch (err) {
        console.error('Failed to load analytics:', err);
        setError(err instanceof Error ? err.message : 'Failed to fetch platform analytics reports.');
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  const handleRetry = () => {
    setLoading(true);
    setError(null);
    analyticsService
      .getAnalyticsData()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to retrieve analytics listings.');
        setLoading(false);
      });
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Analyzing platform databases...</p>
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
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Analytics</h3>
        <p className="text-gray-400 text-sm max-w-md mb-6">{error}</p>
        <button
          onClick={handleRetry}
          className="bg-indigo-600 hover:bg-indigo-700 active:scale-95 text-white font-semibold text-sm px-6 py-2.5 rounded-lg transition-all shadow-lg shadow-indigo-600/15 cursor-pointer"
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
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
              Platform Analytics
            </h1>
            <p className="text-gray-400 text-sm">
              Comprehensive performance insights, monthly growth trends, and transaction distributions
            </p>
          </div>

          {/* Stats Summaries */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {data.stats.map((stat, idx) => (
              <StatCard key={idx} stat={stat} />
            ))}
          </div>

          {/* Platform Growth Trend Chart */}
          <div className="bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl p-6 shadow-xl">
            <h2 className="text-lg font-bold text-white tracking-tight">Platform Growth Trends</h2>
            <p className="text-gray-400 text-sm mt-0.5 mb-6">Monthly active users, providers, and request metrics</p>
            <GrowthChart data={data.growthTrends} />
          </div>

          {/* Side-by-Side Charts Row */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl p-6 shadow-xl">
              <h2 className="text-lg font-bold text-white tracking-tight">Category Performance</h2>
              <p className="text-gray-400 text-sm mt-0.5 mb-6">Comparative totals of requests and revenue by category</p>
              <CategoryChart data={data.categoryPerformance} />
            </div>

            <div className="bg-[#111111]/85 backdrop-blur-md border border-gray-800 rounded-xl p-6 shadow-xl">
              <h2 className="text-lg font-bold text-white tracking-tight">Daily Activity Pattern</h2>
              <p className="text-gray-400 text-sm mt-0.5 mb-6">Hourly active request traffic distribution patterns</p>
              <ActivityChart data={data.dailyActivity} />
            </div>
          </div>

          {/* Top Rankings Provider Table */}
          <TopProvidersTable providers={data.topProviders} />
        </main>
      </div>
    </div>
  );
};
