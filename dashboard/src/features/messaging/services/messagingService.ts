import apiClient from '../../../services/apiClient';
import type { MessagingData } from '../types';

export const messagingService = {
  getMessagingData: async (): Promise<MessagingData> => {
    const response = await apiClient.get<MessagingData>('/messaging');
    return response.data;
  },

  toggleLock: async (id: string | number): Promise<void> => {
    await apiClient.post(`/admin-dashboard/conversations/${id}/toggle-lock`);
  },

  resolveReport: async (id: string | number): Promise<void> => {
    await apiClient.post(`/admin-dashboard/reports/${id}/resolve`);
  },
};
