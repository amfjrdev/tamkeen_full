import React from 'react';
import type { Conversation } from '../types';
import { ConversationRow } from './ConversationRow';

interface ConversationsTableProps {
  conversations: Conversation[];
  onToggleLock?: (id: number) => void;
}

export const ConversationsTable: React.FC<ConversationsTableProps> = ({ conversations, onToggleLock }) => {
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl">
      <div className="overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <table className="w-full min-w-[800px] border-collapse">
          <thead className="bg-[#161616] text-left border-b border-gray-800">
            <tr>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Participants</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Last Message</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Time</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Messages</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider">Status</th>
              <th className="px-6 py-4 text-xs font-semibold text-gray-400 uppercase tracking-wider text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {conversations.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-10 text-center text-gray-500 text-sm font-medium">
                  No conversations found.
                </td>
              </tr>
            ) : (
              conversations.map((conv) => (
                <ConversationRow 
                  key={conv.id} 
                  conv={conv} 
                  onToggleLock={onToggleLock}
                />
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
