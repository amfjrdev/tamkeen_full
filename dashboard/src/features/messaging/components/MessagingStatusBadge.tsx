import React from 'react';
import type { ConversationStatus } from '../types';
import { Icons } from '../../../components/common/Icons';

interface MessagingStatusBadgeProps {
  status: ConversationStatus;
  isLocked?: boolean;
}

export const MessagingStatusBadge: React.FC<MessagingStatusBadgeProps> = ({ status, isLocked }) => {
  if (status === 'reported') {
    return (
      <div className="flex flex-wrap items-center gap-2">
        <span className="px-2.5 py-1 rounded-full text-xs font-semibold bg-rose-500/10 text-rose-400 border border-rose-500/20 flex items-center gap-1.5 shrink-0">
          <Icons.alert className="w-3.5 h-3.5" /> Reported
        </span>
        {isLocked && (
          <span className="px-2.5 py-1 rounded-full text-xs font-semibold bg-amber-500/10 text-amber-400 border border-amber-500/20 flex items-center gap-1.5 shrink-0">
            <Icons.lockClosed className="w-3.5 h-3.5" /> Locked
          </span>
        )}
      </div>
    );
  }
  return (
    <span className="px-2.5 py-1 rounded-full text-xs font-semibold bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
      Active
    </span>
  );
};
