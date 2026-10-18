import { Link } from 'react-router-dom'
import { api } from '../api'
import { useAsync, usePageMeta } from '../hooks'
import { useShop } from '../shop'
import LiveClock from '../components/LiveClock'
import ProductCard from '../components/ProductCard'

export default function Home() {
  const shop = useShop()
  usePageMeta(
    `${shop.name} – Đồng hồ chính hãng, bảo hành rõ ràng`,
    'Cửa hàng đồng hồ gia đình: đồng hồ nam, nữ chính hãng, máy cơ, năng lượng ánh sáng, bảo hành rõ ràng, tư vấn tận tâm.',
  )

  const featured = useAsync(() => api.products({ featured: true, pageSize: 4 }), [])
  const brands = useAsync(() => api.brands(), [])
  const zalo = shop.zalo ? `https://zalo.me/${shop.zalo.replace(/\D/g, '')}` : undefined

  return (
    <>
      <section className="hero">
        <div className="container hero__grid">
          <div className="hero__text">
            <h1>Đồng hồ chính hãng, từ một cửa hàng gia đình</h1>
            <p className="lead">
              Chúng tôi chọn từng mẫu, kiểm tra kỹ trước khi bán và đứng ra bảo hành sau đó.
              Bạn có thể ghé xem trực tiếp, hoặc nhắn để được tư vấn chọn mẫu hợp cổ tay.
            </p>
            <div className="hero__actions">
              <Link to="/san-pham" className="btn btn--primary">Xem sản phẩm</Link>
              {zalo && <a href={zalo} target="_blank" rel="noreferrer" className="btn btn--ghost-light">Nhắn Zalo cho cửa hàng</a>}
            </div>
          </div>
          <div className="hero__clock">
            <LiveClock />
          </div>
        </div>
      </section>

      <section className="section container">
        <div className="section__head">
          <h2>Mẫu được chọn nhiều</h2>
          <Link to="/san-pham" className="text-link">Xem tất cả sản phẩm</Link>
        </div>
        {featured.loading && <p className="muted">Đang tải sản phẩm…</p>}
        {featured.error && <p className="notice notice--error">{featured.error}</p>}
        {featured.data && featured.data.items.length === 0 && (
          <p className="muted">Chưa có sản phẩm nổi bật. Thêm sản phẩm trong trang quản trị và bật mục “Nổi bật”.</p>
        )}
        <div className="grid grid--4">
          {featured.data?.items.map(p => <ProductCard key={p.id} p={p} />)}
        </div>
      </section>

      <section className="section section--tint">
        <div className="container promise">
          <div>
            <h3>Hàng chính hãng, có phiếu bảo hành</h3>
            <p>Mỗi chiếc đều có phiếu bảo hành của cửa hàng. Đồng hồ gặp vấn đề, bạn mang đến đúng một nơi để được xử lý.</p>
          </div>
          <div>
            <h3>Xem và thử trước khi quyết định</h3>
            <p>Ghé cửa hàng để thử trên tay, hoặc nhắn Zalo để được gửi thêm ảnh, video thực tế của chiếc bạn quan tâm.</p>
          </div>
          <div>
            <h3>Thanh toán theo cách bạn quen</h3>
            <p>Chuyển khoản bằng mã QR, trả tiền khi nhận hàng, hoặc liên hệ riêng để thống nhất cách thanh toán phù hợp.</p>
          </div>
        </div>
      </section>

      {brands.data && brands.data.length > 0 && (
        <section className="section container">
          <h2>Mua theo hãng</h2>
          <div className="chips">
            {brands.data.map(b => (
              <Link key={b.id} to={`/san-pham?brand=${b.slug}`} className="chip">
                {b.name} <span className="chip__n">{b.productCount}</span>
              </Link>
            ))}
          </div>
        </section>
      )}

      <section className="section container">
        <div className="visit">
          <div>
            <h2>Ghé cửa hàng</h2>
            <p>{shop.address}</p>
            <p className="muted">{shop.openingHours}</p>
          </div>
          <Link to="/lien-he" className="btn btn--primary">Xem đường đi và liên hệ</Link>
        </div>
      </section>
    </>
  )
}
