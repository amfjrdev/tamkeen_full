import React from 'react';
import type { User } from '../types';
import { StatusBadge } from './StatusBadge';
import { Icons } from '../../../components/common/Icons';

interface UserRowProps {
  user: User;
}

export const UserRow: React.FC<UserRowProps> = ({ user }) => {
  return (
    <tr className="hover:bg-gray-800/20 border-b border-gray-800/50 transition-colors">
      <td className="px-6 py-4.5">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-indigo-600/10 border border-indigo-500/25 flex items-center justify-center text-indigo-400 text-sm font-semibold tracking-wide">
            {user.initials}
          </div>
          <div>
            <p className="text-white font-medium text-sm hover:text-indigo-400 cursor-pointer transition-colors">
              {user.name}
            </p>
            <p className="text-gray-500 text-xs">{user.email}</p>
          </div>
        </div>
      </td>
      <td className="px-6 py-4.5">
        <StatusBadge status={user.status} />
      </td>
      <td className="px-6 py-4.5 text-sm text-gray-400 font-medium">
        {user.joined}
      </td>
      <td className="px-6 py-4.5 text-sm text-gray-300 font-medium font-mono">
        {user.requests} requests
      </td>
      <td className="px-6 py-4.5">
        <div className="flex items-center justify-end gap-3.5">
          <button 
            className="text-gray-400 hover:text-indigo-400 p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
            aria-label={`View profile for ${user.name}`}
          >
            <Icons.eye />
          </button>
          <button 
            className="text-gray-400 hover:text-emerald-400 p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
            aria-label={`Security details for ${user.name}`}
          >
            <Icons.shield />
          </button>
          <button 
            className="text-gray-400 hover:text-rose-500 p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
            aria-label={`Block ${user.name}`}
          >
            <Icons.userX />
          </button>
          <button 
            className="text-gray-400 hover:text-white p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
            aria-label="More options"
          >
            <Icons.dots />
          </button>
        </div>
      </td>
    </tr>
  );
};
