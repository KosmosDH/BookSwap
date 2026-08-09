import { ArrowLeft, MapPin, MessageCircle, Star, UserRound } from 'lucide-react'
import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { Avatar } from '../components/Avatar'
import { EmptyState } from '../components/EmptyState'
import { useAuth } from '../context/AuthContext'
import { api } from '../lib/api'
import { formatDate } from '../lib/constants'
import type { Review, UserSummary } from '../lib/types'

export function UserPage() {
  const { id } = useParams(); const { user } = useAuth(); const navigate = useNavigate()
  const [profile, setProfile] = useState<UserSummary | null>(null); const [reviews, setReviews] = useState<Review[]>([])
  useEffect(() => {
    if (!id) return
    void Promise.all([
      api.get<UserSummary>(`/users/${id}`).then(({ data }) => setProfile(data)),
      api.get<Review[]>(`/reviews/user/${id}`).then(({ data }) => setReviews(data))
    ])
  }, [id])
  if (!profile) return <div className="page-loader"><span /></div>
  return <div className="page user-page"><div className="container user-page-container">
    <Link to="/catalog" className="back-link"><ArrowLeft size={17} />Вернуться в каталог</Link>
    <section className="public-profile-card">
      <Avatar name={profile.displayName} url={profile.avatarUrl} size={96} />
      <div className="public-profile-copy"><span className="section-kicker">Профиль участника</span><h1>{profile.displayName}</h1><p><MapPin size={16} />{profile.city}</p><div className="public-rating"><Star fill="currentColor" /> <b>{profile.rating ? profile.rating.toFixed(1) : 'Новый участник'}</b><span>{profile.reviewsCount} отзывов</span></div></div>
      {user?.id !== profile.id && <button className="button button-primary" onClick={() => user ? navigate(`/chat/${profile.id}`) : navigate('/login')}><MessageCircle size={18} />Написать</button>}
    </section>
    <section className="reviews-section"><div className="section-heading"><div><span className="section-kicker">Репутация</span><h2>Отзывы после обменов</h2></div></div>
      {reviews.length ? <div className="reviews-grid">{reviews.map(review => <article key={review.id}><div className="review-head"><Avatar name={review.author.displayName} url={review.author.avatarUrl} size={42} /><div><b>{review.author.displayName}</b><span>{formatDate(review.createdAt)}</span></div><div className="review-stars">{Array.from({ length: 5 }).map((_, index) => <Star key={index} size={15} fill={index < review.rating ? 'currentColor' : 'none'} />)}</div></div><p>{review.text}</p></article>)}</div> : <EmptyState title="Отзывов пока нет" text="Они появятся после завершённых обменов с этим участником." />}
    </section>
  </div></div>
}
