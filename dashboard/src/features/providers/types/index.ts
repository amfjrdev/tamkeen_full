export type ProviderStatus = 'approved' | 'pending' | 'rejected';

export interface ProviderStat {
  label: string;
  value: string;
  icon: 'pending' | 'active' | 'rating';
  color: 'orange' | 'green' | 'yellow';
}

export interface Provider {
  id: number;
  name: string;
  email: string;
  initials: string;
  verified: boolean;
  category: string;
  status: ProviderStatus;
  pendingSince?: string;
  rating: number | null;
  completed: number;
  credits: number;
  creditsTotal: number;
  totalSpent: number;
  lastSpent: string | null;
  connectPermission: boolean;
}

export interface ProvidersResponse {
  stats: ProviderStat[];
  providers: Provider[];
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
