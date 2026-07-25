import apiClient from '../../../services/apiClient';
import type { TransactionsData } from '../types';

/**
 * Backend Request Specification:
 * -------------------------------------------------------------
 * Endpoint: GET /api/transactions
 * Description: Retrieves consolidated ledger transaction details, completion ratios, pending balances, checkout logs, and payment method details.
 * Query Parameters:
 *   - status: 'completed' | 'pending' | 'failed' (optional)
 *   - search: string (optional)
 * Response Header: Content-Type: application/json
 * Response Body (JSON): TransactionsData
 * -------------------------------------------------------------
 */

const mockTransactionsData: TransactionsData = {
  stats: [
    { label: 'Total Transactions', value: '3,284' },
    { label: 'Completed', value: '3,142' },
    { label: 'Pending', value: '95' },
    { label: 'Failed', value: '47' },
  ],
  transactions: [
    {
      id: 'TXN-2024-5891',
      provider: 'Maria Garcia',
      package: 'Professional Package',
      credits: 100,
      amount: 299,
      method: 'Credit Card',
      status: 'completed',
      date: '2024-05-30 14:23:45',
    },
    {
      id: 'TXN-2024-5890',
      provider: 'John Smith',
      package: 'Starter Package',
      credits: 30,
      amount: 99,
      method: 'PayPal',
      status: 'completed',
      date: '2024-05-30 13:15:22',
    },
    {
      id: 'TXN-2024-5889',
      provider: 'David Kim',
      package: 'Premium Package',
      credits: 250,
      amount: 599,
      method: 'Bank Transfer',
      status: 'pending',
      date: '2024-05-30 12:45:10',
    },
    {
      id: 'TXN-2024-5885',
      provider: 'James Wilson',
      package: 'Premium Package',
      credits: 250,
      amount: 599,
      method: 'Crypto',
      status: 'pending',
      date: '2024-05-30 08:40:12',
    },
    {
      id: 'TXN-2024-5884',
      provider: 'Emily Davis',
      package: 'Professional Package',
      credits: 100,
      amount: 299,
      method: 'Credit Card',
      status: 'completed',
      date: '2024-05-29 18:25:47',
    },
  ],
  activity: [
    { id: 1, user: 'Maria Garcia', package: 'Professional Package', credits: 100, amount: 299, status: 'completed', time: '14:23:45' },
    { id: 2, user: 'John Smith', package: 'Starter Package', credits: 30, amount: 99, status: 'completed', time: '13:15:22' },
    { id: 3, user: 'David Kim', package: 'Premium Package', credits: 250, amount: 599, status: 'pending', time: '12:45:10' },
    { id: 4, user: 'Rachel Brown', package: 'Enterprise Package', credits: 500, amount: 999, status: 'completed', time: '11:30:05' },
    { id: 5, user: 'Tom Martinez', package: 'Professional Package', credits: 100, amount: 299, status: 'failed', time: '10:22:18' },
  ],
  navItems: [
    { label: 'Dashboard', icon: 'dashboard', active: false },
    { label: 'Users', icon: 'users', active: false },
    { label: 'Providers', icon: 'providers', active: false },
    { label: 'Messaging', icon: 'messaging', active: false },
    { label: 'Categories', icon: 'categories', active: false },
    { label: 'Revenue', icon: 'revenue', active: false },
    { label: 'Transactions', icon: 'transactions', active: true },
    { label: 'Packages', icon: 'packages', active: false },
    { label: 'Analytics', icon: 'analytics', active: false },
    { label: 'Settings', icon: 'settings', active: false },
  ],
};

export const transactionService = {
  getTransactionsData: async (status?: string, search?: string): Promise<TransactionsData> => {
    try {
      const response = await apiClient.get<TransactionsData>('/transactions', {
        params: { status, search },
      });
      return response.data;
    } catch (error) {
      console.warn('Transactions API error, falling back to mock data:', error);
      // Simulate network delay for mock data
      await new Promise((resolve) => setTimeout(resolve, 500));
      return mockTransactionsData;
    }
  },
};
