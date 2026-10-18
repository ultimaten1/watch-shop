import { useState, type FormEvent } from 'react'
import { api } from '../api'
import { usePageMeta } from '../hooks'
import { dateTime, money, paymentLabel, statusLabel } from '../format'
import type { Order } from '../types'

export default function OrderLookup({ initialCode = '' }: { initialCode?: string }) {
  usePageMeta('Tra cứu đơn hàng')
  const [code, setCode] = useState(initialCode)
  const [phone, setPhone] = useState('')
  const [order, setOrder] = useState<Order | null>(null)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setBusy(true); setError(''); setOrder(null)
    try {
      setOrder(await api.lookupOrder(code, phone.replace(/[\s.-]/g, '')))
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không tra cứu được.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="container page narrow">
      <h1>Tra cứu đơn hàng</h1>
      <form className="stack" onSubmit={submit}>
        <label>Mã đơn
          <input required value={code} onChange={e => setCode(e.target.value)} placeholder="DH261018123" />
        </label>
        <label>Số điện thoại đã dùng khi đặt
          <input required type="tel" value={phone} onChange={e => setPhone(e.target.value)} />
        </label>
        <button className="btn btn--primary" disabled={busy}>{busy ? 'Đang tìm…' : 'Tra cứu'}</button>
      </form>

      {error && <p className="notice notice--error" role="alert">{error}</p>}

      {order && (
        <section className="order-sum">
          <h2>Đơn {order.code}</h2>
          <p><strong>{statusLabel[order.status]}</strong> · đặt lúc {dateTime(order.createdAt)} · {paymentLabel[order.paymentMethod]}</p>
          <ul className="summary__list">
            {order.items.map(i => (
              <li key={i.productId}><span>{i.productName} × {i.quantity}</span><span>{money(i.unitPrice * i.quantity)}</span></li>
            ))}
          </ul>
          <p className="summary__total"><span>Tổng cộng</span><strong>{money(order.total)}</strong></p>
          {order.payment && (
            <p>Chưa thanh toán? Chuyển khoản tới {order.payment.bankName} {order.payment.accountNumber} ({order.payment.accountName}), nội dung <strong>{order.payment.transferContent}</strong>.</p>
          )}
        </section>
      )}
    </div>
  )
}
