import React, { useState, useEffect } from 'react';
import { Sidebar } from '../components/layout/Sidebar';
import { Header } from '../components/layout/Header';
import { Icons } from '../components/common/Icons';
import { packageService } from '../features/packages/services/packageService';
import { StatCard } from '../features/packages/components/StatCard';
import { PackageCard } from '../features/packages/components/PackageCard';
import { AnalyticsTable } from '../features/packages/components/AnalyticsTable';
import { BestPractices } from '../features/packages/components/BestPractices';
import type { PackagesData, PackageItem } from '../features/packages/types';

interface PackagesPageProps {
  onNavigate?: (label: string) => void;
}

export const PackagesPage: React.FC<PackagesPageProps> = ({ onNavigate }) => {
  const [data, setData] = useState<PackagesData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [sidebarOpen, setSidebarOpen] = useState(false);

  // Modal form states
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPackage, setEditingPackage] = useState<PackageItem | null>(null);
  
  // Form input fields
  const [formName, setFormName] = useState('');
  const [formDesc, setFormDesc] = useState('');
  const [formPrice, setFormPrice] = useState(99);
  const [formCredits, setFormCredits] = useState(30);
  const [formColor, setFormColor] = useState('blue');
  const [formFeatures, setFormFeatures] = useState('');
  const [formIsPopular, setFormIsPopular] = useState(false);
  const [formPopularity, setFormPopularity] = useState(10);

  useEffect(() => {
    const fetchPackages = async () => {
      try {
        setLoading(true);
        setError(null);
        const result = await packageService.getPackagesData();
        setData(result);
      } catch (err) {
        console.error('Failed to load packages:', err);
        setError(err instanceof Error ? err.message : 'Failed to fetch credit package templates.');
      } finally {
        setLoading(false);
      }
    };

    fetchPackages();
  }, []);

  const handleRetry = () => {
    setLoading(true);
    setError(null);
    packageService
      .getPackagesData()
      .then((result) => {
        setData(result);
        setLoading(false);
      })
      .catch((err) => {
        setError(err.message || 'Failed to fetch packages database.');
        setLoading(false);
      });
  };

  const handleToggleStatus = async (id: number) => {
    try {
      setLoading(true);
      await packageService.togglePackageStatus(id);
      const result = await packageService.getPackagesData();
      setData(result);
    } catch (err) {
      console.error('Failed to toggle package status:', err);
      alert(err instanceof Error ? err.message : 'Failed to toggle package status.');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this credit package?')) return;
    try {
      setLoading(true);
      await packageService.deletePackage(id);
      const result = await packageService.getPackagesData();
      setData(result);
    } catch (err) {
      console.error('Failed to delete package:', err);
      alert(err instanceof Error ? err.message : 'Failed to delete package.');
    } finally {
      setLoading(false);
    }
  };

  const openCreateModal = () => {
    setEditingPackage(null);
    setFormName('');
    setFormDesc('');
    setFormPrice(99);
    setFormCredits(30);
    setFormColor('blue');
    setFormFeatures('30 Connect Credits, Valid for 30 days, Email support');
    setFormIsPopular(false);
    setFormPopularity(10);
    setIsModalOpen(true);
  };

  const openEditModal = (pkg: PackageItem) => {
    setEditingPackage(pkg);
    setFormName(pkg.name);
    setFormDesc(pkg.description);
    setFormPrice(pkg.price);
    setFormCredits(pkg.credits);
    setFormColor(pkg.color);
    setFormFeatures(pkg.features.join(', '));
    setFormIsPopular(!!pkg.isPopular);
    setFormPopularity(pkg.popularity);
    setIsModalOpen(true);
  };

  const handleSavePackage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!formName.trim() || !formDesc.trim()) {
      alert('Please fill out the name and description fields.');
      return;
    }

    const price = Number(formPrice) || 0;
    const credits = Number(formCredits) || 1;
    const features = formFeatures.split(',').map((f) => f.trim()).filter((f) => f !== '');

    try {
      setLoading(true);
      if (editingPackage) {
        // Edit existing package
        await packageService.updatePackage(editingPackage.id, {
          name: formName.trim(),
          description: formDesc.trim(),
          price,
          credits,
          color: formColor,
          features,
          isPopular: formIsPopular,
          popularity: formPopularity,
          status: editingPackage.status,
        });
      } else {
        // Create new package
        await packageService.createPackage({
          name: formName.trim(),
          description: formDesc.trim(),
          price,
          credits,
          color: formColor,
          features,
          isPopular: formIsPopular,
          popularity: formPopularity,
        });
      }
      setIsModalOpen(false);
      const result = await packageService.getPackagesData();
      setData(result);
    } catch (err) {
      console.error('Failed to save package:', err);
      alert(err instanceof Error ? err.message : 'Failed to save package.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center gap-4">
        <div className="relative w-16 h-16">
          <div className="absolute inset-0 rounded-full border-4 border-indigo-500/20" />
          <div className="absolute inset-0 rounded-full border-4 border-t-indigo-600 animate-spin" />
        </div>
        <p className="text-gray-400 text-sm font-medium animate-pulse">Loading pricing tiers...</p>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="min-h-screen bg-[#0a0a0a] flex flex-col items-center justify-center p-6 text-center">
        <div className="w-16 h-16 rounded-2xl bg-red-500/10 border border-red-500/20 flex items-center justify-center text-red-500 mb-4 shadow-lg shadow-red-500/5">
          <svg className="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
          </svg>
        </div>
        <h3 className="text-white text-lg font-semibold mb-2">Error Loading Packages</h3>
        <p className="text-gray-400 text-sm max-w-md mb-6">{error}</p>
        <button
          onClick={handleRetry}
          className="bg-indigo-600 hover:bg-indigo-700 active:scale-95 text-white font-semibold text-sm px-6 py-2.5 rounded-lg transition-all shadow-lg shadow-indigo-600/15 cursor-pointer"
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#0a0a0a] text-white">
      <Sidebar
        navItems={data.navItems}
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
          {/* Header Panel */}
          <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl lg:text-3xl font-bold tracking-tight text-white mb-1">
                Connect Packages Management
              </h1>
              <p className="text-gray-400 text-sm">
                Create pricing tiers, allocate connect credits, and track sales revenue logs
              </p>
            </div>
            
            <button
              onClick={openCreateModal}
              className="flex items-center justify-center gap-2 px-4.5 py-2.5 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-sm font-semibold active:scale-95 transition-all shadow-lg shadow-indigo-500/25 cursor-pointer"
            >
              <Icons.plus className="w-4 h-4" />
              Create Package
            </button>
          </div>

          {/* Stats Summaries */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            {data.stats.map((stat, idx) => (
              <StatCard key={idx} stat={stat} />
            ))}
          </div>

          {/* Packages Card Grids */}
          <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-6">
            {data.packages.map((pkg) => (
              <PackageCard
                key={pkg.id}
                pkg={pkg}
                onEdit={openEditModal}
                onDelete={handleDelete}
                onToggleStatus={handleToggleStatus}
              />
            ))}
          </div>

          {/* Analytics Ledger */}
          <AnalyticsTable analyticsData={data.analytics} />

          {/* Best Practices Section */}
          <BestPractices />
        </main>
      </div>

      {/* Pricing Form Dialog Modals */}
      {isModalOpen && (
        <>
          <div
            className="fixed inset-0 bg-black/60 backdrop-blur-sm z-50 transition-opacity"
            onClick={() => setIsModalOpen(false)}
          />
          <div className="fixed inset-0 flex items-center justify-center p-4 z-50 pointer-events-none">
            <form
              onSubmit={handleSavePackage}
              className="bg-[#111111] border border-gray-800 w-full max-w-lg rounded-xl shadow-2xl p-6 relative pointer-events-auto max-h-[90vh] overflow-y-auto"
            >
              <button
                type="button"
                onClick={() => setIsModalOpen(false)}
                className="absolute top-4 right-4 text-gray-500 hover:text-white transition-colors cursor-pointer"
              >
                <Icons.close className="w-5 h-5" />
              </button>

              <h3 className="text-white text-lg font-bold mb-4 tracking-tight border-b border-gray-800 pb-2">
                {editingPackage ? 'Modify Pricing Package' : 'Create Pricing Package'}
              </h3>

              <div className="space-y-4">
                <div>
                  <label className="block text-gray-400 text-xs font-semibold uppercase mb-1">
                    Package Name
                  </label>
                  <input
                    type="text"
                    value={formName}
                    onChange={(e) => setFormName(e.target.value)}
                    placeholder="e.g. Starter Package"
                    className="w-full bg-gray-900 border border-gray-800 rounded-lg px-3.5 py-2 text-sm text-white placeholder-gray-600 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                    required
                  />
                </div>

                <div>
                  <label className="block text-gray-400 text-xs font-semibold uppercase mb-1">
                    Description
                  </label>
                  <textarea
                    value={formDesc}
                    onChange={(e) => setFormDesc(e.target.value)}
                    placeholder="Provide a description of the target audience or tier value"
                    rows={2}
                    className="w-full bg-gray-900 border border-gray-800 rounded-lg px-3.5 py-2 text-sm text-white placeholder-gray-600 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                    required
                  />
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-gray-400 text-xs font-semibold uppercase mb-1">
                      Price (DA)
                    </label>
                    <input
                      type="number"
                      value={formPrice}
                      onChange={(e) => setFormPrice(Number(e.target.value))}
                      className="w-full bg-gray-900 border border-gray-800 rounded-lg px-3.5 py-2 text-sm text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                      min={0}
                      required
                    />
                  </div>
                  <div>
                    <label className="block text-gray-400 text-xs font-semibold uppercase mb-1">
                      Connect Credits
                    </label>
                    <input
                      type="number"
                      value={formCredits}
                      onChange={(e) => setFormCredits(Number(e.target.value))}
                      className="w-full bg-gray-900 border border-gray-800 rounded-lg px-3.5 py-2 text-sm text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                      min={1}
                      required
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-gray-400 text-xs font-semibold uppercase mb-1">
                      Theme Color
                    </label>
                    <select
                      value={formColor}
                      onChange={(e) => setFormColor(e.target.value)}
                      className="w-full bg-gray-900 border border-gray-800 rounded-lg px-3.5 py-2 text-sm text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 cursor-pointer"
                    >
                      <option value="blue">Blue</option>
                      <option value="indigo">Indigo</option>
                      <option value="pink">Pink</option>
                      <option value="orange">Orange</option>
                      <option value="gray">Gray</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-gray-400 text-xs font-semibold uppercase mb-1">
                      Popularity Rating (%)
                    </label>
                    <input
                      type="number"
                      value={formPopularity}
                      onChange={(e) => setFormPopularity(Number(e.target.value))}
                      className="w-full bg-gray-900 border border-gray-800 rounded-lg px-3.5 py-2 text-sm text-white focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                      min={0}
                      max={100}
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-gray-400 text-xs font-semibold uppercase mb-1">
                    Features (comma separated)
                  </label>
                  <input
                    type="text"
                    value={formFeatures}
                    onChange={(e) => setFormFeatures(e.target.value)}
                    placeholder="30 Connect Credits, Valid for 30 days, Priority support"
                    className="w-full bg-gray-900 border border-gray-800 rounded-lg px-3.5 py-2 text-sm text-white placeholder-gray-600 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500"
                  />
                </div>

                <div className="flex items-center gap-2 pt-2 cursor-pointer select-none">
                  <input
                    type="checkbox"
                    id="isPopular"
                    checked={formIsPopular}
                    onChange={(e) => setFormIsPopular(e.target.checked)}
                    className="rounded border-gray-800 text-indigo-600 focus:ring-indigo-500 h-4 w-4 bg-gray-900 cursor-pointer"
                  />
                  <label htmlFor="isPopular" className="text-gray-300 text-sm font-semibold cursor-pointer">
                    Highlight as &quot;Most Popular&quot; package
                  </label>
                </div>
              </div>

              <div className="pt-6 flex justify-end gap-3 border-t border-gray-800 mt-6">
                <button
                  type="button"
                  onClick={() => setIsModalOpen(false)}
                  className="px-4 py-2 border border-gray-700 hover:bg-gray-800 text-white rounded-lg text-xs font-semibold cursor-pointer transition-all"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-lg text-xs font-semibold cursor-pointer transition-all shadow-md shadow-indigo-600/20"
                >
                  Save Changes
                </button>
              </div>
            </form>
          </div>
        </>
      )}
    </div>
  );
};
