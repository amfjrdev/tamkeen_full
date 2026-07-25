import React from 'react';

interface MethodBadgeProps {
  method: string;
}

export const MethodBadge: React.FC<MethodBadgeProps> = ({ method }) => {
  return (
    <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-semibold bg-gray-800 text-gray-300 border border-gray-700/60 shadow-sm backdrop-blur-md">
      {method}
    </span>
  );
};
