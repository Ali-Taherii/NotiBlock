import { useState, useEffect } from 'react';
import {
  FiAlertTriangle,
  FiCheckCircle,
  FiXCircle,
  FiClock,
  FiRefreshCcw,
  FiBarChart2,
  FiClipboard,
  FiFilter
} from 'react-icons/fi';
import { getMyRecalls } from '../../../api/recalls';
import RecallUpdateRequestModal from './RecallUpdateRequestModal';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function RecallsSection() {
  const [recalls, setRecalls] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showUpdateModal, setShowUpdateModal] = useState(false);
  const [activeRecall, setActiveRecall] = useState(null);
  const [statusFilter, setStatusFilter] = useState('all');
  const { toast, success, error, hideToast } = useToast();

  useEffect(() => {
    fetchRecalls();
  }, []);

  const fetchRecalls = async () => {
    try {
      setLoading(true);
      const response = await getMyRecalls();
      console.log('Recalls response:', response); // Debug log
      
      // The interceptor already unwraps to response.data
      // Backend returns: { success: true, data: [...recalls], message: "..." }
      const recallsData = response?.data || response || [];
      setRecalls(Array.isArray(recallsData) ? recallsData : []);
    } catch (err) {
      console.error('Error fetching recalls:', err);
      error('Failed to load recalls');
      setRecalls([]);
    } finally {
      setLoading(false);
    }
  };

  const getStatusBadge = (status) => {
    const statusConfig = {
      0: { color: 'bg-orange-100 text-orange-800', icon: FiAlertTriangle, label: 'Active' },
      1: { color: 'bg-green-100 text-green-800', icon: FiCheckCircle, label: 'Resolved' },
      2: { color: 'bg-gray-100 text-gray-800', icon: FiXCircle, label: 'Cancelled' },
      3: { color: 'bg-yellow-100 text-yellow-800', icon: FiClock, label: 'Pending Approval' },
      4: { color: 'bg-red-100 text-red-800', icon: FiXCircle, label: 'Rejected' },
    };

    const config = statusConfig[status] || statusConfig[0];
    const Icon = config.icon;

    return (
      <span className={`inline-flex items-center gap-1 px-2 py-1 rounded text-xs font-medium ${config.color}`}>
        <Icon className="text-sm" />
        {config.label}
      </span>
    );
  };

  const canSubmitUpdate = (recall) => {
    return recall.status !== 3 && recall.status !== 4;
  };

  const openUpdateModal = (recall) => {
    setActiveRecall(recall);
    setShowUpdateModal(true);
  };

  const closeUpdateModal = () => {
    setActiveRecall(null);
    setShowUpdateModal(false);
  };

  const handleUpdateSubmitted = () => {
    success('Update request submitted for regulator review.');
    closeUpdateModal();
    fetchRecalls();
  };

  const statusCounts = recalls.reduce((acc, recall) => {
    const key = recall.status;
    acc[key] = (acc[key] || 0) + 1;
    return acc;
  }, {});

  const pendingUpdateTotal = recalls.reduce(
    (acc, recall) => acc + (recall.pendingUpdateRequestCount || 0),
    0
  );

  const summaryCards = [
    { label: 'Total Recalls', value: recalls.length, icon: FiBarChart2, accent: 'bg-blue-50 text-blue-600' },
    { label: 'Pending Approval', value: statusCounts[3] || 0, icon: FiClock, accent: 'bg-yellow-50 text-yellow-700' },
    { label: 'Active', value: statusCounts[0] || 0, icon: FiAlertTriangle, accent: 'bg-orange-50 text-orange-700' },
    { label: 'Resolved', value: statusCounts[1] || 0, icon: FiCheckCircle, accent: 'bg-green-50 text-green-700' },
    { label: 'Pending Update Requests', value: pendingUpdateTotal, icon: FiClipboard, accent: 'bg-purple-50 text-purple-700' }
  ];

  const statusFiltersConfig = [
    { value: 'all', label: 'All', statuses: null },
    { value: 'pending', label: 'Pending Approval', statuses: [3] },
    { value: 'active', label: 'Active', statuses: [0] },
    { value: 'resolved', label: 'Resolved', statuses: [1] },
    { value: 'cancelled', label: 'Cancelled', statuses: [2] },
    { value: 'rejected', label: 'Rejected', statuses: [4] }
  ];

  const activeFilter = statusFiltersConfig.find((filter) => filter.value === statusFilter);

  const filteredRecalls = !activeFilter?.statuses
    ? recalls
    : recalls.filter((recall) => activeFilter.statuses?.includes(recall.status));

  const getFilterCount = (filter) => {
    if (!filter.statuses) {
      return recalls.length;
    }

    return filter.statuses.reduce((sum, status) => sum + (statusCounts[status] || 0), 0);
  };

  const hasRecalls = recalls.length > 0;
  const hasFilteredResults = filteredRecalls.length > 0;

  if (loading) {
    return <div className="text-center py-8">Loading recalls...</div>;
  }

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      {showUpdateModal && activeRecall && (
        <RecallUpdateRequestModal
          recall={activeRecall}
          onClose={closeUpdateModal}
          onSuccess={handleUpdateSubmitted}
        />
      )}
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">My Recalls</h2>
        <button
          onClick={fetchRecalls}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          <FiRefreshCcw className="text-sm" />
          Refresh
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-5 mb-6">
        {summaryCards.map((card) => {
          const Icon = card.icon;
          return (
            <div key={card.label} className="bg-white border border-gray-100 rounded-lg p-4 shadow-sm">
              <div className="flex items-center justify-between">
                <div>
                  <p className="text-sm text-gray-500">{card.label}</p>
                  <p className="text-2xl font-semibold text-gray-900 mt-1">{card.value}</p>
                </div>
                <span className={`p-3 rounded-full ${card.accent}`}>
                  <Icon className="text-lg" />
                </span>
              </div>
            </div>
          );
        })}
      </div>

      {hasRecalls && (
        <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between mb-6">
          <div className="flex items-center gap-2 text-sm text-gray-600">
            <FiFilter className="text-gray-400" />
            <span>Filter by status</span>
          </div>
          <div className="flex flex-wrap gap-2">
            {statusFiltersConfig.map((filter) => {
              const count = getFilterCount(filter);
              const isActive = statusFilter === filter.value;
              return (
                <button
                  key={filter.value}
                  onClick={() => setStatusFilter(filter.value)}
                  className={`text-sm px-3 py-1.5 rounded-full border transition-colors ${
                    isActive
                      ? 'border-blue-500 bg-blue-50 text-blue-700'
                      : 'border-gray-200 bg-white text-gray-600 hover:border-blue-300'
                  }`}
                >
                  {filter.label} ({count})
                </button>
              );
            })}
          </div>
        </div>
      )}

      {!hasRecalls ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg border border-dashed border-gray-300">
          <FiAlertTriangle className="mx-auto text-gray-400 text-5xl mb-3" />
          <p className="text-gray-600">No recalls issued yet.</p>
          <p className="text-sm text-gray-500 mt-1">Go to Approved Tickets to issue recalls.</p>
        </div>
      ) : !hasFilteredResults ? (
        <div className="text-center py-10 bg-white border border-dashed border-gray-200 rounded-lg text-gray-600">
          No recalls match "{activeFilter?.label}" right now.
        </div>
      ) : (
        <div className="space-y-4">
          {filteredRecalls.map((recall) => (
            <div
              key={recall.id}
              className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="font-semibold text-lg">Product: {recall.productId}</h3>
                    {getStatusBadge(recall.status)}
                    {recall.pendingUpdateRequestCount > 0 && (
                      <span className="text-xs px-2 py-1 rounded bg-blue-100 text-blue-800">
                        {recall.pendingUpdateRequestCount} update request(s)
                      </span>
                    )}
                  </div>
                  
                  <div className="space-y-2 text-sm">
                    <p className="text-gray-600"><strong>Reason:</strong></p>
                    <p className="text-gray-800">{recall.reason}</p>
                    
                    <p className="text-gray-600 mt-2"><strong>Action Required:</strong></p>
                    <p className="text-gray-800">{recall.actionRequired}</p>
                  </div>

                  <div className="flex gap-4 text-sm text-gray-600 mt-3">
                    <span><strong>Issued:</strong> {new Date(recall.issuedAt).toLocaleDateString()}</span>
                    {recall.resolvedAt && (
                      <span><strong>Resolved:</strong> {new Date(recall.resolvedAt).toLocaleDateString()}</span>
                    )}
                  </div>

                  <div className="grid gap-3 md:grid-cols-3 text-sm mt-4">
                    <div className="bg-gray-50 p-3 rounded border border-gray-100">
                      <p className="text-gray-500 uppercase text-xs tracking-wide">Regulator Decision</p>
                      <p className="text-gray-900 font-medium mt-1">
                        {recall.status === 3 && 'Pending review'}
                        {recall.status === 4 && 'Rejected'}
                        {recall.status === 0 && 'Active on chain'}
                        {recall.status === 1 && 'Resolved'}
                        {recall.status === 2 && 'Cancelled'}
                      </p>
                      {recall.regulatorNotes && (
                        <p className="text-gray-600 text-xs mt-1">{recall.regulatorNotes}</p>
                      )}
                    </div>
                    <div className="bg-gray-50 p-3 rounded border border-gray-100">
                      <p className="text-gray-500 uppercase text-xs tracking-wide">Blockchain</p>
                      {recall.transactionHash ? (
                        <p className="text-gray-900 font-mono text-xs break-all mt-1">
                          {recall.transactionHash}
                        </p>
                      ) : (
                        <p className="text-gray-700 mt-1">Awaiting regulator activation</p>
                      )}
                    </div>
                    <div className="bg-gray-50 p-3 rounded border border-gray-100">
                      <p className="text-gray-500 uppercase text-xs tracking-wide">Last Updated</p>
                      <p className="text-gray-900 font-medium mt-1">
                        {new Date(recall.lastUpdatedAt || recall.createdAt).toLocaleString()}
                      </p>
                    </div>
                  </div>
                </div>
                <div className="flex flex-col items-end gap-3 ml-4">
                  {canSubmitUpdate(recall) ? (
                    <button
                      onClick={() => openUpdateModal(recall)}
                      className="px-4 py-2 text-sm bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
                      disabled={recall.pendingUpdateRequestCount > 0}
                    >
                      {recall.pendingUpdateRequestCount > 0 ? 'Awaiting Regulator Review' : 'Propose Update'}
                    </button>
                  ) : (
                    <span className="text-xs text-gray-500 text-center">Updates available once recall is approved.</span>
                  )}
                  {recall.approvedAt && (
                    <p className="text-xs text-gray-500">
                      Approved on {new Date(recall.approvedAt).toLocaleDateString()}
                    </p>
                  )}
                  {recall.rejectedAt && (
                    <p className="text-xs text-red-500">
                      Rejected on {new Date(recall.rejectedAt).toLocaleDateString()}
                    </p>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
