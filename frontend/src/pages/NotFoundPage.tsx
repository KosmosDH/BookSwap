import { ArrowLeft } from 'lucide-react'
import { Link } from 'react-router-dom'
export function NotFoundPage() { return <div className="not-found"><span>404</span><h1>Эта страница потерялась между полками</h1><p>Возможно, ссылка устарела или адрес введён неверно.</p><Link className="button button-primary" to="/"><ArrowLeft size={18} />На главную</Link></div> }
