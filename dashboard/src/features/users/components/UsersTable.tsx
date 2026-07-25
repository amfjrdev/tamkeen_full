import React from 'react';
import type { User } from '../types';
import { UserRow } from './UserRow';

interface UsersTableProps {
  users: User[];
}

export const UsersTable: React.FC<UsersTableProps> = ({ users }) => {
  return (
    <div className="bg-[#111111] border border-gray-800 rounded-xl overflow-hidden shadow-2xl">
      <div className="overflow-x-auto scrollbar-thin scrollbar-track-gray-900 scrollbar-thumb-gray-800">
        <table className="w-full border-collapse">
          <thead className="bg-[#161616] border-b border-gray-800">
            <tr>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">User</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Status</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Joined</th>
              <th className="text-left text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Requests</th>
              <th className="text-right text-xs font-semibold text-gray-400 uppercase tracking-wider px-6 py-4">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-850">
            {users.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-6 py-10 text-center text-gray-500 text-sm font-medium">
                  No users found matching your filters.
                </td>
              </tr>
            ) : (
              users.map((user) => (
                <UserRow key={user.id} user={user} />
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
