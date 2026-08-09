import { ArrowRight, BookHeart, Repeat2, Search, ShieldCheck, Sparkles, Users } from 'lucide-react'
import { FormEvent, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { BookCard } from '../components/BookCard'
import { api } from '../lib/api'
import type { BookListItem } from '../lib/types'

export function HomePage() {
  const [books, setBooks] = useState<BookListItem[]>([])
  const [search, setSearch] = useState('')
  const navigate = useNavigate()
  useEffect(() => { api.get<BookListItem[]>('/books/featured').then(({ data }) => setBooks(data)).catch(() => {}) }, [])
  const submit = (event: FormEvent) => { event.preventDefault(); navigate(`/catalog?search=${encodeURIComponent(search)}`) }

  return <>
    <section className="hero-section">
      <div className="hero-orb hero-orb-one" /><div className="hero-orb hero-orb-two" />
      <div className="container hero-grid">
        <div className="hero-copy">
          <span className="eyebrow"><Sparkles size={15} /> Книжный обмен нового поколения</span>
          <h1>Прочитанные книги заслуживают <em>новых читателей</em></h1>
          <p>Обменивайтесь книгами с людьми рядом, отдавайте ненужные издания и находите следующую историю без лишних покупок.</p>
          <form className="hero-search" onSubmit={submit}>
            <Search size={21} /><input value={search} onChange={e => setSearch(e.target.value)} placeholder="Название, автор или жанр" /><button>Найти</button>
          </form>
          <div className="hero-proof"><div className="avatar-stack"><span>А</span><span>М</span><span>Е</span><span>+2k</span></div><p><b>2 000+ читателей</b><br />уже делятся книгами</p></div>
        </div>
        <div className="hero-visual">
          <div className="floating-card card-one"><BookHeart size={22} /><div><b>Книга нашла читателя</b><span>«Солярис» · Казань</span></div></div>
          <div className="hero-book-stack">
            <div className="hero-book book-a"><span>Научная<br />фантастика</span><b>SOLARIS</b></div>
            <div className="hero-book book-b"><span>Классика</span><b>1984</b></div>
            <div className="hero-book book-c"><span>Технологии</span><b>CLEAN<br />CODE</b></div>
          </div>
          <div className="floating-card card-two"><Repeat2 size={22} /><div><b>Новый обмен</b><span>Ответ получен 2 мин назад</span></div></div>
        </div>
      </div>
    </section>

    <section className="section featured-section">
      <div className="container">
        <div className="section-heading"><div><span className="section-kicker">Свежее в каталоге</span><h2>Книги, которые ищут новый дом</h2></div><Link to="/catalog" className="text-link">Смотреть все <ArrowRight size={18} /></Link></div>
        <div className="book-grid">{books.map(book => <BookCard key={book.id} book={book} />)}</div>
      </div>
    </section>

    <section className="section how-section">
      <div className="container">
        <div className="center-heading"><span className="section-kicker">Просто начать</span><h2>Три шага к новой книге</h2><p>Никаких сложных правил: создайте объявление, договоритесь и обменяйтесь.</p></div>
        <div className="steps-grid">
          <article><span className="step-number">01</span><div className="step-icon"><BookHeart /></div><h3>Добавьте книгу</h3><p>Укажите автора, состояние, город и формат сделки. Это займёт пару минут.</p></article>
          <article><span className="step-number">02</span><div className="step-icon"><Users /></div><h3>Найдите читателя</h3><p>Получайте заявки или сами предлагайте обмен владельцам интересных книг.</p></article>
          <article><span className="step-number">03</span><div className="step-icon"><Repeat2 /></div><h3>Обменяйтесь</h3><p>Обсудите детали в чате, встретьтесь и оставьте честный отзыв.</p></article>
        </div>
      </div>
    </section>

    <section className="section trust-section"><div className="container trust-grid"><div><span className="section-kicker light">Безопасное сообщество</span><h2>Доверие строится на прозрачности</h2><p>Рейтинги после завершённых обменов, система жалоб и модерация помогают поддерживать уважительную среду.</p><Link to="/register" className="button button-light">Присоединиться <ArrowRight size={18} /></Link></div><div className="trust-points"><div><ShieldCheck /><span><b>Отзывы только после сделки</b><small>Никакой случайной накрутки рейтинга</small></span></div><div><Users /><span><b>Публичные профили</b><small>История и репутация участников</small></span></div><div><Sparkles /><span><b>Живая модерация</b><small>Жалобы рассматриваются администраторами</small></span></div></div></div></section>
  </>
}
