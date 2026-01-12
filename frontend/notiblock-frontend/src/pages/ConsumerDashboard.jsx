import { useState } from 'react';
import { FiPackage, FiFileText, FiAlertTriangle } from 'react-icons/fi';
import LogoutButton from '../components/shared/LogoutButton';
import MyProductsSection from '../components/dashboard/Consumer/MyProductsSection';
import MyReportsSection from '../components/dashboard/Consumer/MyReportsSection';
import ActiveRecallsSection from '../components/dashboard/Consumer/ActiveRecallsSection';

export default function ConsumerDashboard() {
  const [activeTab, setActiveTab] = useState('products');

  const tabs = [
    { id: 'products', label: 'My Products', icon: FiPackage },
    { id: 'reports', label: 'My Reports', icon: FiFileText },
    { id: 'recalls', label: 'Active Recalls', icon: FiAlertTriangle },
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="p-6">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-800">Consumer Dashboard</h1>
          <LogoutButton />
        </div>

        {/* Tab Navigation */}
        <div className="bg-white rounded-lg shadow-sm mb-6">
          <div className="flex border-b border-gray-200">
            {tabs.map((tab) => {
              const Icon = tab.icon;
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`flex items-center gap-2 px-6 py-4 font-medium transition-colors ${
                    activeTab === tab.id
                      ? 'border-b-2 border-blue-600 text-blue-600'
                      : 'text-gray-600 hover:text-gray-800'
                  }`}
                >
                  <Icon className="w-5 h-5" />
                  {tab.label}
                </button>
              );
            })}
          </div>
        </div>

        {/* Tab Content */}
        <div className="bg-white rounded-lg shadow-sm p-6">
          {activeTab === 'products' && <MyProductsSection />}
          {activeTab === 'reports' && <MyReportsSection />}
          {activeTab === 'recalls' && <ActiveRecallsSection />}
        </div>
      </div>
    </div>
  );
}