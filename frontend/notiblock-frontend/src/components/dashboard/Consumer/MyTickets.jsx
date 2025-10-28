import { useTickets } from "../../../hooks/useTickets";
import { useAuth } from "../../../hooks/useAuth"
import Ticket from "./Ticket";

export default function MyTickets() {   

  const { user } = useAuth();
  const userId = user?.userId;

  const { tickets, loading, error, refetch } = useTickets({ userId });

  if (loading) return <div className="p-6">Loading tickets...</div>;
  if (error) return <div className="p-6 text-red-600">Error: {error}</div>;

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold mb-4">My Issue Tickets</h1>

      {tickets.length === 0 ? (
        <p className="text-gray-600">No tickets found.</p>
      ) : (
        <div className="space-y-4">
          {tickets.map(ticket => (
            <Ticket key={ticket.id} ticket={ticket} refetch={refetch} />
          ))}
        </div>
      )}
    </div>
  )
}

