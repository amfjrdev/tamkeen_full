import React, { useState, useEffect, useCallback } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { ServiceRequestStatCard } from '../features/service-requests/components/ServiceRequestStatCard';
import { ServiceRequestsTable } from '../features/service-requests/components/ServiceRequestsTable';
import { ServiceRequestDetailModal } from '../features/service-requests/components/ServiceRequestDetailModal';
import { serviceRequestService } from '../features/service-requests/services/serviceRequestService';
import type { ServiceRequestSummary, ServiceRequestsPagedResponse } from '../features/service-requests/types';

interface ServiceRequestsPageProps {
  onNavigate?: (label: string) => void;
}

export const ServiceRequestsPage: React.FC<ServiceRequestsPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<ServiceRequestsPagedResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Filters
  const [selectedStatus, setSelectedStatus] = useState<string>('all');
  const [selectedWilaya, setSelectedWilaya] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [page, setPage] = useState(1);

  // Modal
  const [selectedRequest, setSelectedRequest] = useState<ServiceRequestSummary | null>(null);
  const [feedback, setFeedback] = useState<{ message: string; type: 'success' | 'error' } | null>(null);
  const [connectsCost, setConnectsCost] = useState<number>(10);
  const [inputCost, setInputCost] = useState<string>('10');
  const [isSavingCost, setIsSavingCost] = useState<boolean>(false);

  const loadRequests = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const res = await serviceRequestService.getServiceRequests({
        status: selectedStatus !== 'all' ? selectedStatus : undefined,
        wilaya: selectedWilaya !== 'all' ? selectedWilaya : undefined,
        search: searchQuery || undefined,
        page,
        pageSize: 20,
      });
      setData(res);
    } catch (err) {
      console.error('Failed to load service requests', err);
      setError(err instanceof Error ? err.message : 'Failed to retrieve service requests');
    } finally {
      setLoading(false);
    }
  }, [selectedStatus, selectedWilaya, searchQuery, page]);

  useEffect(() => {
    loadRequests();
    serviceRequestService.getConnectsCost().then((c) => {
      setConnectsCost(c);
      setInputCost(c.toString());
    });
  }, [loadRequests]);

  const showToast = (message: string, type: 'success' | 'error' = 'success') => {
    setFeedback({ message, type });
    setTimeout(() => setFeedback(null), 4000);
  };

  const handleSaveCost = async () => {
    const val = parseInt(inputCost, 10);
    if (isNaN(val) || val < 0) {
      showToast('Please enter a valid number of Connects (>= 0)', 'error');
      return;
    }
    try {
      setIsSavingCost(true);
      const updated = await serviceRequestService.updateConnectsCost(val);
      setConnectsCost(updated);
      setInputCost(updated.toString());
      showToast(`Connects cost updated to ${updated} Connects.`);
    } catch (err) {
      showToast('Failed to update Connects cost', 'error');
    } finally {
      setIsSavingCost(false);
    }
  };

  const handleApprove = async (id: string) => {
    try {
      await serviceRequestService.approveRequest(id);
      showToast('Service request approved & published successfully.');
      loadRequests();
    } catch (err: unknown) {
      showToast(err instanceof Error ? err.message : 'Failed to approve request', 'error');
    }
  };

  const handleReject = (req: ServiceRequestSummary) => {
    setSelectedRequest(req);
  };

  // Compute stat counts from current dataset
  const totalCount = data?.totalCount || 0;
  const pendingCount = data?.items.filter(i => i.status === 'PendingAdminReview').length || 0;
  const approvedCount = data?.items.filter(i => i.status === 'Approved').length || 0;
  const completedCount = data?.items.filter(i => i.status === 'Completed').length || 0;

  const statusTabs = [
    { label: 'All Requests', value: 'all' },
    { label: 'Pending Review', value: 'PendingAdminReview' },
    { label: 'Approved', value: 'Approved' },
    { label: 'In Progress', value: 'ProviderSelected' },
    { label: 'Completed', value: 'Completed' },
    { label: 'Rejected', value: 'Rejected' },
  ];

  const wilayas = [
    'all', 'Adrar', 'Chlef', 'Laghouat', 'Oum El Bouaghi', 'Batna', 'Béjaïa', 'Biskra',
    'Béchar', 'Blida', 'Bouira', 'Tamanrasset', 'Tébessa', 'Tlemcen', 'Tiaret',
    'Tizi Ouzou', 'Algiers', 'Djelfa', 'Jijel', 'Sétif', 'Saïda', 'Skikda',
    'Sidi Bel Abbès', 'Annaba', 'Guelma', 'Constantine', 'Médéa', 'Mostaganem',
    'M\'Sila', 'Mascara', 'Ouargla', 'Oran'
  ];

  const navItems = [
    { label: 'Dashboard', icon: 'dashboard', active: false },
    { label: 'Clients', icon: 'users', active: false },
    { label: 'Providers', icon: 'providers', active: false },
    { label: 'Service Requests', icon: 'serviceRequests', active: true },
    { label: 'Messaging', icon: 'messaging', active: false },
    { label: 'Categories', icon: 'categories', active: false },
    { label: 'Revenue', icon: 'revenue', active: false },
    { label: 'Transactions', icon: 'transactions', active: false },
    { label: 'Packages', icon: 'packages', active: false },
    { label: 'Analytics', icon: 'analytics', active: false },
  ];

  return (
    <div className="min-h-screen bg-[#0a0a0a] text-white flex">
      <Sidebar
        navItems={navItems}
        isOpen={sidebarOpen}
        onClose={() => setSidebarOpen(false)}
        onNavigate={onNavigate}
      />

      <div className="flex-1 lg:ml-64 flex flex-col min-w-0">
        <Header onMenuToggle={() => setSidebarOpen(!sidebarOpen)} sidebarOpen={sidebarOpen} />

        <main className="p-6 space-y-6">
          {/* Toast feedback */}
          {feedback && (
            <div
              className={`p-4 rounded-xl border flex items-center justify-between text-sm font-medium transition-all ${
                feedback.type === 'success'
                  ? 'bg-emerald-500/10 border-emerald-500/30 text-emerald-400'
                  : 'bg-red-500/10 border-red-500/30 text-red-400'
              }`}
            >
              <span>{feedback.message}</span>
              <button onClick={() => setFeedback(null)} className="text-gray-400 hover:text-white">✕</button>
            </div>
          )}

          {/* Page Title & Breadcrumb */}
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold text-white tracking-tight">Service Requests Moderation</h1>
              <p className="text-gray-400 text-xs mt-1">Review, moderate, and manage client-submitted marketplace requests.</p>
            </div>

            {/* Admin Connects Control */}
            <div className="flex items-center gap-3 bg-[#141414] border border-gray-800 rounded-xl px-4 py-2.5 shadow-sm">
              <div className="flex items-center gap-2">
                <span className="w-2 h-2 rounded-full bg-amber-400 animate-pulse" />
                <span className="text-xs font-semibold text-gray-300">Connects per Proposal:</span>
              </div>
              <div className="flex items-center gap-2">
                <input
                  type="number"
                  min="0"
                  max="1000"
                  value={inputCost}
                  onChange={(e) => setInputCost(e.target.value)}
                  className="w-16 px-2.5 py-1 bg-black/50 border border-gray-700 rounded-lg text-sm text-center font-bold text-amber-400 focus:outline-none focus:border-indigo-500"
                />
                <button
                  onClick={handleSaveCost}
                  disabled={isSavingCost || inputCost === connectsCost.toString()}
                  className={
                    inputCost !== connectsCost.toString()
                      ? "px-3 py-1 text-xs font-semibold rounded-lg transition-all bg-indigo-600 hover:bg-indigo-500 text-white cursor-pointer shadow-md"
                      : "px-3 py-1 text-xs font-semibold rounded-lg transition-all bg-gray-800 text-gray-500 cursor-not-allowed"
                  }
                >
                  {isSavingCost ? 'Saving...' : 'Save'}
                </button>
              </div>
            </div>
          </div>

          {/* Stat Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <ServiceRequestStatCard
              label="Total Requests"
              value={totalCount}
              color="blue"
              icon={
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2" />
                </svg>
              }
            />
            <ServiceRequestStatCard
              label="Pending Review"
              value={pendingCount}
              color="orange"
              icon={
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              }
            />
            <ServiceRequestStatCard
              label="Active / Approved"
              value={approvedCount}
              color="green"
              icon={
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              }
            />
            <ServiceRequestStatCard
              label="Completed"
              value={completedCount}
              color="purple"
              icon={
                <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                </svg>
              }
            />
          </div>

          {/* Controls: Tabs, Search, Wilaya Filter */}
          <div className="bg-[#111111] border border-gray-800 rounded-xl p-4 space-y-4">
            {/* Status tabs */}
            <div className="flex flex-wrap gap-2 border-b border-gray-800 pb-3">
              {statusTabs.map((tab) => (
                <button
                  key={tab.value}
                  onClick={() => {
                    setSelectedStatus(tab.value);
                    setPage(1);
                  }}
                  className={`px-3.5 py-1.5 rounded-lg text-xs font-semibold transition-colors cursor-pointer ${
                    selectedStatus === tab.value
                      ? 'bg-indigo-600 text-white shadow-lg shadow-indigo-600/20'
                      : 'text-gray-400 hover:text-white hover:bg-gray-800/60'
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </div>

            {/* Filter inputs */}
            <div className="flex flex-col sm:flex-row gap-3">
              <div className="relative flex-1">
                <input
                  type="text"
                  value={searchQuery}
                  onChange={(e) => {
                    setSearchQuery(e.target.value);
                    setPage(1);
                  }}
                  placeholder="Search by title, keyword, or client..."
                  className="w-full bg-[#181818] border border-gray-800 rounded-lg pl-9 pr-4 py-2 text-xs text-white placeholder-gray-500 focus:outline-none focus:border-indigo-500"
                />
                <svg className="w-4 h-4 text-gray-500 absolute left-3 top-2.5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
              </div>

              <div className="sm:w-48">
                <select
                  value={selectedWilaya}
                  onChange={(e) => {
                    setSelectedWilaya(e.target.value);
                    setPage(1);
                  }}
                  aria-label="Filter by Wilaya"
                  className="w-full bg-[#181818] border border-gray-800 rounded-lg px-3 py-2 text-xs text-white focus:outline-none focus:border-indigo-500"
                >
                  <option value="all">All Wilayas</option>
                  {wilayas.filter(w => w !== 'all').map((w) => (
                    <option key={w} value={w}>{w}</option>
                  ))}
                </select>
              </div>
            </div>
          </div>

          {/* Table / List */}
          {loading ? (
            <div className="bg-[#111111] border border-gray-800 rounded-xl p-16 flex flex-col items-center justify-center gap-3">
              <div className="w-10 h-10 border-4 border-indigo-500/20 border-t-indigo-600 rounded-full animate-spin" />
              <p className="text-gray-400 text-xs font-medium">Loading service requests...</p>
            </div>
          ) : error ? (
            <div className="bg-[#111111] border border-red-500/20 rounded-xl p-12 text-center">
              <p className="text-red-400 text-sm font-semibold">{error}</p>
              <button
                onClick={loadRequests}
                className="mt-3 px-4 py-1.5 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg text-xs font-bold transition-colors"
              >
                Retry
              </button>
            </div>
          ) : (
            <ServiceRequestsTable
              requests={data?.items || []}
              onViewDetails={(req) => setSelectedRequest(req)}
              onApprove={handleApprove}
              onReject={handleReject}
            />
          )}
        </main>
      </div>

      {/* Details & Moderation Modal */}
      {selectedRequest && (
        <ServiceRequestDetailModal
          request={selectedRequest}
          onClose={() => setSelectedRequest(null)}
          onStatusChanged={() => {
            loadRequests();
            showToast('Request status updated successfully.');
          }}
        />
      )}
    </div>
  );
};
