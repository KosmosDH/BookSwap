import { Heart } from 'lucide-react'
import { useEffect, useState } from 'react'
import { BookCard } from '../components/BookCard'
import { EmptyState } from '../components/EmptyState'
import { api, getErrorMessage } from '../lib/api'
import type { BookListItem } from '../lib/types'

export function FavoritesPage() {
  const [books, setBooks] = useState<BookListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get<BookListItem[]>('/favorites')
      .then(({ data }) => setBooks(data))
      .catch(error => setError(getErrorMessage(error)))
      .finally(() => setLoading(false))
  }, [])

  return <div className="page"><div className="container">
    <div className="page-heading inline-icon"><Heart /><div>
      <span className="section-kicker">Личная коллекция</span>
      <h1>Избранные книги</h1>
      <p>Книги, к которым хочется вернуться позже.</p>
    </div></div>

    {loading ? <div className="page-loader"><span /></div>
      : error ? <EmptyState title="Не удалось загрузить избранное" text={error} />
      : books.length ? <div className="book-grid">{books.map(book =>
        <BookCard
          key={book.id}
          book={book}
          onFavoriteChanged={value => !value && setBooks(items => items.filter(item => item.id !== book.id))}
        />
      )}</div>
      : <EmptyState title="Избранное пока пусто" text="Нажимайте на сердечко в каталоге, чтобы сохранить интересные книги." />}
  </div></div>
}
