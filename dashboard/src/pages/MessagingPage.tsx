import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { MessagingStatCard } from '../features/messaging/components/MessagingStatCard';
import { ConversationsTable } from '../features/messaging/components/ConversationsTable';
import { ReportedMessageCard } from '../features/messaging/components/ReportedMessageCard';
import { messagingService } from '../features/messaging/services/messagingService';
import type { MessagingData, Conversation } from '../features/messaging/types';
import { Icons } from '../components/common/Icons';

interface MessagingPageProps {
  onNavigate?: (label: string) => void;
}

export const MessagingPage: React.FC<MessagingPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<MessagingData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Search & Filters state
  const [searchQuery, setSearchQuery] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const [statusFilter, setStatusFilter] = useState<string>('all'); // all, active, reported, locked

  const loadMessagingData = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await messagingService.getMessagingData();
      setData(result);
    } catch (err) {
      console.error('Failed to load messaging metrics', err);
      setError(err instanceof Error ? err.message : 'Failed to retrieve messaging databases.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMessagingData();
  }, []);

  const handleRetry = () => {
    loadMessagingData();
  };

  // Interactive callbacks to demonstrate local state moderation
  const handleToggleLock = async (id: number | string) => {
    try {
      setLoading(true);
      await messagingService.toggleLock(id);
      loadMessagingData();
    } catch (err) {
      console.error(err);
      alert('Failed to toggle lock state.');
      setLoading(false);
    }
  };

  const handleResolveReport = async (id: number | string) => {
    try {
      setLoading(true);
      await messagingService.resolveReport(id);
      loadMessagingData();
      alert(`Dispute report resolved successfully!`);
    } catch (err) {
      console.error(err);
      alert('Failed to resolve dispute report.');
      setLoading(false);
    }
  };

  const handleTakeActionReport = async (id: number | string) => {
    if (!data) return;
    const reportItem = data.reportedMessages.find((rep) => rep.id === id);
    if (reportItem) {
      try {
        setLoading(true);
        // Find matching conversation and lock it
        const convMatch = data.conversations.find((conv) => 
          (conv.participant1 === reportItem.participant1 && conv.participant2 === reportItem.participant2) ||
          (conv.participant2 === reportItem.participant1 && conv.participant1 === reportItem.participant2)
        );
        if (convMatch && !convMatch.isLocked) {
          await messagingService.toggleLock(convMatch.id);
        }
        await messagingService.resolveReport(id);
        loadMessagingData();
        alert(`Moderation action taken on chat. The conversation room has been locked.`);
      } catch (err) {
        console.error(err);
        alert('Failed to take moderation action.');
        setLoading(false);
      }
    }
  };

  // Client-side filtering logic
  const getFilteredConversations = (): Conversation[] => {
    if (!data) return [];
    return data.conversations.filter((conv) => {
      const matchesSearch = 
        conv.participant1.toLowerCase().includes(searchQuery.toLowerCase()) ||
        conv.participant2.toLowerCase().includes(searchQuery.toLowerCase()) ||
        conv.lastMessage.toLowerCase().includes(searchQuery.toLowerCase());
      
      const matchesStatus = 
        statusFilter === 'all' ||
        (statusFilter === 'active' && conv.status === 'active' && !conv.isLocked) ||
        (statusFilter === 'reported' && conv.status === 'reported') ||
        (statusFilter === 'locked' && conv.isLocked);

      return matchesSearch && matchesStatus;
    });
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading messaging database...</p>
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
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Messaging Console</h3>
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

  const filteredConversations = getFilteredConversations();

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
          <div>
            <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
              Messaging Control Center
            </h1>
            <p className="text-gray-400 text-sm">
              Monitor, search, and moderate platform chat activities
            </p>
          </div>

          {/* Stats Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {data.stats.map((stat) => (
              <MessagingStatCard key={stat.id} stat={stat} />
            ))}
          </div>

          {/* All Conversations */}
          <section className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-xl">
            <div className="p-5 border-b border-gray-800">
              <h2 className="text-lg font-semibold text-white">All Conversations</h2>
              <p className="text-gray-400 text-sm">Client to provider communications log</p>
            </div>

            <div className="p-5 border-b border-gray-800 flex flex-col sm:flex-row gap-4">
              <div className="relative flex-1 max-w-md">
                <div className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                  <Icons.search />
                </div>
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  placeholder="Search conversations by participant or message snippet..."
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
                Filter conversations
              </button>
            </div>

            {/* Advanced Filters */}
            {showFilters && (
              <div className="bg-[#161616] border-b border-gray-800 p-4 flex flex-wrap gap-4 items-center">
                <div className="space-y-1.5">
                  <label className="text-gray-400 text-xs font-semibold uppercase tracking-wider block">Chat Status</label>
                  <select 
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                    className="bg-gray-855 border border-gray-700 text-sm text-white rounded-lg px-3 py-1.5 focus:outline-none focus:border-indigo-500"
                  >
                    <option value="all">All Chats</option>
                    <option value="active">Active & Open</option>
                    <option value="reported">Reported / Flagged</option>
                    <option value="locked">Moderated & Locked</option>
                  </select>
                </div>
                <button 
                  onClick={() => {
                    setStatusFilter('all');
                    setSearchQuery('');
                  }}
                  className="text-gray-500 hover:text-white text-xs font-semibold mt-6 transition-colors"
                >
                  Reset Filters
                </button>
              </div>
            )}

            <ConversationsTable 
              conversations={filteredConversations} 
              onToggleLock={handleToggleLock}
            />
          </section>

          {/* Reported Flags Section */}
          <section className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-xl">
            <div className="p-5 border-b border-gray-800 flex items-center gap-2">
              <div className="text-red-500">
                <Icons.alert className="w-6 h-6" />
              </div>
              <div>
                <h2 className="text-lg font-semibold text-white">Reported Messages</h2>
                <p className="text-gray-400 text-sm">Review logs flagged by platform participants</p>
              </div>
            </div>

            <div className="p-5 space-y-4">
              {data.reportedMessages.length === 0 ? (
                <div className="text-center py-8 text-gray-500 text-sm font-medium">
                  All flagged issues have been resolved. Excellent work!
                </div>
              ) : (
                data.reportedMessages.map((report) => (
                  <ReportedMessageCard 
                    key={report.id} 
                    report={report} 
                    onResolve={handleResolveReport}
                    onTakeAction={handleTakeActionReport}
                  />
                ))
              )}
            </div>
          </section>
        </main>
      </div>
    </div>
  );
};
