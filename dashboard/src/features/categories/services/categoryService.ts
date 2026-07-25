import apiClient from '../../../services/apiClient';
import type { CategoriesData } from '../types';

export const categoryService = {
  getCategoriesData: async (): Promise<CategoriesData> => {
    const response = await apiClient.get<CategoriesData>('/admin-dashboard/categories');
    return response.data;
  },

  createCategory: async (category: { name: string; description: string }): Promise<void> => {
    await apiClient.post('/admin-dashboard/categories', category);
  },
};
