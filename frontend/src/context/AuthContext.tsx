import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import { api } from '../lib/api'
import type { AuthResponse, UserProfile } from '../lib/types'

type AuthContextValue = {
  user: UserProfile | null
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (data: { email: string; password: string; displayName: string; city: string }) => Promise<void>
  logout: () => void
  refresh: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<UserProfile | null>(() => {
    const stored = localStorage.getItem('bookswap_user')
    return stored ? JSON.parse(stored) : null
  })
  const [loading, setLoading] = useState(true)

  const persist = (response: AuthResponse) => {
    localStorage.setItem('bookswap_token', response.token)
    localStorage.setItem('bookswap_user', JSON.stringify(response.user))
    setUser(response.user)
  }

  const refresh = async () => {
    if (!localStorage.getItem('bookswap_token')) { setLoading(false); return }
    try {
      const { data } = await api.get<UserProfile>('/auth/me')
      localStorage.setItem('bookswap_user', JSON.stringify(data))
      setUser(data)
    } catch { logout() } finally { setLoading(false) }
  }

  useEffect(() => { void refresh() }, [])

  const login = async (email: string, password: string) => {
    const { data } = await api.post<AuthResponse>('/auth/login', { email, password })
    persist(data)
  }
  const register = async (payload: { email: string; password: string; displayName: string; city: string }) => {
    const { data } = await api.post<AuthResponse>('/auth/register', payload)
    persist(data)
  }
  const logout = () => {
    localStorage.removeItem('bookswap_token')
    localStorage.removeItem('bookswap_user')
    setUser(null)
  }

  const value = useMemo(() => ({ user, loading, login, register, logout, refresh }), [user, loading])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export const useAuth = () => {
  const value = useContext(AuthContext)
  if (!value) throw new Error('useAuth must be used inside AuthProvider')
  return value
}
