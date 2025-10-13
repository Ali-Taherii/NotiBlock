import { useState } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import LogoutButton from '../components/shared/LogoutButton'


const approvedTicketsMock = [
  { id: 1, serial: 'ABC123', model: 'Toaster X200', consumer: 'sara@example.com', status: 'approved' },
  { id: 2, serial: 'DEF456', model: 'Fan TurboPro', consumer: 'ali@example.com', status: 'approved' },
]

export default function ManufacturerDashboard() {
  const [selectedTicket, setSelectedTicket] = useState(null)
  const [reason, setReason] = useState('')
    const [txHash, setTxHash] = useState('')
    const [qrData, setQrData] = useState('')

  const handleCreateRecall = (e) => {
    e.preventDefault()

    // Simulate blockchain transaction
    const fakeTxHash = '0x' + Math.random().toString(16).substr(2, 64)
    setTxHash(fakeTxHash)

    const qrPayload = JSON.stringify({
      productSerial: selectedTicket.serial,
      recallReason: reason,
      txHash: fakeTxHash,
    })
      
    setQrData(qrPayload)

    console.log('📦 Recall created on blockchain:', fakeTxHash)
    console.log('🔗 QR Code Payload:', qrPayload)
  }

  return (
    <div className="p-6 flex flex-col items-center">
      <div className="w-full flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Manufacturer Dashboard</h1>
        <LogoutButton />
      </div>

      {!selectedTicket && (
        <>
          <h2 className="text-xl font-semibold mb-4">Approved Tickets</h2>
          <div className="space-y-4 w-full">
            {approvedTicketsMock.map(ticket => (
              <div key={ticket.id} className="p-4 border rounded shadow bg-white">
                <p><strong>Product:</strong> {ticket.model}</p>
                <p><strong>Serial:</strong> {ticket.serial}</p>
                <p><strong>Consumer:</strong> {ticket.consumer}</p>
                <button
                  className="mt-3 px-3 py-1 bg-blue-600 text-white rounded"
                  onClick={() => setSelectedTicket(ticket)}
                >
                  Create Recall
                </button>
              </div>
            ))}
          </div>
        </>
      )}

      {selectedTicket && (
        <div className="mt-8 max-w-xl bg-white p-6 rounded shadow border w-full">
          <h2 className="text-xl font-bold mb-4">Create Recall for {selectedTicket.model}</h2>
          <form onSubmit={handleCreateRecall} className="space-y-4">
            <p><strong>Serial:</strong> {selectedTicket.serial}</p>
            <p><strong>Consumer:</strong> {selectedTicket.consumer}</p>

            <div>
              <label className="block mb-1 font-medium">Recall Reason</label>
              <textarea
                className="w-full border px-3 py-2 rounded"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                required
              />
            </div>

            <button type="submit" className="bg-green-600 text-white px-4 py-2 rounded hover:bg-green-700">
              Issue Recall (Mock Blockchain Tx)
            </button>
          </form>

          {txHash && (
            <div className="mt-6 text-center">
              <p className="text-sm text-gray-600">Blockchain TX Hash:</p>
              <code className="text-xs break-all text-blue-600">{txHash}</code>

              <div className="mt-4 flex flex-col items-center">
                <QRCodeSVG value={qrData} size={128} />
                <p className="text-xs mt-2 text-gray-600">QR Code (recall info)</p>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
