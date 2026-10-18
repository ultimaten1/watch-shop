import { useEffect } from 'react'
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'
import { useCart } from '../cart'
import { useShop } from '../shop'

export default function Layout() {
  const shop = useShop()
  const { count } = useCart()
  const { pathname } = useLocation()

  useEffect(() => { window.scrollTo(0, 0) }, [pathname])

  const zaloHref = shop.zalo ? `https://zalo.me/${shop.zalo.replace(/\D/g, '')}` : undefined

  return (
    <div className="shell">
      <a href="#main" className="skip-link">Bỏ qua điều hướng</a>
      <header className="site-header">
        <div className="container site-header__row">
          <Link to="/" className="brand-mark">
            <svg viewBox="0 0 32 32" width="30" height="30" aria-hidden="true">
              <circle cx="16" cy="16" r="15" fill="#0E2438" />
              <circle cx="16" cy="16" r="12.5" fill="none" stroke="#B8863B" strokeWidth="1.2" />
              <path d="M16 8v8l5 3" stroke="#E8D7B0" strokeWidth="2.2" fill="none" strokeLinecap="round" />
            </svg>
            <span>{shop.name}</span>
          </Link>
          <nav className="main-nav" aria-label="Điều hướng chính">
            <NavLink to="/san-pham">Sản phẩm</NavLink>
            <NavLink to="/gioi-thieu">Về cửa hàng</NavLink>
            <NavLink to="/lien-he">Liên hệ</NavLink>
            <NavLink to="/tra-cuu">Tra cứu đơn</NavLink>
          </nav>
          <Link to="/gio-hang" className="cart-link" aria-label={`Giỏ hàng, ${count} sản phẩm`}>
            Giỏ hàng
            {count > 0 && <span className="cart-count">{count}</span>}
          </Link>
        </div>
      </header>

      <main id="main">
        <Outlet />
      </main>

      <footer className="site-footer">
        <div className="container footer-grid">
          <div>
            <h4>{shop.name}</h4>
            <p>{shop.slogan}</p>
          </div>
          <div>
            <h4>Ghé cửa hàng</h4>
            <p>{shop.address}</p>
            <p>{shop.openingHours}</p>
          </div>
          <div>
            <h4>Liên hệ</h4>
            {shop.phone && <p><a href={`tel:${shop.phone}`}>{shop.phone}</a></p>}
            {shop.email && <p><a href={`mailto:${shop.email}`}>{shop.email}</a></p>}
            {shop.facebookUrl && <p><a href={shop.facebookUrl} target="_blank" rel="noreferrer">Facebook</a></p>}
          </div>
        </div>
        <div className="container footer-note">
          © {new Date().getFullYear()} {shop.name}. Hàng chính hãng, có phiếu bảo hành.
        </div>
      </footer>

      {(zaloHref || shop.phone) && (
        <div className="float-contact">
          {zaloHref && <a href={zaloHref} target="_blank" rel="noreferrer" className="float-contact__zalo">Chat Zalo</a>}
          {shop.phone && <a href={`tel:${shop.phone}`} className="float-contact__call">Gọi {shop.phone}</a>}
        </div>
      )}
    </div>
  )
}
