import React from 'react';
import type { UserStatus } from '../types';

interface StatusBadgeProps {
  status: UserStatus;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  const styles: Record<UserStatus, string> = {
    active: 'bg-emerald-500/10 text-emerald-500 border border-emerald-500/20',
    blocked: 'bg-rose-500/10 text-rose-500 border border-rose-500/20',
    pending: 'bg-amber-500/10 text-amber-500 border border-amber-500/20',
  };

  const style = styles[status] || styles.active;

  return (
    <span className={`px-2.5 py-1 rounded-full text-xs font-semibold tracking-wide capitalize ${style}`}>
      {status}
    </span>
  );
};
