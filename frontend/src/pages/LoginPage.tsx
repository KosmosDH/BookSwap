import { BookOpen, Eye, EyeOff, LogIn } from 'lucide-react'
import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../lib/api'

export function LoginPage() {
  const { login } = useAuth(); const navigate = useNavigate()
  const [email, setEmail] = useState('anna@bookswap.local'); const [password, setPassword] = useState('User123')
  const [show, setShow] = useState(false); const [error, setError] = useState(''); const [busy, setBusy] = useState(false)
  const submit = async (e: FormEvent) => { e.preventDefault(); setBusy(true); setError(''); try { await login(email, password); navigate('/dashboard') } catch (err) { setError(getErrorMessage(err)) } finally { setBusy(false) } }
  return <div className="auth-page"><div className="auth-panel auth-art"><div><span className="brand-mark large"><BookOpen /></span><h2>Каждая книга может начать новую главу.</h2><p>Войдите, чтобы управлять объявлениями, обменами и сообщениями.</p></div><blockquote>«Книги — это способ разговаривать с теми, кого никогда не встретишь»</blockquote></div><div className="auth-panel auth-form-panel"><form className="auth-form" onSubmit={submit}><span className="section-kicker">С возвращением</span><h1>Войти в BookSwap</h1><p>Нет аккаунта? <Link to="/register">Зарегистрироваться</Link></p>{error && <div className="form-error">{error}</div>}<label>Email<input type="email" value={email} onChange={e => setEmail(e.target.value)} required /></label><label>Пароль<div className="password-field"><input type={show ? 'text' : 'password'} value={password} onChange={e => setPassword(e.target.value)} required /><button type="button" onClick={() => setShow(!show)}>{show ? <EyeOff /> : <Eye />}</button></div></label><button className="button button-primary button-wide" disabled={busy}><LogIn size={18} />{busy ? 'Входим…' : 'Войти'}</button><div className="demo-credentials"><b>Демо-пользователь</b><span>anna@bookswap.local / User123</span><b>Администратор</b><span>admin@bookswap.local / Admin123</span></div></form></div></div>
}
