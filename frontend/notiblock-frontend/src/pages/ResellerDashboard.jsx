import { useState } from 'react';
import { FiPackage, FiFileText, FiCheckCircle } from 'react-icons/fi';
import LogoutButton from '../components/shared/LogoutButton';
import ProductsSection from '../components/dashboard/Reseller/ProductsSection';
import ConsumerReportsSection from '../components/dashboard/Reseller/ConsumerReportsSection';

export default function ResellerDashboard() {
  const [activeTab, setActiveTab] = useState('products');

  const tabs = [
    { id: 'products', label: 'Products', icon: FiPackage },
    { id: 'reports', label: 'Consumer Reports', icon: FiFileText },
    { id: 'tickets', label: 'My Tickets', icon: FiCheckCircle },
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="p-6">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-800">Reseller Dashboard</h1>
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
          {activeTab === 'products' && <ProductsSection />}
          {activeTab === 'reports' && <ConsumerReportsSection />}
          {activeTab === 'tickets' && (
            <div className="text-center py-12">
              <FiCheckCircle className="w-16 h-16 text-gray-400 mx-auto mb-4" />
              <h2 className="text-xl font-semibold text-gray-700 mb-2">My Tickets Section</h2>
              <p className="text-gray-500">Track your submitted tickets</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
