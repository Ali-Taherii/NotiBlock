import { useState } from 'react'
import { updateTicket } from '../../../api/tickets'

export default function EditTicketModal({ ticket, onClose, refetch }) {
  const [productId, setProductId] = useState(ticket.productId || '')
  const [issueDescription, setIssueDescription] = useState(ticket.issueDescription || '')
  const [status, setStatus] = useState(null)
  const [isLoading, setIsLoading] = useState(false)

  const handleUpdate = async (e) => {
    e.preventDefault()

    setIsLoading(true)
    setStatus(null)

    try {
      await updateTicket(ticket.id, { productId, issueDescription })
      refetch() // Refetch tickets after update
      setStatus('Ticket updated successfully!')
      setTimeout(() => {
        setStatus(null)
        onClose()
      }, 1000)
    } catch (err) {
      console.error(err)
      setStatus('Error updating ticket. Please try again.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center z-50">
      <div className="bg-white p-6 rounded shadow-lg max-w-md w-full mx-4">
        <h2 className="text-xl font-bold mb-4">Edit Ticket</h2>

        <form onSubmit={handleUpdate} className="space-y-4">
          <div>
            <label className="block mb-1 font-medium">Product Serial/ID</label>
            <input 
              type="text" 
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
              value={productId}
              onChange={(e) => setProductId(e.target.value)}
              placeholder="Enter product serial or ID"
              disabled={isLoading}
            />
          </div>
          
          <div>
            <label className="block mb-1 font-medium">Issue Description</label>
            <textarea 
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500 min-h-[100px]"
              value={issueDescription}
              onChange={(e) => setIssueDescription(e.target.value)}
              placeholder="Describe the issue with the product"
              disabled={isLoading}
            />
          </div>

          <div className="flex justify-end space-x-2 pt-4">
            <button 
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-400 rounded hover:bg-gray-50 transition-colors"
              disabled={isLoading}
            >
              Cancel
            </button>
            <button 
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
              disabled={isLoading}
            >
              {isLoading ? 'Saving...' : 'Save Changes'}
            </button>
          </div>

          {status && (
            <p className={`text-sm mt-2 ${status.includes('Error') ? 'text-red-600' : 'text-green-600'}`}>
              {status}
            </p>
          )}
        </form>
      </div>
    </div>
  )
}