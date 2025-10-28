import { useState } from 'react'
import { updateTicketStatus } from '../../../api/tickets'

export default function RegulatorTicket({ ticket, refetch }) {
  const [loading, setLoading] = useState(false)
  const [status, setStatus] = useState(null)

  const isPending = ticket.status === 'Pending' || ticket.status === 'pending'

  const getStatusColor = (status) => {
    switch (status.toLowerCase()) {
      case 'approved':
        return 'text-green-600'
      case 'rejected':
        return 'text-red-600'
      case 'pending':
      default:
        return 'text-yellow-600'
    }
  }

  const handleStatusUpdate = async (newStatus) => {
    setLoading(true)
    setStatus(null)
    
    try {
      await updateTicketStatus(ticket.id, newStatus)
      setStatus(`Ticket ${newStatus} successfully!`)
      refetch() // Refresh the ticket list
      
      // Clear success message after 2 seconds
      setTimeout(() => {
        setStatus(null)
      }, 2000)
    } catch (err) {
      console.error(`Error ${newStatus} ticket:`, err)
      setStatus(`Error ${newStatus} ticket. Please try again.`)
      
      // Clear error message after 3 seconds
      setTimeout(() => {
        setStatus(null)
      }, 3000)
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
          <p>
            <strong>Status:</strong> 
            <span className={`ml-1 ${getStatusColor(ticket.status)}`}>
              {ticket.status}
            </span>
          </p>
          <p><strong>Submitted:</strong> {new Date(ticket.createdAt).toLocaleDateString()}</p>
          {ticket.updatedAt && ticket.updatedAt !== ticket.createdAt && (
            <p><strong>Last Updated:</strong> {new Date(ticket.updatedAt).toLocaleDateString()}</p>
          )}
          {ticket.userId && (
            <p><strong>Consumer ID:</strong> {ticket.userId}</p>
          )}
        </div>

        {isPending && (
          <div className="flex space-x-2 ml-4">
            <button
              onClick={() => handleStatusUpdate('approved')}
              disabled={loading}
              className="px-3 py-1 bg-green-600 text-white text-sm rounded hover:bg-green-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              title="Approve ticket"
            >
              {loading ? 'Processing...' : 'Approve'}
            </button>
            <button
              onClick={() => handleStatusUpdate('rejected')}
              disabled={loading}
              className="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              title="Reject ticket"
            >
              {loading ? 'Processing...' : 'Reject'}
            </button>
          </div>
        )}
      </div>

      {status && (
        <div className={`mt-2 text-sm ${status.includes('Error') ? 'text-red-600' : 'text-green-600'}`}>
          {status}
        </div>
      )}
    </div>
  )
}