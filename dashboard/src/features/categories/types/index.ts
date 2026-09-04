export interface CategoryStat {
  label: string;
  value: string;
}

export interface Category {
  id: string | number;
  localId?: number;
  name: string;
  description?: string;
  icon: 'home' | 'wrench' | 'heart' | 'graduation' | 'car' | string;
  color: string;
  providers: number;
  requests: number;
  rating: number;
  completionRate: number;
  growth: string;
}

export interface CategoriesData {
  summaryStats: CategoryStat[];
  categories: Category[];
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
