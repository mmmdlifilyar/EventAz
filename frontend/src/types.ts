export type TicketTier = {
  id: string
  name: string
  price: number
  totalQuantity: number
  available: number
}

export type EventItem = {
  id: string
  title: string
  description: string
  location: string
  startDate: string
  endDate: string
  status: string
  categoryId: string
  categoryName: string
  tiers: TicketTier[]
  averageRating: number
}

export type Category = { id: string; name: string }
export type PagedResult<T> = { items: T[]; page: number; pageSize: number; totalCount: number }
export type Review = { id: string; eventId: string; rating: number; comment?: string | null; createdAt: string }
export type TicketHistory = {
  id: string
  eventTitle: string
  location: string
  eventStartDate: string
  tierName: string
  pricePaid: number
  purchaseDate: string
  status: string
}
export type PromoCode = {
  id: string
  eventId: string
  code: string
  discountPercent: number
  expiryDate: string
  usageLimit: number
  usedCount: number
  isActive: boolean
}
export type AdminStatistics = {
  totalUsers: number
  totalEvents: number
  publishedEvents: number
  ticketsSold: number
  totalRevenue: number
  topSellingEvents: { eventId: string; title: string; ticketsSold: number; revenue: number }[]
}

