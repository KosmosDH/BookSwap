import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function ProtectedRoute({ admin = false }: { admin?: boolean }) {
  const { user, loading } = useAuth()
  if (loading) return <div className="page-loader"><span /></div>
  if (!user) return <Navigate to="/login" replace />
  if (admin && !user.roles.includes('Admin')) return <Navigate to="/" replace />
  return <Outlet />
}
