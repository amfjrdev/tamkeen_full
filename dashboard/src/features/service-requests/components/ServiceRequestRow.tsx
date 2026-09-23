import React from 'react';
import type { ServiceRequestSummary } from '../types';

interface ServiceRequestRowProps {
  request: ServiceRequestSummary;
  onViewDetails: (request: ServiceRequestSummary) => void;
  onApprove: (id: string) => void;
  onReject: (request: ServiceRequestSummary) => void;
}

export const ServiceRequestRow: React.FC<ServiceRequestRowProps> = ({
  request,
  onViewDetails,
  onApprove,
  onReject,
}) => {
  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Approved':
        return (
          <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
            Approved
          </span>
        );
      case 'PendingAdminReview':
        return (
          <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-amber-500/10 text-amber-400 border border-amber-500/20 animate-pulse">
            Pending Review
          </span>
        );
      case 'ProviderSelected':
        return (
          <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-purple-500/10 text-purple-400 border border-purple-500/20">
            In Progress
          </span>
        );
      case 'Completed':
        return (
          <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-blue-500/10 text-blue-400 border border-blue-500/20">
            Completed
          </span>
        );
      case 'Rejected':
        return (
          <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-red-500/10 text-red-400 border border-red-500/20">
            Rejected
          </span>
        );
      default:
        return (
          <span className="px-2.5 py-1 text-xs font-semibold rounded-full bg-gray-500/10 text-gray-400 border border-gray-500/20">
            {status}
          </span>
        );
    }
  };

  return (
    <tr className="hover:bg-gray-800/30 transition-colors cursor-pointer" onClick={() => onViewDetails(request)}>
      {/* Title & Category */}
      <td className="px-6 py-4">
        <div>
          <p className="text-white font-semibold text-sm hover:text-indigo-400 transition-colors line-clamp-1">
            {request.title}
          </p>
          <span className="text-xs text-indigo-400/80 font-medium">
            {request.categoryName}
          </span>
        </div>
      </td>

      {/* Client */}
      <td className="px-6 py-4">
        <div className="flex items-center gap-2.5">
          <div className="w-8 h-8 rounded-full bg-indigo-600/30 border border-indigo-500/30 flex items-center justify-center text-xs font-bold text-white shrink-0">
            {request.clientName.substring(0, 2).toUpperCase()}
          </div>
          <div>
            <p className="text-white text-xs font-medium">{request.clientName}</p>
            <p className="text-[11px] text-gray-400">{request.wilaya}</p>
          </div>
        </div>
      </td>

      {/* Wilaya */}
      <td className="px-6 py-4 text-xs font-medium text-gray-300">
        <span className="px-2 py-0.5 rounded bg-gray-800 text-gray-300">
          {request.wilaya}
        </span>
      </td>

      {/* Budget */}
      <td className="px-6 py-4 text-xs font-bold text-emerald-400">
        {request.budget ? `${request.budget.toLocaleString()} DA` : 'Flexible'}
      </td>

      {/* Status */}
      <td className="px-6 py-4">
        {getStatusBadge(request.status)}
      </td>

      {/* Applications */}
      <td className="px-6 py-4 text-xs font-medium text-gray-300">
        <span className="text-white font-bold">{request.applicationCount}</span> applicants
      </td>

      {/* Created Date */}
      <td className="px-6 py-4 text-xs text-gray-400">
        {new Date(request.createdAt).toLocaleDateString()}
      </td>

      {/* Actions */}
      <td className="px-6 py-4 text-right space-x-2" onClick={(e) => e.stopPropagation()}>
        {request.status === 'PendingAdminReview' ? (
          <div className="flex items-center justify-end gap-2">
            <button
              onClick={() => onApprove(request.id)}
              className="p-1.5 bg-emerald-500/10 hover:bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 rounded-lg text-xs font-semibold transition-colors"
              title="Approve Request"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
              </svg>
            </button>
            <button
              onClick={() => onReject(request)}
              className="p-1.5 bg-red-500/10 hover:bg-red-500/20 text-red-400 border border-red-500/30 rounded-lg text-xs font-semibold transition-colors"
              title="Reject Request"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        ) : (
          <button
            onClick={() => onViewDetails(request)}
            className="text-gray-400 hover:text-white text-xs font-medium"
          >
            View
          </button>
        )}
      </td>
    </tr>
  );
};
