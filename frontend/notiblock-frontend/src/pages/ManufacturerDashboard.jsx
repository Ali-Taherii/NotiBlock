import { useState } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import LogoutButton from '../components/shared/LogoutButton'
import ManufacturerTicket from '../components/dashboard/Manufacturer/ManufacturerTicket'
import { useApprovedTickets } from '../hooks/useApprovedTickets'
import { createRecall } from '../api/recalls'


const approvedTicketsMock = [
  { id: 1, serial: 'ABC123', model: 'Toaster X200', consumer: 'sara@example.com', status: 'approved' },
  { id: 2, serial: 'DEF456', model: 'Fan TurboPro', consumer: 'ali@example.com', status: 'approved' },
]

export default function ManufacturerDashboard() {
  const { tickets, loading, error, refetch } = useApprovedTickets()
  const [selectedTicket, setSelectedTicket] = useState(null)
  const [reason, setReason] = useState('')
  const [actionRequired, setActionRequired] = useState('')
  const [recallData, setRecallData] = useState(null)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [status, setStatus] = useState(null)

  const handleTicketSelect = (ticket) => {
    setSelectedTicket(ticket)
    setReason('')
    setActionRequired('')
    setRecallData(null)
    setStatus(null)
  }

  const handleCreateRecall = async (e) => {
    e.preventDefault()

    if (!reason.trim() || !actionRequired.trim()) {
      setStatus('Please fill in all fields.')
      return
    }

    setIsSubmitting(true)
    setStatus(null)

    try {
      const recallPayload = {
        productId: selectedTicket.productId || selectedTicket.id,
        reason: reason.trim(),
        actionRequired: actionRequired.trim(),
      }

      const result = await createRecall(recallPayload)
      
      // Generate QR code data
    const qrPayload = JSON.stringify({
        recallId: result.id || 'generated',
        productSerial: selectedTicket.productId || selectedTicket.id,
      recallReason: reason,
        actionRequired: actionRequired,
        timestamp: new Date().toISOString(),
    })
      
      setRecallData({
        recall: result,
        qrData: qrPayload,
      })

      setStatus('Recall created successfully!')
      refetch() // Refresh the tickets list
      
    } catch (err) {
      console.error('Error creating recall:', err)
      setStatus('Error creating recall. Please try again.')
    } finally {
      setIsSubmitting(false)
  }
  }

  if (loading) return <div className="p-6">Loading approved tickets...</div>
  if (error) return <div className="p-6 text-red-600">Error: {error}</div>

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Manufacturer Dashboard</h1>
        <LogoutButton />
      </div>

      {!selectedTicket && (
        <>
          <h2 className="text-xl font-semibold mb-4">Approved Tickets</h2>
          {tickets.length === 0 ? (
            <p className="text-gray-600">No approved tickets found.</p>
          ) : (
            <div className="space-y-4">
              {tickets.map(ticket => (
                <ManufacturerTicket 
                  key={ticket.id} 
                  ticket={ticket} 
                  onCreateRecall={() => setSelectedTicket(ticket)}
                />
              ))}
            </div>
          )}
                <button
            onClick={() => refetch()}
            className="mt-4 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
            Refresh Tickets
                </button>
              </div>
            ))}
          </div>
        </>
      )}

      {selectedTicket && (
        <div className="max-w-2xl mx-auto">
          <div className="bg-white p-6 rounded shadow border">
            <div className="flex justify-between items-center mb-4">
              <h2 className="text-xl font-bold">Create Recall</h2>
              <button
                onClick={() => setSelectedTicket(null)}
                className="text-gray-500 hover:text-gray-700"
              >
                ✕ Close
              </button>
            </div>
            
            <div className="mb-4 p-3 bg-gray-50 rounded">
              <p><strong>Product Serial:</strong> {selectedTicket.productId || selectedTicket.id}</p>
              <p><strong>Issue:</strong> {selectedTicket.issueDescription}</p>
              <p><strong>Consumer ID:</strong> {selectedTicket.userId}</p>
            </div>

          <form onSubmit={handleCreateRecall} className="space-y-4">
            <p><strong>Serial:</strong> {selectedTicket.serial}</p>
            <p><strong>Consumer:</strong> {selectedTicket.consumer}</p>

            <div>
              <label className="block mb-1 font-medium">Recall Reason</label>
              <textarea
                  className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                  placeholder="Describe the reason for the recall"
                  rows="3"
                  required
                />
              </div>

              <div>
                <label className="block mb-1 font-medium">Action Required</label>
                <textarea
                  className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                  value={actionRequired}
                  onChange={(e) => setActionRequired(e.target.value)}
                  placeholder="What action should consumers take?"
                  rows="3"
                required
              />
            </div>

              <div className="flex space-x-3">
                <button 
                  type="submit" 
                  disabled={isSubmitting}
                  className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  {isSubmitting ? 'Creating Recall...' : 'Issue Recall'}
                </button>
                <button
                  type="button"
                  onClick={() => setSelectedTicket(null)}
                  className="bg-gray-500 text-white px-4 py-2 rounded hover:bg-gray-600"
                >
                  Cancel
            </button>
              </div>

              {status && (
                <p className={`text-sm ${status.includes('Error') ? 'text-red-600' : 'text-green-600'}`}>
                  {status}
                </p>
              )}
          </form>
          </div>

          {recallData && (
            <div className="mt-6 bg-white p-6 rounded shadow border">
              <h3 className="text-lg font-bold mb-4 text-green-600">✅ Recall Created Successfully!</h3>
              
              <div className="mb-4">
                <p><strong>Recall ID:</strong> {recallData.recall.id}</p>
                <p><strong>Product:</strong> {selectedTicket.productId || selectedTicket.id}</p>
                <p><strong>Created:</strong> {new Date().toLocaleDateString()}</p>
              </div>

              <div className="text-center">
                <h4 className="font-medium mb-2">QR Code for Recall Information</h4>
                <div className="inline-block p-4 bg-white border rounded">
                  <QRCodeSVG value={recallData.qrData} size={150} />
                </div>
                <p className="text-xs mt-2 text-gray-600">
                  Scan this QR code to view recall details
                </p>
              </div>

              <div className="mt-4 text-center">
                <button
                  onClick={() => {
                    setSelectedTicket(null)
                    setRecallData(null)
                  }}
                  className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
                >
                  Create Another Recall
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
