import React, { useState } from 'react';
import { DashboardPage } from './pages/DashboardPage';
import { UsersPage } from './pages/UsersPage';
import { ProvidersPage } from './pages/ProvidersPage';
import { MessagingPage } from './pages/MessagingPage';
import { CategoriesPage } from './pages/CategoriesPage';
import { RevenuePage } from './pages/RevenuePage';
import { TransactionsPage } from './pages/TransactionsPage';
import { PackagesPage } from './pages/PackagesPage';
import { AnalyticsPage } from './pages/AnalyticsPage';

type ActivePage = 'Dashboard' | 'Users' | 'Providers' | 'Messaging' | 'Categories' | 'Revenue' | 'Transactions' | 'Packages' | 'Analytics' | string;

const App: React.FC = () => {
  const [activePage, setActivePage] = useState<ActivePage>('Dashboard');

  const handleNavigate = (pageLabel: string) => {
    setActivePage(pageLabel);
  };

  // Render active page dynamically
  switch (activePage) {
    case 'Dashboard':
      return <DashboardPage onNavigate={handleNavigate} />;
    case 'Users':
      return <UsersPage onNavigate={handleNavigate} />;
    case 'Providers':
      return <ProvidersPage onNavigate={handleNavigate} />;
    case 'Messaging':
      return <MessagingPage onNavigate={handleNavigate} />;
    case 'Categories':
      return <CategoriesPage onNavigate={handleNavigate} />;
    case 'Revenue':
      return <RevenuePage onNavigate={handleNavigate} />;
    case 'Transactions':
      return <TransactionsPage onNavigate={handleNavigate} />;
    case 'Packages':
      return <PackagesPage onNavigate={handleNavigate} />;
    case 'Analytics':
      return <AnalyticsPage onNavigate={handleNavigate} />;
    default:
      // Fallback placeholder for other pages that are not yet built
      return (
        <div className="min-h-screen bg-[#0a0a0a] text-white flex flex-col items-center justify-center p-6 text-center">
          <h1 className="text-3xl font-bold mb-4">{activePage} Page</h1>
          <p className="text-gray-400 max-w-md mb-8">
            This module is currently under construction as part of the step-by-step layout integration.
          </p>
          <button
            onClick={() => setActivePage('Dashboard')}
            className="bg-indigo-600 hover:bg-indigo-700 text-white font-semibold text-sm px-6 py-2.5 rounded-lg transition-colors"
          >
            Return to Dashboard
          </button>
        </div>
      );
  }
};

export default App;
