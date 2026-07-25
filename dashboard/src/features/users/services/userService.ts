import apiClient from '../../../services/apiClient';
import type { UsersResponse } from '../types';

export const userService = {
  getUsers: async (role?: 'client' | 'provider', search?: string): Promise<UsersResponse> => {
    const response = await apiClient.get<UsersResponse>('/admin-dashboard/users', {
      params: { role, search },
    });
    return response.data;
  },
};
