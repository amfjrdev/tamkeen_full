export interface StatItem {
  label: string;
  value: string;
  change: string;
  icon: 'revenue' | 'users' | 'providers' | 'messages';
  color: 'green' | 'blue' | 'purple' | 'pink';
}

export interface WeeklyActivity {
  labels: string[];
  users: number[];
  requests: number[];
  messages: number[];
}

export interface ServiceCategory {
  name: string;
  value: number;
  color: string;
}

export interface ConversionRateItem {
  month: string;
  value: number;
}

export interface RecentActivityItem {
  id: number;
  name: string;
  description: string;
  time: string;
  type: 'pending' | 'success' | 'info';
}

export interface NavItem {
  label: string;
  icon: string;
  active: boolean;
}

export interface DashboardData {
  stats: StatItem[];
  weeklyActivity: WeeklyActivity;
  serviceCategories: ServiceCategory[];
  conversionRate: ConversionRateItem[];
  recentActivity: RecentActivityItem[];
  navItems: NavItem[];
}
