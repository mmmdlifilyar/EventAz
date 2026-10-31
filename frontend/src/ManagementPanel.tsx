import { useCallback, useEffect, useState, type FormEvent } from 'react'
import {
  createCategory, createEvent, createPromoCode, deactivatePromoCode, getAdminEvents,
  getAdminStatistics, getPromoCodesByEvent, publishEvent, validateTicket,
} from './api'
import type { AdminStatistics, Category, EventItem, PromoCode } from './types'

type Props = {
  role: 'Admin' | 'Organizer'
  categories: Category[]
  onCategoriesChanged: () => void
  onEventsChanged: () => void
  onNotice: (message: string) => void
  onError: (message: string) => void
}

type TierDraft = { id: string; name: string; price: string; totalQuantity: string }
const blankTier = (): TierDraft => ({ id: crypto.randomUUID(), name: 'Standard', price: '', totalQuantity: '' })

export default function ManagementPanel({ role, categories, onCategoriesChanged, onEventsChanged, onNotice, onError }: Props) {
  const isAdmin = role === 'Admin'
  const [tab, setTab] = useState<'create' | 'events' | 'stats' | 'promos' | 'category' | 'validate'>('create')
  const [stats, setStats] = useState<AdminStatistics | null>(null)
  const [events, setEvents] = useState<EventItem[]>([])
  const [selectedEventId, setSelectedEventId] = useState('')
  const [promos, setPromos] = useState<PromoCode[]>([])
  const [tiers, setTiers] = useState<TierDraft[]>([blankTier()])
  const [loading, setLoading] = useState(false)
  const [eventsLoading, setEventsLoading] = useState(false)
  const [createdEventId, setCreatedEventId] = useState<string | null>(null)

  const refreshAdminEvents = useCallback(async () => {
    if (!isAdmin) return
    setEventsLoading(true)
    try { setEvents(await getAdminEvents()) }
    catch (error) { onError((error as Error).message) }
    finally { setEventsLoading(false) }
  }, [isAdmin, onError])

  useEffect(() => {
    if (!isAdmin) return
    void getAdminStatistics().then(setStats).catch((error) => onError((error as Error).message))
    void refreshAdminEvents()
  }, [isAdmin, onError, refreshAdminEvents])

  useEffect(() => {
    if (!selectedEventId || !isAdmin) { setPromos([]); return }
    void getPromoCodesByEvent(selectedEventId).then(setPromos).catch((error) => onError((error as Error).message))
  }, [isAdmin, onError, selectedEventId])

  async function submitEvent(form: FormEvent<HTMLFormElement>) {
    form.preventDefault()
    const formElement = form.currentTarget
    const data = new FormData(formElement)
    const startDate = new Date(String(data.get('startDate')))
    const endDate = new Date(String(data.get('endDate')))
    if (endDate <= startDate) { onError('Bitmə vaxtı başlama vaxtından sonra olmalıdır.'); return }
    const payload = {
      title: String(data.get('title')),
      description: String(data.get('description')),
      location: String(data.get('location')),
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      categoryId: String(data.get('categoryId')),
      tiers: tiers.map((tier) => ({ name: tier.name.trim(), price: Number(tier.price), totalQuantity: Number(tier.totalQuantity) })),
    }
    setLoading(true)
    try {
      const created = await createEvent(payload)
      formElement.reset()
      setTiers([blankTier()])
      setCreatedEventId(created.id)
      if (isAdmin) {
        try {
          await publishEvent(created.id)
          setCreatedEventId(null)
          onNotice(`Tədbir yaradıldı və yayımlandı. ID: ${created.id}`)
        } catch (error) {
          onError(`Tədbir yaradıldı, amma yayımlanmadı. Bu ID ilə yenidən cəhd et: ${created.id}. ${(error as Error).message}`)
        }
        await refreshAdminEvents()
      } else {
        onNotice(`Tədbir yaradıldı. Admin təsdiqindən sonra yayımlanacaq. ID: ${created.id}`)
      }
      onEventsChanged()
    } catch (error) { onError((error as Error).message) }
    finally { setLoading(false) }
  }

  async function submitPromo(form: FormEvent<HTMLFormElement>) {
    form.preventDefault()
    const formElement = form.currentTarget
    const data = new FormData(formElement)
    setLoading(true)
    try {
      await createPromoCode({
        eventId: selectedEventId,
        code: String(data.get('code')).trim().toUpperCase(),
        discountPercent: Number(data.get('discountPercent')),
        expiryDate: new Date(String(data.get('expiryDate'))).toISOString(),
        usageLimit: Number(data.get('usageLimit')),
      })
      formElement.reset()
      setPromos(await getPromoCodesByEvent(selectedEventId))
      onNotice('Promo kod yaradıldı.')
    } catch (error) { onError((error as Error).message) }
    finally { setLoading(false) }
  }

  async function submitCategory(form: FormEvent<HTMLFormElement>) {
    form.preventDefault()
    const formElement = form.currentTarget
    const data = new FormData(formElement)
    setLoading(true)
    try {
      await createCategory(String(data.get('name')).trim())
      formElement.reset()
      onCategoriesChanged()
      onNotice('Kateqoriya yaradıldı.')
    } catch (error) { onError((error as Error).message) }
    finally { setLoading(false) }
  }

  async function submitValidation(form: FormEvent<HTMLFormElement>) {
    form.preventDefault()
    const formElement = form.currentTarget
    const data = new FormData(formElement)
    setLoading(true)
    try {
      const result = await validateTicket(String(data.get('qrCode')).trim())
      onNotice(`Bilet təsdiqləndi: ${result.eventTitle ?? 'Tədbir'}`)
      formElement.reset()
    } catch (error) { onError((error as Error).message) }
    finally { setLoading(false) }
  }

  async function deactivate(id: string) {
    try {
      await deactivatePromoCode(id)
      setPromos((current) => current.map((promo) => promo.id === id ? { ...promo, isActive: false } : promo))
      onNotice('Promo kod deaktiv edildi.')
    } catch (error) { onError((error as Error).message) }
  }

  async function publishDraft(eventId: string) {
    setLoading(true)
    try {
      await publishEvent(eventId)
      setCreatedEventId((current) => current === eventId ? null : current)
      onNotice('Tədbir uğurla yayımlandı.')
      await refreshAdminEvents()
      onEventsChanged()
    } catch (error) { onError((error as Error).message) }
    finally { setLoading(false) }
  }

  const tabs: { id: typeof tab; label: string; icon: string }[] = [
    { id: 'create', label: 'Tədbir yarat', icon: '＋' },
    ...(isAdmin ? [{ id: 'events' as const, label: 'Tədbirlər', icon: '▤' }, { id: 'stats' as const, label: 'Statistika', icon: '▥' }, { id: 'promos' as const, label: 'Promo kodlar', icon: '%' }, { id: 'category' as const, label: 'Kateqoriyalar', icon: '▦' }] : []),
    { id: 'validate', label: 'Bilet yoxla', icon: '⌑' },
  ]

  return <section className="management-layout">
    <aside className="management-sidebar"><div className="management-kicker">EVENTAZ İDARƏETMƏ</div><h2>{isAdmin ? 'İdarəetmə paneli' : 'Təşkilatçı paneli'}</h2><p>{isAdmin ? 'Platformanı və tədbirləri buradan idarə et.' : 'Tədbir yarat və giriş biletlərini yoxla.'}</p><nav>{tabs.map((item) => <button key={item.id} className={tab === item.id ? 'management-tab selected' : 'management-tab'} onClick={() => setTab(item.id)}><span>{item.icon}</span>{item.label}<b>→</b></button>)}</nav><div className="sidebar-tip"><span>✳</span><p>Əməliyyatlar rol icazələrinə uyğun olaraq API tərəfindən yoxlanılır.</p></div></aside>
    <div className="management-content">
      {tab === 'create' && <section className="management-card"><div className="management-card-head"><div><span className="eyebrow dark-eyebrow">YENİ TƏDBİR</span><h3>Tədbir məlumatlarını daxil et</h3><p>{isAdmin ? 'Admin olaraq tədbiri yaradıb dərhal yayımlaya bilərsən.' : 'Tədbir yaradıldıqdan sonra Admin tərəfindən yayımlanmalıdır.'}</p></div><span className="management-icon">✳</span></div><form className="management-form" onSubmit={(event) => void submitEvent(event)}>
        <div className="form-grid"><label>Tədbirin adı<input name="title" required maxLength={150} placeholder="Məsələn, Bakı Caz Gecəsi" /></label><label>Məkan<input name="location" required maxLength={200} placeholder="Bakı, Azərbaycan" /></label><label className="wide">Təsvir<textarea name="description" required rows={3} placeholder="Tədbir haqqında qısa məlumat" /></label><label>Kateqoriya<select name="categoryId" required defaultValue=""><option value="" disabled>Kateqoriya seç</option>{categories.map((category) => <option key={category.id} value={category.id}>{category.name}</option>)}</select></label><label>Başlama vaxtı<input name="startDate" type="datetime-local" required /></label><label>Bitmə vaxtı<input name="endDate" type="datetime-local" required /></label></div>
        <div className="tier-editor-head"><div><b>Bilet növləri</b><small>Ən azı bir bilet növü əlavə et.</small></div><button type="button" className="mini-outline" onClick={() => setTiers((current) => [...current, blankTier()])}>＋ Növ əlavə et</button></div>
        {tiers.map((tier, index) => <div className="tier-editor-row" key={index}><label>Növ<input value={tier.name} onChange={(event) => setTiers((current) => current.map((item, i) => i === index ? { ...item, name: event.target.value } : item))} required placeholder="VIP / Standard" /></label><label>Qiymət (AZN)<input type="number" min="0" step="0.01" value={tier.price} onChange={(event) => setTiers((current) => current.map((item, i) => i === index ? { ...item, price: event.target.value } : item))} required /></label><label>Miqdar<input type="number" min="1" step="1" value={tier.totalQuantity} onChange={(event) => setTiers((current) => current.map((item, i) => i === index ? { ...item, totalQuantity: event.target.value } : item))} required /></label>{tiers.length > 1 && <button type="button" className="remove-tier" onClick={() => setTiers((current) => current.filter((_, i) => i !== index))} aria-label="Bilet növünü sil">×</button>}</div>)}
        <button className="management-submit" disabled={loading}>{loading ? 'Gözlə...' : isAdmin ? 'Yarat və yayımla　→' : 'Tədbir yarat　→'}</button>
      </form>{createdEventId && <div className="created-event-note"><div><b>Tədbir yaradıldı</b><small>ID: {createdEventId} · {isAdmin ? 'Yayımlama tamamlanmayıb.' : 'Admin təsdiqindən sonra yayımlanacaq.'}</small></div>{isAdmin && <button className="mini-outline" disabled={loading} onClick={() => void publishDraft(createdEventId)}>Yenidən yayımla</button>}</div>}</section>}

      {tab === 'events' && isAdmin && <section className="management-card"><div className="management-card-head"><div><span className="eyebrow dark-eyebrow">TƏDBİRLƏRƏ NƏZARƏT</span><h3>Tədbirləri idarə et</h3><p>Organizer tərəfindən yaradılmış qaralama tədbirləri burada təsdiqlə.</p></div><button className="mini-outline" disabled={eventsLoading} onClick={() => void refreshAdminEvents()}>Yenilə ↻</button></div>{eventsLoading ? <div className="panel-loading">Tədbirlər yüklənir...</div> : <div className="admin-event-list">{events.map((event) => <article className="admin-event-row" key={event.id}><div className="admin-event-main"><span className={`admin-event-status ${event.status.toLowerCase()}`}>{event.status === 'Draft' ? 'Qaralama' : event.status === 'Published' ? 'Yayımlanıb' : event.status}</span><h4>{event.title}</h4><p>{event.categoryName} · {event.location} · {new Date(event.startDate).toLocaleDateString('az-AZ')}</p><small>ID: {event.id}</small></div>{event.status === 'Draft' && <button className="management-submit" disabled={loading || new Date(event.startDate) <= new Date()} onClick={() => void publishDraft(event.id)}>{new Date(event.startDate) <= new Date() ? 'Tarix keçib' : 'Yayımla →'}</button>}</article>)}{events.length === 0 && <p className="table-empty">Hələ tədbir yaradılmayıb.</p>}</div>}</section>}

      {tab === 'stats' && isAdmin && <section className="management-card"><div className="management-card-head"><div><span className="eyebrow dark-eyebrow">PLATFORMANIN İCMALI</span><h3>Ümumi statistika</h3><p>Satış və tədbir göstəricilərinə nəzər sal.</p></div><button className="mini-outline" onClick={() => void getAdminStatistics().then(setStats).catch((error) => onError((error as Error).message))}>Yenilə ↻</button></div>{stats ? <><div className="stats-grid">{[['İstifadəçilər', stats.totalUsers], ['Tədbirlər', stats.totalEvents], ['Yayımlanan', stats.publishedEvents], ['Satılan biletlər', stats.ticketsSold]].map(([label, value]) => <div className="stat-tile" key={label}><span>{label}</span><b>{Number(value).toLocaleString('az-AZ')}</b></div>)}<div className="stat-tile revenue-tile"><span>Ümumi gəlir</span><b>{Number(stats.totalRevenue).toLocaleString('az-AZ')} ₼</b></div></div><div className="table-heading"><b>Ən çox satılan tədbirlər</b><span>Satış gəlirinə görə</span></div><div className="management-table">{stats.topSellingEvents.map((event, index) => <div className="management-table-row" key={event.eventId}><span className="rank-number">0{index + 1}</span><b>{event.title}</b><span>{event.ticketsSold} bilet</span><strong>{event.revenue.toLocaleString('az-AZ')} ₼</strong></div>)}{stats.topSellingEvents.length === 0 && <p className="table-empty">Satış məlumatı hələ yoxdur.</p>}</div></> : <div className="panel-loading">Statistika yüklənir...</div>}</section>}

      {tab === 'promos' && isAdmin && <section className="management-card"><div className="management-card-head"><div><span className="eyebrow dark-eyebrow">ENDİRİM KODLARI</span><h3>Promo kodları idarə et</h3><p>Tədbir seç, yeni endirim yarat və mövcud kodları deaktiv et.</p></div><span className="management-icon">%</span></div><label className="standalone-field">Tədbir<select value={selectedEventId} onChange={(event) => setSelectedEventId(event.target.value)}><option value="">Tədbir seç</option>{events.map((event) => <option key={event.id} value={event.id}>{event.title}</option>)}</select></label>{selectedEventId && <><form className="promo-form" onSubmit={(event) => void submitPromo(event)}><label>Kod<input name="code" required maxLength={50} placeholder="YAY2026" /></label><label>Endirim faizi<input name="discountPercent" type="number" required min="1" max="100" step="0.01" placeholder="15" /></label><label>Bitmə tarixi<input name="expiryDate" type="datetime-local" required /></label><label>İstifadə limiti<input name="usageLimit" type="number" required min="1" step="1" placeholder="100" /></label><button className="management-submit" disabled={loading}>Promo kod yarat　→</button></form><div className="table-heading"><b>Mövcud kodlar</b><span>{promos.length} kod</span></div><div className="management-table">{promos.map((promo) => <div className="management-table-row promo-table-row" key={promo.id}><b className="promo-code">{promo.code}</b><span>{promo.discountPercent}% endirim</span><span>{promo.usedCount}/{promo.usageLimit} istifadə</span><span className={promo.isActive ? 'status-active' : 'status-inactive'}>{promo.isActive ? 'Aktiv' : 'Deaktiv'}</span>{promo.isActive && <button className="deactivate-button" onClick={() => void deactivate(promo.id)}>Deaktiv et</button>}</div>)}{promos.length === 0 && <p className="table-empty">Bu tədbir üçün promo kod yoxdur.</p>}</div></>}</section>}

      {tab === 'category' && isAdmin && <section className="management-card"><div className="management-card-head"><div><span className="eyebrow dark-eyebrow">TƏDBİR NÖVLƏRİ</span><h3>Kateqoriyalar</h3><p>Yeni tədbir kateqoriyası yarat və siyahını yenilə.</p></div><span className="management-icon">▦</span></div><form className="category-form" onSubmit={(event) => void submitCategory(event)}><label>Yeni kateqoriyanın adı<input name="name" required maxLength={100} placeholder="Məsələn, Teatr" /></label><button className="management-submit" disabled={loading}>Kateqoriya yarat　→</button></form><div className="category-chips">{categories.map((category) => <span key={category.id}>{category.name}</span>)}</div></section>}

      {tab === 'validate' && <section className="management-card validate-card"><div className="management-card-head"><div><span className="eyebrow dark-eyebrow">GİRİŞƏ NƏZARƏT</span><h3>Bileti yoxla</h3><p>QR kodun içindəki mətni daxil et. Bilet tədbir vaxtında təsdiqlənəcək.</p></div><span className="management-icon">⌑</span></div><form className="validate-form" onSubmit={(event) => void submitValidation(event)}><label>QR kod məlumatı<textarea name="qrCode" rows={4} required placeholder="QR koddan oxunan mətni bura yapışdır" /></label><button className="management-submit" disabled={loading}>{loading ? 'Yoxlanıyor...' : 'Bileti yoxla　→'}</button></form><p className="validation-note">Bu əməliyyat bileti istifadə edilmiş kimi qeyd edir. Eyni bileti ikinci dəfə qəbul etmək mümkün deyil.</p></section>}
    </div>
  </section>
}
