import { useState, useEffect } from 'react';
import { FiAlertTriangle, FiPackage, FiCalendar, FiX } from 'react-icons/fi';
import { getRecallsByProduct } from '../../../api/recalls';
import { getMyProducts } from '../../../api/products';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function ActiveRecallsSection() {
  const [recalls, setRecalls] = useState([]);
  const [myProducts, setMyProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedRecall, setSelectedRecall] = useState(null);
  const [filterStatus, setFilterStatus] = useState('all');
  const { toast, error, hideToast } = useToast();

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      
      // Fetch my products
      const productsResponse = await getMyProducts('consumer');
      const products = productsResponse?.data.items || productsResponse || [];
      setMyProducts(Array.isArray(products) ? products : []);
      
      if (!Array.isArray(products) || products.length === 0) {
        setRecalls([]);
        return;
      }
      
      // Fetch recalls for each product
      const recallPromises = products.map(product => 
        getRecallsByProduct(product.serialNumber)
          .then(res => {
            const data = res?.data || res || [];
            return Array.isArray(data) ? data : [];
          })
          .catch(err => {
            console.error(`Error fetching recalls for ${product.serialNumber}:`, err);
            return [];
          })
      );
      
      const recallArrays = await Promise.all(recallPromises);
      
      // Flatten and deduplicate recalls by ID
      const allRecalls = recallArrays.flat().filter(Boolean); // Remove any null/undefined
      const uniqueRecalls = Array.from(
        new Map(allRecalls.map(recall => [recall.id, recall])).values()
      );
      
      setRecalls(uniqueRecalls);
    } catch (err) {
      console.error('Error fetching data:', err);
      error('Failed to load recalls');
      setRecalls([]);
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status) => {
    // Handle numeric enum values: 0 = Active, 1 = Resolved, 2 = Cancelled
    const statusValue = typeof status === 'number' ? status : 
      status?.toLowerCase?.() === 'active' ? 0 :
      status?.toLowerCase?.() === 'resolved' ? 1 :
      status?.toLowerCase?.() === 'cancelled' ? 2 : 0;

    switch (statusValue) {
      case 0: // Active
        return 'bg-red-100 text-red-800 border-red-200';
      case 1: // Resolved
        return 'bg-green-100 text-green-800 border-green-200';
      case 2: // Cancelled
        return 'bg-gray-100 text-gray-800 border-gray-200';
      default:
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
    }
  };

  const getStatusLabel = (status) => {
    const statusValue = typeof status === 'number' ? status : 0;
    switch (statusValue) {
      case 0: return 'Active';
      case 1: return 'Resolved';
      case 2: return 'Cancelled';
      default: return 'Unknown';
    }
  };

  const filteredRecalls = filterStatus === 'all' 
    ? recalls 
    : recalls.filter(r => {
        const statusValue = typeof r.status === 'number' ? r.status : 0;
        return (filterStatus === 'active' && statusValue === 0) ||
               (filterStatus === 'resolved' && statusValue === 1) ||
               (filterStatus === 'cancelled' && statusValue === 2);
      });

  if (loading) {
    return (
      <div className="text-center py-8">
        <div className="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
        <p className="mt-2 text-gray-600">Loading recalls...</p>
      </div>
    );
  }

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-800">Active Recalls</h2>
        <div className="flex items-center gap-3">
          <button
            onClick={fetchData}
            className="px-3 py-2 text-sm bg-blue-600 text-white rounded-lg hover:bg-blue-700"
          >
            Refresh
          </button>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Status</option>
            <option value="active">Active</option>
            <option value="resolved">Resolved</option>
            <option value="cancelled">Cancelled</option>
          </select>
        </div>
      </div>

      {/* No Products Warning */}
      {myProducts.length === 0 && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4 mb-6">
          <div className="flex items-start gap-3">
            <FiPackage className="w-5 h-5 text-blue-600 mt-0.5" />
            <div>
              <h3 className="font-semibold text-blue-800">No Registered Products</h3>
              <p className="text-sm text-blue-700 mt-1">
                You don't have any registered products yet. Register your products to receive recall notifications.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Info Banner */}
      {recalls.length > 0 && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4 mb-6">
          <div className="flex items-start gap-3">
            <FiAlertTriangle className="w-5 h-5 text-yellow-600 mt-0.5" />
            <div>
              <h3 className="font-semibold text-yellow-800">Product Safety Alert</h3>
              <p className="text-sm text-yellow-700 mt-1">
                You have {filteredRecalls.length} recall{filteredRecalls.length !== 1 ? 's' : ''} affecting your registered products.
                Please review the details below and take appropriate action.
              </p>
            </div>
          </div>
        </div>
      )}

      {/* Recalls List */}
      {filteredRecalls.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <FiPackage className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <p className="text-gray-600">
            {filterStatus === 'all' 
              ? 'No recalls affecting your products' 
              : `No ${filterStatus} recalls affecting your products`}
          </p>
          {myProducts.length > 0 && (
            <p className="text-sm text-gray-500 mt-1">Great news! Your products are safe.</p>
          )}
        </div>
      ) : (
        <div className="space-y-4">
          {filteredRecalls.map((recall) => (
            <div
              key={recall.id}
              className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer bg-white"
              onClick={() => setSelectedRecall(recall)}
            >
              <div className="flex items-start justify-between mb-3">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="font-semibold text-lg text-gray-800">
                      Product: {recall.productId || 'Unknown'}
                    </h3>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(recall.status)}`}>
                      {getStatusLabel(recall.status)}
                    </span>
                  </div>
                  <p className="text-sm text-gray-600">{recall.manufacturerName}</p>
                </div>
                <FiAlertTriangle className="w-6 h-6 text-red-600 flex-shrink-0 ml-4" />
              </div>

              <div className="mb-3">
                <p className="text-sm text-gray-600 font-medium">Reason:</p>
                <p className="text-sm text-gray-800 line-clamp-2">{recall.reason}</p>
              </div>

              <div className="flex items-center gap-6 text-sm text-gray-600">
                <div className="flex items-center gap-2">
                  <FiCalendar className="w-4 h-4" />
                  <span>
                    Issued: {recall.issuedAt ? new Date(recall.issuedAt).toLocaleDateString() : 'N/A'}
                  </span>
                </div>
              </div>

              {recall.transactionHash && (
                <div className="mt-3 pt-3 border-t border-gray-200">
                  <p className="text-xs text-gray-500">
                    <span className="font-medium">Blockchain:</span> {recall.transactionHash.substring(0, 20)}...
                  </p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Recall Details Modal */}
      {selectedRecall && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-3xl w-full max-h-[90vh] overflow-y-auto">
            <div className="flex items-start justify-between mb-4">
              <div className="flex-1">
                <h3 className="text-2xl font-semibold mb-2">Recall Details</h3>
                <p className="text-gray-600">Product: {selectedRecall.productId}</p>
              </div>
              <button
                onClick={() => setSelectedRecall(null)}
                className="p-2 hover:bg-gray-100 rounded-full"
              >
                <FiX className="w-5 h-5" />
              </button>
            </div>

            <div className="space-y-4">
              <div className="flex items-center gap-2">
                <span className={`px-3 py-1 rounded-full text-sm font-medium border ${getStatusColor(selectedRecall.status)}`}>
                  {getStatusLabel(selectedRecall.status)}
                </span>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-600">Recall ID:</label>
                <p className="font-mono text-sm">{selectedRecall.id}</p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-600">Manufacturer:</label>
                <p>{selectedRecall.manufacturerName || 'N/A'}</p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-600">Reason for Recall:</label>
                <p className="bg-gray-50 p-3 rounded-lg whitespace-pre-wrap">{selectedRecall.reason}</p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-600">Action Required:</label>
                <p className="bg-yellow-50 p-3 rounded-lg whitespace-pre-wrap border border-yellow-200">
                  {selectedRecall.actionRequired}
                </p>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">Issued Date:</label>
                  <p>{selectedRecall.issuedAt ? new Date(selectedRecall.issuedAt).toLocaleString() : 'N/A'}</p>
                </div>
                {selectedRecall.resolvedAt && (
                  <div>
                    <label className="text-sm font-medium text-gray-600">Resolved Date:</label>
                    <p>{new Date(selectedRecall.resolvedAt).toLocaleString()}</p>
                  </div>
                )}
              </div>

              {selectedRecall.transactionHash && (
                <div>
                  <label className="text-sm font-medium text-gray-600">Blockchain Verification:</label>
                  <div className="bg-green-50 p-3 rounded-lg border border-green-200">
                    <p className="text-sm text-green-800 mb-1">✓ Verified on blockchain</p>
                    <p className="font-mono text-xs text-gray-600">{selectedRecall.transactionHash}</p>
                    <a
                      href={`https://sepolia.etherscan.io/tx/${selectedRecall.transactionHash}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-xs text-blue-600 hover:text-blue-800 mt-2 inline-block"
                    >
                      View on Etherscan →
                    </a>
                  </div>
                </div>
              )}
            </div>

            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setSelectedRecall(null)}
                className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
              >
                Close
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
