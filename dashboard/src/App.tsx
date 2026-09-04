import React, { useState } from 'react';
import { LandingPage } from './pages/LandingPage';
import { LoginPage } from './pages/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { ClientsPage } from './pages/ClientsPage';
import { ProvidersPage } from './pages/ProvidersPage';
import { MessagingPage } from './pages/MessagingPage';
import { CategoriesPage } from './pages/CategoriesPage';
import { RevenuePage } from './pages/RevenuePage';
import { TransactionsPage } from './pages/TransactionsPage';
import { PackagesPage } from './pages/PackagesPage';
import { AnalyticsPage } from './pages/AnalyticsPage';

type ActivePage = 
  | 'Landing'
  | 'Login'
  | 'Dashboard'
  | 'Clients'
  | 'Users'
  | 'Providers'
  | 'Messaging'
  | 'Categories'
  | 'Revenue'
  | 'Transactions'
  | 'Packages'
  | 'Analytics'
  | string;

const App: React.FC = () => {
  const [activePage, setActivePage] = useState<ActivePage>('Landing');
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(() => {
    return localStorage.getItem('admin_token') === 'true';
  });

  const handleLogin = async (email: string, password: string): Promise<boolean> => {
    // Validate credentials (supports both production and testing credentials)
    if (
      (email === 'admin@tamkeendz.com' || email === 'admin@test.com') && 
      password === 'Admin123!'
    ) {
      localStorage.setItem('admin_token', 'true');
      setIsAuthenticated(true);
      setActivePage('Dashboard');
      return true;
    }
    return false;
  };

  const handleNavigate = (pageLabel: string) => {
    if (pageLabel === 'Logout') {
      localStorage.removeItem('admin_token');
      setIsAuthenticated(false);
      setActivePage('Landing');
    } else {
      setActivePage(pageLabel);
    }
  };

  // 1. Unprotected Public Pages
  if (activePage === 'Landing') {
    return <LandingPage onNavigate={handleNavigate} />;
  }

  if (activePage === 'Login') {
    return <LoginPage onLogin={handleLogin} onNavigate={handleNavigate} />;
  }

  // 2. Auth Guard for Administrative Pages
  if (!isAuthenticated) {
    return <LoginPage onLogin={handleLogin} onNavigate={handleNavigate} />;
  }

  // Render active admin page dynamically
  switch (activePage) {
    case 'Dashboard':
      return <DashboardPage onNavigate={handleNavigate} />;
    case 'Clients':
    case 'Users':
      return <ClientsPage onNavigate={handleNavigate} />;
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
      return (
        <div className="min-h-screen bg-[#0a0a0a] text-white flex flex-col items-center justify-center p-6 text-center">
          <h1 className="text-3xl font-bold mb-4">{activePage} Page</h1>
          <p className="text-gray-400 max-w-md mb-8">
            This module is currently under construction.
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
