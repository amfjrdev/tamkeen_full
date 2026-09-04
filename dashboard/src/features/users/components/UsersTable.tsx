import React from 'react';
import type { Client } from '../types';
import { UserRow } from './UserRow';

interface UsersTableProps {
  users?: Client[];
  clients?: Client[];
  totalCount?: number;
  currentPage?: number;
  pageSize?: number;
  totalPages?: number;
  onPageChange?: (page: number) => void;
  isLoading?: boolean;
}

export const UsersTable: React.FC<UsersTableProps> = ({
  users,
  clients,
  totalCount = 0,
  currentPage = 1,
  pageSize = 10,
  totalPages = 1,
  onPageChange,
  isLoading = false,
}) => {
  const items = clients || users || [];
  const effectiveTotal = totalCount > 0 ? totalCount : items.length;
  const effectiveTotalPages = totalPages > 0 ? totalPages : Math.ceil(effectiveTotal / pageSize) || 1;

  const handlePrevPage = () => {
    if (currentPage > 1 && onPageChange) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNextPage = () => {
    if (currentPage < effectiveTotalPages && onPageChange) {
      onPageChange(currentPage + 1);
    }
  };

  // Generate page numbers to display with smart windowing
  const getPageNumbers = () => {
    const pages: number[] = [];
    const maxVisiblePages = 5;
    let start = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
    const end = Math.min(effectiveTotalPages, start + maxVisiblePages - 1);

    if (end - start + 1 < maxVisiblePages) {
      start = Math.max(1, end - maxVisiblePages + 1);
    }

    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  };

  const startItem = effectiveTotal === 0 ? 0 : (currentPage - 1) * pageSize + 1;
  const endItem = Math.min(startItem + items.length - 1, effectiveTotal);

  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl transition-all">
      <div className="overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <table className="w-full border-collapse">
          <thead className="bg-[#161616] border-b border-gray-800">
            <tr>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Client
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Status
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Joined
              </th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Requests
              </th>
              <th className="text-right text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">
                Actions
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {isLoading ? (
              <tr>
                <td colSpan={5} className="px-6 py-12 text-center text-gray-500">
                  <div className="flex flex-col items-center justify-center gap-3">
                    <div className="w-8 h-8 rounded-full border-2 border-indigo-500/20 border-t-indigo-500 animate-spin" />
                    <span className="text-sm font-medium">Loading clients data...</span>
                  </div>
                </td>
              </tr>
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-6 py-12 text-center text-gray-500">
                  <div className="flex flex-col items-center justify-center gap-2">
                    <svg className="w-10 h-10 opacity-30 text-gray-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                    </svg>
                    <span className="text-sm font-medium">No clients found matching your filters.</span>
                  </div>
                </td>
              </tr>
            ) : (
              items.map((client) => (
                <UserRow key={client.id} user={client} />
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination Footer */}
      {effectiveTotal > 0 && (
        <div className="px-6 py-4 border-t border-gray-800 flex flex-col sm:flex-row items-center justify-between gap-4 bg-[#111111]/40">
          <span className="text-gray-400 text-xs sm:text-sm font-medium">
            Showing <span className="text-white font-semibold">{startItem}</span> to{' '}
            <span className="text-white font-semibold">{endItem}</span> of{' '}
            <span className="text-white font-semibold">{effectiveTotal.toLocaleString()}</span> clients
          </span>

          {effectiveTotalPages > 1 && onPageChange && (
            <div className="flex items-center gap-1.5 sm:gap-2">
              <button
                onClick={handlePrevPage}
                disabled={currentPage === 1 || isLoading}
                className={`px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all active:scale-95 ${
                  currentPage === 1 || isLoading
                    ? 'bg-gray-900 border-gray-800 text-gray-600 cursor-not-allowed'
                    : 'bg-gray-850 border-gray-700 text-white hover:bg-gray-800 cursor-pointer'
                }`}
              >
                Previous
              </button>

              <div className="flex items-center gap-1">
                {getPageNumbers().map((pageNum) => (
                  <button
                    key={pageNum}
                    onClick={() => !isLoading && onPageChange(pageNum)}
                    disabled={isLoading}
                    className={`w-8 h-8 flex items-center justify-center rounded-lg text-xs font-semibold border transition-all active:scale-95 cursor-pointer ${
                      currentPage === pageNum
                        ? 'bg-indigo-600 border-indigo-500 text-white font-bold shadow-lg shadow-indigo-600/20'
                        : 'bg-gray-850 border-gray-700 text-gray-300 hover:text-white hover:bg-gray-800'
                    }`}
                  >
                    {pageNum}
                  </button>
                ))}
              </div>

              <button
                onClick={handleNextPage}
                disabled={currentPage === effectiveTotalPages || isLoading}
                className={`px-3 py-1.5 rounded-lg text-xs font-semibold border transition-all active:scale-95 ${
                  currentPage === effectiveTotalPages || isLoading
                    ? 'bg-gray-900 border-gray-800 text-gray-600 cursor-not-allowed'
                    : 'bg-gray-850 border-gray-700 text-white hover:bg-gray-800 cursor-pointer'
                }`}
              >
                Next
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export const ClientsTable = UsersTable;
