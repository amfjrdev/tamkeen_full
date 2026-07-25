import React from 'react';
import type { Transaction } from '../types';
import { TransactionRow } from './TransactionRow';

interface TransactionsTableProps {
  transactions: Transaction[];
  totalCount: number;
  currentPage: number;
  itemsPerPage: number;
  onPageChange: (page: number) => void;
  onView: (id: string) => void;
  onInvoice: (id: string) => void;
  onDownload: (id: string) => void;
}

export const TransactionsTable: React.FC<TransactionsTableProps> = ({
  transactions,
  totalCount,
  currentPage,
  itemsPerPage,
  onPageChange,
  onView,
  onInvoice,
  onDownload,
}) => {
  const totalPages = Math.ceil(totalCount / itemsPerPage) || 1;

  const handlePrevPage = () => {
    if (currentPage > 1) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNextPage = () => {
    if (currentPage < totalPages) {
      onPageChange(currentPage + 1);
    }
  };

  // Generate page numbers to show
  const getPageNumbers = () => {
    const pages = [];
    const maxVisiblePages = 5;
    let start = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    const end = Math.min(totalPages, start + maxVisiblePages - 1);


    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  };

  const showingCount = Math.min(transactions.length, itemsPerPage);

  return (
    <div className="bg-[#111111]/80 backdrop-blur-md border border-gray-800 rounded-xl overflow-hidden shadow-xl">
      <div className="overflow-x-auto">
        <table className="w-full min-w-[1000px]">
          <thead className="bg-[#161616]/90 border-b border-gray-800">
            <tr>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Transaction ID
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Provider
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Package
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Credits
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Amount
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Payment Method
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Status
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Date
              </th>
              <th className="text-right text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-800/60">
            {transactions.length > 0 ? (
              transactions.map((txn) => (
                <TransactionRow
                  key={txn.id}
                  transaction={txn}
                  onView={onView}
                  onInvoice={onInvoice}
                  onDownload={onDownload}
                />
              ))
            ) : (
              <tr>
                <td colSpan={9} className="px-6 py-12 text-center text-gray-500">
                  <div className="flex flex-col items-center justify-center gap-2">
                    <svg className="w-8 h-8 opacity-45" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
                    </svg>
                    <span className="text-sm">No transactions match the selected criteria.</span>
                  </div>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination Footer */}
      <div className="px-6 py-4 border-t border-gray-800 flex flex-col sm:flex-row items-center justify-between gap-4 bg-[#111111]/30">
        <span className="text-gray-400 text-sm">
          Showing {showingCount} of {totalCount.toLocaleString()} transactions
        </span>
        
        {totalPages > 1 && (
          <div className="flex items-center gap-2">
            <button
              onClick={handlePrevPage}
              disabled={currentPage === 1}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all active:scale-95 cursor-pointer ${
                currentPage === 1
                  ? 'bg-gray-900 border-gray-800 text-gray-600 cursor-not-allowed'
                  : 'bg-gray-850 border-gray-700 text-white hover:bg-gray-800'
              }`}
            >
              Previous
            </button>

            {getPageNumbers().map((pageNum) => (
              <button
                key={pageNum}
                onClick={() => onPageChange(pageNum)}
                className={`w-8 h-8 flex items-center justify-center rounded-lg text-xs font-semibold border transition-all active:scale-95 cursor-pointer ${
                  currentPage === pageNum
                    ? 'bg-indigo-600 border-indigo-500 text-white font-bold'
                    : 'bg-gray-850 border-gray-700 text-gray-300 hover:text-white hover:bg-gray-800'
                }`}
              >
                {pageNum}
              </button>
            ))}

            <button
              onClick={handleNextPage}
              disabled={currentPage === totalPages}
              className={`px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all active:scale-95 cursor-pointer ${
                currentPage === totalPages
                  ? 'bg-gray-900 border-gray-800 text-gray-600 cursor-not-allowed'
                  : 'bg-gray-850 border-gray-700 text-white hover:bg-gray-800'
              }`}
            >
              Next
            </button>
          </div>
        )}
      </div>
    </div>
  );
};
