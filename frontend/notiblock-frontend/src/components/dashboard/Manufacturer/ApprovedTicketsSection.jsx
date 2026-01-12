import { useState, useEffect } from 'react';
import { getTicketsByStatus } from '../../../api/resellerTickets';
import { createRecall } from '../../../api/recalls';
import { QRCodeSVG } from 'qrcode.react';
import { FiAlertTriangle, FiCheckCircle } from 'react-icons/fi';

export default function ApprovedTicketsSection() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedTicket, setSelectedTicket] = useState(null);
  const [recallForm, setRecallForm] = useState({
    reason: '',
    actionRequired: '',
  });
  const [recallData, setRecallData] = useState(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [status, setStatus] = useState(null);

  useEffect(() => {
    fetchApprovedTickets();
  }, []);

  const fetchApprovedTickets = async () => {
    try {
      setLoading(true);
      const response = await getTicketsByStatus('1'); // Status 1 = Approved
      // Handle paginated response format: { success, data: { items, totalCount, ... } }
      const items = response?.data?.items || response?.items || response || [];
      setTickets(Array.isArray(items) ? items : []);
    } catch (err) {
      console.error('Error fetching approved tickets:', err);
      setError('Failed to load approved tickets');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateRecall = async (e) => {
    e.preventDefault();
    
    if (!recallForm.reason.trim() || !recallForm.actionRequired.trim()) {
      setStatus('Please fill in all fields.');
      return;
    }

    setIsSubmitting(true);
    setStatus(null);

    try {
      const recallPayload = {
        productSerialNumber: selectedTicket.productSerialNumber || selectedTicket.serialNumber,
        reason: recallForm.reason.trim(),
        actionRequired: recallForm.actionRequired.trim(),
      };

      const result = await createRecall(recallPayload);
      
      const qrPayload = JSON.stringify({
        recallId: result.id || result.data?.id,
        productSerial: selectedTicket.productSerialNumber || selectedTicket.serialNumber,
        recallReason: recallForm.reason,
        actionRequired: recallForm.actionRequired,
        timestamp: new Date().toISOString(),
      });

      setRecallData({
        recall: result,
        qrData: qrPayload,
      });
      
      setStatus('Recall created successfully!');
      fetchApprovedTickets();
    } catch (err) {
      console.error('Error creating recall:', err);
      setStatus(err.message || 'Error creating recall. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleSelectTicket = (ticket) => {
    setSelectedTicket(ticket);
    setRecallForm({ reason: '', actionRequired: '' });
    setRecallData(null);
    setStatus(null);
  };

  const handleCancel = () => {
    setSelectedTicket(null);
    setRecallData(null);
    setStatus(null);
  };

  if (loading) {
    return <div className="text-center py-8">Loading approved tickets...</div>;
  }

  if (error) {
    return <div className="text-red-600 py-8">{error}</div>;
  }

  if (selectedTicket) {
    return (
      <div className="max-w-3xl mx-auto">
        <div className="bg-white p-6 rounded-lg shadow border">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-bold">Create Recall</h2>
            <button
              onClick={handleCancel}
              className="text-gray-500 hover:text-gray-700 text-xl"
            >
              ✕
            </button>
          </div>
          
          <div className="mb-6 p-4 bg-yellow-50 border border-yellow-200 rounded">
            <h3 className="font-semibold mb-2 flex items-center gap-2">
              <FiAlertTriangle className="text-yellow-600" />
              Ticket Details
            </h3>
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <span className="font-medium">Product Serial:</span>
                <p className="text-gray-700">{selectedTicket.productSerialNumber || 'N/A'}</p>
              </div>
              <div>
                <span className="font-medium">Category:</span>
                <p className="text-gray-700">{selectedTicket.category || 'N/A'}</p>
              </div>
              <div className="col-span-2">
                <span className="font-medium">Issue Description:</span>
                <p className="text-gray-700">{selectedTicket.description || 'N/A'}</p>
              </div>
            </div>
          </div>

          <form onSubmit={handleCreateRecall} className="space-y-4">
            <div>
              <label className="block mb-1 font-medium">Recall Reason *</label>
              <textarea
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                value={recallForm.reason}
                onChange={(e) => setRecallForm({ ...recallForm, reason: e.target.value })}
                placeholder="Describe the reason for the recall..."
                rows="4"
                required
              />
            </div>

            <div>
              <label className="block mb-1 font-medium">Action Required *</label>
              <textarea
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                value={recallForm.actionRequired}
                onChange={(e) => setRecallForm({ ...recallForm, actionRequired: e.target.value })}
                placeholder="What action should consumers take?"
                rows="4"
                required
              />
            </div>

            <div className="flex gap-3">
              <button 
                type="submit" 
                disabled={isSubmitting}
                className="flex items-center gap-2 bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                <FiAlertTriangle />
                {isSubmitting ? 'Issuing Recall...' : 'Issue Recall'}
              </button>
              <button
                type="button"
                onClick={handleCancel}
                className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
              >
                Cancel
              </button>
            </div>

            {status && (
              <div className={`p-3 rounded ${status.includes('Error') ? 'bg-red-50 text-red-700' : 'bg-green-50 text-green-700'}`}>
                {status}
              </div>
            )}
          </form>
        </div>

        {recallData && (
          <div className="mt-6 bg-white p-6 rounded-lg shadow border">
            <div className="flex items-center gap-2 mb-4">
              <FiCheckCircle className="text-green-600 text-2xl" />
              <h3 className="text-lg font-bold text-green-600">Recall Created Successfully!</h3>
            </div>
            
            <div className="mb-6 space-y-2">
              <p><strong>Recall ID:</strong> {recallData.recall.id || recallData.recall.data?.id}</p>
              <p><strong>Product Serial:</strong> {selectedTicket.productSerialNumber}</p>
              <p><strong>Status:</strong> <span className="text-orange-600">Active</span></p>
              <p><strong>Created:</strong> {new Date().toLocaleString()}</p>
            </div>

            <div className="text-center p-6 bg-gray-50 rounded">
              <h4 className="font-medium mb-3">QR Code for Recall Information</h4>
              <div className="inline-block p-4 bg-white border-2 border-gray-300 rounded">
                <QRCodeSVG value={recallData.qrData} size={200} />
              </div>
              <p className="text-xs mt-3 text-gray-600">
                Scan this QR code to view recall details
              </p>
            </div>

            <div className="mt-6 text-center">
              <button
                onClick={() => {
                  setSelectedTicket(null);
                  setRecallData(null);
                }}
                className="bg-blue-600 text-white px-6 py-2 rounded hover:bg-blue-700"
              >
                Back to Approved Tickets
              </button>
            </div>
          </div>
        )}
      </div>
    );
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">Approved Tickets</h2>
        <button
          onClick={fetchApprovedTickets}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Refresh
        </button>
      </div>

      {tickets.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg border border-dashed border-gray-300">
          <FiCheckCircle className="mx-auto text-gray-400 text-5xl mb-3" />
          <p className="text-gray-600">No approved tickets found.</p>
          <p className="text-sm text-gray-500 mt-1">Tickets approved by regulators will appear here.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {tickets.map((ticket) => (
            <div key={ticket.id} className="bg-white p-5 rounded-lg shadow border hover:border-blue-300 transition-colors">
              <div className="flex justify-between items-start">
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-2">
                    <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded">
                      {ticket.status}
                    </span>
                    <span className="px-2 py-1 bg-blue-100 text-blue-800 text-xs font-medium rounded">
                      {ticket.category}
                    </span>
                  </div>
                  <p className="mb-1"><strong>Product Serial:</strong> {ticket.productSerialNumber || 'N/A'}</p>
                  <p className="mb-1"><strong>Description:</strong> {ticket.description}</p>
                  <p className="text-sm text-gray-600">
                    <strong>Approved:</strong> {new Date(ticket.updatedAt || ticket.createdAt).toLocaleDateString()}
                  </p>
                </div>
                <button
                  onClick={() => handleSelectTicket(ticket)}
                  className="ml-4 flex items-center gap-2 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
                >
                  <FiAlertTriangle />
                  Issue Recall
                </button>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
