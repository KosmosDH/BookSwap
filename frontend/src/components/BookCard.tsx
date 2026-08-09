import { Heart, MapPin } from 'lucide-react'
import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api, resolveImage } from '../lib/api'
import { conditionLabels, dealLabels } from '../lib/constants'
import type { BookListItem } from '../lib/types'
import { useAuth } from '../context/AuthContext'

export function BookCard({ book, onFavoriteChanged }: { book: BookListItem; onFavoriteChanged?: (value: boolean) => void }) {
  const { user } = useAuth()
  const [isFavorite, setIsFavorite] = useState(book.isFavorite)
  const [favoriteBusy, setFavoriteBusy] = useState(false)

  const toggleFavorite = async (event: React.MouseEvent) => {
    event.preventDefault()
    event.stopPropagation()
    if (!user || favoriteBusy) return

    setFavoriteBusy(true)
    try {
      if (isFavorite) await api.delete(`/favorites/${book.id}`)
      else await api.post(`/favorites/${book.id}`)

      const nextValue = !isFavorite
      setIsFavorite(nextValue)
      onFavoriteChanged?.(nextValue)
    } finally {
      setFavoriteBusy(false)
    }
  }

  return <Link to={`/books/${book.id}`} className="book-card">
    <div className="book-cover-wrap">
      <img className="book-cover" src={resolveImage(book.coverUrl)} alt={book.title} />
      <span className={`deal-badge deal-${book.dealType}`}>{dealLabels[book.dealType]}</span>
      {user && user.id !== book.owner.id && <button
        className={`heart-button ${isFavorite ? 'active' : ''}`}
        onClick={toggleFavorite}
        disabled={favoriteBusy}
        aria-label={isFavorite ? 'Удалить из избранного' : 'Добавить в избранное'}
      >
        <Heart size={19} fill={isFavorite ? 'currentColor' : 'none'} />
      </button>}
    </div>
    <div className="book-card-body">
      <div className="book-card-copy">
        <h3>{book.title}</h3>
        <p className="book-author">{book.author}</p>
      </div>
      <div className="book-card-meta">
        <span><MapPin size={14} />{book.city}</span>
        <span>{conditionLabels[book.condition]}</span>
      </div>
      <div className="genre-row">{book.genres.slice(0, 2).map(genre => <span key={genre}>{genre}</span>)}</div>
    </div>
  </Link>
}
