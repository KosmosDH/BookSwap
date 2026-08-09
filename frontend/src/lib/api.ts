import axios from 'axios'

export const api = axios.create({ baseURL: import.meta.env.VITE_API_URL || '/api' })

api.interceptors.request.use(config => {
  const token = localStorage.getItem('bookswap_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem('bookswap_token')
      localStorage.removeItem('bookswap_user')
    }
    return Promise.reject(error)
  }
)

export const getErrorMessage = (error: unknown) => {
  if (axios.isAxiosError(error)) return error.response?.data?.message || error.message
  return error instanceof Error ? error.message : 'Неизвестная ошибка'
}

export const resolveImage = (url?: string) => {
  if (!url) return '/placeholder-book.svg'
  if (url.startsWith('http')) return url
  return url
}
