import { useState, useEffect } from 'react';
import { getApprovedManufacturerTickets } from '../../../api/regulatorReviews';
import { createRecall } from '../../../api/recalls';
import { FiAlertTriangle, FiCheckCircle, FiPackage, FiClock } from 'react-icons/fi';

export default function ApprovedTicketsSection() {
  const [tickets, setTickets] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [selectedTicket, setSelectedTicket] = useState(null);
  const [selectedProduct, setSelectedProduct] = useState(null);
  const [recallForm, setRecallForm] = useState({
    reason: '',
    actionRequired: '',
  });
  const [recallSubmitted, setRecallSubmitted] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [status, setStatus] = useState(null);

  useEffect(() => {
    fetchApprovedTickets();
  }, []);

  const fetchApprovedTickets = async () => {
    try {
      setLoading(true);
      const response = await getApprovedManufacturerTickets();
      const items = response?.data?.items || response?.items || response || [];
      setTickets(Array.isArray(items) ? items : []);
    } catch (err) {
      console.error('Error fetching approved tickets:', err);
      setError('Failed to load approved tickets');
    } finally {
      setLoading(false);
    }
  };

  const getProductsFromTicket = (ticket) => {
    const consumerReports = ticket?.consumerReports || [];
    const reportsArray = Array.isArray(consumerReports) ? consumerReports : [consumerReports];

    const products = reportsArray.flatMap(report => {
      const reportProducts = report?.product || [];
      return Array.isArray(reportProducts) ? reportProducts : [reportProducts];
    }).filter(Boolean); 

    return products;
  };

  const handleCreateRecall = async (e) => {
    e.preventDefault();
    
    if (!recallForm.reason.trim() || !recallForm.actionRequired.trim()) {
      setStatus('Please fill in all fields.');
      return;
    }

    if (!selectedProduct) {
      setStatus('Please select a product.');
      return;
    }

    setIsSubmitting(true);
    setStatus(null);

    try {
      const recallPayload = {
        productId: selectedProduct.serialNumber,
        reason: recallForm.reason.trim(),
        actionRequired: recallForm.actionRequired.trim(),
      };

      await createRecall(recallPayload);
      setRecallSubmitted(true);
      setStatus('Recall submitted for regulator approval. You will be notified once it is activated.');
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
    setSelectedProduct(null);
    setRecallForm({ reason: '', actionRequired: '' });
    setRecallSubmitted(false);
    setStatus(null);
  };

  const handleCancel = () => {
    setSelectedTicket(null);
    setSelectedProduct(null);
    setRecallSubmitted(false);
    setStatus(null);
  };

  if (loading) {
    return <div className="text-center py-8">Loading approved tickets...</div>;
  }

  if (error) {
    return <div className="text-red-600 py-8">{error}</div>;
  }

  if (selectedTicket) {
    const products = getProductsFromTicket(selectedTicket);

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
            <h3 className="font-semibold mb-3 flex items-center gap-2">
              <FiAlertTriangle className="text-yellow-600" />
              Ticket Details
            </h3>
            
            {/* Products Section */}
            <div className="mb-4">
              <span className="block font-medium mb-2">Affected Products:</span>
              <div className="flex flex-wrap gap-2">
                {products.length > 0 ? (
                  products.map((product, idx) => (
                    <button
                      key={idx}
                      onClick={() => setSelectedProduct(product)}
                      disabled={recallSubmitted}
                      className={`flex items-center gap-2 px-3 py-2 rounded-lg border-2 transition-all ${
                        selectedProduct?.serialNumber === product.serialNumber
                          ? 'border-blue-500 bg-blue-50 text-blue-800'
                          : 'border-gray-300 bg-white hover:border-blue-300 hover:bg-blue-50'
                      } ${recallSubmitted ? 'opacity-50 cursor-not-allowed' : ''}`}
                    >
                      <FiPackage className={selectedProduct?.serialNumber === product.serialNumber ? 'text-blue-600' : 'text-gray-600'} />
                      <div className="text-left">
                        <div className="font-medium text-sm">{product.model || product.name || 'Unknown Product'}</div>
                        <div className="text-xs text-gray-600">S/N: {product.serialNumber}</div>
                      </div>
                    </button>
                  ))
                ) : (
                  <p className="text-gray-500 text-sm">No products associated with this ticket</p>
                )}
              </div>
              {selectedProduct && (
                <p className="mt-2 text-sm text-blue-700 flex items-center gap-1">
                  <FiCheckCircle /> Selected: {selectedProduct.model || selectedProduct.name} ({selectedProduct.serialNumber})
                </p>
              )}
            </div>

            <div className="grid grid-cols-2 gap-3 text-sm border-t pt-3">
              <div>
                <span className="font-medium">Category:</span>
                <p className="text-gray-700">{selectedTicket.category || 'N/A'}</p>
              </div>
              <div>
                <span className="font-medium">Status:</span>
                <p className="text-gray-700">{selectedTicket.status || 'N/A'}</p>
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
                disabled={recallSubmitted}
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
                disabled={recallSubmitted}
              />
            </div>

            <div className="flex gap-3">
              {!recallSubmitted ? (
                <>
                  <button 
                    type="submit" 
                    disabled={isSubmitting || !selectedProduct}
                    className="flex items-center gap-2 bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 disabled:opacity-50 disabled:cursor-not-allowed"
                  >
                    <FiAlertTriangle />
                    {isSubmitting ? 'Submitting...' : 'Submit Recall Proposal'}
                  </button>
                  <button
                    type="button"
                    onClick={handleCancel}
                    className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
                  >
                    Cancel
                  </button>
                </>
              ) : (
                <button
                  type="button"
                  onClick={handleCancel}
                  className="bg-gray-700 text-white px-4 py-2 rounded hover:bg-gray-800"
                >
                  Close
                </button>
              )}
            </div>

            {status && (
              <div className={`p-3 rounded ${
                status.includes('Error') || status.includes('Please') 
                  ? 'bg-red-50 text-red-700' 
                  : status.includes('successfully') || status.includes('🎉')
                  ? 'bg-green-50 text-green-700'
                  : 'bg-blue-50 text-blue-700'
              }`}>
                {status}
              </div>
            )}
          </form>

          {recallSubmitted && (
            <div className="mt-4 p-4 bg-blue-50 border border-blue-200 rounded flex items-start gap-3">
              <FiClock className="text-blue-600 text-xl" />
              <div>
                <p className="font-semibold text-blue-900">Awaiting regulator approval</p>
                <p className="text-sm text-blue-800">Regulators will review this recall, activate it on-chain, and notify all stakeholders once approved.</p>
              </div>
            </div>
          )}
        </div>
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
          {tickets.map((ticket) => {
            const products = getProductsFromTicket(ticket);
            
            return (
              <div key={ticket.id} className="bg-white p-5 rounded-lg shadow border hover:border-blue-300 transition-colors">
                <div className="mb-3">
                  <div className="flex items-center gap-2 mb-3">
                    <span className="px-2 py-1 bg-green-100 text-green-800 text-xs font-medium rounded">
                      {ticket.status}
                    </span>
                    <span className="px-2 py-1 bg-blue-100 text-blue-800 text-xs font-medium rounded">
                      {ticket.category}
                    </span>
                  </div>
                  
                  {/* Products Display */}
                  <div className="mb-3">
                    <span className="text-sm font-medium text-gray-700 block mb-2">Affected Products:</span>
                    <div className="flex flex-wrap gap-2">
                      {products.length > 0 ? (
                        products.map((product, idx) => (
                          <div
                            key={idx}
                            className="flex items-center gap-2 px-3 py-1.5 bg-gray-50 border border-gray-200 rounded-lg text-sm"
                          >
                            <FiPackage className="text-gray-600" />
                            <div>
                              <span className="font-medium">{product.model || product.name || 'Unknown'}</span>
                              <span className="text-gray-500 ml-2 text-xs">S/N: {product.serialNumber}</span>
                            </div>
                          </div>
                        ))
                      ) : (
                        <span className="text-gray-400 text-sm">No products listed</span>
                      )}
                    </div>
                  </div>

                  <p className="mb-2"><strong>Description:</strong> {ticket.description}</p>
                  <p className="text-sm text-gray-600">
                    <strong>Approved:</strong> {new Date(ticket.updatedAt || ticket.createdAt).toLocaleDateString()}
                  </p>
                </div>
                
                <button
                  onClick={() => handleSelectTicket(ticket)}
                  className="w-full flex items-center justify-center gap-2 px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
                >
                  <FiAlertTriangle />
                  Issue Recall
                </button>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
