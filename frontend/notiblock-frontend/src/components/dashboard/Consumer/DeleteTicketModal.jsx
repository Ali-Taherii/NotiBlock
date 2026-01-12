import { useState } from "react"
import { deleteTicket } from "../../../api/tickets"

export default function DeleteTicketModal({ ticket, onClose, refetch }) {
  const [status, setStatus] = useState(null)
  const [isLoading, setIsLoading] = useState(false)

  const handleDelete = async () => {
    setIsLoading(true)
    
    try {
      await deleteTicket(ticket.id)
      refetch() // Refetch tickets after deletion
      setStatus('Ticket deleted successfully!')
      
      setTimeout(() => {
        setStatus(null)
        onClose()
      }, 1000)
    } catch (err) {
      console.error(err)
      setStatus('Error deleting ticket. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white p-6 rounded shadow-lg max-w-md w-full mx-4">
        <h2 className="text-xl font-semibold mb-4">Delete Ticket</h2>
        
        <div className="mb-6">
          <p className="text-gray-700 mb-2">
            Are you sure you want to delete this ticket?
          </p>
          <div className="bg-gray-50 p-3 rounded border">
            <p><strong>Product:</strong> {ticket.productId || ticket.id}</p>
            <p><strong>Issue:</strong> {ticket.issueDescription}</p>
          </div>
          <p className="text-sm text-red-600 mt-2">
            This action cannot be undone.
          </p>
        </div>

        <div className="flex justify-end space-x-2">
          <button
            className="px-4 py-2 border border-gray-400 rounded hover:bg-gray-50 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            onClick={onClose}
            disabled={isLoading}
          >
            Cancel
          </button>
          <button
            className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            onClick={handleDelete}
            disabled={isLoading}
          >
            {isLoading ? 'Deleting...' : 'Delete Ticket'}
          </button>
        </div>

        {status && (
          <p className={`text-sm mt-4 ${status.includes('Error') ? 'text-red-600' : 'text-green-600'}`}>
            {status}
          </p>
        )}
      </div>
    </div>
  )
}