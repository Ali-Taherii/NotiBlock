import { useAuth } from '../../hooks/useAuth'
import { useNavigate } from 'react-router-dom'

export default function LogoutButton() {
  const { logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = async () => {
    try {
      await logout()
      navigate('/auth')
    } catch (error) {
      console.error('Logout failed:', error)
      // Still navigate to auth page even if logout API fails
      navigate('/auth')
    }
  }

  return (
    <button
      onClick={handleLogout}
      className="bg-red-600 text-white px-4 py-2 rounded hover:bg-red-700 transition-colors"
    >
      Logout
    </button>
  )
}