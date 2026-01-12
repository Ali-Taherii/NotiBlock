import { useState, useEffect } from 'react';
import { getPendingTickets as getAllTickets } from '../../../api/regulatorReviews';
import { getTicketById } from '../../../api/resellerTickets';
import { createReview } from '../../../api/regulatorReviews';
import { useToast } from '../../../hooks/useToast';
import { FiFileText, FiClock, FiCheckCircle, FiXCircle, FiAlertCircle, FiEye } from 'react-icons/fi';

export default function TicketsSection() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedTicket, setSelectedTicket] = useState(null);
  const [showReviewModal, setShowReviewModal] = useState(false);
  const [showReportsModal, setShowReportsModal] = useState(false);
  const [linkedReports, setLinkedReports] = useState([]);
  const [loadingReports, setLoadingReports] = useState(false);
  const [decision, setDecision] = useState('');
  const [notes, setNotes] = useState('');
  const toast = useToast();

  const categoryLabels = {
    0: 'Safety Issue',
    1: 'Quality Issue',
    2: 'Defect',
    3: 'Mislabeling',
    4: 'Contamination',
    5: 'Performance Issue',
    6: 'Other'
  };

  const statusLabels = {
    0: { label: 'Open', icon: FiClock, color: 'blue' },
    1: { label: 'In Review', icon: FiAlertCircle, color: 'yellow' },
    2: { label: 'Resolved', icon: FiCheckCircle, color: 'green' },
    3: { label: 'Closed', icon: FiXCircle, color: 'gray' },
    4: { label: 'Escalated', icon: FiAlertCircle, color: 'red' }
  };

  const priorityLabels = {
    0: { label: 'Low', color: 'text-gray-600' },
    1: { label: 'Medium', color: 'text-blue-600' },
    2: { label: 'High', color: 'text-orange-600' },
    3: { label: 'Critical', color: 'text-red-600' }
  };

  useEffect(() => {
    fetchTickets();
  }, []);

  async function fetchTickets() {
    try {
      setLoading(true);
      const response = await getAllTickets();
      // Handle different response structures
      const ticketsData = response.data?.items || response.data?.data || response.data || [];
      setTickets(Array.isArray(ticketsData) ? ticketsData : []);
    } catch (error) {
      console.error('Error fetching tickets:', error);
      toast.error('Failed to load tickets');
      setTickets([]);
    } finally {
      setLoading(false);
    }
  }

  function openReviewModal(ticket) {
    setSelectedTicket(ticket);
    setDecision('');
    setNotes('');
    setShowReviewModal(true);
  }

  async function openReportsModal(ticket) {
    setSelectedTicket(ticket);
    setShowReportsModal(true);
    setLoadingReports(true);
    
    try {
        const response = await getTicketById(ticket.id);
        const ticketData = response.data?.data || response.data;
      setLinkedReports(ticketData.consumerReports || []);
    } catch (error) {
      console.error('Error fetching linked reports:', error);
      toast.error('Failed to load linked reports');
      setLinkedReports([]);
    } finally {
      setLoadingReports(false);
    }
  }

  async function handleCreateReview(e) {
    e.preventDefault();
    
    if (!decision || !notes.trim()) {
      toast.error('Please fill in all fields');
      return;
    }

    if (notes.trim().length < 10) {
      toast.error('Notes must be at least 10 characters');
      return;
    }

    try {
      await createReview({
        ticketId: selectedTicket.id,
        decision: parseInt(decision),
        notes: notes.trim()
      });
      
      toast.success('Review submitted successfully');
      setShowReviewModal(false);
      setSelectedTicket(null);
      setDecision('');
      setNotes('');
      fetchTickets();
    } catch (error) {
      console.error('Error creating review:', error);
      toast.error(error.response?.data?.message || 'Failed to submit review');
    }
  }

  if (loading) {
    return <div className="p-4">Loading tickets...</div>;
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold">All Tickets</h2>
        <button
          onClick={fetchTickets}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Refresh
        </button>
      </div>

      {tickets.length === 0 ? (
        <p className="text-gray-600">No tickets found.</p>
      ) : (
        <div className="space-y-4">
          {tickets.map((ticket) => {
            const statusInfo = statusLabels[ticket.status] || statusLabels[0];
            const StatusIcon = statusInfo.icon;
            const priorityInfo = priorityLabels[ticket.priority] || priorityLabels[0];

            return (
              <div key={ticket.id} className="border rounded-lg p-5 bg-white shadow-sm">
                {/* Top Section - Subject and Metadata in Columns */}
                <div className="flex justify-between items-start mb-4">
                  <div className="flex-1 mr-8">
                    <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide block mb-1">Subject</span>
                    <h3 className="font-semibold text-lg text-gray-800">{ticket.subject}</h3>
                  </div>
                  
                  <div className="flex gap-6 text-sm">
                    <div>
                      <span className="text-xs font-semibold text-gray-500 block mb-0.5">Created</span>
                      <p className="text-gray-700">{new Date(ticket.createdAt).toLocaleDateString()}</p>
                    </div>
                    {ticket.consumerReports.length > 0 && (
                      <div>
                        <span className="text-xs font-semibold text-gray-500 block mb-0.5">Linked Reports</span>
                        <p className="text-gray-700 flex items-center gap-1">
                          <FiFileText className="w-4 h-4" />
                          {ticket.consumerReports.length}
                        </p>
                      </div>
                    )}
                  </div>
                </div>

                {/* Badges */}
                <div className="flex gap-2 text-sm flex-wrap mb-4">
                  <span className={`inline-flex items-center gap-1 px-2.5 py-1 rounded-full font-medium bg-${statusInfo.color}-100 text-${statusInfo.color}-700`}>
                    <StatusIcon className="w-3.5 h-3.5" />
                    {statusInfo.label}
                  </span>
                  <span className={`inline-flex items-center px-2.5 py-1 rounded-full font-medium ${priorityInfo.color === 'text-gray-600' ? 'bg-gray-100 text-gray-700' : priorityInfo.color === 'text-blue-600' ? 'bg-blue-100 text-blue-700' : priorityInfo.color === 'text-orange-600' ? 'bg-orange-100 text-orange-700' : 'bg-red-100 text-red-700'}`}>
                    {priorityInfo.label} Priority
                  </span>
                  <span className="inline-flex items-center px-2.5 py-1 rounded-full font-medium bg-purple-100 text-purple-700">
                    {categoryLabels[ticket.category]}
                  </span>
                </div>

                {/* Description */}
                <div className="mb-4">
                  <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide block mb-1">Description</span>
                  <p className="text-gray-700 leading-relaxed">{ticket.description}</p>
                </div>

                {/* Action Buttons */}
                <div className="flex gap-3">
                  {ticket.status !== 2 && ticket.status !== 3 && (
                    <button
                      onClick={() => openReviewModal(ticket)}
                      className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 transition-colors"
                    >
                      Create Review
                    </button>
                  )}
                  {ticket.consumerReports.length > 0 && (
                    <button
                      onClick={() => openReportsModal(ticket)}
                      className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors flex items-center gap-2"
                    >
                      <FiEye />
                      View Linked Reports ({ticket.consumerReports.length})
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Review Modal */}
      {showReviewModal && selectedTicket && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full mx-4">
            <h3 className="text-xl font-semibold mb-4">Create Review for Ticket</h3>
            
            <div className="mb-4 p-3 bg-gray-50 rounded">
              <p className="font-semibold">{selectedTicket.subject}</p>
              <p className="text-sm text-gray-600 mt-1">{selectedTicket.description}</p>
            </div>

            <form onSubmit={handleCreateReview}>
              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">
                  Decision <span className="text-red-500">*</span>
                </label>
                <select
                  value={decision}
                  onChange={(e) => setDecision(e.target.value)}
                  className="w-full border rounded px-3 py-2"
                  required
                >
                  <option value="">Select Decision</option>
                  <option value="0">Approve</option>
                  <option value="1">Reject</option>
                  <option value="2">Needs More Info</option>
                  <option value="3">Reopen</option>
                </select>
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">
                  Review Notes <span className="text-red-500">*</span>
                </label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  className="w-full border rounded px-3 py-2"
                  rows="4"
                  placeholder="Enter your review notes (minimum 10 characters)..."
                  required
                  minLength={10}
                  maxLength={2000}
                />
                <p className="text-sm text-gray-500 mt-1">
                  {notes.length}/2000 characters (minimum 10)
                </p>
              </div>

              <div className="flex justify-end gap-3">
                <button
                  type="button"
                  onClick={() => {
                    setShowReviewModal(false);
                    setSelectedTicket(null);
                    setDecision('');
                    setNotes('');
                  }}
                  className="px-4 py-2 border rounded hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  Submit Review
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Linked Reports Modal */}
      {showReportsModal && selectedTicket && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg p-6 max-w-4xl w-full max-h-[80vh] overflow-y-auto">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="text-xl font-semibold">Linked Consumer Reports</h3>
                <p className="text-sm text-gray-600 mt-1">Ticket: {selectedTicket.id}</p>
              </div>
              <button
                onClick={() => {
                  setShowReportsModal(false);
                  setSelectedTicket(null);
                  setLinkedReports([]);
                }}
                className="text-gray-400 hover:text-gray-600"
              >
                <FiXCircle className="w-6 h-6" />
              </button>
            </div>

            {loadingReports ? (
              <div className="text-center py-8">
                <p className="text-gray-600">Loading reports...</p>
              </div>
            ) : linkedReports.length === 0 ? (
              <div className="text-center py-8">
                <p className="text-gray-600">No reports linked to this ticket.</p>
              </div>
            ) : (
              <div className="space-y-4">
                {linkedReports.map((report, index) => (
                  <div key={report.id} className="border rounded-lg p-4 bg-gray-50">
                    <div className="flex justify-between items-start mb-3">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <span className="font-semibold text-gray-700">Report #{index + 1}</span>
                          <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                            report.status === 0 ? 'bg-yellow-100 text-yellow-700' :
                            report.status === 1 ? 'bg-blue-100 text-blue-700' :
                            report.status === 2 ? 'bg-red-100 text-red-700' :
                            report.status === 3 ? 'bg-green-100 text-green-700' :
                            'bg-gray-100 text-gray-700'
                          }`}>
                            {report.status === 0 ? 'Pending' :
                             report.status === 1 ? 'Under Review' :
                             report.status === 2 ? 'Escalated to Regulator' :
                             report.status === 3 ? 'Resolved' : 'Closed'}
                          </span>
                        </div>
                        <p className="text-sm text-gray-600 mb-2">
                          Product: {report.serialNumber || 'N/A'}
                        </p>
                      </div>
                      <div className="text-right text-sm text-gray-500">
                        <p>Submitted: {new Date(report.createdAt).toLocaleDateString()}</p>
                      </div>
                    </div>

                    <div className="mb-2">
                      <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide block mb-1">
                        Description
                      </span>
                      <p className="text-gray-700">{report.description}</p>
                    </div>

                    {report.evidenceUrl && (
                      <div>
                        <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide block mb-1">
                          Evidence
                        </span>
                        <a
                          href={report.evidenceUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          className="text-blue-600 hover:underline text-sm"
                        >
                          View Evidence
                        </a>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            )}

            <div className="mt-6 flex justify-end">
              <button
                onClick={() => {
                  setShowReportsModal(false);
                  setSelectedTicket(null);
                  setLinkedReports([]);
                }}
                className="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700"
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
