import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ArrowLeft, MessageCircle, Search, Send } from 'lucide-react'
import { FormEvent, useEffect, useRef, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Avatar } from '../components/Avatar'
import { EmptyState } from '../components/EmptyState'
import { useAuth } from '../context/AuthContext'
import { api, getErrorMessage } from '../lib/api'
import { formatDateTime } from '../lib/constants'
import type { ChatMessage, Conversation, UserSummary } from '../lib/types'

export function ChatPage() {
  const { partnerId } = useParams(); const navigate = useNavigate(); const { user } = useAuth()
  const [conversations, setConversations] = useState<Conversation[]>([]); const [partner, setPartner] = useState<UserSummary | null>(null)
  const [messages, setMessages] = useState<ChatMessage[]>([]); const [text, setText] = useState(''); const [search, setSearch] = useState('')
  const [connection, setConnection] = useState<HubConnection | null>(null); const [error, setError] = useState(''); const endRef = useRef<HTMLDivElement>(null)

  const loadConversations = () => api.get<Conversation[]>('/messages/conversations').then(({ data }) => setConversations(data))
  useEffect(() => { void loadConversations() }, [])
  useEffect(() => {
    const token = localStorage.getItem('bookswap_token') || ''
    const hub = new HubConnectionBuilder().withUrl(import.meta.env.VITE_HUB_URL || '/hubs/chat', { accessTokenFactory: () => token }).configureLogging(LogLevel.Warning).withAutomaticReconnect().build()
    hub.on('ReceiveMessage', (message: ChatMessage) => {
      const relevant = partnerId && ((message.sender.id === partnerId && message.receiver.id === user?.id) || (message.receiver.id === partnerId && message.sender.id === user?.id))
      if (relevant) setMessages(items => items.some(item => item.id === message.id) ? items : [...items, message])
      void loadConversations()
    })
    hub.start().then(() => setConnection(hub)).catch(err => setError(getErrorMessage(err)))
    return () => { void hub.stop() }
  }, [user?.id, partnerId])

  useEffect(() => {
    if (!partnerId) { setPartner(null); setMessages([]); return }
    const known = conversations.find(item => item.user.id === partnerId)?.user
    const loadPartner = known ? Promise.resolve(known) : api.get<UserSummary>(`/users/${partnerId}`).then(({ data }) => data)
    void Promise.all([loadPartner, api.get<ChatMessage[]>(`/messages/${partnerId}`).then(({ data }) => data)]).then(([person, history]) => { setPartner(person); setMessages(history) }).catch(err => setError(getErrorMessage(err)))
  }, [partnerId, conversations])
  useEffect(() => { endRef.current?.scrollIntoView({ behavior: 'smooth' }) }, [messages])

  const send = async (event: FormEvent) => {
    event.preventDefault(); if (!partnerId || !connection || !text.trim()) return
    const content = text.trim(); setText('')
    try { await connection.invoke('SendMessage', partnerId, content) } catch (err) { setError(getErrorMessage(err)); setText(content) }
  }
  const filtered = conversations.filter(item => item.user.displayName.toLowerCase().includes(search.toLowerCase()))

  return <div className="chat-page"><div className="container chat-shell">
    <aside className={`chat-sidebar ${partnerId ? 'mobile-hidden' : ''}`}><div className="chat-sidebar-head"><h1>Сообщения</h1><label><Search size={17} /><input value={search} onChange={e => setSearch(e.target.value)} placeholder="Поиск диалога" /></label></div><div className="conversation-list">{filtered.map(item => <button key={item.user.id} className={partnerId === item.user.id ? 'active' : ''} onClick={() => navigate(`/chat/${item.user.id}`)}><Avatar name={item.user.displayName} url={item.user.avatarUrl} /><div><b>{item.user.displayName}</b><span>{item.lastMessage}</span></div><small>{new Intl.DateTimeFormat('ru-RU', { hour: '2-digit', minute: '2-digit' }).format(new Date(item.lastMessageAt))}</small>{item.unreadCount > 0 && <em>{item.unreadCount}</em>}</button>)}{!filtered.length && <EmptyState title="Диалогов пока нет" text="Напишите владельцу книги со страницы объявления." />}</div></aside>
    <section className={`chat-main ${!partnerId ? 'mobile-hidden' : ''}`}>
      {partner ? <><header className="chat-header"><button className="mobile-back" onClick={() => navigate('/chat')}><ArrowLeft /></button><Avatar name={partner.displayName} url={partner.avatarUrl} /><div><b>{partner.displayName}</b><span>{partner.city} · {partner.rating ? `рейтинг ${partner.rating.toFixed(1)}` : 'новый пользователь'}</span></div></header><div className="messages-area">{messages.map(message => <div key={message.id} className={`message-bubble ${message.sender.id === user?.id ? 'mine' : ''}`}><p>{message.content}</p><span>{formatDateTime(message.sentAt)}</span></div>)}{!messages.length && <div className="chat-welcome"><MessageCircle /><h2>Начните разговор</h2><p>Обсудите состояние книги, место встречи и детали обмена.</p></div>}<div ref={endRef} /></div>{error && <div className="chat-error">{error}</div>}<form className="message-form" onSubmit={send}><textarea value={text} onChange={e => setText(e.target.value)} placeholder="Напишите сообщение…" rows={1} onKeyDown={e => { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); e.currentTarget.form?.requestSubmit() } }} /><button disabled={!connection || !text.trim()}><Send /></button></form></> : <div className="chat-empty"><MessageCircle /><h2>Выберите диалог</h2><p>Здесь появится история сообщений с другим читателем.</p></div>}
    </section>
  </div></div>
}
