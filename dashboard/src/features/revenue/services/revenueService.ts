import apiClient from '../../../services/apiClient';
import type { RevenueData } from '../types';

export const revenueService = {
  getRevenueData: async (): Promise<RevenueData> => {
    const response = await apiClient.get<RevenueData>('/revenue');
    return response.data;
  },
};
