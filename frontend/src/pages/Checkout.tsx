import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useCart } from '../cart'
import { usePageMeta } from '../hooks'
import { money } from '../format'
import type { PaymentMethod } from '../types'

const METHODS: { value: PaymentMethod; title: string; hint: string }[] = [
  { value: 'BankTransfer', title: 'Chuyển khoản bằng mã QR', hint: 'Sau khi đặt hàng bạn nhận mã QR đã điền sẵn số tiền và nội dung. Cửa hàng xác nhận khi nhận được tiền.' },
  { value: 'Cod', title: 'Trả tiền khi nhận hàng', hint: 'Cửa hàng sẽ gọi xác nhận đơn trước khi giao.' },
  { value: 'ContactLater', title: 'Liên hệ riêng để thanh toán', hint: 'Cửa hàng liên hệ với bạn để thống nhất cách thanh toán phù hợp.' },
]

export default function Checkout() {
  usePageMeta('Đặt hàng')
  const cart = useCart()
  const nav = useNavigate()
  const [form, setForm] = useState({ customerName: '', phone: '', email: '', address: '', note: '' })
  const [method, setMethod] = useState<PaymentMethod>('BankTransfer')
  const [busy, setBusy] = useState(false)
  const [error, setError] = useState('')

  if (cart.lines.length === 0) {
    return (
      <div className="container page">
        <h1>Đặt hàng</h1>
        <div className="empty">
          <p>Giỏ hàng đang trống nên chưa thể đặt hàng.</p>
          <Link to="/san-pham" className="btn btn--primary">Xem sản phẩm</Link>
        </div>
      </div>
    )
  }

  const bind = (k: keyof typeof form) => ({
    value: form[k],
    onChange: (e: { target: { value: string } }) => setForm(f => ({ ...f, [k]: e.target.value })),
  })

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setBusy(true)
    try {
      const order = await api.createOrder({
        customerName: form.customerName,
        phone: form.phone.replace(/[\s.-]/g, ''),
        email: form.email || undefined,
        address: form.address,
        note: form.note || undefined,
        paymentMethod: method,
        items: cart.lines.map(l => ({ productId: l.productId, quantity: l.quantity })),
      })
      sessionStorage.setItem(`order_${order.code}`, JSON.stringify(order))
      cart.clear()
      nav(`/don-hang/${order.code}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không đặt được hàng, vui lòng thử lại.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="container page">
      <h1>Đặt hàng</h1>
      <form className="checkout" onSubmit={submit}>
        <div className="checkout__form">
          <h2>Thông tin nhận hàng</h2>
          <label>Họ tên
            <input required maxLength={100} autoComplete="name" {...bind('customerName')} />
          </label>
          <label>Số điện thoại
            <input required type="tel" inputMode="tel" autoComplete="tel" placeholder="0901234567" {...bind('phone')} />
          </label>
          <label>Email (không bắt buộc)
            <input type="email" autoComplete="email" {...bind('email')} />
          </label>
          <label>Địa chỉ nhận hàng
            <input required maxLength={300} autoComplete="street-address" {...bind('address')} />
          </label>
          <label>Ghi chú cho cửa hàng
            <textarea rows={3} maxLength={1000} {...bind('note')} />
          </label>

          <h2>Cách thanh toán</h2>
          <div className="radio-cards">
            {METHODS.map(m => (
              <label key={m.value} className={`radio-card ${method === m.value ? 'is-selected' : ''}`}>
                <input type="radio" name="pay" checked={method === m.value} onChange={() => setMethod(m.value)} />
                <span><strong>{m.title}</strong><small>{m.hint}</small></span>
              </label>
            ))}
          </div>
        </div>

        <aside className="summary">
          <h2>Đơn hàng</h2>
          <ul className="summary__list">
            {cart.lines.map(l => (
              <li key={l.productId}>
                <span>{l.name} × {l.quantity}</span>
                <span>{money(l.price * l.quantity)}</span>
              </li>
            ))}
          </ul>
          <p className="summary__total"><span>Tổng cộng</span><strong>{money(cart.total)}</strong></p>
          {error && <p className="notice notice--error" role="alert">{error}</p>}
          <button className="btn btn--primary btn--block" disabled={busy}>
            {busy ? 'Đang gửi đơn…' : 'Đặt hàng'}
          </button>
          <Link to="/gio-hang" className="text-link">Quay lại giỏ hàng</Link>
        </aside>
      </form>
    </div>
  )
}
