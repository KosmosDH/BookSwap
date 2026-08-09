export enum BookCondition { New = 1, Excellent, Good, Fair, Poor }
export enum DealType { Exchange = 1, GiveAway, Sell, Lend }
export enum BookStatus { Available = 1, Reserved, Exchanged, Hidden }
export enum ExchangeStatus { Pending = 1, Accepted, Rejected, Cancelled, Completed }
export enum ComplaintStatus { Open = 1, InReview, Resolved, Rejected }

export interface UserSummary { id: string; displayName: string; city: string; avatarUrl?: string; rating: number; reviewsCount: number }
export interface UserProfile { id: string; email: string; displayName: string; city: string; avatarUrl?: string; bio?: string; roles: string[]; isBlocked: boolean }
export interface AuthResponse { token: string; expiresAt: string; user: UserProfile }
export interface Genre { id: string; name: string; slug: string }
export interface BookListItem {
  id: string; title: string; author: string; city: string; condition: BookCondition; dealType: DealType;
  status: BookStatus; price?: number; coverUrl?: string; createdAt: string; owner: UserSummary; genres: string[]; isFavorite: boolean
}
export interface BookDetails extends Omit<BookListItem, 'coverUrl' | 'genres'> {
  description: string; isbn?: string; language: string; updatedAt: string; genres: Genre[];
  photos: { id: string; url: string; isPrimary: boolean }[]; canEdit: boolean
}
export interface PagedResult<T> { items: T[]; page: number; pageSize: number; totalCount: number; totalPages: number }
export interface Exchange {
  id: string; sender: UserSummary; receiver: UserSummary; requestedBook: BookListItem; offeredBook?: BookListItem;
  message?: string; status: ExchangeStatus; createdAt: string; updatedAt: string; canAccept: boolean; canCancel: boolean; canComplete: boolean
}
export interface NotificationItem { id: string; type: number; title: string; body: string; link?: string; isRead: boolean; createdAt: string }
export interface ChatMessage { id: string; sender: UserSummary; receiver: UserSummary; content: string; sentAt: string; readAt?: string }
export interface Conversation { user: UserSummary; lastMessage: string; lastMessageAt: string; unreadCount: number }
export interface Review { id: string; author: UserSummary; rating: number; text: string; createdAt: string }
export interface AdminStats { users: number; books: number; exchanges: number; openComplaints: number; messages: number }
