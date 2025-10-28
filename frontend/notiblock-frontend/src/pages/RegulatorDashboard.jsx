import LogoutButton from '../components/shared/LogoutButton'
import RegulatorTicket from '../components/dashboard/Regulator/RegulatorTicket'
import { useAllTickets } from '../hooks/useAllTickets'

export default function RegulatorDashboard() {
  const { tickets, loading, error, refetch } = useAllTickets()

  if (loading) return <div className="p-6">Loading tickets...</div>
  if (error) return <div className="p-6 text-red-600">Error: {error}</div>

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold">Regulator Dashboard</h1>
        <LogoutButton />
      </div>

      <h2 className="text-xl font-semibold mb-4">Submitted Tickets</h2>

      {tickets.length === 0 ? (
        <p className="text-gray-600">No tickets found.</p>
      ) : (
        <div className="space-y-4">
          {tickets.map(ticket => (
            <RegulatorTicket key={ticket.id} ticket={ticket} refetch={refetch} />
          ))}
        </div>
      )}
    </div>
  )
}
