import { useState } from 'react'
import EditRecallModal from './EditRecallModal'
import DeleteRecallModal from './DeleteRecallModal'

export default function RecallList({ recalls }) {
  const [selectedRecall, setSelectedRecall] = useState(null)
  const [modalType, setModalType] = useState(null) // NEW: "edit" or "delete"

  const openEditModal = (recall) => {
    setSelectedRecall(recall)
    setModalType('edit')
  }

  const openDeleteModal = (recall) => {
    setSelectedRecall(recall)
    setModalType('delete')
  }

  const closeModal = () => {
    setSelectedRecall(null)
    setModalType(null)
  }

  return (
    <div className="space-y-4">
      {recalls.length === 0 && (
        <p className="text-gray-600">No recalls found.</p>
      )}

      {recalls.map(recall => (
        <div 
          key={recall.id} 
          className="p-4 border border-gray-300 rounded shadow-sm bg-white"
        >
          <h3 className="text-lg font-semibold">
            Product ID: {recall.productId}
          </h3>
          <p className="text-gray-700">Reason: {recall.reason}</p>
          <p className="text-gray-700">Action Required: {recall.actionRequired}</p>

          <button 
            className="mt-2 mr-2 inline-block bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700"
            onClick={() => openEditModal(recall)}
          >
            Edit
          </button>
          <button 
            className="mt-2 inline-block bg-red-600 text-white px-3 py-1 rounded hover:bg-red-700"
            onClick={() => openDeleteModal(recall)}
          >
            Delete
          </button>
        </div>
      ))}

      {/* Correct modal based on type */}
      {selectedRecall && modalType === 'edit' && (
        <EditRecallModal
          recall={selectedRecall}
          onClose={closeModal}
        />
      )}
      {selectedRecall && modalType === 'delete' && (
        <DeleteRecallModal
          recall={selectedRecall}
          onClose={closeModal}
        />
      )}
    </div>
  )
}
