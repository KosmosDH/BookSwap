import { AlertTriangle, ArrowLeft, BookMarked, Heart, MapPin, MessageCircle, Repeat2, ShieldCheck, Star, UserRound } from 'lucide-react'
import { FormEvent, useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { Avatar } from '../components/Avatar'
import { useAuth } from '../context/AuthContext'
import { api, getErrorMessage, resolveImage } from '../lib/api'
import { conditionLabels, dealLabels, formatDate, statusLabels } from '../lib/constants'
import { BookStatus, DealType, type BookDetails, type BookListItem } from '../lib/types'

export function BookPage() {
  const { id } = useParams(); const { user } = useAuth(); const navigate = useNavigate()
  const [book, setBook] = useState<BookDetails | null>(null); const [mine, setMine] = useState<BookListItem[]>([])
  const [modal, setModal] = useState(false); const [offeredBookId, setOfferedBookId] = useState(''); const [message, setMessage] = useState('')
  const [error, setError] = useState(''); const [busy, setBusy] = useState(false)
  const load = () => api.get<BookDetails>(`/books/${id}`).then(({ data }) => setBook(data))
  useEffect(() => { void load() }, [id])
  useEffect(() => { if (modal && user) api.get<BookListItem[]>('/books/mine').then(({ data }) => setMine(data.filter(item => item.status === BookStatus.Available))) }, [modal, user])

  if (!book) return <div className="page-loader"><span /></div>
  const own = user?.id === book.owner.id
  const toggleFavorite = async () => { if (!user) return navigate('/login'); if (book.isFavorite) await api.delete(`/favorites/${book.id}`); else await api.post(`/favorites/${book.id}`); setBook({ ...book, isFavorite: !book.isFavorite }) }
  const sendRequest = async (event: FormEvent) => {
    event.preventDefault(); setBusy(true); setError('')
    try { await api.post('/exchanges', { requestedBookId: book.id, offeredBookId: offeredBookId || null, message }); setModal(false); alert('Заявка отправлена владельцу книги.') }
    catch (err) { setError(getErrorMessage(err)) } finally { setBusy(false) }
  }
  const report = async () => {
    if (!user) return navigate('/login')
    const reason = window.prompt('Опишите причину жалобы:')
    if (reason) { await api.post('/complaints', { bookId: book.id, targetUserId: book.owner.id, reason }); alert('Жалоба отправлена модератору.') }
  }

  return <div className="page book-details-page"><div className="container">
    <Link to="/catalog" className="back-link"><ArrowLeft size={17} />Вернуться в каталог</Link>
    <div className="book-details-grid">
      <section className="details-gallery"><div className="details-cover"><img src={resolveImage(book.photos[0]?.url)} alt={book.title} /><span className={`deal-badge deal-${book.dealType}`}>{dealLabels[book.dealType]}</span></div><div className="gallery-note"><ShieldCheck size={18} /><span>Объявление опубликовано {formatDate(book.createdAt)}</span></div></section>
      <section className="details-main">
        <div className="details-topline"><div className="genre-row">{book.genres.map(genre => <span key={genre.id}>{genre.name}</span>)}</div><button className={`favorite-large ${book.isFavorite ? 'active' : ''}`} onClick={toggleFavorite}><Heart fill={book.isFavorite ? 'currentColor' : 'none'} />{book.isFavorite ? 'В избранном' : 'В избранное'}</button></div>
        <h1>{book.title}</h1><p className="details-author">{book.author}</p>
        <div className="details-facts"><span><MapPin />{book.city}</span><span><BookMarked />{conditionLabels[book.condition]}</span><span className={`status-pill status-${book.status}`}>{statusLabels[book.status]}</span></div>
        {book.dealType === DealType.Sell && <div className="details-price">{book.price?.toLocaleString('ru-RU')} ₽</div>}
        <div className="details-description"><h2>О книге</h2><p>{book.description}</p>{book.isbn && <small>ISBN: {book.isbn}</small>}</div>
        <div className="owner-card"><Avatar name={book.owner.displayName} url={book.owner.avatarUrl} size={58} /><div><span>Владелец</span><h3>{book.owner.displayName}</h3><p><Star size={15} fill="currentColor" />{book.owner.rating ? book.owner.rating.toFixed(1) : 'Новый'} · {book.owner.reviewsCount} отзывов</p></div>{!own && <button className="button button-ghost" onClick={() => navigate(`/chat/${book.owner.id}`)}><MessageCircle size={18} />Написать</button>}</div>
        <div className="details-actions">
          {own ? <Link className="button button-primary button-wide" to="/dashboard?tab=books">Управлять объявлением</Link> : <button className="button button-primary button-wide" disabled={book.status !== BookStatus.Available} onClick={() => user ? setModal(true) : navigate('/login')}><Repeat2 size={19} />{book.dealType === DealType.Exchange ? 'Предложить обмен' : book.dealType === DealType.GiveAway ? 'Отправить заявку' : book.dealType === DealType.Sell ? 'Связаться о покупке' : 'Попросить почитать'}</button>}
          {!own && <button className="report-button" onClick={report}><AlertTriangle size={16} />Пожаловаться на объявление</button>}
        </div>
      </section>
    </div>
  </div>
  {modal && <div className="modal-backdrop" onMouseDown={() => setModal(false)}><form className="modal-card" onSubmit={sendRequest} onMouseDown={e => e.stopPropagation()}><div className="modal-icon"><Repeat2 /></div><h2>Заявка на «{book.title}»</h2><p>Владелец получит уведомление и сможет ответить вам в личном кабинете.</p>{error && <div className="form-error">{error}</div>}{book.dealType === DealType.Exchange && <label>Что предлагаете взамен?<select value={offeredBookId} onChange={e => setOfferedBookId(e.target.value)} required><option value="">Выберите свою книгу</option>{mine.map(item => <option value={item.id} key={item.id}>{item.title} — {item.author}</option>)}</select>{mine.length === 0 && <small>Сначала добавьте доступную книгу в личном кабинете.</small>}</label>}<label>Сообщение владельцу<textarea value={message} onChange={e => setMessage(e.target.value)} placeholder="Например: готов встретиться в центре города" rows={4} /></label><div className="modal-actions"><button type="button" className="button button-ghost" onClick={() => setModal(false)}>Отмена</button><button className="button button-primary" disabled={busy || (book.dealType === DealType.Exchange && !offeredBookId)}>{busy ? 'Отправляем…' : 'Отправить заявку'}</button></div></form></div>}
  </div>
}
