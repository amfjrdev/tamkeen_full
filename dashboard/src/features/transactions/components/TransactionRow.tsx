import React from 'react';
import type { Transaction } from '../types';
import { StatusBadge } from './StatusBadge';
import { MethodBadge } from './MethodBadge';
import { Icons } from '../../../components/common/Icons';

interface TransactionRowProps {
  transaction: Transaction;
  onView: (id: string) => void;
  onInvoice: (id: string) => void;
  onDownload: (id: string) => void;
}

export const TransactionRow: React.FC<TransactionRowProps> = ({
  transaction,
  onView,
  onInvoice,
  onDownload,
}) => {
  return (
    <tr className="hover:bg-gray-800/30 transition-colors border-b border-gray-800/50">
      <td className="px-6 py-4 text-indigo-400 font-mono font-medium text-sm whitespace-nowrap">
        {transaction.id}
      </td>
      <td className="px-6 py-4 text-white font-semibold text-sm whitespace-nowrap">
        {transaction.provider}
      </td>
      <td className="px-6 py-4 text-gray-300 text-sm whitespace-nowrap">
        {transaction.package}
      </td>
      <td className="px-6 py-4 text-sm whitespace-nowrap">
        <span className="text-indigo-400 font-bold font-mono">{transaction.credits}</span>{' '}
        <span className="text-gray-500 text-xs">credits</span>
      </td>
      <td className="px-6 py-4 text-emerald-400 font-mono font-extrabold text-sm whitespace-nowrap">
        ${transaction.amount}
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <MethodBadge method={transaction.method} />
      </td>
      <td className="px-6 py-4 whitespace-nowrap">
        <StatusBadge status={transaction.status} />
      </td>
      <td className="px-6 py-4 text-gray-400 text-sm whitespace-nowrap">
        <div className="font-mono text-xs">{transaction.date.split(' ')[0]}</div>
        <div className="text-gray-500 text-[10px]">{transaction.date.split(' ')[1]}</div>
      </td>
      <td className="px-6 py-4 text-right whitespace-nowrap">
        <div className="flex items-center justify-end gap-3">
          <button
            onClick={() => onView(transaction.id)}
            title="View Details"
            className="text-gray-400 hover:text-white hover:bg-gray-800 p-1.5 rounded-md transition-all active:scale-95 cursor-pointer"
          >
            <Icons.eye className="w-4 h-4" />
          </button>
          <button
            onClick={() => onInvoice(transaction.id)}
            title="Generate Invoice"
            className="text-indigo-400 hover:text-indigo-300 hover:bg-indigo-950/20 p-1.5 rounded-md transition-all active:scale-95 cursor-pointer"
          >
            <Icons.file className="w-4 h-4" />
          </button>
          <button
            onClick={() => onDownload(transaction.id)}
            title="Download Receipt"
            className="text-gray-400 hover:text-white hover:bg-gray-800 p-1.5 rounded-md transition-all active:scale-95 cursor-pointer"
          >
            <Icons.downloadSmall className="w-4 h-4" />
          </button>
        </div>
      </td>
    </tr>
  );
};
