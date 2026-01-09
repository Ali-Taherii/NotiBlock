import { useState, useEffect } from 'react';
import { FiAlertTriangle, FiPackage, FiCalendar } from 'react-icons/fi';
import { getRecalls } from '../../../api/recalls';
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
      
      // Fetch all recalls
      const recallsResponse = await getRecalls();
      const allRecalls = recallsResponse?.data?.items || recallsResponse?.items || recallsResponse || [];
      
      // Fetch my products
      const productsResponse = await getMyProducts('consumer');
      const products = productsResponse?.data?.items || productsResponse?.items || productsResponse || [];
      setMyProducts(products);
      
      // Filter recalls that affect my products
      const myProductSerials = products.map(p => p.serialNumber);
      const affectingRecalls = allRecalls.filter(recall => 
        recall.affectedProducts?.some(serial => myProductSerials.includes(serial)) ||
        products.some(p => p.modelName === recall.productModel)
      );
      
      setRecalls(affectingRecalls);
    } catch (err) {
      console.error('Error fetching data:', err);
      error('Failed to load recalls');
    } finally {
      setLoading(false);
    }
  };

  const getStatusColor = (status) => {
    switch (status?.toLowerCase()) {
      case 'active':
        return 'bg-red-100 text-red-800 border-red-200';
      case 'resolved':
        return 'bg-green-100 text-green-800 border-green-200';
      case 'cancelled':
        return 'bg-gray-100 text-gray-800 border-gray-200';
      default:
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
    }
  };

  const getSeverityColor = (severity) => {
    switch (severity?.toLowerCase()) {
      case 'high':
      case 'critical':
        return 'text-red-600';
      case 'medium':
        return 'text-yellow-600';
      case 'low':
        return 'text-green-600';
      default:
        return 'text-gray-600';
    }
  };

  const filteredRecalls = filterStatus === 'all' 
    ? recalls 
    : recalls.filter(r => r.status?.toLowerCase() === filterStatus.toLowerCase());

  if (loading) {
    return <div className="text-center py-8">Loading recalls...</div>;
  }

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-800">Active Recalls</h2>
        <div className="flex items-center gap-3">
          <label className="text-sm font-medium text-gray-600">Filter:</label>
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
          >
            <option value="all">All Recalls</option>
            <option value="active">Active</option>
            <option value="resolved">Resolved</option>
            <option value="cancelled">Cancelled</option>
          </select>
        </div>
      </div>

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
          <p className="text-sm text-gray-500 mt-1">Great news! Your products are safe.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {filteredRecalls.map((recall) => (
            <div
              key={recall.id}
              className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer"
              onClick={() => setSelectedRecall(recall)}
            >
              <div className="flex items-start justify-between mb-3">
                <div className="flex-1">
                  <div className="flex items-center gap-3 mb-2">
                    <h3 className="font-semibold text-lg text-gray-800">{recall.productModel}</h3>
                    <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(recall.status)}`}>
                      {recall.status}
                    </span>
                  </div>
                  <p className="text-sm text-gray-600 line-clamp-2">{recall.reason}</p>
                </div>
                <FiAlertTriangle className={`w-6 h-6 ${getSeverityColor(recall.severity)} flex-shrink-0 ml-4`} />
              </div>

              <div className="flex items-center gap-6 text-sm text-gray-600">
                <div className="flex items-center gap-2">
                  <FiCalendar className="w-4 h-4" />
                  <span>
                    Issued: {recall.issuedDate ? new Date(recall.issuedDate).toLocaleDateString() : 
                            recall.createdAt ? new Date(recall.createdAt).toLocaleDateString() : 'N/A'}
                  </span>
                </div>
                {recall.severity && (
                  <div>
                    <span className="font-medium">Severity:</span> <span className={getSeverityColor(recall.severity)}>{recall.severity}</span>
                  </div>
                )}
              </div>

              {recall.affectedProducts && recall.affectedProducts.length > 0 && (
                <div className="mt-3 pt-3 border-t border-gray-200">
                  <p className="text-sm text-gray-600">
                    <span className="font-medium">Affected Serial Numbers:</span>{' '}
                    {recall.affectedProducts.slice(0, 3).join(', ')}
                    {recall.affectedProducts.length > 3 && ` +${recall.affectedProducts.length - 3} more`}
                  </p>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Recall Details Modal */}
      {selectedRecall && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-3xl w-full mx-4 max-h-[80vh] overflow-y-auto">
            <div className="flex items-start justify-between mb-4">
              <h3 className="text-2xl font-semibold">{selectedRecall.productModel}</h3>
              <span className={`px-3 py-1 rounded-full text-sm font-medium border ${getStatusColor(selectedRecall.status)}`}>
                {selectedRecall.status}
              </span>
            </div>

            <div className="space-y-4">
              <div>
                <label className="text-sm font-medium text-gray-600">Recall ID:</label>
                <p className="font-mono">{selectedRecall.id}</p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-600">Manufacturer:</label>
                <p>{selectedRecall.manufacturerName || 'N/A'}</p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-600">Severity:</label>
                <p className={`font-semibold ${getSeverityColor(selectedRecall.severity)}`}>
                  {selectedRecall.severity || 'Not specified'}
                </p>
              </div>

              <div>
                <label className="text-sm font-medium text-gray-600">Reason for Recall:</label>
                <p className="bg-gray-50 p-3 rounded-lg whitespace-pre-wrap">{selectedRecall.reason}</p>
              </div>

              {selectedRecall.resolution && (
                <div>
                  <label className="text-sm font-medium text-gray-600">Resolution:</label>
                  <p className="bg-gray-50 p-3 rounded-lg whitespace-pre-wrap">{selectedRecall.resolution}</p>
                </div>
              )}

              {selectedRecall.affectedProducts && selectedRecall.affectedProducts.length > 0 && (
                <div>
                  <label className="text-sm font-medium text-gray-600">Affected Products:</label>
                  <div className="bg-gray-50 p-3 rounded-lg">
                    <p className="text-sm mb-2">{selectedRecall.affectedProducts.length} product(s) affected</p>
                    <div className="max-h-32 overflow-y-auto">
                      <ul className="list-disc list-inside space-y-1">
                        {selectedRecall.affectedProducts.map((serial, idx) => (
                          <li key={idx} className="font-mono text-sm">
                            {serial}
                            {myProducts.some(p => p.serialNumber === serial) && (
                              <span className="ml-2 text-xs bg-red-100 text-red-800 px-2 py-0.5 rounded">
                                Your Product
                              </span>
                            )}
                          </li>
                        ))}
                      </ul>
                    </div>
                  </div>
                </div>
              )}

              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">Issued Date:</label>
                  <p>{selectedRecall.issuedDate ? new Date(selectedRecall.issuedDate).toLocaleString() : 
                      selectedRecall.createdAt ? new Date(selectedRecall.createdAt).toLocaleString() : 'N/A'}</p>
                </div>
                {selectedRecall.resolvedDate && (
                  <div>
                    <label className="text-sm font-medium text-gray-600">Resolved Date:</label>
                    <p>{new Date(selectedRecall.resolvedDate).toLocaleString()}</p>
                  </div>
                )}
              </div>

              {selectedRecall.qrCodeData && (
                <div>
                  <label className="text-sm font-medium text-gray-600">QR Code:</label>
                  <div className="bg-gray-50 p-3 rounded-lg">
                    <img src={selectedRecall.qrCodeData} alt="Recall QR Code" className="w-32 h-32" />
                  </div>
                </div>
              )}
            </div>

            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setSelectedRecall(null)}
                className="flex-1 px-4 py-2 border border-gray-300 rounded-lg hover:bg-gray-50 transition-colors"
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
