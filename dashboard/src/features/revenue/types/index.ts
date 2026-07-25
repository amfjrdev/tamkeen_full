export interface RevenueStat {
  id: number;
  label: string;
  value: string;
  trend: string;
  icon: 'dollar' | 'chart-up' | 'credit-card' | 'check-circle' | 'alert-circle' | 'clock' | string;
  color: string;
  negative?: boolean;
}

export interface GrowthItem {
  month: string;
  revenue: number;
  forecast: number;
}

export interface DailyRevenueItem {
  day: string;
  value: number;
}

export interface PaymentMethod {
  name: string;
  value: number;
  color: string;
}

export interface SalesTrendItem {
  month: string;
  starter: number;
  professional: number;
  premium: number;
  enterprise: number;
  [key: string]: number | string;
}

export interface TopProviderItem {
  rank: number;
  name: string;
  spent: number;
  credits: number;
  packages: number;
}

export interface Breakdown {
  starter: number;
  professional: number;
  premium: number;
  enterprise: number;
}

export interface Earnings {
  gross: number;
  fees: number;
  refunds: number;
  net: number;
}

export interface Profit {
  operating: number;
  marketing: number;
  other: number;
  net: number;
}

export interface RevenueData {
  stats: RevenueStat[];
  growthData: GrowthItem[];
  dailyRevenue: DailyRevenueItem[];
  paymentMethods: PaymentMethod[];
  salesTrend: SalesTrendItem[];
  topProviders: TopProviderItem[];
  breakdown: Breakdown;
  earnings: Earnings;
  profit: Profit;
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
