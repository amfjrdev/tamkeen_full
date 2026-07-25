import apiClient from '../../../services/apiClient';
import type { ProvidersResponse } from '../types';

export const providerService = {
  getProviders: async (status?: string, category?: string, search?: string): Promise<ProvidersResponse> => {
    const response = await apiClient.get<ProvidersResponse>('/admin-dashboard/providers', {
      params: { status, category, search },
    });
    return response.data;
  },
};
