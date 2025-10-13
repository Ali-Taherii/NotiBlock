import { useState } from 'react'
import EditTicketModal from './EditTicketModal'
import DeleteTicketModal from './DeleteTicketModal'

export default function Ticket({ ticket, refetch }) {
  const [showEditModal, setShowEditModal] = useState(false)
  const [showDeleteModal, setShowDeleteModal] = useState(false)

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

  return (
    <>
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
          </div>

          {isPending && (
            <div className="flex space-x-2 ml-4">
              <button
                onClick={() => setShowEditModal(true)}
                className="px-3 py-1 bg-blue-600 text-white text-sm rounded hover:bg-blue-700 transition-colors"
                title="Edit ticket"
              >
                Edit
              </button>
              <button
                onClick={() => setShowDeleteModal(true)}
                className="px-3 py-1 bg-red-600 text-white text-sm rounded hover:bg-red-700 transition-colors"
                title="Delete ticket"
              >
                Delete
              </button>
            </div>
          )}
        </div>
      </div>

      {showEditModal && (
        <EditTicketModal
          ticket={ticket}
          onClose={() => setShowEditModal(false)}
          refetch={refetch}
        />
      )}

      {showDeleteModal && (
        <DeleteTicketModal
          ticket={ticket}
          onClose={() => setShowDeleteModal(false)}
          refetch={refetch}
        />
      )}
    </>
  )
}