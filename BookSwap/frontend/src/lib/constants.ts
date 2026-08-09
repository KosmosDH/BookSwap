import { BookCondition, BookStatus, DealType, ExchangeStatus } from './types'

export const conditionLabels: Record<BookCondition, string> = {
  [BookCondition.New]: 'Новая', [BookCondition.Excellent]: 'Отличное', [BookCondition.Good]: 'Хорошее',
  [BookCondition.Fair]: 'Удовлетворительное', [BookCondition.Poor]: 'Плохое'
}
export const dealLabels: Record<DealType, string> = {
  [DealType.Exchange]: 'Обмен', [DealType.GiveAway]: 'Отдам', [DealType.Sell]: 'Продажа', [DealType.Lend]: 'Дам почитать'
}
export const statusLabels: Record<BookStatus, string> = {
  [BookStatus.Available]: 'Доступна', [BookStatus.Reserved]: 'Зарезервирована', [BookStatus.Exchanged]: 'Передана', [BookStatus.Hidden]: 'Скрыта'
}
export const exchangeLabels: Record<ExchangeStatus, string> = {
  [ExchangeStatus.Pending]: 'Ожидает ответа', [ExchangeStatus.Accepted]: 'Принята', [ExchangeStatus.Rejected]: 'Отклонена',
  [ExchangeStatus.Cancelled]: 'Отменена', [ExchangeStatus.Completed]: 'Завершена'
}
export const formatDate = (value: string) => new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' }).format(new Date(value))
export const formatDateTime = (value: string) => new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' }).format(new Date(value))
