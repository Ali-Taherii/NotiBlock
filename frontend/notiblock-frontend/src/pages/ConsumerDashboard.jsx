import { Link } from 'react-router-dom'
import LogoutButton from '../components/shared/LogoutButton'

const mockProducts = [
  { serial: 'ABC123', model: 'Toaster X200', date: '2023-06-15' },
  { serial: 'DEF456', model: 'Fan TurboPro', date: '2023-09-21' },
]

export default function ConsumerDashboard() {

  return (
    <div className="p-6">
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-bold">My Registered Products</h1>
        <LogoutButton />
      </div>

      <div className="space-y-4">
        {mockProducts.map((p, i) => (
          <div key={i} className="p-4 bg-white shadow rounded border">
            <p><strong>Serial:</strong> {p.serial}</p>
            <p><strong>Model:</strong> {p.model}</p>
            <p><strong>Registered on:</strong> {p.date}</p>
          </div>
        ))}
      </div>

      <div className="mt-6 flex gap-4">
        <Link
          to={`/consumer/report-issue`}
          className="px-4 py-2 bg-blue-600 text-white rounded"
        >
          Report Issue
        </Link>
        <Link
          to={`/consumer/my-tickets`}
          className="px-4 py-2 bg-gray-700 text-white rounded"
        >
          View My Tickets
        </Link>
      </div>
    </div>
  )
}