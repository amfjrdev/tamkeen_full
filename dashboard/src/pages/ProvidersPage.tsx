import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { ProviderStatCard } from '../features/providers/components/ProviderStatCard';
import { ProvidersTable } from '../features/providers/components/ProvidersTable';
import { providerService } from '../features/providers/services/providerService';
import type { ProvidersResponse, Provider } from '../features/providers/types';
import { Icons } from '../components/common/Icons';

interface ProvidersPageProps {
  onNavigate?: (label: string) => void;
}

export const ProvidersPage: React.FC<ProvidersPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<ProvidersResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Interactive filter states
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [categoryFilter, setCategoryFilter] = useState<string>('all');
  const [showFilters, setShowFilters] = useState(false);

  useEffect(() => {
    const loadProviders = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await providerService.getProviders();
        setData(result);
      } catch (err) {
        console.error('Failed to load providers data', err);
        setError(err instanceof Error ? err.message : 'Failed to retrieve providers directory.');
      } finally {
        setLoading(false);
      }
    };
    loadProviders();
  }, []);

  const handleRetry = () => {
    setLoading(true);
    setError(null);
    providerService.getProviders()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to retrieve providers directory.');
        setLoading(false);
      });
  };

  // Client-side filtering logic
  const getFilteredProviders = (): Provider[] => {
    if (!data) return [];
    return data.providers.filter((provider) => {
      const matchesSearch = 
        provider.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        provider.email.toLowerCase().includes(searchQuery.toLowerCase()) ||
        provider.category.toLowerCase().includes(searchQuery.toLowerCase());
      
      const matchesStatus = statusFilter === 'all' || provider.status === statusFilter;
      const matchesCategory = categoryFilter === 'all' || provider.category === categoryFilter;

      return matchesSearch && matchesStatus && matchesCategory;
    });
  };

  // Get distinct categories for filters
  const getCategoriesList = (): string[] => {
    if (!data) return [];
    const categories = data.providers.map((p) => p.category);
    return Array.from(new Set(categories));
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading providers profile dashboard...</p>
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
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Providers</h3>
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

  const filteredProviders = getFilteredProviders();
  const categories = getCategoriesList();

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
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
                Providers Control Panel
              </h1>
              <p className="text-gray-400 text-sm">
                Manage provider approvals, performance, billing spend, and connection permissions
              </p>
            </div>
            <button className="bg-indigo-600 hover:bg-indigo-700 active:scale-95 text-white font-semibold text-sm px-4.5 py-2.5 rounded-lg transition-all shadow-lg shadow-indigo-600/15 flex items-center gap-2">
              <span>+</span> Register New Provider
            </button>
          </div>

          {/* Stats Row */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            {data.stats.map((stat, idx) => (
              <ProviderStatCard key={idx} stat={stat} />
            ))}
          </div>

          {/* Toolbar */}
          <div className="space-y-4">
            <div className="flex flex-col sm:flex-row gap-4">
              <div className="relative flex-1 max-w-md">
                <div className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                  <Icons.search />
                </div>
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search providers by name, email, category..."
                  className="w-full bg-gray-800/30 border border-gray-700 rounded-lg pl-10 pr-4 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
                />
              </div>
              <button 
                onClick={() => setShowFilters(!showFilters)}
                className={`flex items-center justify-center gap-2 px-4 py-2 rounded-lg text-sm font-medium border transition-colors ${
                  showFilters 
                    ? 'bg-indigo-600/10 border-indigo-500 text-indigo-400' 
                    : 'bg-gray-800/40 hover:bg-gray-800 border-gray-700 text-gray-300'
                }`}
              >
                <Icons.filter />
                Filter by Status
              </button>
            </div>

            {/* Expandable Advanced Filters Row */}
            {showFilters && (
              <div className="bg-[#111111] border border-gray-800 rounded-xl p-4 flex flex-wrap gap-6 items-center animate-fadeIn">
                <div className="space-y-1.5">
                  <label className="text-gray-400 text-xs font-semibold uppercase tracking-wider block">Approval Status</label>
                  <select 
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="bg-gray-800/40 border border-gray-700 text-sm text-white rounded-lg px-3 py-1.5 focus:outline-none focus:border-indigo-500"
                  >
                    <option value="all">All Statuses</option>
                    <option value="approved">Approved</option>
                    <option value="pending">Pending</option>
                    <option value="rejected">Rejected</option>
                  </select>
                </div>
                
                <div className="space-y-1.5">
                  <label className="text-gray-400 text-xs font-semibold uppercase tracking-wider block">Service Category</label>
                  <select 
                    value={categoryFilter}
                    onChange={(e) => setCategoryFilter(e.target.value)}
                    className="bg-gray-800/40 border border-gray-700 text-sm text-white rounded-lg px-3 py-1.5 focus:outline-none focus:border-indigo-500"
                  >
                    <option value="all">All Categories</option>
                    {categories.map((cat) => (
                      <option key={cat} value={cat}>{cat}</option>
                    ))}
                  </select>
                </div>

                <button 
                  onClick={() => {
                    setStatusFilter('all');
                    setCategoryFilter('all');
                    setSearchQuery('');
                  }}
                  className="text-gray-500 hover:text-white text-xs font-semibold mt-6 transition-colors"
                >
                  Reset Filters
                </button>
              </div>
            )}
          </div>

          {/* Table */}
          <ProvidersTable providers={filteredProviders} />
        </main>
      </div>
    </div>
  );
};
