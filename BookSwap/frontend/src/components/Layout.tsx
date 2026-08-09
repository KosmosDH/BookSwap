import { Bell, BookOpen, Heart, LogOut, Menu, MessageCircle, Plus, Search, User, X } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { api } from '../lib/api'
import { Avatar } from './Avatar'

export function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const [mobileOpen, setMobileOpen] = useState(false)
  const [unread, setUnread] = useState(0)

  useEffect(() => {
    if (!user) return
    const load = async () => {
      try { const { data } = await api.get<{ count: number }>('/notifications/unread-count'); setUnread(data.count) } catch { /* no-op */ }
    }
    void load()
    const timer = window.setInterval(load, 30000)
    return () => window.clearInterval(timer)
  }, [user])

  const signOut = () => { logout(); navigate('/') }
  const close = () => setMobileOpen(false)

  return <div className="app-shell">
    <header className="site-header">
      <div className="container header-inner">
        <Link to="/" className="brand" onClick={close}>
          <span className="brand-mark"><BookOpen size={23} /></span>
          <span>Book<span>Swap</span></span>
        </Link>
        <nav className="desktop-nav">
          <NavLink to="/catalog"><Search size={17} />Каталог</NavLink>
          {user && <NavLink to="/favorites"><Heart size={17} />Избранное</NavLink>}
          {user && <NavLink to="/chat"><MessageCircle size={17} />Сообщения</NavLink>}
        </nav>
        <div className="header-actions">
          {user ? <>
            <Link className="icon-link notification-link" to="/dashboard?tab=notifications" aria-label="Уведомления"><Bell size={20} />{unread > 0 && <b>{unread > 9 ? '9+' : unread}</b>}</Link>
            <Link className="button button-primary desktop-only" to="/dashboard?tab=new"><Plus size={18} />Добавить книгу</Link>
            <Link className="profile-link" to="/dashboard"><Avatar name={user.displayName} url={user.avatarUrl} size={38} /><span>{user.displayName.split(' ')[0]}</span></Link>
          </> : <>
            <Link className="button button-ghost desktop-only" to="/login">Войти</Link>
            <Link className="button button-primary" to="/register">Регистрация</Link>
          </>}
          <button className="mobile-menu-button" onClick={() => setMobileOpen(value => !value)}>{mobileOpen ? <X /> : <Menu />}</button>
        </div>
      </div>
      {mobileOpen && <div className="mobile-menu">
        <NavLink to="/catalog" onClick={close}><Search size={18} />Каталог</NavLink>
        {user && <NavLink to="/favorites" onClick={close}><Heart size={18} />Избранное</NavLink>}
        {user && <NavLink to="/chat" onClick={close}><MessageCircle size={18} />Сообщения</NavLink>}
        {user && <NavLink to="/dashboard" onClick={close}><User size={18} />Личный кабинет</NavLink>}
        {user && <NavLink to="/dashboard?tab=new" onClick={close}><Plus size={18} />Добавить книгу</NavLink>}
        {user && <button onClick={signOut}><LogOut size={18} />Выйти</button>}
      </div>}
    </header>
    <main><Outlet /></main>
    <footer className="site-footer">
      <div className="container footer-grid">
        <div><Link to="/" className="brand footer-brand"><span className="brand-mark"><BookOpen size={21} /></span><span>Book<span>Swap</span></span></Link><p>Книги находят новых читателей, а люди — новые истории.</p></div>
        <div><h4>Платформа</h4><Link to="/catalog">Каталог</Link><Link to="/register">Создать аккаунт</Link><Link to="/login">Войти</Link></div>
        <div><h4>Проект</h4><a href="/swagger" target="_blank">API Swagger</a><a href="/health" target="_blank">Статус сервиса</a><span>Учебный дипломный проект</span></div>
      </div>
      <div className="container footer-bottom">© 2026 BookSwap. Сделано для обмена знаниями.</div>
    </footer>
  </div>
}
