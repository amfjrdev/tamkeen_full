export interface AnalyticsStat {
  label: string;
  value: string;
  trend: string;
  trendLabel: string;
  icon: string;
  color: string;
  positive: boolean;
}

export interface GrowthTrend {
  month: string;
  users: number;
  providers: number;
  requests: number;
}

export interface CategoryPerformance {
  name: string;
  requests: number;
  revenue: number;
}

export interface DailyActivity {
  time: string;
  value: number;
}

export interface TopProvider {
  rank: number;
  name: string;
  completions: number;
  rating: number;
  revenue: number;
}

export interface AnalyticsData {
  stats: AnalyticsStat[];
  growthTrends: GrowthTrend[];
  categoryPerformance: CategoryPerformance[];
  dailyActivity: DailyActivity[];
  topProviders: TopProvider[];
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
