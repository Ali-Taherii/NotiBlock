import { deleteRecall } from "../api/recalls"
import { useState } from "react";

export default function DeleteRecallModal({ recall, onClose, refetch }) {
  const [status, setStatus] = useState(null)

  const handleDelete = async () => {
    try {
      await deleteRecall(recall.id)
      refetch() // Refetch recalls after deletion
      setStatus('Recall deleted successfully!')
      setTimeout(() => {
        setStatus(null)
        onClose()
      }, 500)
    } catch (err) {
      console.error(err)
      setStatus('Error deleting recall.')
    }
  }

  return (
    <div className="fixed inset-0 flex items-center justify-center bg-black">
      <div className="bg-white p-6 rounded shadow-lg">
        <h2 className="text-xl font-semibold mb-4">Delete Recall</h2>
        <p>Are you sure you want to delete the recall for Product: {recall.productId}?</p>
        <div className="mt-4 flex justify-end space-x-2">
          <button
            className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700"
            onClick={handleDelete}
          >
            Delete
          </button>
          <button
            className="bg-gray-300 text-gray-700 px-4 py-2 rounded hover:bg-gray-400"
            onClick={onClose}
          >
            Cancel
          </button>
        </div>
        {status && <p className="mt-4 text-green-600">{status}</p>}
      </div>
    </div>
  )
}