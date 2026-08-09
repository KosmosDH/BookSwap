import { Filter, Search, SlidersHorizontal } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { BookCard } from '../components/BookCard'
import { EmptyState } from '../components/EmptyState'
import { api } from '../lib/api'
import { BookCondition, DealType, type BookListItem, type Genre, type PagedResult } from '../lib/types'
import { conditionLabels, dealLabels } from '../lib/constants'

export function CatalogPage() {
  const [params, setParams] = useSearchParams()
  const [genres, setGenres] = useState<Genre[]>([])
  const [result, setResult] = useState<PagedResult<BookListItem>>({ items: [], page: 1, pageSize: 12, totalCount: 0, totalPages: 0 })
  const [loading, setLoading] = useState(true)
  const query = useMemo(() => Object.fromEntries(params.entries()), [params])

  useEffect(() => { api.get<Genre[]>('/genres').then(({ data }) => setGenres(data)) }, [])
  useEffect(() => {
    setLoading(true)
    api.get<PagedResult<BookListItem>>('/books', { params: query }).then(({ data }) => setResult(data)).finally(() => setLoading(false))
  }, [query])

  const update = (key: string, value: string) => {
    const next = new URLSearchParams(params)
    if (value) next.set(key, value); else next.delete(key)
    if (key !== 'page') next.delete('page')
    setParams(next)
  }

  return <div className="page catalog-page">
    <div className="container">
      <div className="page-heading"><span className="section-kicker">Каталог сообщества</span><h1>Найдите следующую книгу</h1><p>{result.totalCount} объявлений от читателей из разных городов</p></div>
      <div className="catalog-toolbar">
        <label className="catalog-search"><Search size={20} /><input value={params.get('search') || ''} onChange={e => update('search', e.target.value)} placeholder="Поиск по названию, автору, описанию" /></label>
        <select value={params.get('sort') || 'newest'} onChange={e => update('sort', e.target.value)}><option value="newest">Сначала новые</option><option value="oldest">Сначала старые</option><option value="title">По названию</option></select>
      </div>
      <div className="catalog-layout">
        <aside className="filters-panel">
          <div className="filters-title"><Filter size={18} /><b>Фильтры</b><button onClick={() => setParams(new URLSearchParams())}>Сбросить</button></div>
          <label>Жанр<select value={params.get('genreId') || ''} onChange={e => update('genreId', e.target.value)}><option value="">Все жанры</option>{genres.map(item => <option value={item.id} key={item.id}>{item.name}</option>)}</select></label>
          <label>Город<input value={params.get('city') || ''} onChange={e => update('city', e.target.value)} placeholder="Например, Казань" /></label>
          <label>Тип сделки<select value={params.get('dealType') || ''} onChange={e => update('dealType', e.target.value)}><option value="">Любой</option>{Object.entries(dealLabels).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</select></label>
          <label>Состояние<select value={params.get('condition') || ''} onChange={e => update('condition', e.target.value)}><option value="">Любое</option>{Object.entries(conditionLabels).map(([value, label]) => <option value={value} key={value}>{label}</option>)}</select></label>
          <div className="filter-tip"><SlidersHorizontal size={19} /><p>Выберите несколько параметров, чтобы найти книги поблизости.</p></div>
        </aside>
        <section className="catalog-results">
          {loading ? <div className="cards-skeleton">{Array.from({ length: 6 }).map((_, i) => <span key={i} />)}</div> : result.items.length ? <div className="book-grid catalog-grid">{result.items.map(book => <BookCard key={book.id} book={book} onFavoriteChanged={() => setResult({ ...result })} />)}</div> : <EmptyState title="Ничего не найдено" text="Попробуйте изменить запрос или сбросить часть фильтров." />}
          {result.totalPages > 1 && <div className="pagination">{Array.from({ length: result.totalPages }).map((_, index) => <button key={index} className={result.page === index + 1 ? 'active' : ''} onClick={() => update('page', String(index + 1))}>{index + 1}</button>)}</div>}
        </section>
      </div>
    </div>
  </div>
}
