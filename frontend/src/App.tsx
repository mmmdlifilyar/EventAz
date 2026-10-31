import { useCallback, useEffect, useMemo, useState } from 'react'
import { addFavorite, authenticate, createReview, downloadTicketQr, getCategories, getEvent, getEventReviews, getEvents, getFavorites, getMyTickets, getRoles, getToken, purchaseTicket, removeFavorite, type EventFilters } from './api'
import type { Category, EventItem, Review, TicketHistory } from './types'
import ManagementPanel from './ManagementPanel'

const initialFilters: EventFilters = { search: '', categoryId: '', location: '', fromDate: '', toDate: '', minPrice: '', maxPrice: '', sortBy: 'date', page: 1 }
const palettes = ['plum', 'blue', 'orange', 'green', 'pink', 'night']
const months = ['yanvar', 'fevral', 'mart', 'aprel', 'may', 'iyun', 'iyul', 'avqust', 'sentyabr', 'oktyabr', 'noyabr', 'dekabr']

function dateParts(value: string) {
  const date = new Date(value)
  return { day: date.getDate(), month: months[date.getMonth()], full: `${date.getDate()} ${months[date.getMonth()]}, ${date.getFullYear()}` }
}

function timeLabel(value: string) {
  return new Intl.DateTimeFormat('az-AZ', { hour: '2-digit', minute: '2-digit' }).format(new Date(value))
}

function money(value: number) {
  return new Intl.NumberFormat('az-AZ', { style: 'currency', currency: 'AZN', maximumFractionDigits: 0 }).format(value)
}

function escapeCalendarText(value: string) {
  return value.replace(/\\/g, '\\\\').replace(/\r?\n/g, '\\n').replace(/,/g, '\\,').replace(/;/g, '\\;')
}

function calendarDate(value: string) {
  return new Date(value).toISOString().replace(/[-:]/g, '').replace(/\.\d{3}/, '')
}

