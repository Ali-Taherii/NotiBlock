import { useState } from 'react'
import LogoutButton from '../components/shared/LogoutButton'

const mockInitialTickets = [
  { id: 1, serial: 'ABC123', model: 'Toaster X200', consumer: 'sara@example.com', status: 'pending' },
  { id: 2, serial: 'DEF456', model: 'Fan TurboPro', consumer: 'ali@example.com', status: 'pending' },
  { id: 3, serial: 'XYZ789', model: 'OvenMaster 3000', consumer: 'mina@example.com', status: 'approved' }
]

export default function RegulatorDashboard() {
  const [tickets, setTickets] = useState(mockInitialTickets)

  const updateTicketStatus = (id, newStatus) => {
    setTickets(prev => prev.map(ticket =>
      ticket.id === id ? { ...ticket, status: newStatus } : ticket
    ))

    console.log(`✅ Ticket ${id} ${newStatus.toUpperCase()} by regulator`)

    if (newStatus === 'approved') {
      console.log('📢 Notification triggered to manufacturer view (mocked)')
    }
  }

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Regulator Dashboard</h1>
        <LogoutButton />
      </div>

      <h2 className="text-xl font-semibold mb-4">Submitted Tickets</h2>

      <div className="space-y-4">
        {tickets.map(ticket => (
          <div key={ticket.id} className="p-4 border rounded shadow bg-white">
            <p><strong>Product:</strong> {ticket.model}</p>
            <p><strong>Serial:</strong> {ticket.serial}</p>
            <p><strong>Consumer:</strong> {ticket.consumer}</p>
            <p><strong>Status:</strong> <span className={
              ticket.status === 'approved' ? 'text-green-600' :
              ticket.status === 'rejected' ? 'text-red-600' :
              'text-yellow-600'
            }>{ticket.status}</span></p>

            {ticket.status === 'pending' && (
              <div className="mt-3 flex gap-3">
                <button
                  className="bg-green-600 text-white px-3 py-1 rounded hover:bg-green-700"
                  onClick={() => updateTicketStatus(ticket.id, 'approved')}
                >
                  Approve
                </button>
                <button
                  className="bg-red-600 text-white px-3 py-1 rounded hover:bg-red-700"
                  onClick={() => updateTicketStatus(ticket.id, 'rejected')}
                >
                  Reject
                </button>
              </div>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}
