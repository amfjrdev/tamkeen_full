export type ServiceRequestStatus = 
  | 'PendingAdminReview'
  | 'Approved'
  | 'Rejected'
  | 'ProviderSelected'
  | 'Completed'
  | 'Cancelled';

export interface ServiceRequestStat {
  label: string;
  value: string;
  icon: 'pending' | 'active' | 'rating' | 'revenue';
  color: 'orange' | 'green' | 'blue' | 'purple';
}

export interface ServiceRequestSummary {
  id: string;
  clientId: string;
  clientName: string;
  clientAvatarUrl?: string;
  categoryId: string;
  categoryName: string;
  title: string;
  description: string;
  wilaya: string;
  budget?: number;
  status: ServiceRequestStatus;
  rejectionReason?: string;
  selectedProviderId?: string;
  selectedProviderName?: string;
  applicationCount: number;
  createdAt: string;
  approvedAt?: string;
  completedAt?: string;
}

export interface ServiceRequestApplication {
  id: string;
  serviceRequestId: string;
  providerId: string;
  providerName: string;
  providerAvatarUrl?: string;
  providerRating: number;
  providerReviewCount: number;
  coverLetter: string;
  proposedPrice?: number;
  connectsSpent: number;
  status: string;
  createdAt: string;
}

export interface ServiceRequestReview {
  id: string;
  serviceRequestId: string;
  clientId: string;
  providerId: string;
  rating: number;
  comment: string;
  createdAt: string;
}

export interface ServiceRequestDetail extends ServiceRequestSummary {
  selectedApplicationId?: string;
  applications: ServiceRequestApplication[];
  review?: ServiceRequestReview;
}

export interface ServiceRequestsPagedResponse {
  items: ServiceRequestSummary[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
