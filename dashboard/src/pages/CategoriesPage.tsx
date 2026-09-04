import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { CategoryCard } from '../features/categories/components/CategoryCard';
import { CategoryTable } from '../features/categories/components/CategoryTable';
import { categoryService } from '../features/categories/services/categoryService';
import type { CategoriesData } from '../features/categories/types';
import { Icons } from '../components/common/Icons';

interface CategoriesPageProps {
  onNavigate?: (label: string) => void;
}

export const CategoriesPage: React.FC<CategoriesPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<CategoriesData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Modals & Action States
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [newCatName, setNewCatName] = useState('');
  const [newCatDesc, setNewCatDesc] = useState('');
  const [isCreating, setIsCreating] = useState(false);

  const [deleteModal, setDeleteModal] = useState<{
    isOpen: boolean;
    id: string | number | null;
    name: string;
  }>({
    isOpen: false,
    id: null,
    name: '',
  });
  const [isDeleting, setIsDeleting] = useState(false);

  const loadCategoriesData = async () => {
    try {
      setLoading(true);
      setError(null);
      const result = await categoryService.getCategoriesData();
      setData(result);
    } catch (err) {
      console.error('Failed to load categories', err);
      setError(err instanceof Error ? err.message : 'Failed to retrieve service categories listings.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadCategoriesData();
  }, []);

  const handleRetry = () => {
    loadCategoriesData();
  };

  // Open Add Category Modal
  const handleOpenAddModal = () => {
    setNewCatName('');
    setNewCatDesc('');
    setIsAddModalOpen(true);
  };

  // Submit Add Category
  const handleCreateCategory = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newCatName.trim()) return;

    try {
      setIsCreating(true);
      await categoryService.createCategory({
        name: newCatName.trim(),
        description: newCatDesc.trim(),
      });
      setIsAddModalOpen(false);
      await loadCategoriesData();
    } catch (err) {
      console.error(err);
      alert(err instanceof Error ? err.message : 'Failed to create category');
    } finally {
      setIsCreating(false);
    }
  };

  // Trigger Delete Confirmation
  const handleDeleteClick = (id: string | number, name: string) => {
    setDeleteModal({
      isOpen: true,
      id,
      name,
    });
  };

  // Confirm and execute category deletion
  const handleConfirmDelete = async () => {
    if (!deleteModal.id) return;

    try {
      setIsDeleting(true);
      await categoryService.deleteCategory(deleteModal.id);
      setDeleteModal({ isOpen: false, id: null, name: '' });
      await loadCategoriesData();
    } catch (err) {
      console.error('Failed to delete category', err);
      alert(err instanceof Error ? err.message : 'Failed to delete category');
    } finally {
      setIsDeleting(false);
    }
  };

  if (loading && !data) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading service categories database...</p>
      </div>
    );
  }

  if (error && !data) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center p-6 text-center">
        <div className="w-16 h-16 rounded-2xl bg-red-500/10 border border-red-500/20 flex items-center justify-center text-red-500 mb-4 shadow-lg shadow-red-500/5">
          <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Categories</h3>
        <p className="text-gray-400 text-sm max-w-md mb-6">{error}</p>
        <button
          onClick={handleRetry}
          className="bg-indigo-600 hover:bg-indigo-700 active:scale-95 text-white font-semibold text-sm px-6 py-2.5 rounded-lg transition-all shadow-lg shadow-indigo-600/15"
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#0a0a0a] text-white">
      <Sidebar 
        navItems={data?.navItems || []} 
        isOpen={sidebarOpen} 
        onClose={() => setSidebarOpen(false)} 
        onNavigate={onNavigate}
      />

      <div className="lg:ml-64 transition-all duration-300">
        <Header 
          onMenuToggle={() => setSidebarOpen(!sidebarOpen)} 
          sidebarOpen={sidebarOpen}
        />

        <main className="p-4 lg:p-6 space-y-8">
          {/* Page Header */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
                Categories Management
              </h1>
              <p className="text-gray-400 text-sm">
                Organize, create, delete, and audit platform service categories
              </p>
            </div>
            
            <button 
              onClick={handleOpenAddModal}
              className="flex items-center justify-center gap-2 px-4.5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold active:scale-95 transition-all shadow-lg shadow-indigo-500/25 cursor-pointer"
            >
              <Icons.plus className="w-4 h-4" />
              Add Category
            </button>
          </div>

          {/* Summary Stats Grid */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {data?.summaryStats.map((stat, idx) => (
              <div key={idx} className="bg-[#111111] border border-gray-800 rounded-xl p-5 hover:bg-[#151515] transition-colors duration-200">
                <p className="text-gray-400 text-xs font-semibold uppercase tracking-wider mb-1.5">{stat.label}</p>
                <p className="text-white text-3xl font-extrabold tracking-tight font-mono">{stat.value}</p>
              </div>
            ))}
          </div>

          {/* Category Cards Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {data?.categories.map((cat) => (
              <CategoryCard 
                key={cat.id} 
                category={cat} 
                onDelete={handleDeleteClick}
                isDeleting={deleteModal.id === cat.id && isDeleting}
              />
            ))}
          </div>

          {/* Statistics Table */}
          <CategoryTable 
            categories={data?.categories || []} 
            onDelete={handleDeleteClick}
            deletingId={isDeleting ? deleteModal.id : null}
          />
        </main>
      </div>

      {/* Add Category Modal */}
      {isAddModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/70 backdrop-blur-sm animate-fadeIn">
          <div className="bg-[#141414] border border-gray-800 rounded-2xl w-full max-w-md p-6 shadow-2xl space-y-5">
            <div className="flex items-center justify-between border-b border-gray-800 pb-4">
              <h3 className="text-lg font-bold text-white">Add New Service Category</h3>
              <button
                onClick={() => setIsAddModalOpen(false)}
                className="text-gray-400 hover:text-white p-1 rounded-lg hover:bg-gray-800 transition-colors cursor-pointer"
              >
                ✕
              </button>
            </div>

            <form onSubmit={handleCreateCategory} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold text-gray-300 uppercase tracking-wider mb-1.5">
                  Category Name *
                </label>
                <input
                  type="text"
                  required
                  value={newCatName}
                  onChange={(e) => setNewCatName(e.target.value)}
                  placeholder="e.g. Electrician, Plumbing, Cleaning..."
                  className="w-full bg-gray-900 border border-gray-700 rounded-lg px-3.5 py-2.5 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                />
              </div>

              <div>
                <label className="block text-xs font-semibold text-gray-300 uppercase tracking-wider mb-1.5">
                  Description
                </label>
                <textarea
                  rows={3}
                  value={newCatDesc}
                  onChange={(e) => setNewCatDesc(e.target.value)}
                  placeholder="Optional brief description of services included in this category..."
                  className="w-full bg-gray-900 border border-gray-700 rounded-lg px-3.5 py-2.5 text-sm text-white placeholder-gray-500 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 resize-none"
                />
              </div>

              <div className="flex items-center justify-end gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setIsAddModalOpen(false)}
                  className="px-4 py-2 text-sm font-medium text-gray-400 hover:text-white bg-gray-850 hover:bg-gray-800 rounded-lg border border-gray-700 transition-colors cursor-pointer"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={isCreating || !newCatName.trim()}
                  className="px-5 py-2 text-sm font-semibold text-white bg-indigo-600 hover:bg-indigo-700 rounded-lg shadow-lg shadow-indigo-600/20 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer flex items-center gap-2"
                >
                  {isCreating ? (
                    <>
                      <div className="w-4 h-4 rounded-full border-2 border-white/20 border-t-white animate-spin" />
                      Creating...
                    </>
                  ) : (
                    'Create Category'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteModal.isOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-sm animate-fadeIn">
          <div className="bg-[#141414] border border-gray-800 rounded-2xl w-full max-w-md p-6 shadow-2xl space-y-4">
            <div className="w-12 h-12 rounded-full bg-rose-500/10 border border-rose-500/20 flex items-center justify-center text-rose-500 mx-auto">
              <Icons.trash className="w-6 h-6" />
            </div>

            <div className="text-center space-y-2">
              <h3 className="text-lg font-bold text-white">Delete Category</h3>
              <p className="text-gray-400 text-sm">
                Are you sure you want to delete <span className="text-white font-semibold">"{deleteModal.name}"</span>?
              </p>
              <p className="text-xs text-rose-400/90 bg-rose-500/10 border border-rose-500/20 rounded-lg p-2.5 mt-2 text-left">
                ⚠️ This category will be completely removed from provider registration options and from the mobile application category list.
              </p>
            </div>

            <div className="flex items-center justify-end gap-3 pt-4 border-t border-gray-800">
              <button
                type="button"
                onClick={() => setDeleteModal({ isOpen: false, id: null, name: '' })}
                disabled={isDeleting}
                className="px-4 py-2 text-sm font-medium text-gray-400 hover:text-white bg-gray-850 hover:bg-gray-800 rounded-lg border border-gray-700 transition-colors cursor-pointer"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleConfirmDelete}
                disabled={isDeleting}
                className="px-5 py-2 text-sm font-semibold text-white bg-rose-600 hover:bg-rose-700 rounded-lg shadow-lg shadow-rose-600/20 active:scale-95 transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer flex items-center gap-2"
              >
                {isDeleting ? (
                  <>
                    <div className="w-4 h-4 rounded-full border-2 border-white/20 border-t-white animate-spin" />
                    Deleting...
                  </>
                ) : (
                  'Yes, Delete Category'
                )}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
