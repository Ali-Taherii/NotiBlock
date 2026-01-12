import { useState } from 'react'

export default function ManufacturerTicket({ ticket, onCreateRecall }) {
  const [loading, setLoading] = useState(false)

  const handleCreateRecall = async () => {
    setLoading(true)
    try {
      await onCreateRecall(ticket)
    } catch (err) {
      console.error('Error creating recall:', err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="p-4 border rounded shadow-sm bg-white">
      <div className="flex justify-between items-start">
        <div className="flex-1">
          <p><strong>Product Serial:</strong> {ticket.productId || ticket.id}</p>
          <p><strong>Issue:</strong> {ticket.issueDescription}</p>
          <p><strong>Status:</strong> <span className="text-green-600">{ticket.status}</span></p>
          <p><strong>Approved:</strong> {new Date(ticket.updatedAt || ticket.createdAt).toLocaleDateString()}</p>
          {ticket.userId && (
            <p><strong>Consumer ID:</strong> {ticket.userId}</p>
          )}
        </div>

        <div className="ml-4">
          <button
            onClick={handleCreateRecall}
            disabled={loading}
            className="px-3 py-1 bg-blue-600 text-white text-sm rounded hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            title="Issue recall for this product"
          >
            {loading ? 'Processing...' : 'Issue Recall'}
          </button>
        </div>
      </div>
    </div>
  )
}