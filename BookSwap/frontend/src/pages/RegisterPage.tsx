import { BookOpen, UserPlus } from 'lucide-react'
import { FormEvent, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../lib/api'

export function RegisterPage() {
  const { register } = useAuth(); const navigate = useNavigate()
  const [form, setForm] = useState({ email: '', password: '', displayName: '', city: '' }); const [error, setError] = useState(''); const [busy, setBusy] = useState(false)
  const set = (key: keyof typeof form, value: string) => setForm({ ...form, [key]: value })
  const submit = async (e: FormEvent) => { e.preventDefault(); setBusy(true); setError(''); try { await register(form); navigate('/dashboard?tab=new') } catch (err) { setError(getErrorMessage(err)) } finally { setBusy(false) } }
  return <div className="auth-page reverse"><div className="auth-panel auth-art register-art"><div><span className="brand-mark large"><BookOpen /></span><h2>Освободите полку. Пополните список прочитанного.</h2><p>Создайте профиль и найдите людей, которым интересны те же книги.</p></div><div className="auth-benefits"><span>✓ Бесплатные объявления</span><span>✓ Чат в реальном времени</span><span>✓ Рейтинг и отзывы</span></div></div><div className="auth-panel auth-form-panel"><form className="auth-form" onSubmit={submit}><span className="section-kicker">Новый читатель</span><h1>Создать аккаунт</h1><p>Уже зарегистрированы? <Link to="/login">Войти</Link></p>{error && <div className="form-error">{error}</div>}<div className="two-fields"><label>Имя<input value={form.displayName} onChange={e => set('displayName', e.target.value)} required minLength={2} /></label><label>Город<input value={form.city} onChange={e => set('city', e.target.value)} required /></label></div><label>Email<input type="email" value={form.email} onChange={e => set('email', e.target.value)} required /></label><label>Пароль<input type="password" value={form.password} onChange={e => set('password', e.target.value)} required minLength={6} placeholder="Минимум 6 символов и цифра" /></label><button className="button button-primary button-wide" disabled={busy}><UserPlus size={18} />{busy ? 'Создаём…' : 'Зарегистрироваться'}</button><small className="legal-copy">Продолжая, вы соглашаетесь соблюдать правила сообщества BookSwap.</small></form></div></div>
}
