import { Route, Routes } from 'react-router-dom'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { HomePage } from './pages/HomePage'
import { CatalogPage } from './pages/CatalogPage'
import { BookPage } from './pages/BookPage'
import { LoginPage } from './pages/LoginPage'
import { RegisterPage } from './pages/RegisterPage'
import { DashboardPage } from './pages/DashboardPage'
import { FavoritesPage } from './pages/FavoritesPage'
import { ChatPage } from './pages/ChatPage'
import { AdminPage } from './pages/AdminPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { UserPage } from './pages/UserPage'

export default function App() {
  return <Routes>
    <Route element={<Layout />}>
      <Route path="/" element={<HomePage />} />
      <Route path="/catalog" element={<CatalogPage />} />
      <Route path="/books/:id" element={<BookPage />} />
      <Route path="/users/:id" element={<UserPage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route element={<ProtectedRoute />}>
        <Route path="/dashboard" element={<DashboardPage />} />
        <Route path="/favorites" element={<FavoritesPage />} />
        <Route path="/chat" element={<ChatPage />} />
        <Route path="/chat/:partnerId" element={<ChatPage />} />
      </Route>
      <Route element={<ProtectedRoute admin />}><Route path="/admin" element={<AdminPage />} /></Route>
      <Route path="*" element={<NotFoundPage />} />
    </Route>
  </Routes>
}
