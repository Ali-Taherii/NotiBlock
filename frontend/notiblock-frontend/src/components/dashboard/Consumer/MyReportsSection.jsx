import { useState, useEffect, useCallback } from 'react';
import { FiPlus, FiFileText, FiEye } from 'react-icons/fi';
import { getMyReports, submitReport } from '../../../api/consumerReports';
import { getMyProducts } from '../../../api/products';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function MyReportsSection() {
  const [reports, setReports] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedReport, setSelectedReport] = useState(null);
  const [formData, setFormData] = useState({
    productSerialNumber: '',
    issueDescription: '',
  });
  const { toast, success, error, hideToast } = useToast();

  const fetchReports = useCallback(async () => {
    try {
      setLoading(true);
      // Using the generic endpoint since we don't have userId
      const response = await getMyReports();
      const items = response?.data?.items || response?.items || response || [];
      setReports(items);
    } catch (err) {
      console.error('Error fetching reports:', err);
      error('Failed to load reports');
    } finally {
      setLoading(false);
    }
  }, [error]);

  const fetchProducts = useCallback(async () => {
    try {
      const response = await getMyProducts('consumer');
      const items = response?.data?.items || response?.items || response || [];
      setProducts(items);
    } catch (error) {
      console.error('Error fetching products:', error);
    }
  }, []);

  useEffect(() => {
    fetchReports();
    fetchProducts();
  }, [fetchReports, fetchProducts]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await submitReport(formData);
      success('Report submitted successfully!');
      setFormData({ productSerialNumber: '', issueDescription: '' });
      setShowCreateForm(false);
      fetchReports();
    } catch (err) {
      console.error('Error submitting report:', err);
      error(err.response?.data?.message || 'Failed to submit report');
    }
  };

  // Delete functionality can be added later if needed
  // const handleDelete = async (id) => {
  //   if (!confirm('Are you sure you want to delete this report?')) return;
  //   
  //   try {
  //     await deleteReport(id);
  //     alert('Report deleted successfully!');
  //     fetchReports();
  //   } catch (error) {
  //     console.error('Error deleting report:', error);
  //     alert(error.response?.data?.message || 'Failed to delete report');
  //   }
  // };

  const getStatusText = (status) => {
    const statusMap = {
      0: 'Pending',
      1: 'Under Review',
      2: 'Escalated to Reseller',
      3: 'Resolved',
      4: 'Closed'
    };
    return statusMap[status] || `Unknown (${status})`;
  };

  const getStatusColor = (status) => {
    switch (Number(status)) {
      case 0: // Pending
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 1: // Under Review
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case 2: // Escalated to Reseller
        return 'bg-orange-100 text-orange-800 border-orange-200';
      case 3: // Resolved
        return 'bg-green-100 text-green-800 border-green-200';
      case 4: // Closed
        return 'bg-gray-100 text-gray-800 border-gray-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  if (loading) {
    return <div className="text-center py-8">Loading reports...</div>;
  }

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-800">My Reports</h2>
        <button
          onClick={() => setShowCreateForm(!showCreateForm)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <FiPlus className="w-4 h-4" />
          {showCreateForm ? 'Cancel' : 'Submit New Report'}
        </button>
      </div>

      {/* Create Form */}
      {showCreateForm && (
        <div className="bg-gray-50 p-6 rounded-lg mb-6 border border-gray-200">
          <h3 className="text-lg font-semibold mb-4">Submit New Report</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Product
              </label>
              <select
                value={formData.productSerialNumber}
                onChange={(e) => setFormData({ ...formData, productSerialNumber: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
              >
                <option value="">Select a product</option>
                {products.map((product) => (
                  <option key={product.serialNumber} value={product.serialNumber}>
                    {product.modelName} - {product.serialNumber}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Issue Description
              </label>
              <textarea
                value={formData.issueDescription}
                onChange={(e) => setFormData({ ...formData, issueDescription: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                rows="4"
                required
              />
            </div>
            <button
              type="submit"
              className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              Submit Report
            </button>
          </form>
        </div>
      )}

      {/* Reports Table */}
      {reports.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <FiFileText className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <p className="text-gray-600">No reports submitted yet</p>
          <p className="text-sm text-gray-500 mt-1">Click "Submit New Report" to report an issue</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 font-medium text-gray-700">ID</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Product</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Status</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Submitted</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Actions</th>
              </tr>
            </thead>
            <tbody>
              {reports.map((report) => (
                <tr key={report.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 font-mono text-sm">{report.id}</td>
                  <td className="py-3 px-4">{report.serialNumber || 'N/A'}</td>
                  <td className="py-3 px-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(report.status)}`}>
                      {getStatusText(report.status)}
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    {report.submittedAt ? new Date(report.submittedAt).toLocaleDateString() : 
                     report.createdAt ? new Date(report.createdAt).toLocaleDateString() : 'N/A'}
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => setSelectedReport(report)}
                        className="text-blue-600 hover:text-blue-800 transition-colors"
                        title="View Details"
                      >
                        <FiEye className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Report Details Modal */}
      {selectedReport && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-2xl w-full mx-4 max-h-[80vh] overflow-y-auto">
            <h3 className="text-xl font-semibold mb-4">Report Details</h3>
            <div className="space-y-3">
              <div>
                <label className="text-sm font-medium text-gray-600">Report ID:</label>
                <p className="font-mono">{selectedReport.id}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-600">Product:</label>
                <p>{selectedReport.serialNumber}</p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-600">Status:</label>
                <p>
                  <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(selectedReport.status)}`}>
                    {getStatusText(selectedReport.status)}
                  </span>
                </p>
              </div>
              <div>
                <label className="text-sm font-medium text-gray-600">Issue Description:</label>
                <p className="bg-gray-50 p-3 rounded-lg whitespace-pre-wrap">{selectedReport.description}</p>
              </div>
              {selectedReport.resolutionNotes && (
                <div>
                  <label className="text-sm font-medium text-gray-600">Resolution Notes:</label>
                  <p className="bg-gray-50 p-3 rounded-lg whitespace-pre-wrap">{selectedReport.resolutionNotes}</p>
                </div>
              )}
              <div>
                <label className="text-sm font-medium text-gray-600">Submitted:</label>
                <p>{selectedReport.submittedAt ? new Date(selectedReport.submittedAt).toLocaleString() : 
                    selectedReport.createdAt ? new Date(selectedReport.createdAt).toLocaleString() : 'N/A'}</p>
              </div>
            </div>
            <div className="flex gap-3 mt-6">
              <button
                onClick={() => setSelectedReport(null)}
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
