import React from 'react';
import type { TransactionStatus } from '../types';
import { Icons } from '../../../components/common/Icons';

interface StatusBadgeProps {
  status: TransactionStatus;
}

export const StatusBadge: React.FC<StatusBadgeProps> = ({ status }) => {
  const styles: Record<TransactionStatus, string> = {
    completed: 'bg-emerald-500/10 text-emerald-500 border border-emerald-500/20',
    pending: 'bg-amber-500/10 text-amber-500 border border-amber-500/20',
    failed: 'bg-rose-500/10 text-rose-500 border border-rose-500/20',
  };

  const icons: Record<TransactionStatus, React.ComponentType<React.SVGProps<SVGSVGElement>>> = {
    completed: Icons.checkCircle,
    pending: Icons.clock,
    failed: Icons.xCircle,
  };

  const IconComponent = icons[status] || icons.pending;
  const styleClass = styles[status] || styles.pending;

  return (
    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold backdrop-blur-md transition-all ${styleClass}`}>
      <IconComponent className="w-3.5 h-3.5" />
      <span className="capitalize">{status}</span>
    </span>
  );
};
