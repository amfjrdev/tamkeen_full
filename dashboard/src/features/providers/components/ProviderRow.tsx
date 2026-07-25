import React from 'react';
import type { Provider } from '../types';
import { ProviderStatusBadge } from './ProviderStatusBadge';
import { CategoryBadge } from './CategoryBadge';
import { PermissionBadge } from './PermissionBadge';
import { Icons } from '../../../components/common/Icons';

interface ProviderRowProps {
  provider: Provider;
}

export const ProviderRow: React.FC<ProviderRowProps> = ({ provider }) => {
  return (
    <tr className="hover:bg-gray-800/20 border-b border-gray-800/50 transition-colors">
      {/* Provider Details */}
      <td className="px-6 py-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-indigo-600/10 border border-indigo-500/25 flex items-center justify-center text-indigo-400 text-sm font-semibold tracking-wide shrink-0">
            {provider.initials}
          </div>
          <div>
            <div className="flex items-center gap-1.5">
              <p className="text-white font-medium text-sm hover:text-indigo-400 cursor-pointer transition-colors">
                {provider.name}
              </p>
              {provider.verified && (
                <span className="text-sky-400" aria-label="Verified user">
                  <Icons.verified />
                </span>
              )}
            </div>
            <p className="text-gray-500 text-xs truncate max-w-[180px]">{provider.email}</p>
          </div>
        </div>
      </td>

      {/* Category */}
      <td className="px-6 py-4">
        <CategoryBadge category={provider.category} />
      </td>

      {/* Status */}
      <td className="px-6 py-4">
        <div className="flex flex-col gap-1 items-start">
          <ProviderStatusBadge status={provider.status} />
          {provider.status === 'pending' && provider.pendingSince && (
            <span className="text-gray-500 text-[10px] font-medium tracking-wide font-mono">
              {provider.pendingSince}
            </span>
          )}
        </div>
      </td>

      {/* Performance (Rating / Completions) */}
      <td className="px-6 py-4">
        {provider.rating !== null ? (
          <div className="flex flex-col gap-1 items-start">
            <div className="flex items-center gap-1">
              <Icons.star />
              <span className="text-white text-sm font-semibold font-mono">{provider.rating}</span>
            </div>
            <span className="text-gray-500 text-xs font-medium">
              {provider.completed} completed
            </span>
          </div>
        ) : (
          <span className="text-gray-500 text-xs font-semibold">N/A</span>
        )}
      </td>

      {/* Credits */}
      <td className="px-6 py-4">
        {provider.creditsTotal > 0 ? (
          <div className="flex flex-col gap-1 items-start">
            <div className="flex items-center gap-1.5 text-indigo-400">
              <Icons.card />
              <span className="text-sm font-bold font-mono">{provider.credits}</span>
            </div>
            <span className="text-gray-500 text-[10px] font-medium font-mono">
              of {provider.creditsTotal}
            </span>
          </div>
        ) : (
          <span className="text-gray-500 text-xs font-semibold">None</span>
        )}
      </td>

      {/* Total Spent */}
      <td className="px-6 py-4">
        <div className="flex flex-col gap-1 items-start">
          <span className={`text-sm font-bold font-mono ${provider.totalSpent > 0 ? 'text-emerald-400' : 'text-gray-500'}`}>
            ${provider.totalSpent.toLocaleString()}
          </span>
          {provider.lastSpent && (
            <span className="text-gray-500 text-[10px] font-medium font-mono">
              Last: {provider.lastSpent}
            </span>
          )}
        </div>
      </td>

      {/* Connect Permission */}
      <td className="px-6 py-4">
        <PermissionBadge enabled={provider.connectPermission} />
      </td>

      {/* Actions */}
      <td className="px-6 py-4">
        <div className="flex items-center justify-end gap-3">
          <button 
            className="text-gray-400 hover:text-indigo-400 p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
            aria-label={`View profile for ${provider.name}`}
          >
            <Icons.eye />
          </button>
          {provider.status === 'pending' ? (
            <>
              <button 
                className="text-emerald-400 hover:text-emerald-300 hover:bg-emerald-500/10 p-1.5 rounded-lg transition-all cursor-pointer"
                aria-label={`Approve ${provider.name}`}
              >
                <Icons.checkCircle />
              </button>
              <button 
                className="text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 p-1.5 rounded-lg transition-all cursor-pointer"
                aria-label={`Reject ${provider.name}`}
              >
                <Icons.xCircle />
              </button>
            </>
          ) : (
            <button 
              className="text-gray-400 hover:text-white p-1.5 rounded-lg hover:bg-gray-800/60 transition-all cursor-pointer"
              aria-label="More options"
            >
              <Icons.dots />
            </button>
          )}
        </div>
      </td>
    </tr>
  );
};
