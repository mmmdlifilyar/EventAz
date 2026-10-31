import type { AdminStatistics, Category, EventItem, PagedResult, PromoCode, Review, TicketHistory } from './types'

const API_BASE = (import.meta.env.VITE_API_BASE_URL ?? '').replace(/\/$/, '')

export function getToken() {
  return localStorage.getItem('eventaz-token')
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  if (init.body) headers.set('Content-Type', 'application/json')
  const token = getToken()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  let response: Response
  try {
    response = await fetch(`${API_BASE}${path}`, { ...init, headers })
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw error
    throw new Error('API serverinə qoşulmaq mümkün olmadı. WebAPI-nin işlədiyini yoxla.')
  }
  if (!response.ok) {
    let message = `Sorğu alınmadı (${response.status}).`
    try {
      const body = await response.json()
      message = body.detail ?? body.title ?? body.error ?? message
      const knownErrors: Record<string, string> = {
        EventNotFinished: 'Tədbir başa çatdıqdan sonra rəy yaza bilərsən.',
        TicketRequired: 'Rəy yazmaq üçün bu tədbirə bilet almalısan.',
        AlreadyReviewed: 'Bu tədbirə artıq rəy yazmısan.',
        PublishedEventNotFound: 'Tədbir tapılmadı və ya yayımlanmayıb.',
      }
      if (typeof body.error === 'string') message = knownErrors[body.error] ?? body.error
      if (body.errors) message = Object.values(body.errors).flatMap((value) => Array.isArray(value) ? value : [value]).join(' ')
    } catch {
      // Cavab JSON olmadıqda status mesajı göstərilir.
    }
    throw new Error(message)
  }
  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}

export type EventFilters = {
  search: string
  categoryId: string
  location: string
  fromDate: string
  toDate: string
  minPrice: string
  maxPrice: string
  sortBy: string
  page: number
}

export async function getEvents(filters: EventFilters, pageSize = 9, signal?: AbortSignal) {
  const query = new URLSearchParams({ page: String(filters.page), pageSize: String(pageSize), sortBy: filters.sortBy })
  if (filters.search) query.set('search', filters.search)
  if (filters.categoryId) query.set('categoryId', filters.categoryId)
  if (filters.location) query.set('location', filters.location)
  if (filters.fromDate) query.set('fromDate', filters.fromDate)
  if (filters.toDate) query.set('toDate', filters.toDate)
  if (filters.minPrice) query.set('minPrice', filters.minPrice)
  if (filters.maxPrice) query.set('maxPrice', filters.maxPrice)
  return request<PagedResult<EventItem>>(`/api/events?${query}`, { signal })
}

export const getEvent = (id: string) => request<EventItem>(`/api/events/${id}`)
export const getAdminEvents = () => request<EventItem[]>('/api/events/admin')
export const getCategories = () => request<Category[]>('/api/categories')
export const createCategory = (name: string) => request<string>('/api/categories', { method: 'POST', body: JSON.stringify({ name }) })
export const getFavorites = () => request<EventItem[]>('/api/favorites')
export const addFavorite = (id: string) => request<void>(`/api/favorites/${id}`, { method: 'PUT' })
export const removeFavorite = (id: string) => request<void>(`/api/favorites/${id}`, { method: 'DELETE' })
export const purchaseTicket = (ticketTierId: string, promoCode: string) =>
  request<{ id: string; pricePaid: number; status: string }>(`/api/tickets`, {
    method: 'POST', body: JSON.stringify({ ticketTierId, promoCode: promoCode || null }),
  })

export const authenticate = (mode: 'login' | 'register', data: Record<string, string>) =>
  request<{ token: string }>(`/api/auth/${mode}`, { method: 'POST', body: JSON.stringify(data) })

export const getMyTickets = () => request<TicketHistory[]>('/api/tickets/mine')
export const getEventReviews = (eventId: string) => request<Review[]>(`/api/reviews/event/${eventId}`)
export const createReview = (eventId: string, rating: number, comment: string) =>
  request<Review>('/api/reviews', {
    method: 'POST', body: JSON.stringify({ eventId, rating, comment: comment || null }),
  })
export const createEvent = (payload: unknown) =>
  request<{ id: string }>('/api/events', { method: 'POST', body: JSON.stringify(payload) })
export const publishEvent = (eventId: string) =>
  request<void>(`/api/events/${eventId}/publish`, { method: 'POST' })
export const getAdminStatistics = () => request<AdminStatistics>('/api/admin/statistics')
export const createPromoCode = (payload: unknown) =>
  request<PromoCode>('/api/promo-codes', { method: 'POST', body: JSON.stringify(payload) })
export const getPromoCodesByEvent = (eventId: string) =>
  request<PromoCode[]>(`/api/promo-codes/by-event/${eventId}`)
export const deactivatePromoCode = (id: string) =>
  request<void>(`/api/promo-codes/${id}`, { method: 'DELETE' })
export const validateTicket = (qrCode: string) =>
  request<{ status: string; ticketId?: string; eventId?: string; eventTitle?: string }>('/api/tickets/validate', {
    method: 'POST', body: JSON.stringify({ qrCode }),
  })

export function getRoles() {
  const token = getToken()
  if (!token) return [] as string[]
  try {
    const encodedPayload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
      .padEnd(Math.ceil(token.split('.')[1].length / 4) * 4, '=')
    const payload = JSON.parse(window.atob(encodedPayload)) as Record<string, unknown>
    const roleClaims = Object.entries(payload).filter(([key]) => key === 'role' || key === 'roles' || key.endsWith('/role'))
    return roleClaims.flatMap(([, value]) => Array.isArray(value) ? value.map(String) : value ? [String(value)] : [])
  } catch {
    return []
  }
}

export async function downloadTicketQr(ticketId: string) {
  const response = await fetch(`${API_BASE}/api/tickets/${ticketId}/qr`, {
    headers: { Authorization: `Bearer ${getToken()}` },
  })
  if (!response.ok) throw new Error('QR bileti yükləmək mümkün olmadı.')
  return URL.createObjectURL(await response.blob())
}

