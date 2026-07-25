import React from 'react';
import type { Conversation } from '../types';
import { MessagingStatusBadge } from './MessagingStatusBadge';
import { Icons } from '../../../components/common/Icons';

interface ConversationRowProps {
  conv: Conversation;
  onToggleLock?: (id: number) => void;
}

export const ConversationRow: React.FC<ConversationRowProps> = ({ conv, onToggleLock }) => {
  return (
    <tr className="hover:bg-gray-800/20 border-b border-gray-800/50 transition-colors">
      {/* Participants */}
      <td className="px-6 py-4.5">
        <div className="flex flex-col">
          <span className="text-white font-semibold text-sm flex items-center gap-1">
            {conv.participant1}
            <span className="text-indigo-400 opacity-80" aria-hidden="true">
              <Icons.exchange className="w-3.5 h-3.5 mx-0.5" />
            </span>
            {conv.participant2}
          </span>
        </div>
      </td>

      {/* Last Message snippet */}
      <td className="px-6 py-4.5 text-sm text-gray-400 max-w-xs truncate font-medium">
        {conv.lastMessage}
      </td>

      {/* Time */}
      <td className="px-6 py-4.5 text-xs text-gray-500 font-semibold font-mono whitespace-nowrap">
        {conv.time}
      </td>

      {/* Message Count */}
      <td className="px-6 py-4.5 text-sm text-white font-bold font-mono">
        {conv.messageCount}
      </td>

      {/* Status */}
      <td className="px-6 py-4.5">
        <MessagingStatusBadge status={conv.status} isLocked={conv.isLocked} />
      </td>

      {/* Action buttons */}
      <td className="px-6 py-4.5 text-right">
        <div className="flex items-center justify-end gap-3.5">
          <button 
            className="text-gray-400 hover:text-indigo-400 p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
            aria-label={`Inspect chat history between ${conv.participant1} and ${conv.participant2}`}
          >
            <Icons.eye />
          </button>
          
          <button 
            onClick={() => onToggleLock && onToggleLock(conv.id)}
            className={`p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer ${
              conv.isLocked 
                ? 'text-emerald-400 hover:text-emerald-300' 
                : 'text-rose-400 hover:text-rose-300'
            }`}
            aria-label={conv.isLocked ? "Unlock chat room" : "Lock chat room"}
          >
            {conv.isLocked ? <Icons.lockOpen /> : <Icons.lockClosed />}
          </button>
          
          <button 
            className="text-gray-400 hover:text-white p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
            aria-label="More moderation settings"
          >
            <Icons.dots />
          </button>
        </div>
      </td>
    </tr>
  );
};
