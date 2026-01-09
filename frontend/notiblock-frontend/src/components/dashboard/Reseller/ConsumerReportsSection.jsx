import { useState, useEffect, useCallback } from 'react';
import { FiFileText, FiEye, FiAlertCircle } from 'react-icons/fi';
import { getResellerReports, actionOnReport } from '../../../api/consumerReports';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function ConsumerReportsSection() {
  const [reports, setReports] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedReport, setSelectedReport] = useState(null);
  const [actionForm, setActionForm] = useState({
    action: '',
    resolutionNotes: '',
  });
  const { toast, success, error, hideToast } = useToast();

  const fetchReports = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getResellerReports();
      const items = response?.data?.items || response?.items || response || [];
      setReports(items);
    } catch (err) {
      console.error('Error fetching reseller reports:', err);
      error('Failed to load consumer reports');
    } finally {
      setLoading(false);
    }
  }, [error]);

  useEffect(() => {
    fetchReports();
  }, [fetchReports]);

  const handleTakeAction = async (e) => {
    e.preventDefault();
    
    if (!actionForm.action) {
      error('Please select an action');
      return;
    }

    try {
      // Map action to enum value and prepare DTO
      const actionData = {
        action: parseInt(actionForm.action), // Convert string to number for enum
        resolutionNotes: actionForm.resolutionNotes || null,
      };
      
      await actionOnReport(selectedReport.id, actionData);
      success('Action taken successfully!');
      setSelectedReport(null);
      setActionForm({ action: '', resolutionNotes: '' });
      fetchReports();
    } catch (err) {
      console.error('Error taking action:', err);
      error(err.response?.data?.message || 'Failed to take action on report');
    }
  };

  const getStatusText = (status) => {
    const statusMap = {
      0: 'Pending',
      1: 'Under Review',
      2: 'Escalated to Reseller',
      3: 'Resolved',
      4: 'Closed',
    };
    return statusMap[status] || 'Unknown';
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 0:
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 1:
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case 2:
        return 'bg-orange-100 text-orange-800 border-orange-200';
      case 3:
        return 'bg-green-100 text-green-800 border-green-200';
      case 4:
        return 'bg-gray-100 text-gray-800 border-gray-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  if (loading) {
    return <div className="text-center py-8">Loading reports...</div>;
  }

  // Detail View
  if (selectedReport) {
    return (
      <div className="max-w-3xl mx-auto">
        <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
        
        <div className="bg-white rounded-lg shadow border p-6">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-xl font-bold text-gray-800">Report Details</h2>
            <button
              onClick={() => setSelectedReport(null)}
              className="text-gray-500 hover:text-gray-700 text-xl font-bold"
            >
              ✕
            </button>
          </div>

          <div className="space-y-4 mb-6">
            <div>
              <label className="text-sm font-medium text-gray-600">Report ID</label>
              <p className="text-gray-800 font-mono">{selectedReport.id}</p>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-600">Product Serial Number</label>
              <p className="text-gray-800 font-mono">{selectedReport.serialNumber}</p>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-600">Consumer</label>
              <p className="text-gray-800">
                {selectedReport.consumerName || 'N/A'} ({selectedReport.consumerEmail || 'N/A'})
              </p>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-600">Status</label>
              <div className="mt-1">
                <span className={`inline-block px-3 py-1 rounded-full text-sm font-medium border ${getStatusColor(selectedReport.status)}`}>
                  {getStatusText(selectedReport.status)}
                </span>
              </div>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-600">Issue Description</label>
              <p className="text-gray-800 bg-gray-50 p-3 rounded border border-gray-200 mt-1">
                {selectedReport.description}
              </p>
            </div>

            <div>
              <label className="text-sm font-medium text-gray-600">Reported Date</label>
              <p className="text-gray-800">
                {selectedReport.createdAt ? new Date(selectedReport.createdAt).toLocaleString() : 'N/A'}
              </p>
            </div>
          </div>

          {/* Action Form */}
          {(selectedReport.status === 0 || selectedReport.status === 1) && (
            <form onSubmit={handleTakeAction} className="border-t pt-6">
              <h3 className="text-lg font-semibold mb-4 text-gray-800">Take Action</h3>
              
              <div className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Action <span className="text-red-500">*</span>
                  </label>
                  <select
                    value={actionForm.action}
                    onChange={(e) => setActionForm({ ...actionForm, action: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
                    required
                  >
                    <option value="">Select an action</option>
                    {selectedReport.status === 0 && (
                      <option value="0">Review - Start reviewing the report</option>
                    )}
                    <option value="1">Request More Info - Ask consumer for details</option>
                    <option value="2">Resolve - Mark issue as resolved</option>
                    <option value="3">Escalate - Link to a reseller ticket</option>
                    <option value="4">Close - Close the report</option>
                  </select>
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">
                    Resolution Notes
                  </label>
                  <textarea
                    value={actionForm.resolutionNotes}
                    onChange={(e) => setActionForm({ ...actionForm, resolutionNotes: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
                    rows="3"
                    placeholder="Add resolution notes or comments (max 2000 characters)..."
                    maxLength="2000"
                  />
                </div>

                <div className="flex gap-3">
                  <button
                    type="submit"
                    className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                  >
                    Submit Action
                  </button>
                  <button
                    type="button"
                    onClick={() => setSelectedReport(null)}
                    className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition-colors"
                  >
                    Cancel
                  </button>
                </div>
              </div>
            </form>
          )}
        </div>
      </div>
    );
  }

  // List View
  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-800">Consumer Reports</h2>
        <button
          onClick={fetchReports}
          className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          Refresh
        </button>
      </div>

      {reports.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <FiFileText className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <p className="text-gray-600">No consumer reports found</p>
          <p className="text-sm text-gray-500 mt-1">Reports related to your products will appear here</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 font-medium text-gray-700">Report ID</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Product Serial</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Consumer</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Status</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Reported Date</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Actions</th>
              </tr>
            </thead>
            <tbody>
              {reports.map((report) => (
                <tr key={report.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 font-mono text-sm">{report.id}</td>
                  <td className="py-3 px-4 font-mono text-sm">{report.serialNumber}</td>
                  <td className="py-3 px-4">
                    {report.consumerName || 'N/A'}
                    <br />
                    <span className="text-xs text-gray-500">{report.consumerEmail || ''}</span>
                  </td>
                  <td className="py-3 px-4">
                    <span className={`inline-block px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(report.status)}`}>
                      {getStatusText(report.status)}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-sm">
                    {report.createdAt ? new Date(report.createdAt).toLocaleDateString() : 'N/A'}
                  </td>
                  <td className="py-3 px-4">
                    <button
                      onClick={() => setSelectedReport(report)}
                      className="text-blue-600 hover:text-blue-800 transition-colors flex items-center gap-1"
                      title="View Details"
                    >
                      <FiEye className="w-4 h-4" />
                      <span className="text-sm">View</span>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Summary Stats */}
      {reports.length > 0 && (
        <div className="mt-6 grid grid-cols-1 md:grid-cols-4 gap-4">
          <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-1">
              <FiAlertCircle className="text-yellow-600" />
              <span className="text-sm font-medium text-yellow-800">Pending</span>
            </div>
            <p className="text-2xl font-bold text-yellow-900">
              {reports.filter(r => r.status === 0).length}
            </p>
          </div>
          
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-1">
              <FiFileText className="text-blue-600" />
              <span className="text-sm font-medium text-blue-800">Under Review</span>
            </div>
            <p className="text-2xl font-bold text-blue-900">
              {reports.filter(r => r.status === 1).length}
            </p>
          </div>

          <div className="bg-orange-50 border border-orange-200 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-1">
              <FiAlertCircle className="text-orange-600" />
              <span className="text-sm font-medium text-orange-800">Escalated</span>
            </div>
            <p className="text-2xl font-bold text-orange-900">
              {reports.filter(r => r.status === 2).length}
            </p>
          </div>

          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-1">
              <FiFileText className="text-green-600" />
              <span className="text-sm font-medium text-green-800">Resolved</span>
            </div>
            <p className="text-2xl font-bold text-green-900">
              {reports.filter(r => r.status === 3).length}
            </p>
          </div>
        </div>
      )}
    </div>
  );
}
