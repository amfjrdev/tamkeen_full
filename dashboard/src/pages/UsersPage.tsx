import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { UsersTable } from '../features/users/components/UsersTable';
import { userService } from '../features/users/services/userService';
import type { UsersResponse, User } from '../features/users/types';
import { Icons } from '../components/common/Icons';

interface UsersPageProps {
  onNavigate?: (label: string) => void;
}

export const UsersPage: React.FC<UsersPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<UsersResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  
  // Interactive UI states
  const [activeTab, setActiveTab] = useState<'clients' | 'providers'>('clients');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedStatus, setSelectedStatus] = useState<string>('all');
  const [showFilters, setShowFilters] = useState(false);

  useEffect(() => {
    const loadUsers = async () => {
      try {
        setLoading(true);
        setError(null);
        // Load user data (could pass role filter to backend service if needed)
        const result = await userService.getUsers();
        setData(result);
      } catch (err) {
        console.error('Failed to load users data', err);
        setError(err instanceof Error ? err.message : 'Failed to retrieve users directory.');
      } finally {
        setLoading(false);
      }
    };
    loadUsers();
  }, []);

  const handleRetry = () => {
    setLoading(true);
    setError(null);
    userService.getUsers()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to retrieve users directory.');
        setLoading(false);
      });
  };

  // Client-side filtering logic to make the dashboard feel alive and professional
  const getFilteredUsers = (): User[] => {
    if (!data) return [];
    
    return data.users.filter((user) => {
      // 1. Search Query Filter (Name or Email)
      const matchesSearch = 
        user.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        user.email.toLowerCase().includes(searchQuery.toLowerCase());
      
      // 2. Status Filter
      const matchesStatus = selectedStatus === 'all' || user.status === selectedStatus;

      // 3. Tab Filter (Clients vs Providers mock separation)
      // Since all mock users in the template are clients, we can separate them dynamically:
      // Even ID for clients, Odd ID for providers (just a clean way to mock tabs).
      const matchesTab = activeTab === 'clients' ? user.id % 2 !== 0 : user.id % 2 === 0;

      return matchesSearch && matchesStatus && matchesTab;
    });
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading users database...</p>
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
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Users</h3>
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

  const filteredUsers = getFilteredUsers();

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
                Users Management
              </h1>
              <p className="text-gray-400 text-sm">
                Manage and monitor clients and service providers accounts
              </p>
            </div>
            <button className="bg-indigo-600 hover:bg-indigo-700 active:scale-95 text-white font-semibold text-sm px-4.5 py-2.5 rounded-lg transition-all shadow-lg shadow-indigo-600/15 flex items-center gap-2">
              <span>+</span> Add New User
            </button>
          </div>

          {/* Tabs */}
          <div className="border-b border-gray-800">
            <div className="flex gap-8">
              <button
                onClick={() => setActiveTab('clients')}
                className={`pb-3 text-sm font-semibold border-b-2 transition-colors relative ${
                  activeTab === 'clients'
                    ? 'border-indigo-500 text-indigo-400'
                    : 'border-transparent text-gray-400 hover:text-white'
                }`}
              >
                Clients
                {activeTab === 'clients' && (
                  <span className="absolute bottom-[-2px] left-0 right-0 h-[2px] bg-indigo-500 blur-[2px]" />
                )}
              </button>
              <button
                onClick={() => setActiveTab('providers')}
                className={`pb-3 text-sm font-semibold border-b-2 transition-colors relative ${
                  activeTab === 'providers'
                    ? 'border-indigo-500 text-indigo-400'
                    : 'border-transparent text-gray-400 hover:text-white'
                }`}
              >
                Providers
                {activeTab === 'providers' && (
                  <span className="absolute bottom-[-2px] left-0 right-0 h-[2px] bg-indigo-500 blur-[2px]" />
                )}
              </button>
            </div>
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
                  placeholder={`Search ${activeTab === 'clients' ? 'clients' : 'providers'} by name or email...`}
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
                Filter
              </button>
            </div>

            {/* Expandable Advanced Filters Row */}
            {showFilters && (
              <div className="bg-[#111111] border border-gray-800 rounded-xl p-4 flex flex-wrap gap-4 items-center animate-fadeIn">
                <div className="space-y-1.5">
                  <label className="text-gray-400 text-xs font-semibold uppercase tracking-wider block">Status</label>
                  <select 
                    value={selectedStatus}
                    onChange={(e) => setSelectedStatus(e.target.value)}
                    className="bg-gray-800/40 border border-gray-700 text-sm text-white rounded-lg px-3 py-1.5 focus:outline-none focus:border-indigo-500"
                  >
                    <option value="all">All Statuses</option>
                    <option value="active">Active</option>
                    <option value="pending">Pending</option>
                    <option value="blocked">Blocked</option>
                  </select>
                </div>
                <button 
                  onClick={() => {
                    setSelectedStatus('all');
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
          <UsersTable users={filteredUsers} />
        </main>
      </div>
    </div>
  );
};
