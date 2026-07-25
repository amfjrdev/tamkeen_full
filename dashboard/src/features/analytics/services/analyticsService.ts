import apiClient from '../../../services/apiClient';
import type { AnalyticsData } from '../types';

/**
 * Backend Request Specification:
 * -------------------------------------------------------------
 * Endpoint: GET /api/analytics
 * Description: Retrieves consolidated platform analytics metrics, user growth trends, categories, activity patterns, and provider rankings.
 * Response Header: Content-Type: application/json
 * Response Body (JSON): AnalyticsData
 * -------------------------------------------------------------
 */

const mockAnalyticsData: AnalyticsData = {
  stats: [
    { label: 'Total Revenue', value: '$104K', trend: '+28.4%', trendLabel: 'from last month', icon: 'dollar', color: 'green', positive: true },
    { label: 'Active Users', value: '9,243', trend: '+15.2%', trendLabel: 'from last month', icon: 'users', color: 'blue', positive: true },
    { label: 'Avg Response Time', value: '2.4h', trend: '-12%', trendLabel: 'from last month', icon: 'clock', color: 'orange', positive: true },
    { label: 'Growth Rate', value: '34%', trend: '+8.3%', trendLabel: 'from last month', icon: 'trendingUp', color: 'purple', positive: true },
  ],
  growthTrends: [
    { month: 'Jan', users: 1200, providers: 300, requests: 1250 },
    { month: 'Feb', users: 1400, providers: 350, requests: 1450 },
    { month: 'Mar', users: 1700, providers: 420, requests: 1750 },
    { month: 'Apr', users: 2100, providers: 500, requests: 2200 },
    { month: 'May', users: 2500, providers: 600, requests: 2600 },
  ],
  categoryPerformance: [
    { name: 'Home Services', requests: 58000, revenue: 42000 },
    { name: 'Tech Support', requests: 32000, revenue: 28000 },
    { name: 'Healthcare', requests: 24000, revenue: 19000 },
    { name: 'Education', requests: 18000, revenue: 15000 },
    { name: 'Transportation', requests: 12000, revenue: 9000 },
  ],
  dailyActivity: [
    { time: '00:00', value: 120 },
    { time: '04:00', value: 80 },
    { time: '08:00', value: 450 },
    { time: '12:00', value: 700 },
    { time: '16:00', value: 750 },
    { time: '20:00', value: 550 },
    { time: '23:00', value: 280 },
  ],
  topProviders: [
    { rank: 1, name: 'Maria Garcia', completions: 203, rating: 4.9, revenue: 8120 },
    { rank: 2, name: 'John Smith', completions: 156, rating: 4.8, revenue: 6240 },
    { rank: 3, name: 'Rachel Brown', completions: 142, rating: 4.7, revenue: 5680 },
    { rank: 4, name: 'Tom Martinez', completions: 128, rating: 4.6, revenue: 5120 },
    { rank: 5, name: 'David Kim', completions: 115, rating: 4.5, revenue: 4600 },
  ],
  navItems: [
    { label: 'Dashboard', icon: 'dashboard', active: false },
    { label: 'Users', icon: 'users', active: false },
    { label: 'Providers', icon: 'providers', active: false },
    { label: 'Messaging', icon: 'messaging', active: false },
    { label: 'Categories', icon: 'categories', active: false },
    { label: 'Revenue', icon: 'revenue', active: false },
    { label: 'Transactions', icon: 'transactions', active: false },
    { label: 'Packages', icon: 'packages', active: false },
    { label: 'Analytics', icon: 'analytics', active: true },
    { label: 'Settings', icon: 'settings', active: false },
  ],
};

export const analyticsService = {
  getAnalyticsData: async (): Promise<AnalyticsData> => {
    try {
      const response = await apiClient.get<AnalyticsData>('/analytics');
      return response.data;
    } catch (error) {
      console.warn('Analytics API error, falling back to mock data:', error);
      // Simulate network delay
      await new Promise((resolve) => setTimeout(resolve, 500));
      return mockAnalyticsData;
    }
  },
};
