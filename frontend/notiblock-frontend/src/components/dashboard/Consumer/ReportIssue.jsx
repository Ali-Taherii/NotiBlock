import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { createTicket } from '../../../api/tickets'

export default function ReportIssue() {
  const [productId, setProductId] = useState('')
  const [issueDescription, setIssueDescription] = useState('')
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    try {
      await createTicket({ productId, issueDescription })
      navigate('/consumer/my-tickets')
    } catch (error) {
      console.error("Error creating ticket:", error)
    }
  }

  return (
    <div className="p-6 max-w-md mx-auto">
      <h1 className="text-2xl font-bold mb-4">Report a Product Issue</h1>

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block mb-1 font-medium">Product Serial</label>
          <input
            type="text"
            value={productId}
            onChange={(e) => setProductId(e.target.value)}
            className="w-full border px-3 py-2 rounded"
            required
          />
        </div>

        <div>
          <label className="block mb-1 font-medium">Issue Description</label>
          <textarea
            value={issueDescription}
            onChange={(e) => setIssueDescription(e.target.value)}
            className="w-full border px-3 py-2 rounded"
            required
          ></textarea>
        </div>

        <button type="submit" className="bg-blue-600 text-white px-4 py-2 rounded">
          Submit Ticket
        </button>
      </form>
    </div>
  )
}
