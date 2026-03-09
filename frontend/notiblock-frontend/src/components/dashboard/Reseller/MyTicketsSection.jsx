import { useState, useEffect, useCallback } from 'react';
import { FiPlus, FiCheckCircle, FiEye, FiLink, FiEdit2 } from 'react-icons/fi';
import { getMyTickets, createTicket, linkReportToTicket, updateTicket } from '../../../api/resellerTickets';
import { getResellerReports } from '../../../api/consumerReports';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function MyTicketsSection() {
    const [tickets, setTickets] = useState([]);
    const [reports, setReports] = useState([]);
    const [showCreateForm, setShowCreateForm] = useState(false);
    const [showEditModal, setShowEditModal] = useState(false);
    const [loading, setLoading] = useState(true);
    const [selectedTicket, setSelectedTicket] = useState(null);
    const [showLinkReports, setShowLinkReports] = useState(false);
    const [selectedReports, setSelectedReports] = useState([]);
    const [formData, setFormData] = useState({
    category: '0',
    description: '',
    priority: '0',
    });
  const { toast, success, error, hideToast } = useToast();

  const fetchTickets = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getMyTickets();
      const items = response?.data?.items || response?.items || response || [];
      setTickets(items);
    } catch (err) {
      console.error('Error fetching tickets:', err);
      error('Failed to load tickets');
    } finally {
      setLoading(false);
    }
  }, [error]);

  const fetchReports = useCallback(async () => {
    try {
      const response = await getResellerReports();
      const items = response?.data?.items || response?.items || response || [];
      setReports(items);
    } catch (err) {
      console.error('Error fetching reports:', err);
    }
  }, []);

  useEffect(() => {
    fetchTickets();
    fetchReports();
  }, [fetchTickets, fetchReports]);

  const handleCreateTicket = async (e) => {
    e.preventDefault();
    try {
      const ticketData = {
        category: parseInt(formData.category),
        description: formData.description,
        priority: parseInt(formData.priority),
      };
      
      await createTicket(ticketData);
      success('Ticket created successfully!');
      setFormData({ category: '0', description: '', priority: '0' });
      setShowCreateForm(false);
      fetchTickets();
    } catch (err) {
      console.error('Error creating ticket:', err);
      error(err.response?.data?.message || 'Failed to create ticket');
    }
  };

  const handleLinkReports = async () => {
    if (selectedReports.length === 0) {
      error('Please select at least one report to link');
      return;
    }

    try {
      await linkReportToTicket(selectedTicket.id, selectedReports);
      success(`${selectedReports.length} report(s) linked successfully!`);
      setSelectedReports([]);
      setShowLinkReports(false);
      setSelectedTicket(null);
      fetchTickets();
      fetchReports();
    } catch (err) {
      console.error('Error linking reports:', err);
      error(err.response?.data?.message || 'Failed to link reports');
    }
  };

  const toggleReportSelection = (reportId) => {
    setSelectedReports(prev => 
      prev.includes(reportId) 
        ? prev.filter(id => id !== reportId)
        : [...prev, reportId]
    );
  };

  const handleEditTicket = (ticket) => {
    setSelectedTicket(ticket);
    setFormData({
      category: ticket.category.toString(),
      description: ticket.description,
      priority: ticket.priority.toString(),
    });
    setShowEditModal(true);
  };

  const handleUpdateTicket = async (e) => {
    e.preventDefault();
    try {
      const ticketData = {
        category: parseInt(formData.category),
        description: formData.description,
        priority: parseInt(formData.priority),
      };
      
      await updateTicket(selectedTicket.id, ticketData);
      success('Ticket updated successfully!');
      setFormData({ category: '0', description: '', priority: '0' });
      setShowEditModal(false);
      setSelectedTicket(null);
      fetchTickets();
    } catch (err) {
      console.error('Error updating ticket:', err);
      error(err.response?.data?.message || 'Failed to update ticket');
    }
  };



  const getCategoryText = (category) => {
    const categories = {
      0: 'Product Defect',
      1: 'Quality Issue',
      2: 'Safety Concern',
      3: 'Counterfeit Suspicion',
      4: 'Supply Chain Issue',
      5: 'Customer Complaint',
      6: 'Other',
    };
    return categories[category] || 'Unknown';
  };

  const getStatusText = (status) => {
    const statuses = {
      0: 'Pending',
      1: 'Under Review',
      2: 'Approved',
      3: 'Rejected',
      4: 'Resolved',
    };
    return statuses[status] || 'Unknown';
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 0:
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 1:
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case 2:
        return 'bg-green-100 text-green-800 border-green-200';
      case 3:
        return 'bg-red-100 text-red-800 border-red-200';
      case 4:
        return 'bg-gray-100 text-gray-800 border-gray-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const getPriorityText = (priority) => {
    const priorities = ['Low', 'Medium', 'High', 'Critical'];
    return priorities[priority] || 'Unknown';
  };

  const getPriorityColor = (priority) => {
    switch (priority) {
      case 0:
        return 'text-gray-600';
      case 1:
        return 'text-blue-600';
      case 2:
        return 'text-orange-600';
      case 3:
        return 'text-red-600';
      default:
        return 'text-gray-600';
    }
  };

  // Available reports (not already linked to other tickets, and only pending or under review)
  const availableReports = reports.filter(report => 
    !report.resellerTicketId && (report.status === 0 || report.status === 1)
  );

  if (loading) {
    return <div className="text-center py-8">Loading tickets...</div>;
  }

  // Link Reports Modal
  if (showLinkReports && selectedTicket) {
    return (
      <div className="max-w-4xl mx-auto">
        <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
        
        <div className="bg-white rounded-lg shadow border p-6">
          <div className="flex justify-between items-center mb-6">
            <h2 className="text-xl font-bold text-gray-800">Link Reports to Ticket</h2>
            <button
              onClick={() => {
                setShowLinkReports(false);
                setSelectedReports([]);
                setSelectedTicket(null);
              }}
              className="text-gray-500 hover:text-gray-700 text-xl font-bold"
            >
              ✕
            </button>
          </div>

          <div className="mb-4 p-4 bg-gray-50 rounded-lg">
            <p className="text-sm text-gray-600">Ticket: <span className="font-semibold">{selectedTicket.id}</span></p>
            <p className="text-sm text-gray-600">Category: <span className="font-semibold">{getCategoryText(selectedTicket.category)}</span></p>
          </div>

          {availableReports.length === 0 ? (
            <div className="text-center py-8">
              <p className="text-gray-600">No available reports to link</p>
              <p className="text-sm text-gray-500 mt-1">All reports are already linked to tickets</p>
            </div>
          ) : (
            <>
              <div className="mb-4">
                <h3 className="font-semibold mb-3">Select Reports to Link ({selectedReports.length} selected)</h3>
                <div className="space-y-2 max-h-96 overflow-y-auto">
                  {availableReports.map((report) => (
                    <label
                      key={report.id}
                      className="flex items-start gap-3 p-3 border border-gray-200 rounded-lg hover:bg-gray-50 cursor-pointer"
                    >
                      <input
                        type="checkbox"
                        checked={selectedReports.includes(report.id)}
                        onChange={() => toggleReportSelection(report.id)}
                        className="mt-1"
                      />
                      <div className="flex-1">
                        <p className="font-medium text-gray-800">Report #{report.id}</p>
                        <p className="text-sm text-gray-600">Product: {report.serialNumber}</p>
                        <p className="text-sm text-gray-500 line-clamp-2">{report.description}</p>
                      </div>
                    </label>
                  ))}
                </div>
              </div>

              <div className="flex gap-3">
                <button
                  onClick={handleLinkReports}
                  disabled={selectedReports.length === 0}
                  className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Link {selectedReports.length} Report(s)
                </button>
                <button
                  onClick={() => {
                    setShowLinkReports(false);
                    setSelectedReports([]);
                    setSelectedTicket(null);
                  }}
                  className="px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </>
          )}
        </div>
      </div>
    );
  }

  // Main View
  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-800">My Tickets</h2>
        <button
          onClick={() => setShowCreateForm(!showCreateForm)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <FiPlus className="w-4 h-4" />
          {showCreateForm ? 'Cancel' : 'Create Ticket'}
        </button>
      </div>

      {/* Create Form */}
      {showCreateForm && (
        <div className="bg-gray-50 p-6 rounded-lg mb-6 border border-gray-200">
          <h3 className="text-lg font-semibold mb-4">Create New Ticket</h3>
          <form onSubmit={handleCreateTicket} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Category <span className="text-red-500">*</span>
              </label>
              <select
                value={formData.category}
                onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
                required
              >
                <option value="0">Product Defect</option>
                <option value="1">Quality Issue</option>
                <option value="2">Safety Concern</option>
                <option value="3">Counterfeit Suspicion</option>
                <option value="4">Supply Chain Issue</option>
                <option value="5">Customer Complaint</option>
                <option value="6">Other</option>
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Description <span className="text-red-500">*</span>
              </label>
              <textarea
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
                rows="4"
                placeholder="Describe the issue in detail..."
                required
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Priority
              </label>
              <select
                value={formData.priority}
                onChange={(e) => setFormData({ ...formData, priority: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
              >
                <option value="0">Low</option>
                <option value="1">Medium</option>
                <option value="2">High</option>
                <option value="3">Critical</option>
              </select>
            </div>

            <button
              type="submit"
              className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              Create Ticket
            </button>
          </form>
        </div>
      )}

      {/* Edit Modal */}
      {showEditModal && selectedTicket && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full">
            <h3 className="text-xl font-semibold mb-4">Edit Ticket</h3>
            <form onSubmit={handleUpdateTicket} className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Category <span className="text-red-500">*</span>
                </label>
                <select
                  value={formData.category}
                  onChange={(e) => setFormData({ ...formData, category: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  required
                >
                  <option value="0">Product Defect</option>
                  <option value="1">Quality Issue</option>
                  <option value="2">Safety Concern</option>
                  <option value="3">Counterfeit Suspicion</option>
                  <option value="4">Supply Chain Issue</option>
                  <option value="5">Customer Complaint</option>
                  <option value="6">Other</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Description <span className="text-red-500">*</span>
                </label>
                <textarea
                  value={formData.description}
                  onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
                  rows="4"
                  placeholder="Describe the issue in detail..."
                  required
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Priority
                </label>
                <select
                  value={formData.priority}
                  onChange={(e) => setFormData({ ...formData, priority: e.target.value })}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:outline-none"
                >
                  <option value="0">Low</option>
                  <option value="1">Medium</option>
                  <option value="2">High</option>
                  <option value="3">Critical</option>
                </select>
              </div>

              <div className="flex gap-3">
                <button
                  type="submit"
                  className="flex-1 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
                >
                  Update Ticket
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setShowEditModal(false);
                    setSelectedTicket(null);
                    setFormData({ category: '0', description: '', priority: '0' });
                  }}
                  className="flex-1 px-4 py-2 bg-gray-500 text-white rounded-lg hover:bg-gray-600 transition-colors"
                >
                  Cancel
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Tickets List */}
      {tickets.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <FiCheckCircle className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <p className="text-gray-600">No tickets created yet</p>
          <p className="text-sm text-gray-500 mt-1">Create a ticket to escalate issues to regulators</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 font-medium text-gray-700">Ticket ID</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Category</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Priority</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Status</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Created</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Linked Reports</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Actions</th>
              </tr>
            </thead>
            <tbody>
              {tickets.map((ticket) => (
                <tr key={ticket.id} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 font-mono text-sm">{ticket.id}</td>
                  <td className="py-3 px-4">{getCategoryText(ticket.category)}</td>
                  <td className="py-3 px-4">
                    <span className={`font-semibold ${getPriorityColor(ticket.priority)}`}>
                      {getPriorityText(ticket.priority)}
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    <span className={`inline-block px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(ticket.status)}`}>
                      {getStatusText(ticket.status)}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-sm">
                    {ticket.createdAt ? new Date(ticket.createdAt).toLocaleDateString() : 'N/A'}
                  </td>
                  <td className="py-3 px-4 text-center">
                    <span className="inline-block bg-gray-100 px-2 py-1 rounded text-sm">
                      {ticket.consumerReports?.length || 0}
                    </span>
                  </td>
                  <td className="py-3 px-4">
                    <div className="flex items-center gap-2">
                      <button
                        onClick={() => {
                          setSelectedTicket(ticket);
                          setShowLinkReports(true);
                        }}
                        className="text-blue-600 hover:text-blue-800 transition-colors flex items-center gap-1"
                        title="Link Reports"
                      >
                        <FiLink className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => handleEditTicket(ticket)}
                        className="text-green-600 hover:text-green-800 transition-colors"
                        title="Edit Ticket"
                      >
                        <FiEdit2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
