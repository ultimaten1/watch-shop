import { Link } from 'react-router-dom'
import { useCart } from '../cart'
import { usePageMeta } from '../hooks'
import { money } from '../format'
import WatchImage from '../components/WatchImage'

export default function Cart() {
  usePageMeta('Giỏ hàng')
  const cart = useCart()

  if (cart.lines.length === 0) {
    return (
      <div className="container page">
        <h1>Giỏ hàng</h1>
        <div className="empty">
          <p>Giỏ hàng đang trống.</p>
          <Link to="/san-pham" className="btn btn--primary">Xem sản phẩm</Link>
        </div>
      </div>
    )
  }

  return (
    <div className="container page">
      <h1>Giỏ hàng</h1>
      <div className="cart">
        <ul className="cart__lines">
          {cart.lines.map(l => (
            <li key={l.productId} className="cart-line">
              <Link to={`/san-pham/${l.slug}`} className="cart-line__img">
                <WatchImage src={l.imageUrl} alt={l.name} seed={l.productId} />
              </Link>
              <div className="cart-line__info">
                <span className="product-card__brand">{l.brandName}</span>
                <Link to={`/san-pham/${l.slug}`}><strong>{l.name}</strong></Link>
                <span>{money(l.price)}</span>
              </div>
              <div className="qty">
                <button onClick={() => cart.setQty(l.productId, l.quantity - 1)} aria-label="Giảm số lượng">−</button>
                <input value={l.quantity} readOnly aria-label={`Số lượng ${l.name}`} />
                <button onClick={() => cart.setQty(l.productId, l.quantity + 1)} aria-label="Tăng số lượng">+</button>
              </div>
              <strong className="cart-line__sum">{money(l.price * l.quantity)}</strong>
              <button className="text-btn" onClick={() => cart.remove(l.productId)}>Xoá</button>
            </li>
          ))}
        </ul>
        <aside className="summary">
          <h2>Tạm tính</h2>
          <p className="summary__total"><span>Tổng cộng</span><strong>{money(cart.total)}</strong></p>
          <p className="muted">Phí giao hàng (nếu có) cửa hàng sẽ báo khi xác nhận đơn.</p>
          <Link to="/thanh-toan" className="btn btn--primary btn--block">Tiến hành đặt hàng</Link>
          <Link to="/san-pham" className="text-link">Chọn thêm sản phẩm</Link>
        </aside>
      </div>
    </div>
  )
}
