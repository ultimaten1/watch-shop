import { useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { usePageMeta } from '../hooks'
import { useShop } from '../shop'
import { money, paymentLabel, statusLabel } from '../format'
import OrderLookup from './OrderLookup'
import type { Order } from '../types'

function Copy({ text, label }: { text: string; label: string }) {
  const [done, setDone] = useState(false)
  return (
    <button
      type="button" className="text-btn"
      onClick={() => { navigator.clipboard?.writeText(text).then(() => { setDone(true); setTimeout(() => setDone(false), 1500) }) }}
    >
      {done ? 'Đã chép' : label}
    </button>
  )
}

export default function OrderResult() {
  const { code = '' } = useParams()
  const shop = useShop()
  usePageMeta(`Đơn hàng ${code}`)

  let order: Order | null = null
  try {
    const raw = sessionStorage.getItem(`order_${code}`)
    if (raw) order = JSON.parse(raw) as Order
  } catch { /* bỏ qua */ }

  // Mở lại link sau khi đóng tab: chuyển sang trang tra cứu bằng số điện thoại
  if (!order) return <OrderLookup initialCode={code} />

  const pay = order.payment
  return (
    <div className="container page narrow">
      <h1>Đã nhận đơn hàng</h1>
      <p className="lead">
        Mã đơn của bạn là <strong>{order.code}</strong>. Hãy lưu lại mã này cùng số điện thoại để tra cứu đơn sau.
      </p>

      {pay && (
        <section className="pay-box">
          <h2>Chuyển khoản để hoàn tất</h2>
          <div className="pay-box__grid">
            <img src={pay.qrUrl} alt={`Mã QR chuyển ${money(order.total)} tới ${pay.accountName}`} width={260} height={260} />
            <dl>
              <dt>Ngân hàng</dt><dd>{pay.bankName}</dd>
              <dt>Số tài khoản</dt><dd>{pay.accountNumber} <Copy text={pay.accountNumber} label="Chép" /></dd>
              <dt>Chủ tài khoản</dt><dd>{pay.accountName}</dd>
              <dt>Số tiền</dt><dd>{money(order.total)}</dd>
              <dt>Nội dung chuyển khoản</dt><dd><strong>{pay.transferContent}</strong> <Copy text={pay.transferContent} label="Chép" /></dd>
            </dl>
          </div>
          <p className="muted">Vui lòng giữ đúng nội dung chuyển khoản để cửa hàng đối chiếu nhanh. Sau khi nhận được tiền, cửa hàng sẽ liên hệ xác nhận.</p>
        </section>
      )}

      {order.paymentMethod === 'Cod' && <p className="notice notice--ok">Cửa hàng sẽ gọi cho bạn để xác nhận đơn, bạn thanh toán khi nhận hàng.</p>}
      {order.paymentMethod === 'ContactLater' && <p className="notice notice--ok">Cửa hàng sẽ liên hệ với bạn qua số {order.phone} để thống nhất cách thanh toán.</p>}

      <section className="order-sum">
        <h2>Chi tiết đơn</h2>
        <p>{statusLabel[order.status]} · {paymentLabel[order.paymentMethod]}</p>
        <ul className="summary__list">
          {order.items.map(i => (
            <li key={i.productId}><span>{i.productName} × {i.quantity}</span><span>{money(i.unitPrice * i.quantity)}</span></li>
          ))}
        </ul>
        <p className="summary__total"><span>Tổng cộng</span><strong>{money(order.total)}</strong></p>
        <p className="muted">Giao tới: {order.customerName}, {order.phone}, {order.address}</p>
      </section>

      <p>Cần hỗ trợ ngay? Gọi <a href={`tel:${shop.phone}`}>{shop.phone}</a>.</p>
      <Link to="/san-pham" className="btn btn--ghost">Tiếp tục xem sản phẩm</Link>
    </div>
  )
}
