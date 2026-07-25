import React from 'react';
import type { ProviderStatus } from '../types';

interface ProviderStatusBadgeProps {
  status: ProviderStatus;
}

export const ProviderStatusBadge: React.FC<ProviderStatusBadgeProps> = ({ status }) => {
  const styles: Record<ProviderStatus, string> = {
    approved: 'bg-emerald-500/10 text-emerald-500 border border-emerald-500/20',
    pending: 'bg-amber-500/10 text-amber-400 border border-amber-500/20',
    rejected: 'bg-rose-500/10 text-rose-500 border border-rose-500/20',
  };

  return (
    <span className={`px-2.5 py-1 rounded-full text-xs font-semibold capitalize tracking-wide ${styles[status] || styles.approved}`}>
      {status}
    </span>
  );
};
