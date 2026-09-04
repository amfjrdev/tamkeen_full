import apiClient from '../../../services/apiClient';
import type { ClientsResponse, UsersResponse } from '../types';

export interface GetClientsParams {
  page?: number;
  pageSize?: number;
  search?: string;
  status?: string;
  role?: 'client' | 'provider';
}

export const clientService = {
  getClients: async (params?: GetClientsParams): Promise<ClientsResponse> => {
    const page = params?.page ?? 1;
    const pageSize = params?.pageSize ?? 10;
    const search = params?.search && params.search.trim() ? params.search.trim() : undefined;
    const status = params?.status && params.status !== 'all' ? params.status : undefined;
    const role = params?.role ?? 'client';

    try {
      const response = await apiClient.get<ClientsResponse>('/admin-dashboard/clients', {
        params: { page, pageSize, search, status, role },
      });
      return {
        ...response.data,
        clients: response.data.clients || response.data.users || [],
        users: response.data.clients || response.data.users || [],
      };
    } catch {
      // Fallback to /admin-dashboard/users if /admin-dashboard/clients fails
      const fallbackResponse = await apiClient.get<UsersResponse>('/admin-dashboard/users', {
        params: { page, pageSize, search, status, role },
      });
      return {
        ...fallbackResponse.data,
        clients: fallbackResponse.data.clients || fallbackResponse.data.users || [],
        users: fallbackResponse.data.clients || fallbackResponse.data.users || [],
      };
    }
  },
  getUsers: async (role?: 'client' | 'provider', search?: string): Promise<UsersResponse> => {
    return clientService.getClients({ role, search });
  },
};

export const userService = clientService;
