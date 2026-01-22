import { useState, useEffect } from 'react';
import { getMyRecalls, updateRecall, deleteRecall, updateBlockchainStatus } from '../../../api/recalls';
import { FiAlertTriangle, FiEdit2, FiTrash2, FiCheckCircle, FiXCircle } from 'react-icons/fi';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function RecallsSection() {
  const [recalls, setRecalls] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingRecall, setEditingRecall] = useState(null);
  const [editForm, setEditForm] = useState({
    reason: '',
    actionRequired: '',
    status: '',
  });
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

  const handleEdit = (recall) => {
    setEditingRecall(recall);
    setEditForm({
      reason: recall.reason,
      actionRequired: recall.actionRequired,
      status: recall.status,
    });
  };

  const handleUpdate = async (e) => {
    e.preventDefault();
    
    try {
      // Convert status string to number (0, 1, 2)
      const statusValue = typeof editForm.status === 'string' ? 
        parseInt(editForm.status) : editForm.status;
      
      const updatePayload = {
        reason: editForm.reason,
        actionRequired: editForm.actionRequired,
        status: statusValue  // This should be a number (RecallStatus enum)
      };
      
      await updateRecall(editingRecall.id, updatePayload);
      success('Recall updated successfully!');
      setEditingRecall(null);
      fetchRecalls();
    } catch (err) {
      console.error('Error updating recall:', err);
      error(err.response?.data?.message || 'Failed to update recall');
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this recall?')) return;

    try {
      await deleteRecall(id);
      success('Recall deleted successfully!');
      fetchRecalls();
    } catch (err) {
      console.error('Error deleting recall:', err);
      error('Failed to delete recall');
    }
  };

  const handleUpdateBlockchainStatus = async (recall) => {
    if (!window.confirm(`Update recall status to "${getStatusLabel(recall.status)}" on blockchain?`)) return;

    try {
      const statusLabel = getStatusLabel(recall.status);
      await updateBlockchainStatus(recall.id, statusLabel);
      success('Recall status updated on blockchain!');
      fetchRecalls();
    } catch (err) {
      console.error('Error updating blockchain status:', err);
      error(err.response?.data?.message || 'Failed to update blockchain status');
    }
  };

  const getStatusLabel = (status) => {
    const statusLabels = {
      0: 'Active',
      1: 'Resolved',
      2: 'Cancelled',
    };
    return statusLabels[status] || 'Active';
  };

  const getStatusBadge = (status) => {
    const statusConfig = {
      0: { color: 'bg-orange-100 text-orange-800', icon: FiAlertTriangle, label: 'Active' },
      1: { color: 'bg-green-100 text-green-800', icon: FiCheckCircle, label: 'Resolved' },
      2: { color: 'bg-gray-100 text-gray-800', icon: FiXCircle, label: 'Cancelled' },
    };

    // Handle both numeric and string status
    const statusKey = typeof status === 'number' ? status : status;
    const config = statusConfig[statusKey] || statusConfig[0];
    const Icon = config.icon;

    return (
      <span className={`inline-flex items-center gap-1 px-2 py-1 rounded text-xs font-medium ${config.color}`}>
        <Icon className="text-sm" />
        {config.label}
      </span>
    );
  };

  if (loading) {
    return <div className="text-center py-8">Loading recalls...</div>;
  }

  if (editingRecall) {
    return (
      <div className="max-w-2xl mx-auto">
        <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
        
        <div className="bg-white p-6 rounded-lg shadow border">
          <h3 className="text-xl font-semibold mb-4">Edit Recall</h3>
          
          <form onSubmit={handleUpdate} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Reason
              </label>
              <textarea
                value={editForm.reason}
                onChange={(e) => setEditForm({ ...editForm, reason: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                rows="3"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Action Required
              </label>
              <textarea
                value={editForm.actionRequired}
                onChange={(e) => setEditForm({ ...editForm, actionRequired: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                rows="3"
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Status
              </label>
              <select
                value={editForm.status}
                onChange={(e) => setEditForm({ ...editForm, status: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
              >
                <option value="0">Active</option>
                <option value="1">Resolved</option>
                <option value="2">Cancelled</option>
              </select>
            </div>

            <div className="flex gap-2">
              <button
                type="submit"
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700"
              >
                Save Changes
              </button>
              <button
                type="button"
                onClick={() => setEditingRecall(null)}
                className="flex-1 px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      </div>
    );
  }

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">My Recalls</h2>
        <button
          onClick={fetchRecalls}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Refresh
        </button>
      </div>

      {recalls.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg border border-dashed border-gray-300">
          <FiAlertTriangle className="mx-auto text-gray-400 text-5xl mb-3" />
          <p className="text-gray-600">No recalls issued yet.</p>
          <p className="text-sm text-gray-500 mt-1">Go to Approved Tickets to issue recalls.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {recalls.map((recall) => (
            <div
              key={recall.id}
              className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
            >
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="font-semibold text-lg">Product: {recall.productId}</h3>
                    {getStatusBadge(recall.status)}
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

                  {recall.transactionHash && (
                    <div className="mt-3 pt-3 border-t border-gray-200 flex items-center justify-between">
                      <div>
                        <span className="text-xs text-gray-500">
                          <span className="font-medium">Blockchain:</span> {recall.transactionHash.substring(0, 20)}...
                        </span>
                      </div>
                      <button
                        onClick={(e) => {
                          e.stopPropagation();
                          handleUpdateBlockchainStatus(recall);
                        }}
                        className="text-xs px-3 py-1 bg-green-100 text-green-700 hover:bg-green-200 rounded"
                      >
                        Update on Chain
                      </button>
                    </div>
                  )}
                </div>

                <div className="flex gap-2 ml-4">
                  <button
                    onClick={() => handleEdit(recall)}
                    className="p-2 text-blue-600 hover:bg-blue-50 rounded"
                    title="Edit recall"
                  >
                    <FiEdit2 />
                  </button>
                  <button
                    onClick={() => handleDelete(recall.id)}
                    className="p-2 text-red-600 hover:bg-red-50 rounded"
                    title="Delete recall"
                  >
                    <FiTrash2 />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
