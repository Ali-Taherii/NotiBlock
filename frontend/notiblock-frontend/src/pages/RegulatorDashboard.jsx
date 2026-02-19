import { useState } from 'react';
import { FiFileText, FiClipboard, FiShield, FiShuffle } from 'react-icons/fi';
import LogoutButton from '../components/shared/LogoutButton';
import NotificationDropdown from '../components/shared/NotificationDropdown';
import TicketsSection from '../components/dashboard/Regulator/TicketsSection';
import MyReviewsSection from '../components/dashboard/Regulator/MyReviewsSection';
import RecallApprovalsSection from '../components/dashboard/Regulator/RecallApprovalsSection';
import RecallUpdateRequestsSection from '../components/dashboard/Regulator/RecallUpdateRequestsSection';

export default function RegulatorDashboard() {
  const [activeTab, setActiveTab] = useState('tickets');

  const tabs = [
    { id: 'tickets', label: 'All Tickets', icon: FiFileText },
    { id: 'recall-approvals', label: 'Recall Approvals', icon: FiShield },
    { id: 'recall-updates', label: 'Recall Updates', icon: FiShuffle },
    { id: 'reviews', label: 'My Reviews', icon: FiClipboard },
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="p-6">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-800">Regulator Dashboard</h1>
          <div className="flex items-center gap-3">
            <NotificationDropdown />
            <LogoutButton />
          </div>
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
          {activeTab === 'tickets' && <TicketsSection />}
          {activeTab === 'recall-approvals' && <RecallApprovalsSection />}
          {activeTab === 'recall-updates' && <RecallUpdateRequestsSection />}
          {activeTab === 'reviews' && <MyReviewsSection />}
        </div>
      </div>
    </div>
  );
}
