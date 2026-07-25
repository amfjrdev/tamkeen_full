import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { Icons } from '../components/common/Icons';
import { transactionService } from '../features/transactions/services/transactionService';
import { TransactionsTable } from '../features/transactions/components/TransactionsTable';
import { ActivityFeed } from '../features/transactions/components/ActivityFeed';
import type { TransactionsData, Transaction, TransactionStatus } from '../features/transactions/types';

interface TransactionsPageProps {
  onNavigate?: (label: string) => void;
}

export const TransactionsPage: React.FC<TransactionsPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<TransactionsData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Search & Filtering states
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<TransactionStatus | 'all'>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const [showStatusDropdown, setShowStatusDropdown] = useState(false);
  const itemsPerPage = 5;

  // Modals state
  const [selectedTxn, setSelectedTxn] = useState<Transaction | null>(null);
  const [modalType, setModalType] = useState<'view' | 'invoice' | null>(null);

  useEffect(() => {
    const fetchTransactions = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await transactionService.getTransactionsData();
        setData(result);
      } catch (err) {
        console.error('Failed to load transactions:', err);
        setError(err instanceof Error ? err.message : 'Failed to fetch payment ledger entries.');
      } finally {
        setLoading(false);
      }
    };

    fetchTransactions();
  }, []);

  const handleRetry = () => {
    setLoading(true);
    setError(null);
    transactionService
      .getTransactionsData()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to retrieve transactions database.');
        setLoading(false);
      });
  };

  const handleView = (id: string) => {
    if (!data) return;
    const txn = data.transactions.find((t) => t.id === id);
    if (txn) {
      setSelectedTxn(txn);
      setModalType('view');
    }
  };

  const handleInvoice = (id: string) => {
    if (!data) return;
    const txn = data.transactions.find((t) => t.id === id);
    if (txn) {
      setSelectedTxn(txn);
      setModalType('invoice');
    }
  };

  const handleDownload = (id: string) => {
    alert(`Downloading receipt for transaction: ${id}`);
  };

  const handleExportAll = () => {
    if (!data) return;
    const headers = 'Transaction ID,Provider,Package,Credits,Amount,Payment Method,Status,Date\n';
    const rows = data.transactions
      .map(
        (t) =>
          `"${t.id}","${t.provider}","${t.package}",${t.credits},${t.amount},"${t.method}","${t.status}","${t.date}"`
      )
      .join('\n');
    
    const blob = new Blob([headers + rows], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.setAttribute('href', url);
    a.setAttribute('download', `transactions_export_${new Date().toISOString().split('T')[0]}.csv`);
    a.click();
  };

  // Filter transactions in client-side state
  const getFilteredTransactions = () => {
    if (!data) return [];
    return data.transactions.filter((txn) => {
      const matchesSearch =
        txn.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
        txn.provider.toLowerCase().includes(searchTerm.toLowerCase()) ||
        txn.package.toLowerCase().includes(searchTerm.toLowerCase());
      
      const matchesStatus = statusFilter === 'all' || txn.status === statusFilter;
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
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading transaction records...</p>
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
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Transactions</h3>
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

  const filteredTransactions = getFilteredTransactions();
  const paginatedTransactions = filteredTransactions.slice(
    (currentPage - 1) * itemsPerPage,
    currentPage * itemsPerPage
  );

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
          {/* Top Bar Header */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
                Transactions Management
              </h1>
              <p className="text-gray-400 text-sm">
                Monitor system payment ledgers, audit invoices, and view credit acquisitions
              </p>
            </div>
            
            <button
              onClick={handleExportAll}
              className="flex items-center justify-center gap-2 px-4.5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold active:scale-95 transition-all shadow-lg shadow-indigo-500/25 cursor-pointer"
            >
              <Icons.download className="w-4 h-4" />
              Export CSV
            </button>
          </div>

          {/* Stats Summary Panel */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {data.stats.map((stat, idx) => {
              const colorsMap = [
                'border-indigo-500/10 hover:border-indigo-500/30 text-indigo-400',
                'border-emerald-500/10 hover:border-emerald-500/30 text-emerald-400',
                'border-amber-500/10 hover:border-amber-500/30 text-amber-400',
                'border-rose-500/10 hover:border-rose-500/30 text-rose-400',
              ];
              const cardColor = colorsMap[idx % colorsMap.length];
              
              return (
                <div
                  key={idx}
                  className={`bg-[#111111] border rounded-xl p-5 hover:bg-[#141414] transition-all duration-200 ${cardColor}`}
                >
                  <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider mb-2">
                    {stat.label}
                  </p>
                  <p className="text-white text-3xl font-extrabold tracking-tight font-mono">
                    {stat.value}
                  </p>
                </div>
              );
            })}
          </div>

          {/* Filters Bar */}
          <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
            <div className="relative flex-1 w-full max-w-md">
              <div className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                <Icons.search className="w-4 h-4 text-gray-500" />
              </div>
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setCurrentPage(1);
                }}
                placeholder="Search by ID, provider name, or package..."
                className="w-full bg-[#111111]/80 border border-gray-800 rounded-lg pl-10 pr-4 py-2 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
              />
            </div>
            
            <div className="flex gap-3 w-full md:w-auto items-center justify-end relative">
              <div className="relative">
                <button
                  onClick={() => setShowStatusDropdown(!showStatusDropdown)}
                  className="flex items-center gap-2 px-4 py-2 bg-gray-850 hover:bg-gray-800 border border-gray-700/80 rounded-lg text-sm font-semibold text-white transition-all cursor-pointer select-none"
                >
                  Status:{' '}
                  <span className="text-indigo-400 capitalize">{statusFilter}</span>
                  <Icons.chevronDown className="w-3.5 h-3.5" />
                </button>

                {showStatusDropdown && (
                  <>
                    <div className="fixed inset-0 z-10" onClick={() => setShowStatusDropdown(false)} />
                    <div className="absolute right-0 mt-2 w-40 bg-[#161616] border border-gray-800 rounded-lg shadow-xl z-20 py-1 overflow-hidden">
                      {(['all', 'completed', 'pending', 'failed'] as const).map((status) => (
                        <button
                          key={status}
                          onClick={() => {
                            setStatusFilter(status);
                            setCurrentPage(1);
                            setShowStatusDropdown(false);
                          }}
                          className={`w-full text-left px-4 py-2 text-xs font-semibold capitalize hover:bg-gray-800 transition-colors cursor-pointer ${
                            statusFilter === status ? 'text-indigo-400 bg-gray-800/40' : 'text-gray-400'
                          }`}
                        >
                          {status}
                        </button>
                      ))}
                    </div>
                  </>
                )}
              </div>

              <button
                onClick={() => {
                  setSearchTerm('');
                  setStatusFilter('all');
                  setCurrentPage(1);
                }}
                className="flex items-center gap-2 px-4 py-2 bg-gray-850 hover:bg-gray-800 border border-gray-700/80 rounded-lg text-sm font-semibold text-white transition-all cursor-pointer"
              >
                Clear Filters
              </button>
            </div>
          </div>

          {/* Ledger Table */}
          <TransactionsTable
            transactions={paginatedTransactions}
            totalCount={filteredTransactions.length}
            currentPage={currentPage}
            itemsPerPage={itemsPerPage}
            onPageChange={setCurrentPage}
            onView={handleView}
            onInvoice={handleInvoice}
            onDownload={handleDownload}
          />

          {/* Activity Feed Segment */}
          <ActivityFeed activities={data.activity} />
        </main>
      </div>

      {/* Details Dialog Modals */}
      {selectedTxn && modalType && (
        <>
          <div
            className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 transition-opacity"
            onClick={() => {
              setSelectedTxn(null);
              setModalType(null);
            }}
          />
          <div className="fixed inset-0 flex items-center justify-center p-4 z-50 pointer-events-none">
            <div className="bg-[#111111] border border-gray-800 w-full max-w-lg rounded-xl shadow-2xl p-6 relative pointer-events-auto transform scale-100 transition-transform duration-300">
              <button
                onClick={() => {
                  setSelectedTxn(null);
                  setModalType(null);
                }}
                className="absolute top-4 right-4 text-gray-500 hover:text-white transition-colors cursor-pointer"
              >
                <Icons.close className="w-5 h-5" />
              </button>

              {modalType === 'view' ? (
                <div>
                  <h3 className="text-white text-lg font-bold mb-4 tracking-tight border-b border-gray-800 pb-2">
                    Transaction Details
                  </h3>
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">ID</p>
                        <p className="text-white font-mono text-sm font-semibold mt-0.5">{selectedTxn.id}</p>
                      </div>
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">Status</p>
                        <div className="mt-1">
                          <span
                            className={`inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-semibold capitalize ${
                              selectedTxn.status === 'completed'
                                ? 'bg-emerald-500/10 text-emerald-400'
                                : selectedTxn.status === 'pending'
                                ? 'bg-amber-500/10 text-amber-400'
                                : 'bg-rose-500/10 text-rose-400'
                            }`}
                          >
                            {selectedTxn.status}
                          </span>
                        </div>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">Provider</p>
                        <p className="text-white text-sm font-semibold mt-0.5">{selectedTxn.provider}</p>
                      </div>
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">Package</p>
                        <p className="text-white text-sm font-semibold mt-0.5">{selectedTxn.package}</p>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">Credits Allocated</p>
                        <p className="text-indigo-400 text-sm font-bold font-mono mt-0.5">
                          {selectedTxn.credits} credits
                        </p>
                      </div>
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">Total Paid</p>
                        <p className="text-emerald-400 text-sm font-extrabold font-mono mt-0.5">
                          ${selectedTxn.amount}
                        </p>
                      </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">Payment Method</p>
                        <p className="text-white text-sm font-semibold mt-0.5">{selectedTxn.method}</p>
                      </div>
                      <div>
                        <p className="text-gray-500 text-xs font-semibold uppercase">Creation Date</p>
                        <p className="text-white text-sm font-semibold font-mono mt-0.5">{selectedTxn.date}</p>
                      </div>
                    </div>
                  </div>
                </div>
              ) : (
                <div>
                  <div className="border-b border-gray-800 pb-4 mb-4">
                    <div className="flex justify-between items-start">
                      <div>
                        <h4 className="text-white text-xl font-bold tracking-tight">Invoice Receipt</h4>
                        <p className="text-gray-500 text-xs mt-0.5 font-mono">{selectedTxn.id}</p>
                      </div>
                      <div className="text-right">
                        <span className="text-gray-500 text-xs">Date:</span>
                        <p className="text-white text-xs font-mono">{selectedTxn.date.split(' ')[0]}</p>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-4">
                    <div className="text-xs text-gray-400 bg-gray-900 p-3 rounded-lg border border-gray-800">
                      <p className="font-semibold text-white mb-1">Bill To:</p>
                      <p className="font-semibold text-white">{selectedTxn.provider}</p>
                      <p className="mt-0.5 text-gray-500">Platform Registered Service Provider</p>
                    </div>

                    <table className="w-full text-left text-xs">
                      <thead>
                        <tr className="border-b border-gray-800 text-gray-500 uppercase tracking-wider font-semibold">
                          <th className="py-2">Item Description</th>
                          <th className="py-2 text-right">Qty</th>
                          <th className="py-2 text-right">Total</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-gray-800/40">
                        <tr>
                          <td className="py-3">
                            <p className="font-semibold text-white">{selectedTxn.package}</p>
                            <p className="text-[10px] text-indigo-400 font-semibold">{selectedTxn.credits} Connect Credits Package</p>
                          </td>
                          <td className="py-3 text-right font-mono">1</td>
                          <td className="py-3 text-right font-semibold text-white font-mono">${selectedTxn.amount}</td>
                        </tr>
                      </tbody>
                    </table>

                    <div className="border-t border-gray-800 pt-4 flex flex-col items-end gap-1.5 text-sm">
                      <div className="flex justify-between w-full max-w-[200px] text-xs text-gray-400">
                        <span>Payment Method:</span>
                        <span className="font-semibold text-white">{selectedTxn.method}</span>
                      </div>
                      <div className="flex justify-between w-full max-w-[200px] text-xs text-gray-400 border-b border-gray-800 pb-2">
                        <span>Status:</span>
                        <span className="font-semibold text-emerald-400 capitalize">{selectedTxn.status}</span>
                      </div>
                      <div className="flex justify-between w-full max-w-[200px] font-bold text-white mt-1">
                        <span>Total Due:</span>
                        <span className="text-emerald-400 font-mono">${selectedTxn.amount}</span>
                      </div>
                    </div>

                    <div className="pt-4 flex justify-end gap-3 border-t border-gray-800">
                      <button
                        onClick={() => alert('Printing Invoice...')}
                        className="px-4 py-2 border border-gray-700 hover:bg-gray-800 text-white rounded-lg text-xs font-semibold cursor-pointer transition-all"
                      >
                        Print
                      </button>
                      <button
                        onClick={() => {
                          alert(`Receipt downloaded for transaction ${selectedTxn.id}`);
                          setSelectedTxn(null);
                          setModalType(null);
                        }}
                        className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-xs font-semibold cursor-pointer transition-all shadow-md shadow-indigo-600/20"
                      >
                        Download PDF
                      </button>
                    </div>
                  </div>
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
};
