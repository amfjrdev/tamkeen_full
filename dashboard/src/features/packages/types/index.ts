export interface PackageStat {
  label: string;
  value: string;
  icon: string;
  color: string;
}

export interface PackageItem {
  id: number;
  name: string;
  description: string;
  price: number;
  credits: number;
  perCredit: number;
  color: string;
  isPopular?: boolean;
  features: string[];
  sales: number;
  revenue: number;
  popularity: number;
  status: 'active' | 'inactive';
}

export interface PackageAnalytics {
  id: number;
  name: string;
  credits: number;
  price: number;
  sales: number;
  revenue: number;
  conversion: number;
  status: string;
  tag?: string;
}

export interface PackagesData {
  stats: PackageStat[];
  packages: PackageItem[];
  analytics: PackageAnalytics[];
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
