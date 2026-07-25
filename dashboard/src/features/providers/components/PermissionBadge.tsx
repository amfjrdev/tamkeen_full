import React from 'react';
import { Icons } from '../../../components/common/Icons';

interface PermissionBadgeProps {
  enabled: boolean;
}

export const PermissionBadge: React.FC<PermissionBadgeProps> = ({ enabled }) => (
  <span
    className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold border shadow-sm ${
      enabled
        ? 'bg-emerald-500/10 text-emerald-400 border-emerald-500/20'
        : 'bg-rose-500/10 text-rose-400 border-rose-500/20'
    }`}
  >
    {enabled ? <Icons.lockOpen /> : <Icons.lockClosed />}
    {enabled ? 'Enabled' : 'Disabled'}
  </span>
);
