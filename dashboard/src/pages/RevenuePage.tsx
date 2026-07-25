import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { RevenueStatCard } from '../features/revenue/components/RevenueStatCard';
import { GrowthChart } from '../features/revenue/components/GrowthChart';
import { DailyRevenueChart } from '../features/revenue/components/DailyRevenueChart';
import { PaymentMethodChart } from '../features/revenue/components/PaymentMethodChart';
import { SalesTrendChart } from '../features/revenue/components/SalesTrendChart';
import { TopProvidersTable } from '../features/revenue/components/TopProvidersTable';
import { revenueService } from '../features/revenue/services/revenueService';
import type { RevenueData } from '../features/revenue/types';
import { Icons } from '../components/common/Icons';

interface RevenuePageProps {
  onNavigate?: (label: string) => void;
}

export const RevenuePage: React.FC<RevenuePageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<RevenueData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    const loadRevenueData = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await revenueService.getRevenueData();
        setData(result);
      } catch (err) {
        console.error('Failed to load revenue metrics', err);
        setError(err instanceof Error ? err.message : 'Failed to retrieve financial logs.');
      } finally {
        setLoading(false);
      }
    };
    loadRevenueData();
  }, []);

  const handleRetry = () => {
    setLoading(true);
    setError(null);
    revenueService.getRevenueData()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to retrieve financial logs.');
        setLoading(false);
      });
  };

  const handleExportReport = () => {
    alert('Generating financial CSV summary report... The download will begin automatically.');
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading financial accounts...</p>
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
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Financials</h3>
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

  const salesTrendColors = ['#3b82f6', '#8b5cf6', '#ec4899', '#f59e0b']; // starter, professional, premium, enterprise

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
          {/* Header */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
                Revenue & Earnings
              </h1>
              <p className="text-gray-400 text-sm">
                Comprehensive financial analytics, package sales, and net revenue tracking
              </p>
            </div>
            
            <button 
              onClick={handleExportReport}
              className="flex items-center justify-center gap-2 px-4.5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold active:scale-95 transition-all shadow-lg shadow-indigo-500/25 cursor-pointer"
            >
              <Icons.download className="w-4 h-4" />
              Export Report
            </button>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-4">
            {data.stats.map((stat) => (
              <RevenueStatCard key={stat.id} stat={stat} />
            ))}
          </div>

          {/* Growth Chart */}
          <div className="bg-[#111111] border border-gray-800 rounded-xl p-6 shadow-xl">
            <div className="flex flex-col sm:flex-row justify-between sm:items-center gap-4 mb-6">
              <div>
                <h2 className="text-lg font-bold text-white tracking-tight">Revenue Growth & Forecasting</h2>
                <p className="text-gray-400 text-sm">Monthly revenue margins with AI-powered forecasting indexes</p>
              </div>
              <div className="flex gap-4 text-xs font-semibold">
                <div className="flex items-center gap-2">
                  <div className="w-2.5 h-2.5 rounded-full bg-indigo-500"></div>
                  <span className="text-gray-400">Actual Revenue</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="w-2.5 h-2.5 rounded-full bg-purple-500"></div>
                  <span className="text-gray-400">Forecast Margin</span>
                </div>
              </div>
            </div>
            <GrowthChart data={data.growthData} />
          </div>

          {/* Daily Revenue & Payment Method */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <div className="bg-[#111111] border border-gray-800 rounded-xl p-6 shadow-xl">
              <h2 className="text-lg font-bold text-white tracking-tight mb-1">Daily Revenue</h2>
              <p className="text-gray-400 text-sm mb-6">Revenue connection performances this week</p>
              <DailyRevenueChart data={data.dailyRevenue} />
            </div>
            
            <div className="bg-[#111111] border border-gray-800 rounded-xl p-6 shadow-xl">
              <h2 className="text-lg font-bold text-white tracking-tight mb-1">Revenue by Payment Method</h2>
              <p className="text-gray-400 text-sm mb-2">Platform invoice checkout distributions</p>
              <PaymentMethodChart data={data.paymentMethods} />
            </div>
          </div>

          {/* Sales Trend */}
          <div className="bg-[#111111] border border-gray-800 rounded-xl p-6 shadow-xl">
            <div className="flex flex-col sm:flex-row justify-between sm:items-center gap-4 mb-6">
              <div>
                <h2 className="text-lg font-bold text-white tracking-tight mb-1">Connect Credits Sales Trend</h2>
                <p className="text-gray-400 text-sm">Sales performance distributions by package tier</p>
              </div>
              <div className="flex flex-wrap gap-4 text-xs font-semibold">
                {['starter', 'professional', 'premium', 'enterprise'].map((key, idx) => (
                  <div key={key} className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-sm" style={{ backgroundColor: salesTrendColors[idx] }}></div>
                    <span className="text-gray-400 capitalize">{key}</span>
                  </div>
                ))}
              </div>
            </div>
            <SalesTrendChart data={data.salesTrend} />
          </div>

          {/* Top Providers */}
          <TopProvidersTable providers={data.topProviders} />

          {/* Bottom Breakdown Grids */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Revenue Breakdown */}
            <div className="bg-[#151035]/30 border border-indigo-500/25 rounded-xl p-6 shadow-xl">
              <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 rounded-xl bg-indigo-600/10 border border-indigo-500/20 flex items-center justify-center text-indigo-400">
                  <Icons.dollar className="w-5 h-5" />
                </div>
                <h3 className="text-lg font-bold text-white tracking-tight">Revenue Breakdown</h3>
              </div>
              <div className="space-y-4">
                {Object.entries(data.breakdown).map(([key, val]) => (
                  <div key={key} className="flex justify-between items-center border-b border-gray-800/40 pb-2">
                    <span className="text-gray-400 text-sm capitalize">{key} Packages</span>
                    <span className="text-white font-bold font-mono text-sm">${val.toLocaleString()}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Earnings Overview */}
            <div className="bg-[#102518]/30 border border-green-500/25 rounded-xl p-6 shadow-xl">
              <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 rounded-xl bg-green-500/10 border border-green-500/20 flex items-center justify-center text-green-400">
                  <Icons.chartUp className="w-5 h-5" />
                </div>
                <h3 className="text-lg font-bold text-white tracking-tight">Earnings Overview</h3>
              </div>
              <div className="space-y-4">
                <div className="flex justify-between items-center border-b border-gray-800/40 pb-2">
                  <span className="text-gray-400 text-sm">Gross Revenue</span>
                  <span className="text-white font-bold font-mono text-sm">${data.earnings.gross.toLocaleString()}</span>
                </div>
                <div className="flex justify-between items-center border-b border-gray-800/40 pb-2">
                  <span className="text-gray-400 text-sm">Processing Fees</span>
                  <span className="text-rose-400 font-bold font-mono text-sm">-${Math.abs(data.earnings.fees).toLocaleString()}</span>
                </div>
                <div className="flex justify-between items-center border-b border-gray-800/40 pb-2">
                  <span className="text-gray-400 text-sm">Refunds</span>
                  <span className="text-rose-400 font-bold font-mono text-sm">-${Math.abs(data.earnings.refunds).toLocaleString()}</span>
                </div>
                <div className="pt-3 flex justify-between items-center">
                  <span className="text-white font-bold text-sm">Net Revenue</span>
                  <span className="text-emerald-400 font-extrabold text-lg font-mono">${data.earnings.net.toLocaleString()}</span>
                </div>
              </div>
            </div>

            {/* Profit Statistics */}
            <div className="bg-[#251035]/30 border border-purple-500/25 rounded-xl p-6 shadow-xl">
              <div className="flex items-center gap-3 mb-6">
                <div className="w-10 h-10 rounded-xl bg-purple-600/10 border border-purple-500/20 flex items-center justify-center text-purple-400">
                  <Icons.checkCircle className="w-5 h-5" />
                </div>
                <h3 className="text-lg font-bold text-white tracking-tight">Profit Statistics</h3>
              </div>
              <div className="space-y-4">
                <div className="flex justify-between items-center border-b border-gray-800/40 pb-2">
                  <span className="text-gray-400 text-sm">Operating Costs</span>
                  <span className="text-white font-bold font-mono text-sm">${data.profit.operating.toLocaleString()}</span>
                </div>
                <div className="flex justify-between items-center border-b border-gray-800/40 pb-2">
                  <span className="text-gray-400 text-sm">Marketing Spend</span>
                  <span className="text-white font-bold font-mono text-sm">${data.profit.marketing.toLocaleString()}</span>
                </div>
                <div className="flex justify-between items-center border-b border-gray-800/40 pb-2">
                  <span className="text-gray-400 text-sm">Other Expenses</span>
                  <span className="text-white font-bold font-mono text-sm">${data.profit.other.toLocaleString()}</span>
                </div>
                <div className="pt-3 flex justify-between items-center">
                  <span className="text-white font-bold text-sm">Net Profit</span>
                  <span className="text-emerald-400 font-extrabold text-lg font-mono">${data.profit.net.toLocaleString()}</span>
                </div>
              </div>
            </div>
          </div>
        </main>
      </div>
    </div>
  );
};
