import { useState } from 'react'
import LogoutButton from '../components/shared/LogoutButton'
import NotificationDropdown from '../components/shared/NotificationDropdown'
import ProductsSection from '../components/dashboard/Manufacturer/ProductsSection'
import ApprovedTicketsSection from '../components/dashboard/Manufacturer/ApprovedTicketsSection'
import RecallsSection from '../components/dashboard/Manufacturer/RecallsSection'
import ProfileSettings from '../components/dashboard/shared/ProfileSettings'
import { FiPackage, FiCheckCircle, FiAlertTriangle, FiUser } from 'react-icons/fi'

export default function ManufacturerDashboard() {
  const [activeTab, setActiveTab] = useState('products')

  const tabs = [
    { id: 'products', label: 'Products', icon: FiPackage },
    { id: 'tickets', label: 'Approved Tickets', icon: FiCheckCircle },
    { id: 'recalls', label: 'My Recalls', icon: FiAlertTriangle },
    { id: 'profile', label: 'Profile', icon: FiUser },
  ]

  const renderContent = () => {
    switch (activeTab) {
      case 'products':
        return <ProductsSection />
      case 'tickets':
        return <ApprovedTicketsSection />
      case 'recalls':
        return <RecallsSection />
      case 'profile':
        return <ProfileSettings />
      default:
        return <ProductsSection />
    }
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-7xl mx-auto p-6">
        {/* Header */}
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-3xl font-bold text-gray-800">Manufacturer Dashboard</h1>
          <div className="flex items-center gap-3">
            <NotificationDropdown />
            <LogoutButton />
          </div>
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow mb-6">
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              {tabs.map((tab) => {
                const Icon = tab.icon
                return (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id)}
                    className={`flex items-center gap-2 px-6 py-4 border-b-2 font-medium text-sm transition-colors ${
                      activeTab === tab.id
                        ? 'border-blue-500 text-blue-600'
                        : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                    }`}
                  >
                    <Icon className="text-lg" />
                    {tab.label}
                  </button>
                )
              })}
            </nav>
          </div>
        </div>

        {/* Content */}
        <div className="bg-white rounded-lg shadow p-6">
          {renderContent()}
        </div>
      </div>
    </div>
  )
}
