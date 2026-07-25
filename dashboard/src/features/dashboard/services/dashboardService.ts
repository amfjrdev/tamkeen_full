import apiClient from '../../../services/apiClient';
import type { DashboardData } from '../types';

/**
 * Backend Request Specification:
 * -------------------------------------------------------------
 * Endpoint: GET /api/dashboard
 * Description: Retrieves consolidated dashboard statistics, weekly activity, service categories distribution, connection conversion rates, and recent activity.
 * Query Parameters: None
 * Response Header: Content-Type: application/json
 * Response Body (JSON): DashboardData
 * -------------------------------------------------------------
 */

// Mock Data for fallback / local development
const mockDashboardData: DashboardData = {
  stats: [
    { label: 'Total Revenue', value: '$487K', change: '+28.4%', icon: 'revenue', color: 'green' },
    { label: 'Total Users', value: '12,543', change: '+12.5%', icon: 'users', color: 'blue' },
    { label: 'Active Providers', value: '3,284', change: '+8.2%', icon: 'providers', color: 'purple' },
    { label: 'Messages Today', value: '24,891', change: '+23.1%', icon: 'messages', color: 'pink' },
  ],
  weeklyActivity: {
    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
    users: [3800, 3600, 5200, 4800, 6200, 5000, 3700],
    requests: [3200, 3400, 4800, 4500, 5800, 4600, 3500],
    messages: [4000, 3700, 5400, 4900, 6400, 5100, 3800],
  },
  serviceCategories: [
    { name: 'Home Services', value: 35, color: '#6366f1' },
    { name: 'Tech Support', value: 25, color: '#8b5cf6' },
    { name: 'Healthcare', value: 20, color: '#ec4899' },
    { name: 'Education', value: 12, color: '#f59e0b' },
    { name: 'Other', value: 8, color: '#34d399' },
  ],
  conversionRate: [
    { month: 'Jan', value: 70 },
    { month: 'Feb', value: 75 },
    { month: 'Mar', value: 78 },
    { month: 'Apr', value: 74 },
    { month: 'May', value: 82 },
  ],
  recentActivity: [
    { id: 1, name: 'Sarah Johnson', description: 'New provider registered', time: '2 min ago', type: 'pending' },
    { id: 2, name: 'Mike Chen', description: 'Service request completed', time: '12 min ago', type: 'success' },
    { id: 3, name: 'Emily Davis', description: 'Message sent to provider', time: '23 min ago', type: 'info' },
    { id: 4, name: 'James Wilson', description: 'Account verification pending', time: '45 min ago', type: 'pending' },
    { id: 5, name: 'Lisa Anderson', description: 'New client registered', time: '1 hour ago', type: 'success' },
  ],
  navItems: [
    { label: 'Dashboard', icon: 'dashboard', active: true },
    { label: 'Users', icon: 'users', active: false },
    { label: 'Providers', icon: 'providers', active: false },
    { label: 'Messaging', icon: 'messaging', active: false },
    { label: 'Categories', icon: 'categories', active: false },
    { label: 'Revenue', icon: 'revenue', active: false },
    { label: 'Transactions', icon: 'transactions', active: false },
    { label: 'Packages', icon: 'packages', active: false },
    { label: 'Analytics', icon: 'analytics', active: false },
    { label: 'Settings', icon: 'settings', active: false },
  ],
};

export const dashboardService = {
  getDashboardData: async (): Promise<DashboardData> => {
    try {
      const response = await apiClient.get<DashboardData>('/dashboard');
      return response.data;
    } catch (error) {
      console.warn('Dashboard API error, falling back to mock data:', error);
      // Simulate network delay for mock data
      await new Promise((resolve) => setTimeout(resolve, 300));
      return mockDashboardData;
    }
  },
};
