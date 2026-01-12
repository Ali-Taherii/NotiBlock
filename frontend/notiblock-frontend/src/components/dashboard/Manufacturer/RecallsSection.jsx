import { useState, useEffect } from 'react';
import { getMyRecalls, updateRecall, deleteRecall } from '../../../api/recalls';
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
      // Handle paginated response format: { success, data: { items, totalCount, ... } }
      const items = response?.data?.items || response?.items || response || [];
      setRecalls(Array.isArray(items) ? items : []);
    } catch (err) {
      console.error('Error fetching recalls:', err);
      error('Failed to load recalls');
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
      await updateRecall(editingRecall.id, editForm);
      success('Recall updated successfully!');
      setEditingRecall(null);
      fetchRecalls();
    } catch (err) {
      console.error('Error updating recall:', err);
      error('Failed to update recall');
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

  const getStatusBadge = (status) => {
    const statusConfig = {
      Active: { color: 'bg-orange-100 text-orange-800', icon: FiAlertTriangle },
      Resolved: { color: 'bg-green-100 text-green-800', icon: FiCheckCircle },
      Cancelled: { color: 'bg-gray-100 text-gray-800', icon: FiXCircle },
    };

    const config = statusConfig[status] || statusConfig.Active;
    const Icon = config.icon;

    return (
      <span className={`inline-flex items-center gap-1 px-2 py-1 rounded text-xs font-medium ${config.color}`}>
        <Icon className="text-sm" />
        {status}
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
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-bold">Edit Recall</h2>
            <button
              onClick={() => setEditingRecall(null)}
              className="text-gray-500 hover:text-gray-700 text-xl"
            >
              ✕
            </button>
          </div>

          <form onSubmit={handleUpdate} className="space-y-4">
            <div>
              <label className="block mb-1 font-medium">Product Serial</label>
              <input
                type="text"
                value={editingRecall.productSerialNumber}
                disabled
                className="w-full border border-gray-300 rounded px-3 py-2 bg-gray-50"
              />
            </div>

            <div>
              <label className="block mb-1 font-medium">Recall Reason</label>
              <textarea
                value={editForm.reason}
                onChange={(e) => setEditForm({ ...editForm, reason: e.target.value })}
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                rows="3"
                required
              />
            </div>

            <div>
              <label className="block mb-1 font-medium">Action Required</label>
              <textarea
                value={editForm.actionRequired}
                onChange={(e) => setEditForm({ ...editForm, actionRequired: e.target.value })}
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                rows="3"
                required
              />
            </div>

            <div>
              <label className="block mb-1 font-medium">Status</label>
              <select
                value={editForm.status}
                onChange={(e) => setEditForm({ ...editForm, status: e.target.value })}
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
              >
                <option value="Active">Active</option>
                <option value="Resolved">Resolved</option>
                <option value="Cancelled">Cancelled</option>
              </select>
            </div>

            <div className="flex gap-3">
              <button
                type="submit"
                className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
              >
                Update Recall
              </button>
              <button
                type="button"
                onClick={() => setEditingRecall(null)}
                className="px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600"
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
            <div key={recall.id} className="bg-white p-5 rounded-lg shadow border">
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-3">
                    {getStatusBadge(recall.status)}
                    <span className="text-xs text-gray-500">
                      ID: {recall.id}
                    </span>
                  </div>
                  
                  <div className="mb-2">
                    <span className="font-semibold text-lg">{recall.productSerialNumber}</span>
                  </div>

                  <div className="mb-2">
                    <p className="text-sm font-medium text-gray-600">Reason:</p>
                    <p className="text-gray-800">{recall.reason}</p>
                  </div>

                  <div className="mb-2">
                    <p className="text-sm font-medium text-gray-600">Action Required:</p>
                    <p className="text-gray-800">{recall.actionRequired}</p>
                  </div>

                  <div className="flex gap-4 text-sm text-gray-600 mt-3">
                    <span><strong>Issued:</strong> {new Date(recall.issuedAt).toLocaleDateString()}</span>
                    {recall.resolvedAt && (
                      <span><strong>Resolved:</strong> {new Date(recall.resolvedAt).toLocaleDateString()}</span>
                    )}
                  </div>

                  {recall.transactionHash && (
                    <div className="mt-2">
                      <span className="text-xs text-gray-500">
                        Blockchain: {recall.transactionHash.substring(0, 20)}...
                      </span>
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