function App() {
  const [filters, setFilters] = useState(initialFilters)
  const [categories, setCategories] = useState<Category[]>([])
  const [events, setEvents] = useState<EventItem[]>([])
  const [totalCount, setTotalCount] = useState(0)
  const [favorites, setFavorites] = useState<EventItem[]>([])
  const [activeView, setActiveView] = useState<'events' | 'favorites' | 'tickets' | 'manage'>('events')
  const [selectedEvent, setSelectedEvent] = useState<EventItem | null>(null)
  const [reviews, setReviews] = useState<Review[]>([])
  const [reviewsLoading, setReviewsLoading] = useState(false)
  const [reviewRating, setReviewRating] = useState(5)
  const [reviewComment, setReviewComment] = useState('')
  const [tickets, setTickets] = useState<TicketHistory[]>([])
  const [ticketsLoading, setTicketsLoading] = useState(false)
  const [favoritesLoading, setFavoritesLoading] = useState(false)
  const [ticketQrUrls, setTicketQrUrls] = useState<Record<string, string>>({})
  const [authOpen, setAuthOpen] = useState(false)
  const [authMode, setAuthMode] = useState<'login' | 'register'>('login')
  const [authError, setAuthError] = useState('')
  const [authSubmitting, setAuthSubmitting] = useState(false)
  const [reviewSubmitting, setReviewSubmitting] = useState(false)
  const [purchasingTierId, setPurchasingTierId] = useState<string | null>(null)
  const [token, setToken] = useState(getToken())
  const [busy, setBusy] = useState(false)
  const [notice, setNotice] = useState('')
  const [error, setError] = useState('')
  const [searchDraft, setSearchDraft] = useState('')
  const [promoCode, setPromoCode] = useState('')
  const [qrUrl, setQrUrl] = useState('')

  const roles = useMemo(() => getRoles(), [token])
  const managementRole = roles.includes('Admin') ? 'Admin' : roles.includes('Organizer') ? 'Organizer' : null
  const showFavorites = activeView === 'favorites'

  const loadEvents = useCallback(async (signal?: AbortSignal) => {
    setBusy(true)
    setError('')
    try {
      const result = await getEvents(filters, 9, signal)
      setEvents(result.items)
      setTotalCount(result.totalCount)
    } catch (err) {
      if (!(err instanceof DOMException && err.name === 'AbortError'))
        setError(`${(err as Error).message} API ünvanı: https://localhost:7012.`)
    } finally {
      if (!signal?.aborted) setBusy(false)
    }
  }, [filters])

  useEffect(() => {
    const controller = new AbortController()
    const timer = window.setTimeout(() => { void loadEvents(controller.signal) }, 250)
    return () => { window.clearTimeout(timer); controller.abort() }
  }, [loadEvents])
  useEffect(() => { void getCategories().then(setCategories).catch((err) => setError(`Kateqoriyaları yükləmək mümkün olmadı: ${(err as Error).message}`)) }, [])
  useEffect(() => {
    if (!token) { setFavorites([]); return }
    setFavoritesLoading(true)
    void getFavorites().then(setFavorites).catch((err) => setError((err as Error).message)).finally(() => setFavoritesLoading(false))
  }, [token])
  useEffect(() => {
    if (!token || activeView !== 'tickets') { setTickets([]); return }
    setTicketsLoading(true)
    void getMyTickets().then(setTickets).catch((err) => setError((err as Error).message)).finally(() => setTicketsLoading(false))
  }, [token, activeView])
  useEffect(() => {
    if (!selectedEvent) { setReviews([]); return }
    setReviewRating(5)
    setReviewComment('')
    setReviewsLoading(true)
    void getEventReviews(selectedEvent.id).then(setReviews).catch(() => setReviews([])).finally(() => setReviewsLoading(false))
  }, [selectedEvent])
  useEffect(() => {
    const eventId = new URLSearchParams(window.location.search).get('event')
    if (eventId) void getEvent(eventId).then(setSelectedEvent).catch((err) => setError(`Paylaşılan tədbiri açmaq mümkün olmadı: ${(err as Error).message}`))
  }, [])
  useEffect(() => {
    if (!notice) return
    const timer = window.setTimeout(() => setNotice(''), 3500)
    return () => window.clearTimeout(timer)
  }, [notice])
  useEffect(() => { setError('') }, [activeView])

  const visibleEvents = showFavorites ? favorites : events
  const eventListLoading = showFavorites ? favoritesLoading : busy
  const favoriteIds = useMemo(() => new Set(favorites.map((event) => event.id)), [favorites])
  const pageCount = Math.max(1, Math.ceil(totalCount / 9))
  const hasActiveFilters = Object.entries(filters).some(([key, value]) => key !== 'page' && key !== 'sortBy' && value !== '')

  function updateFilter<K extends keyof EventFilters>(key: K, value: EventFilters[K]) {
    setFilters((current) => ({ ...current, [key]: value, page: 1 }))
  }

  function openAuth(mode: 'login' | 'register' = 'login') {
    setAuthError('')
    setAuthMode(mode)
    setAuthOpen(true)
  }

  const showError = useCallback((message: string) => setError(message), [])
  const showNotice = useCallback((message: string) => setNotice(message), [])
  const refreshCategories = useCallback(() => {
    void getCategories().then(setCategories).catch((err) => setError((err as Error).message))
  }, [])

  async function toggleFavorite(event: EventItem) {
    if (!token) { openAuth('login'); return }
    try {
      if (favoriteIds.has(event.id)) {
        await removeFavorite(event.id)
        setFavorites((current) => current.filter((item) => item.id !== event.id))
      } else {
        await addFavorite(event.id)
        setFavorites((current) => [...current, event])
      }
    } catch (err) { setError((err as Error).message) }
  }

  async function openDetails(event: EventItem) {
    setError('')
    setPromoCode('')
    setQrUrl('')
    try { setSelectedEvent(await getEvent(event.id)) }
    catch { setSelectedEvent(event) }
  }

  async function submitReview() {
    if (!selectedEvent) return
    if (!token) { openAuth('login'); return }
    setReviewSubmitting(true)
    try {
      const review = await createReview(selectedEvent.id, reviewRating, reviewComment.trim())
      setReviews((current) => [review, ...current])
      setReviewComment('')
      const refreshed = await getEvent(selectedEvent.id)
      setSelectedEvent(refreshed)
      await loadEvents()
      setNotice('Rəyin əlavə edildi.')
    } catch (err) { setError((err as Error).message) }
    finally { setReviewSubmitting(false) }
  }

  async function loadTicketQr(ticketId: string) {
    try {
      const url = await downloadTicketQr(ticketId)
      setTicketQrUrls((current) => ({ ...current, [ticketId]: url }))
    } catch (err) { setError((err as Error).message) }
  }

  async function handleAuth(form: FormData) {
    setAuthError('')
    setAuthSubmitting(true)
    const data: Record<string, string> = {
      email: String(form.get('email') ?? ''),
      password: String(form.get('password') ?? ''),
    }
    if (authMode === 'register') {
      data.fullName = String(form.get('fullName') ?? '')
      data.role = 'User'
    }
    try {
      const result = await authenticate(authMode, data)
      localStorage.setItem('eventaz-token', result.token)
      setToken(result.token)
      setAuthOpen(false)
      setNotice(authMode === 'login' ? 'Hesabına uğurla daxil oldun.' : 'Qeydiyyat tamamlandı.')
    } catch (err) { setAuthError((err as Error).message) }
    finally { setAuthSubmitting(false) }
  }

  async function handlePurchase(tierId: string) {
    if (!token) { openAuth('login'); return }
    setPurchasingTierId(tierId)
    try {
      const ticket = await purchaseTicket(tierId, promoCode.trim())
      setNotice('Biletin uğurla alındı! QR biletini buradan yükləyə bilərsən.')
      try {
        const url = await downloadTicketQr(ticket.id)
        setQrUrl(url)
      } catch (err) {
        setError(`Bilet alındı, amma QR kodu yüklənmədi: ${(err as Error).message}`)
      }
    } catch (err) { setError((err as Error).message) }
    finally { setPurchasingTierId(null) }
  }

  async function shareEvent(event: EventItem) {
    const url = new URL(window.location.href)
    url.searchParams.set('event', event.id)
    try {
      if (navigator.share) await navigator.share({ title: event.title, text: `${event.title} — ${event.location}`, url: url.toString() })
      else {
        await navigator.clipboard.writeText(url.toString())
        setNotice('Tədbirin linki köçürüldü.')
      }
    } catch (err) {
      if (err instanceof DOMException && err.name === 'AbortError') return
      setError('Tədbir linkini paylaşmaq mümkün olmadı. Brauzerin paylaşım icazəsini yoxla.')
    }
  }

  function downloadCalendarEvent(event: EventItem) {
    const content = [
      'BEGIN:VCALENDAR', 'VERSION:2.0', 'PRODID:-//EventAz//Event Calendar//AZ', 'CALSCALE:GREGORIAN', 'BEGIN:VEVENT',
      `UID:${event.id}@eventaz`, `DTSTAMP:${calendarDate(new Date().toISOString())}`,
      `DTSTART:${calendarDate(event.startDate)}`, `DTEND:${calendarDate(event.endDate)}`,
      `SUMMARY:${escapeCalendarText(event.title)}`, `DESCRIPTION:${escapeCalendarText(event.description)}`,
      `LOCATION:${escapeCalendarText(event.location)}`, 'END:VEVENT', 'END:VCALENDAR',
    ].join('\r\n')
    const url = URL.createObjectURL(new Blob([content], { type: 'text/calendar;charset=utf-8' }))
    const link = document.createElement('a')
    link.href = url
    link.download = `${event.title.trim().replace(/[^\p{L}\p{N}-]+/gu, '-').replace(/^-|-$/g, '') || 'tedbir'}.ics`
    document.body.appendChild(link)
    link.click()
    link.remove()
    window.setTimeout(() => URL.revokeObjectURL(url), 1000)
    setNotice('Tədbir təqvim faylı endirildi.')
  }

  function signOut() {
    localStorage.removeItem('eventaz-token')
    setToken(null)
    setFavorites([])
    setTickets([])
    Object.values(ticketQrUrls).forEach(URL.revokeObjectURL)
    setTicketQrUrls({})
    setSelectedEvent(null)
    setAuthOpen(false)
    setActiveView('events')
    setNotice('Hesabdan çıxış edildi.')
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="brand" href="#top" onClick={() => setActiveView('events')} aria-label="EventAz ana səhifə">
          <span className="brand-mark">e<span>·</span></span><span>event<span className="brand-accent">az</span></span>
        </a>
        <nav className="main-nav" aria-label="Əsas menyu">
          <button className={activeView === 'events' ? 'nav-link active' : 'nav-link'} onClick={() => setActiveView('events')}>Tədbirlər</button>
          <button className={showFavorites ? 'nav-link active' : 'nav-link'} onClick={() => { if (!token) { openAuth('login'); return } setActiveView('favorites') }}>Favoritlər <span className="nav-count">{favorites.length}</span></button>
          {token && <button className={activeView === 'tickets' ? 'nav-link active' : 'nav-link'} onClick={() => setActiveView('tickets')}>Biletlərim</button>}
          {token && managementRole && <button className={activeView === 'manage' ? 'nav-link active' : 'nav-link'} onClick={() => setActiveView('manage')}>İdarəetmə</button>}
        </nav>
        <div className="top-actions">
          <span className="location-pill"><span className="pin">⌖</span> Bakı, Azərbaycan</span>
          {token ? <button className="user-button" onClick={signOut}><span className="avatar">E</span><span>Çıxış</span></button> : <button className="login-button" onClick={() => openAuth('login')}>Daxil ol <span>↗</span></button>}
        </div>
      </header>

      <main id="top">
        {(activeView === 'manage' || activeView === 'tickets') && error && <div className="error-banner page-error"><span>!</span><div>{error}</div><button onClick={() => setError('')}>Bağla</button></div>}
        {(activeView === 'events' || activeView === 'favorites') && <section className="hero">
          <div className="hero-content">
            <div className="eyebrow"><span className="eyebrow-dot" /> ŞƏHƏRİN NƏFƏS ALIR</div>
            <h1>Yaxşı vaxt<br />öz-özünə <em>olmur.</em></h1>
            <p>Şəhərində baş verənləri kəşf et. Sənə uyğun tədbiri tap, yerini indi ayır.</p>
            <div className="hero-meta"><span className="avatar-stack"><i>♫</i><i>✦</i><i>☼</i></span><span>Hər həftə yeni təcrübələr</span><span className="meta-divider" /> <span className="live-dot" /> Canlı tədbirlər</div>
          </div>
          <div className="hero-art" aria-hidden="true">
            <div className="sun-shape" /><div className="art-orb orb-one" /><div className="art-orb orb-two" />
            <div className="ticket-art"><span>EVENTAZ · BAKI</span><strong>LIVE<br />YOUR<br /><i>WEEKEND</i></strong><div className="ticket-line" /><small>№ 00482　·　Bakı, 2026</small></div>
            <div className="art-sticker">YENİ<br />TƏCRÜBƏ<br /><b>BURADA ↘</b></div>
            <div className="art-spark spark-a">✳</div><div className="art-spark spark-b">✦</div>
          </div>
          <div className="hero-bottom"><span>BAKIDA NƏLƏR BAŞ VERİR?</span><span>↓　KƏŞF ET</span></div>
        </section>}

        {(activeView === 'events' || activeView === 'favorites') && <section className="discovery-section" id="events">
          <div className="section-heading">
            <div><div className="eyebrow dark-eyebrow">SƏNİN ŞƏHƏRİN, SƏNİN SEÇİMİN</div><h2>{showFavorites ? 'Seçilmiş tədbirlər' : 'Növbəti planın nədir?'}</h2><p>{showFavorites ? 'Bəyəndiyin tədbirlər burada saxlanılır.' : 'Musiqidən incəsənətə — sənə uyğun olanı tap.'}</p></div>
            <div className="result-note"><b>{showFavorites ? favorites.length : totalCount}</b> tədbir tapıldı <span>✳</span></div>
          </div>

          {!showFavorites && <div className="filter-panel">
            <form className="search-box" onSubmit={(event) => { event.preventDefault(); updateFilter('search', searchDraft) }}><span className="search-icon">⌕</span><input value={searchDraft} onChange={(event) => setSearchDraft(event.target.value)} placeholder="Tədbir, məkan və ya açar söz axtar..." /><button type="submit">Axtar <span>→</span></button></form>
            <div className="filter-row">
              <label className="filter-field"><span className="field-icon">▦</span><select value={filters.categoryId} onChange={(event) => updateFilter('categoryId', event.target.value)}><option value="">Bütün kateqoriyalar</option>{categories.map((item) => <option key={item.id} value={item.id}>{item.name}</option>)}</select></label>
              <label className="filter-field"><span className="field-icon">⌖</span><input value={filters.location} onChange={(event) => updateFilter('location', event.target.value)} placeholder="Məkan" /></label>
              <label className="filter-field"><span className="field-icon">▣</span><input type="date" max={filters.toDate || undefined} value={filters.fromDate} onChange={(event) => updateFilter('fromDate', event.target.value)} aria-label="Başlanğıc tarixi" /></label>
              <label className="filter-field"><span className="field-icon">▣</span><input type="date" min={filters.fromDate || undefined} value={filters.toDate} onChange={(event) => updateFilter('toDate', event.target.value)} aria-label="Son tarix" /></label>
              <label className="filter-field price-field"><span className="field-icon">₼</span><input type="number" min="0" step="0.01" value={filters.minPrice} onChange={(event) => updateFilter('minPrice', event.target.value)} placeholder="Min" aria-label="Minimum qiymət" /><span className="price-dash">—</span><input type="number" min="0" step="0.01" value={filters.maxPrice} onChange={(event) => updateFilter('maxPrice', event.target.value)} placeholder="Maks" aria-label="Maksimum qiymət" /></label>
              <label className="filter-field sort-field"><span className="field-icon">↕</span><select value={filters.sortBy} onChange={(event) => updateFilter('sortBy', event.target.value)}><option value="date">Tarixə görə</option><option value="date_desc">Ən son tarix</option><option value="price_asc">Ucuzdan bahaya</option><option value="price_desc">Bahadan ucuza</option></select></label>
            </div>
            <nav className="category-nav" aria-label="Tədbir kateqoriyaları"><span>Kateqoriyalar</span><div className="category-nav-items"><button type="button" className={!filters.categoryId ? 'category-chip selected' : 'category-chip'} onClick={() => updateFilter('categoryId', '')}>Hamısı</button>{categories.map((category) => <button type="button" key={category.id} className={filters.categoryId === category.id ? 'category-chip selected' : 'category-chip'} onClick={() => updateFilter('categoryId', category.id)}>{category.name}</button>)}</div>{!categories.length && <small className="category-empty">Kateqoriya tapılmadı.</small>}</nav>
            {hasActiveFilters && <div className="filter-actions"><button type="button" onClick={() => { setFilters(initialFilters); setSearchDraft('') }}>Filtrləri təmizlə <span>×</span></button></div>}
          </div>}

          {error && <div className="error-banner"><span>!</span><div>{error}</div><button onClick={() => { setError(''); void loadEvents() }}>Yenidən yoxla</button></div>}
          {eventListLoading && <div className="loading-state"><span className="loader" /> {showFavorites ? 'Favoritlər yüklənir...' : 'Tədbirlər yüklənir...'}</div>}
          {!eventListLoading && !error && visibleEvents.length === 0 && <div className="empty-state"><div>✳</div><h3>{showFavorites ? 'Hələ favorit tədbirin yoxdur' : 'Axtarışa uyğun tədbir tapılmadı'}</h3><p>{showFavorites ? 'Bəyəndiyin tədbirin yanındakı ürək işarəsinə toxun.' : 'Filtrləri dəyiş və başqa axtarış sözü yoxla.'}</p><button className="outline-button" onClick={() => { setActiveView('events'); setFilters(initialFilters); setSearchDraft('') }}>Bütün tədbirlərə qayıt</button></div>}
          {!eventListLoading && visibleEvents.length > 0 && <div className="event-grid">
            {visibleEvents.map((event, index) => {
              const date = dateParts(event.startDate)
              const availableTiers = event.tiers.filter((tier) => tier.available > 0)
              const lowestPrice = availableTiers.length ? Math.min(...availableTiers.map((tier) => tier.price)) : 0
              const isFav = favoriteIds.has(event.id)
              return <article className="event-card" key={event.id}>
                <button className={`event-art ${palettes[index % palettes.length]}`} onClick={() => void openDetails(event)} aria-label={`${event.title} detallarını aç`}>
                  <span className="art-label">{event.categoryName || 'TƏDBİR'}</span><span className="art-date"><b>{date.day}</b><small>{date.month.slice(0, 3)}</small></span>
                  <span className="card-art-symbol">{['♫', '✳', '◉', '✦', '☼', '◌'][index % 6]}</span><span className="art-caption">{event.location.toUpperCase()}</span>
                </button>
                <button className={`heart-button ${isFav ? 'hearted' : ''}`} onClick={() => void toggleFavorite(event)} aria-label={isFav ? 'Favoritlərdən çıxar' : 'Favoritlərə əlavə et'}>{isFav ? '♥' : '♡'}</button>
                <div className="event-card-body"><div className="event-date-line">{date.full} <span>·</span> {event.location}</div><button className="event-title" onClick={() => void openDetails(event)}>{event.title}</button><div className="event-card-footer"><span className="rating">★ {event.averageRating ? event.averageRating.toFixed(1) : 'Yeni'}</span><span className="price-from">{lowestPrice ? `${money(lowestPrice)}-dən` : availableTiers.length ? 'Qiymət göstərilməyib' : 'Biletlər bitib'}</span></div></div>
              </article>
            })}
          </div>}
          {!showFavorites && pageCount > 1 && <div className="pagination"><button disabled={filters.page <= 1} onClick={() => setFilters((current) => ({ ...current, page: current.page - 1 }))}>← Əvvəlki</button><span>{filters.page} / {pageCount}</span><button disabled={filters.page >= pageCount} onClick={() => setFilters((current) => ({ ...current, page: current.page + 1 }))}>Sonrakı →</button></div>}
        </section>}

        {activeView === 'tickets' && <section className="account-page"><div className="account-heading"><div><span className="eyebrow dark-eyebrow">SƏNİN BİLETLƏRİN</span><h2>Yaxınlaşan təcrübələrin.</h2><p>Alınmış biletlər və tədbir məlumatları bir yerdə.</p></div><span className="management-icon">⌑</span></div>{ticketsLoading ? <div className="loading-state"><span className="loader" /> Biletlər yüklənir...</div> : tickets.length === 0 ? <div className="empty-state ticket-empty"><div>✳</div><h3>Hələ bilet almamısan</h3><p>Tədbirlər bölməsindən sənə uyğun tədbiri tap.</p><button className="outline-button" onClick={() => setActiveView('events')}>Tədbirlərə bax</button></div> : <div className="ticket-history-grid">{tickets.map((ticket) => <article className="ticket-history-card" key={ticket.id}><div className="ticket-history-top"><span className="ticket-history-mark">e<span>·</span></span><span className={`ticket-status ${ticket.status.toLowerCase()}`}>{ticket.status === 'Valid' ? 'Etibarlıdır' : ticket.status === 'Used' ? 'İstifadə edilib' : ticket.status === 'Cancelled' ? 'Ləğv edilib' : ticket.status}</span></div><span className="eyebrow dark-eyebrow">{ticket.tierName}</span><h3>{ticket.eventTitle}</h3><p>⌖ {ticket.location}</p><p>▣ {dateParts(ticket.eventStartDate).full}</p><div className="ticket-history-bottom"><span>Ödənilib <b>{money(ticket.pricePaid)}</b></span>{ticketQrUrls[ticket.id] ? <a href={ticketQrUrls[ticket.id]} download={`eventaz-${ticket.id}.png`}>QR yüklə ↓</a> : <button onClick={() => void loadTicketQr(ticket.id)}>QR biletini göstər</button>}</div>{ticketQrUrls[ticket.id] && <img className="ticket-qr-image" src={ticketQrUrls[ticket.id]} alt={`${ticket.eventTitle} QR bileti`} />}</article>)}</div>}</section>}

        {activeView === 'manage' && managementRole && <ManagementPanel role={managementRole} categories={categories} onCategoriesChanged={refreshCategories} onEventsChanged={() => void loadEvents()} onNotice={showNotice} onError={showError} />}

        {(activeView === 'events' || activeView === 'favorites') && <section className="newsletter"><div className="newsletter-star">✳</div><div><span className="eyebrow">YENİLİKLƏRDƏN XƏBƏRDAR OL</span><h2>Yaxşı planlar səndən xəbər gözləyir.</h2></div><button onClick={() => { if (!token) openAuth('register'); else setNotice('Tədbirləri izləməyə davam et!') }}>{token ? 'Tədbirlərə bax' : 'Hesab yarat'} <span>↗</span></button></section>}
      </main>

      <footer className="footer"><a className="brand footer-brand" href="#top"><span className="brand-mark">e<span>·</span></span><span>event<span className="brand-accent">az</span></span></a><span>Bakıdan, sevgi ilə. © 2026 EventAz</span><a href="#top">Yuxarı qayıt ↑</a></footer>

      {selectedEvent && <div className="modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) setSelectedEvent(null) }}><section className="event-modal" role="dialog" aria-modal="true" aria-labelledby="event-modal-title"><button className="modal-close" onClick={() => setSelectedEvent(null)} aria-label="Bağla">×</button><div className={`modal-art ${palettes[Math.abs(selectedEvent.title.length) % palettes.length]}`}><span>{selectedEvent.categoryName}</span><b>✳</b><small>{selectedEvent.location}</small></div><div className="modal-content">{error && <div className="error-banner modal-error"><span>!</span><div>{error}</div><button onClick={() => setError('')}>Bağla</button></div>}<div className="event-date-line">{dateParts(selectedEvent.startDate).full} · {timeLabel(selectedEvent.startDate)}–{timeLabel(selectedEvent.endDate)} · {selectedEvent.location}</div><h2 id="event-modal-title">{selectedEvent.title}</h2><p className="modal-description">{selectedEvent.description}</p><div className="event-actions"><button className="outline-button" onClick={() => void shareEvent(selectedEvent)}>↗ Tədbiri paylaş</button><button className="outline-button" onClick={() => downloadCalendarEvent(selectedEvent)}>▣ Təqvimə əlavə et</button></div><div className="modal-subhead"><h3>Bilet növünü seç</h3><span>Ödəniş təhlükəsizdir</span></div><div className="tier-list">{selectedEvent.tiers.map((tier) => <div className="tier-row" key={tier.id}><div><b>{tier.name}</b><small>{tier.available > 0 ? `${tier.available} bilet qalıb` : 'Bilet bitib'}</small></div><strong>{money(tier.price)}</strong><button disabled={tier.available <= 0 || purchasingTierId !== null} onClick={() => void handlePurchase(tier.id)}>{tier.available <= 0 ? 'Bitib' : purchasingTierId === tier.id ? 'Gözlə...' : token ? 'Al' : 'Daxil ol'}</button></div>)}</div><label className="promo-input">Promo kod <input value={promoCode} onChange={(event) => setPromoCode(event.target.value)} placeholder="KODU DAXİL ET" /></label>{qrUrl && <a className="qr-download" href={qrUrl} download="eventaz-bilet.png"><span>✓</span> QR biletini yüklə</a>}<div className="modal-rating">★ {selectedEvent.averageRating ? selectedEvent.averageRating.toFixed(1) : 'Bu tədbirə hələ rəy yoxdur'}</div><section className="reviews-section"><div className="review-heading"><h3>İştirakçı rəyləri</h3><span>{reviews.length} rəy</span></div>{reviewsLoading ? <p className="no-reviews">Rəylər yüklənir...</p> : reviews.length > 0 ? <div className="review-list">{reviews.map((review) => <article className="review-item" key={review.id}><div className="review-stars">{'★'.repeat(review.rating)}{'☆'.repeat(5 - review.rating)}</div><p>{review.comment || 'İştirakçı şərh yazmayıb.'}</p><small>{new Date(review.createdAt).toLocaleDateString('az-AZ')}</small></article>)}</div> : <p className="no-reviews">Bu tədbirə hələ rəy yazılmayıb.</p>}{token && <div className="review-form"><label>Qiymət<select value={reviewRating} onChange={(event) => setReviewRating(Number(event.target.value))}>{[5, 4, 3, 2, 1].map((rating) => <option key={rating} value={rating}>{rating} ulduz</option>)}</select></label><label>Rəyin<textarea rows={2} maxLength={1000} value={reviewComment} onChange={(event) => setReviewComment(event.target.value)} placeholder="Tədbir təcrübəni bölüş..." /></label><button disabled={reviewSubmitting} onClick={() => void submitReview()}>{reviewSubmitting ? 'Göndərilir...' : 'Rəy yaz　→'}</button><small>Rəy yalnız tədbir başa çatdıqdan və bilet aldıqdan sonra yazıla bilər.</small></div>}</section></div></section></div>}

      {authOpen && <div className="modal-backdrop" onMouseDown={(event) => { if (event.target === event.currentTarget) { setAuthOpen(false); setAuthError('') } }}><section className="auth-modal" role="dialog" aria-modal="true" aria-labelledby="auth-title"><button className="modal-close" onClick={() => { setAuthOpen(false); setAuthError('') }} aria-label="Bağla">×</button><div className="auth-mark">e<span>·</span></div><div className="eyebrow dark-eyebrow">EVENTAZ-A XOŞ GƏLMİSƏN</div><h2 id="auth-title">{authMode === 'login' ? 'Yenidən görüşdük.' : 'Sənin yerin hazırdır.'}</h2><p>{authMode === 'login' ? 'Davam etmək üçün hesabına daxil ol.' : 'Tədbirləri kəşf etmək üçün hesab yarat.'}</p>{authError && <div className="error-banner auth-error"><span>!</span><div>{authError}</div></div>}<form onSubmit={(event) => { event.preventDefault(); void handleAuth(new FormData(event.currentTarget)) }}>{authMode === 'register' && <label>Ad və soyad<input name="fullName" required placeholder="Məsələn, Aysel Məmmədova" /></label>}<label>E-poçt<input name="email" type="email" required placeholder="sen@example.com" /></label><label>Şifrə<input name="password" type="password" minLength={6} required placeholder="Ən azı 6 simvol" /></label><button className="auth-submit" type="submit" disabled={authSubmitting}>{authSubmitting ? 'Gözlə...' : authMode === 'login' ? 'Daxil ol' : 'Hesab yarat'} <span>→</span></button></form><button className="auth-switch" disabled={authSubmitting} onClick={() => { setAuthError(''); setAuthMode((mode) => mode === 'login' ? 'register' : 'login') }}>{authMode === 'login' ? 'Hesabın yoxdur? Qeydiyyatdan keç' : 'Artıq hesabın var? Daxil ol'}</button></section></div>}
      {notice && <div className="toast"><span>✓</span>{notice}</div>}
    </div>
  )
}

export default App
