import React, { useState } from 'react';
import type { ServiceRequestSummary, ServiceRequestDetail } from '../types';
import { serviceRequestService } from '../services/serviceRequestService';

interface ServiceRequestDetailModalProps {
  request: ServiceRequestSummary;
  onClose: () => void;
  onStatusChanged: () => void;
}

export const ServiceRequestDetailModal: React.FC<ServiceRequestDetailModalProps> = ({
  request,
  onClose,
  onStatusChanged,
}) => {
  const [detail, setDetail] = useState<ServiceRequestDetail | null>(null);
  // const [loading, setLoading] = useState(false);
  const [actionLoading, setActionLoading] = useState(false);
  const [showRejectForm, setShowRejectForm] = useState(false);
  const [rejectReason, setRejectReason] = useState('');
  const [error, setError] = useState<string | null>(null);

  React.useEffect(() => {
    const fetchDetail = async () => {
      try {
        
        const data = await serviceRequestService.getServiceRequestById(request.id);
        setDetail(data);
      } catch (err) {
        console.error('Failed to load request detail', err);
      } finally {
        
      }
    };
    fetchDetail();
  }, [request.id]);

  const handleApprove = async () => {
    try {
      setActionLoading(true);
      setError(null);
      await serviceRequestService.approveRequest(request.id);
      onStatusChanged();
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to approve request');
    } finally {
      setActionLoading(false);
    }
  };

  const handleReject = async () => {
    try {
      setActionLoading(true);
      setError(null);
      await serviceRequestService.rejectRequest(request.id, rejectReason);
      onStatusChanged();
      onClose();
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'Failed to reject request');
    } finally {
      setActionLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm">
      <div className="bg-[#121212] border border-gray-800 rounded-2xl w-full max-w-2xl max-h-[90vh] overflow-y-auto shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-800">
          <div>
            <h2 className="text-lg font-bold text-white">Service Request Details</h2>
            <p className="text-xs text-gray-400 mt-0.5">ID: {request.id}</p>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 text-gray-400 hover:text-white rounded-lg hover:bg-gray-800 transition-colors"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Content */}
        <div className="p-6 space-y-6">
          {error && (
            <div className="bg-red-500/10 border border-red-500/30 text-red-400 p-3 rounded-lg text-sm">
              {error}
            </div>
          )}

          {/* Title & Status */}
          <div className="flex items-start justify-between gap-4">
            <div>
              <span className="inline-block px-2.5 py-1 text-xs font-semibold rounded-md bg-indigo-500/10 text-indigo-400 border border-indigo-500/20 mb-2">
                {request.categoryName}
              </span>
              <h3 className="text-xl font-bold text-white leading-snug">{request.title}</h3>
            </div>
            <span
              className={`px-3 py-1 rounded-full text-xs font-semibold uppercase tracking-wider shrink-0 ${
                request.status === 'Approved'
                  ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20'
                  : request.status === 'PendingAdminReview'
                  ? 'bg-amber-500/10 text-amber-400 border border-amber-500/20'
                  : request.status === 'Rejected'
                  ? 'bg-red-500/10 text-red-400 border border-red-500/20'
                  : request.status === 'Completed'
                  ? 'bg-blue-500/10 text-blue-400 border border-blue-500/20'
                  : 'bg-purple-500/10 text-purple-400 border border-purple-500/20'
              }`}
            >
              {request.status === 'PendingAdminReview' ? 'Pending Review' : request.status}
            </span>
          </div>

          {/* Key metadata grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 bg-[#181818] p-4 rounded-xl border border-gray-800/80">
            <div>
              <p className="text-[11px] text-gray-500 uppercase font-medium">Client</p>
              <p className="text-sm font-semibold text-white mt-0.5">{request.clientName}</p>
            </div>
            <div>
              <p className="text-[11px] text-gray-500 uppercase font-medium">Wilaya</p>
              <p className="text-sm font-semibold text-white mt-0.5">{request.wilaya}</p>
            </div>
            <div>
              <p className="text-[11px] text-gray-500 uppercase font-medium">Budget</p>
              <p className="text-sm font-semibold text-emerald-400 mt-0.5">
                {request.budget ? `${request.budget.toLocaleString()} DA` : 'Flexible'}
              </p>
            </div>
            <div>
              <p className="text-[11px] text-gray-500 uppercase font-medium">Created</p>
              <p className="text-sm font-semibold text-gray-300 mt-0.5">
                {new Date(request.createdAt).toLocaleDateString()}
              </p>
            </div>
          </div>

          {/* Description */}
          <div>
            <h4 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-2">Description</h4>
            <div className="bg-[#181818] p-4 rounded-xl border border-gray-800 text-sm text-gray-300 leading-relaxed whitespace-pre-wrap">
              {request.description}
            </div>
          </div>

          {/* Selected Provider if any */}
          {request.selectedProviderName && (
            <div className="bg-indigo-950/20 border border-indigo-500/30 p-4 rounded-xl">
              <p className="text-xs text-indigo-400 font-semibold uppercase">Selected Provider</p>
              <p className="text-base font-bold text-white mt-1">{request.selectedProviderName}</p>
            </div>
          )}

          {/* Applications section */}
          {detail && detail.applications && detail.applications.length > 0 && (
            <div>
              <h4 className="text-xs font-semibold text-gray-400 uppercase tracking-wider mb-3">
                Provider Applications ({detail.applications.length})
              </h4>
              <div className="space-y-3">
                {detail.applications.map((app) => (
                  <div key={app.id} className="bg-[#181818] p-4 rounded-xl border border-gray-800">
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-2">
                        <span className="font-semibold text-white text-sm">{app.providerName}</span>
                        <span className="text-xs text-amber-400 font-medium">★ {app.providerRating.toFixed(1)}</span>
                      </div>
                      <span className="text-sm font-bold text-emerald-400">
                        {app.proposedPrice ? `${app.proposedPrice.toLocaleString()} DA` : 'Standard'}
                      </span>
                    </div>
                    <p className="text-xs text-gray-300 italic">{app.coverLetter}</p>
                  </div>
                ))}
              </div>
            </div>
          )}

          {/* Rejection form if opened */}
          {showRejectForm && (
            <div className="bg-red-500/10 border border-red-500/20 p-4 rounded-xl space-y-3">
              <label className="block text-xs font-semibold text-red-400 uppercase">
                Reason for Rejection
              </label>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                placeholder="Explain why this request is being rejected (e.g., prohibited items, invalid details)..."
                className="w-full bg-[#121212] border border-red-500/30 rounded-lg p-3 text-sm text-white focus:outline-none focus:border-red-500 resize-none h-24"
              />
              <div className="flex justify-end gap-2">
                <button
                  onClick={() => setShowRejectForm(false)}
                  className="px-3 py-1.5 text-xs text-gray-400 hover:text-white rounded-lg"
                >
                  Cancel
                </button>
                <button
                  onClick={handleReject}
                  disabled={actionLoading}
                  className="px-4 py-1.5 bg-red-600 hover:bg-red-500 text-white rounded-lg text-xs font-bold transition-colors disabled:opacity-50"
                >
                  Confirm Rejection
                </button>
              </div>
            </div>
          )}
        </div>

        {/* Footer Actions */}
        <div className="flex items-center justify-between px-6 py-4 border-t border-gray-800 bg-[#151515]">
          <button
            onClick={onClose}
            className="px-4 py-2 text-sm text-gray-400 hover:text-white font-medium transition-colors"
          >
            Close
          </button>

          {request.status === 'PendingAdminReview' && !showRejectForm && (
            <div className="flex items-center gap-3">
              <button
                onClick={() => setShowRejectForm(true)}
                disabled={actionLoading}
                className="px-4 py-2 bg-red-500/10 hover:bg-red-500/20 text-red-400 border border-red-500/30 rounded-lg text-sm font-semibold transition-colors disabled:opacity-50"
              >
                Reject Request
              </button>
              <button
                onClick={handleApprove}
                disabled={actionLoading}
                className="px-5 py-2 bg-emerald-600 hover:bg-emerald-500 text-white rounded-lg text-sm font-bold shadow-lg shadow-emerald-600/20 transition-colors disabled:opacity-50 flex items-center gap-2"
              >
                {actionLoading ? 'Processing...' : 'Approve & Publish'}
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};
