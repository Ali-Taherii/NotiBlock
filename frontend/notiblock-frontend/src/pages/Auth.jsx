import { useState } from 'react'

export default function AuthPage() {
  const [isLogin, setIsLogin] = useState(true)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState('consumer') // default role
  const [message, setMessage] = useState('')

  const handleSubmit = async (e) => {
    e.preventDefault()

    const url = isLogin 
      ? 'https://localhost:7179/api/auth/login'
      : 'https://localhost:7179/api/auth/register'

    const payload = {
      email,
      password,
      ...(isLogin ? {} : { role }) // include role only during signup
    }

    try {
      const res = await fetch(url, {
          method: 'POST',
          credentials: 'include',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      })

        const data = await res.json()
        

      if (!res.ok) {
        throw new Error(data.message || 'Something went wrong')
      }

      setMessage(`✅ ${isLogin ? 'Logged in' : 'Signed up'} successfully!`)
      
      // Optional: Redirect based on role
      // window.location.href = `/${data.role}/dashboard`

    } catch (err) {
      console.error(err)
      setMessage(`❌ ${err.message}`)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-100 px-4">
      <div className="bg-white p-6 rounded shadow-md w-full max-w-md">
        <h2 className="text-2xl font-bold mb-4 text-center">
          {isLogin ? 'Login' : 'Sign Up'}
        </h2>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block mb-1">Email</label>
            <input 
              className="w-full border border-gray-300 px-3 py-2 rounded"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>

          <div>
            <label className="block mb-1">Password</label>
            <input 
              type="password"
              className="w-full border border-gray-300 px-3 py-2 rounded"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>

          {!isLogin && (
            <div>
              <label className="block mb-1">Role</label>
              <select
                className="w-full border border-gray-300 px-3 py-2 rounded"
                value={role}
                onChange={(e) => setRole(e.target.value)}
              >
                <option value="consumer">Consumer</option>
                <option value="reseller">Reseller</option>
                <option value="manufacturer">Manufacturer</option>
              </select>
            </div>
          )}

          <button
            type="submit"
            className="w-full bg-blue-600 text-white py-2 rounded hover:bg-blue-700"
          >
            {isLogin ? 'Login' : 'Sign Up'}
          </button>

          {message && (
            <p className="mt-2 text-center text-sm text-gray-700">{message}</p>
          )}
        </form>

        <p className="mt-4 text-center text-sm">
          {isLogin ? "Don't have an account?" : "Already have an account?"}
          <button
            type="button"
            onClick={() => setIsLogin(!isLogin)}
            className="text-blue-600 ml-1 hover:underline"
          >
            {isLogin ? 'Sign up' : 'Log in'}
          </button>
        </p>
      </div>
    </div>
  )
}
