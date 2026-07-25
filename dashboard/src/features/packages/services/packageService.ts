import apiClient from '../../../services/apiClient';
import type { PackagesData, PackageItem } from '../types';

/**
 * Backend Request Specification:
 * -------------------------------------------------------------
 * Endpoint: GET /api/packages
 * Description: Retrieves pricing tiers, stats summaries, sales details, conversion statistics, and active system package details.
 * Response Header: Content-Type: application/json
 * Response Body (JSON): PackagesData
 * -------------------------------------------------------------
 */

const mockPackagesData: PackagesData = {
  stats: [
    { label: 'Total Package Revenue', value: '1,454,000 DA', icon: 'dollar', color: 'blue' },
    { label: 'Active Packages', value: '3', icon: 'users', color: 'purple' },
    { label: 'Total Sales', value: '1,240', icon: 'trendingUp', color: 'pink' },
    { label: 'Avg Package Value', value: '1,733 DA', icon: 'trendingUp', color: 'green' },
  ],
  packages: [
    {
      id: 1,
      name: 'Starter Pack',
      description: 'Perfect for new providers getting started',
      price: 500,
      credits: 5,
      perCredit: 100.0,
      color: 'blue',
      features: ['5 Connect Credits', 'Valid for 30 days', 'Email support', 'Basic analytics'],
      sales: 640,
      revenue: 320000,
      popularity: 52,
      status: 'active',
    },
    {
      id: 2,
      name: 'Medium Pack',
      description: 'Most popular choice for active providers',
      price: 1200,
      credits: 15,
      perCredit: 80.0,
      color: 'indigo',
      isPopular: true,
      features: ['15 Connect Credits', 'Valid for 60 days', 'Priority support', 'Advanced analytics', 'Featured badge'],
      sales: 420,
      revenue: 504000,
      popularity: 34,
      status: 'active',
    },
    {
      id: 3,
      name: 'Premium Pack',
      description: 'For established providers with high demand',
      price: 3500,
      credits: 50,
      perCredit: 70.0,
      color: 'pink',
      features: ['50 Connect Credits', 'Valid for 90 days', '24/7 Priority support', 'Premium analytics', 'Featured badge', 'Profile boost'],
      sales: 180,
      revenue: 630000,
      popularity: 14,
      status: 'active',
    },
  ],
  analytics: [
    { id: 1, name: 'Starter Pack', credits: 5, price: 500, sales: 640, revenue: 320000, conversion: 52, status: 'active' },
    { id: 2, name: 'Medium Pack', credits: 15, price: 1200, sales: 420, revenue: 504000, conversion: 34, status: 'active', tag: 'Popular choice' },
    { id: 3, name: 'Premium Pack', credits: 50, price: 3500, sales: 180, revenue: 630000, conversion: 14, status: 'active' },
  ],
  navItems: [
    { label: 'Dashboard', icon: 'dashboard', active: false },
    { label: 'Users', icon: 'users', active: false },
    { label: 'Providers', icon: 'providers', active: false },
    { label: 'Messaging', icon: 'messaging', active: false },
    { label: 'Categories', icon: 'categories', active: false },
    { label: 'Revenue', icon: 'revenue', active: false },
    { label: 'Transactions', icon: 'transactions', active: false },
    { label: 'Packages', icon: 'packages', active: true },
    { label: 'Analytics', icon: 'analytics', active: false },
    { label: 'Settings', icon: 'settings', active: false },
  ],
};

export const packageService = {
  getPackagesData: async (): Promise<PackagesData> => {
    try {
      const response = await apiClient.get<PackagesData>('/packages');
      return response.data;
    } catch (error) {
      console.warn('Packages API error, falling back to mock data:', error);
      // Simulate network latency
      await new Promise((resolve) => setTimeout(resolve, 500));
      return mockPackagesData;
    }
  },

  createPackage: async (pkg: {
    name: string;
    description: string;
    price: number;
    credits: number;
    color: string;
    features: string[];
    isPopular: boolean;
    popularity: number;
  }): Promise<PackageItem> => {
    const response = await apiClient.post<PackageItem>('/packages', pkg);
    return response.data;
  },

  updatePackage: async (
    id: number,
    pkg: {
      name: string;
      description: string;
      price: number;
      credits: number;
      color: string;
      features: string[];
      isPopular: boolean;
      popularity: number;
      status: 'active' | 'inactive';
    }
  ): Promise<PackageItem> => {
    const response = await apiClient.put<PackageItem>(`/packages/${id}`, pkg);
    return response.data;
  },

  deletePackage: async (id: number): Promise<void> => {
    await apiClient.delete(`/packages/${id}`);
  },

  togglePackageStatus: async (id: number): Promise<{ status: 'active' | 'inactive' }> => {
    const response = await apiClient.post<{ status: 'active' | 'inactive' }>(`/packages/${id}/toggle-status`);
    return response.data;
  },
};
