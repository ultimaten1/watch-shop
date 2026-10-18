import { useState } from 'react'
import { api } from '../api'
import { useAsync } from '../hooks'
import { dateTime, money, paymentLabel, statusLabel } from '../format'
import type { OrderStatus } from '../types'

const STATUSES = Object.keys(statusLabel) as OrderStatus[]

export default function AdminOrders() {
  const [filter, setFilter] = useState<OrderStatus | ''>('')
  const { data, error, loading, reload } = useAsync(() => api.orders(filter || undefined), [filter])
  const [msg, setMsg] = useState('')

  const change = async (id: number, status: OrderStatus) => {
    setMsg('')
    if (status === 'Cancelled' && !confirm('Huỷ đơn này? Hàng sẽ được trả về kho.')) return
    try {
      await api.setOrderStatus(id, status)
      reload()
    } catch (e) {
      setMsg(e instanceof Error ? e.message : 'Không cập nhật được')
    }
  }

  return (
    <>
      <div className="admin__head">
        <h1>Đơn hàng</h1>
        <select aria-label="Lọc theo trạng thái" value={filter} onChange={e => setFilter(e.target.value as OrderStatus | '')}>
          <option value="">Tất cả trạng thái</option>
          {STATUSES.map(s => <option key={s} value={s}>{statusLabel[s]}</option>)}
        </select>
      </div>
      {msg && <p className="notice notice--error" role="alert">{msg}</p>}
      {error && <p className="notice notice--error">{error}</p>}
      {loading && <p className="muted">Đang tải…</p>}
      {data && data.length === 0 && <p className="muted">Chưa có đơn hàng nào.</p>}

      <div className="order-list">
        {data?.map(o => (
          <article key={o.id} className="order-card">
            <header>
              <strong>{o.code}</strong>
              <span className="muted">{dateTime(o.createdAt)}</span>
              <select value={o.status} onChange={e => change(o.id, e.target.value as OrderStatus)} aria-label={`Trạng thái đơn ${o.code}`}>
                {STATUSES.map(s => <option key={s} value={s}>{statusLabel[s]}</option>)}
              </select>
            </header>
            <p>
              {o.customerName} · <a href={`tel:${o.phone}`}>{o.phone}</a>
              {o.email && <> · {o.email}</>}
            </p>
            <p>{o.address}</p>
            {o.note && <p className="muted">Ghi chú: {o.note}</p>}
            <ul className="summary__list">
              {o.items.map(i => (
                <li key={i.productId}><span>{i.productName} × {i.quantity}</span><span>{money(i.unitPrice * i.quantity)}</span></li>
              ))}
            </ul>
            <p className="summary__total"><span>{paymentLabel[o.paymentMethod]}</span><strong>{money(o.total)}</strong></p>
          </article>
        ))}
      </div>
    </>
  )
}
