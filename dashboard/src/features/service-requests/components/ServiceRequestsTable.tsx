import React from 'react';
import type { ServiceRequestSummary } from '../types';
import { ServiceRequestRow } from './ServiceRequestRow';

interface ServiceRequestsTableProps {
  requests: ServiceRequestSummary[];
  onViewDetails: (request: ServiceRequestSummary) => void;
  onApprove: (id: string) => void;
  onReject: (request: ServiceRequestSummary) => void;
}

export const ServiceRequestsTable: React.FC<ServiceRequestsTableProps> = ({
  requests,
  onViewDetails,
  onApprove,
  onReject,
}) => {
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl">
      <div className="overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <table className="w-full min-w-[1000px] border-collapse">
          <thead className="bg-[#161616] border-b border-gray-800">
            <tr>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Title / Category</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Client</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Wilaya</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Budget</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Status</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Proposals</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Date</th>
              <th className="text-right text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {requests.length === 0 ? (
              <tr>
                <td colSpan={8} className="px-6 py-12 text-center text-gray-500 text-sm font-medium">
                  No service requests found matching your filters.
                </td>
              </tr>
            ) : (
              requests.map((req) => (
                <ServiceRequestRow
                  key={req.id}
                  request={req}
                  onViewDetails={onViewDetails}
                  onApprove={onApprove}
                  onReject={onReject}
                />
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
