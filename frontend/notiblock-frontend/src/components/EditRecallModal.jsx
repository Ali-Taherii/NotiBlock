import { useState } from 'react'
import { updateRecall } from '../api/recalls'

export default function EditRecallModal({ recall, onClose, refetch }) {
  const [productId, setProductId] = useState(recall.productId)
  const [reason, setReason] = useState(recall.reason)
  const [actionRequired, setActionRequired] = useState(recall.actionRequired)
  const [status, setStatus] = useState(null)

  const handleUpdate = async (e) => {
    e.preventDefault()

    try {
        await updateRecall(recall.id, {
        productId,
        reason,
        actionRequired,
      })
        
      refetch() // Refetch recalls after update
      
      setStatus('Recall updated successfully!')
        
      // Optionally close modal after success:
      setTimeout(() => {
        setStatus(null)
        onClose()
      }, 500)
    } catch (err) {
      console.error(err)
      setStatus('Error updating recall.')
    }
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-40 flex items-center justify-center">
      <div className="bg-white p-6 rounded shadow-lg max-w-md w-full">
        <h2 className="text-xl font-bold mb-4">Edit Recall</h2>

        <form onSubmit={handleUpdate} className="space-y-4">
          <div>
            <label className="block mb-1 font-medium">Product ID</label>
            <input 
              type="text" 
              className="w-full border border-gray-300 rounded px-3 py-2"
              value={productId}
              onChange={(e) => setProductId(e.target.value)}
            />
          </div>
          <div>
            <label className="block mb-1 font-medium">Reason</label>
            <input 
              type="text" 
              className="w-full border border-gray-300 rounded px-3 py-2"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
            />
          </div>
          <div>
            <label className="block mb-1 font-medium">Action Required</label>
            <input 
              type="text" 
              className="w-full border border-gray-300 rounded px-3 py-2"
              value={actionRequired}
              onChange={(e) => setActionRequired(e.target.value)}
            />
          </div>

          <div className="flex justify-end space-x-2">
            <button 
              type="button"
              onClick={onClose}
              className="px-4 py-2 border border-gray-400 rounded"
            >
              Cancel
            </button>
            <button 
              type="submit"
              className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
            >
              Save
            </button>
          </div>

          {status && <p className="text-sm mt-2">{status}</p>}
        </form>
      </div>
    </div>
  )
}
