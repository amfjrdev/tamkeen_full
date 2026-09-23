import apiClient from '../../../services/apiClient';
import type { 
  ServiceRequestsPagedResponse, 
  ServiceRequestDetail,
  ServiceRequestStatus 
} from '../types';

export const serviceRequestService = {
  getServiceRequests: async (params?: {
    status?: ServiceRequestStatus | string;
    wilaya?: string;
    search?: string;
    page?: number;
    pageSize?: number;
  }): Promise<ServiceRequestsPagedResponse> => {
    const response = await apiClient.get<ServiceRequestsPagedResponse>('/admin-dashboard/service-requests', {
      params: {
        status: params?.status && params.status !== 'all' ? params.status : undefined,
        wilaya: params?.wilaya && params.wilaya !== 'all' ? params.wilaya : undefined,
        search: params?.search ? params.search : undefined,
        page: params?.page || 1,
        pageSize: params?.pageSize || 20,
      },
    });
    return response.data;
  },

  getServiceRequestById: async (id: string): Promise<ServiceRequestDetail> => {
    const response = await apiClient.get<ServiceRequestDetail>(`/service-requests/${id}`);
    return response.data;
  },

  approveRequest: async (id: string): Promise<void> => {
    await apiClient.post(`/admin-dashboard/service-requests/${id}/approve`);
  },

  rejectRequest: async (id: string, reason?: string): Promise<void> => {
    await apiClient.post(`/admin-dashboard/service-requests/${id}/reject`, {
      reason: reason || 'Does not comply with community guidelines',
    });
  },
};
