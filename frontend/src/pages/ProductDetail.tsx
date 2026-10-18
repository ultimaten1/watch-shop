import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import { useCart } from '../cart'
import { useAsync, usePageMeta } from '../hooks'
import { useShop } from '../shop'
import { discountPercent, money, movementLabel } from '../format'
import WatchImage from '../components/WatchImage'
import ProductCard from '../components/ProductCard'
import NotFound from './NotFound'

export default function ProductDetail() {
  const { slug = '' } = useParams()
  const shop = useShop()
  const cart = useCart()
  const nav = useNavigate()
  const { data: p, loading, error } = useAsync(() => api.product(slug), [slug])
  const related = useAsync(
    () => (p ? api.products({ brand: p.brandSlug, pageSize: 5 }) : Promise.resolve(null)),
    [p?.id],
  )
  const [active, setActive] = useState(0)
  const [qty, setQty] = useState(1)
  const [added, setAdded] = useState(false)

  useEffect(() => { setActive(0); setQty(1); setAdded(false) }, [slug])

  usePageMeta(
    p ? `${p.name} – ${p.brandName} | ${shop.name}` : shop.name,
    p ? `${p.name} chính hãng, giá ${money(p.price)}. ${p.description ?? ''}`.slice(0, 160) : undefined,
  )

  // Dữ liệu có cấu trúc để Google hiện giá và tình trạng hàng
  useEffect(() => {
    if (!p) return
    const el = document.createElement('script')
    el.type = 'application/ld+json'
    el.text = JSON.stringify({
      '@context': 'https://schema.org',
      '@type': 'Product',
      name: p.name,
      brand: { '@type': 'Brand', name: p.brandName },
      description: p.description ?? undefined,
      image: [p.imageUrl, ...p.images].filter(Boolean).map(u => new URL(u as string, window.location.origin).href),
      offers: {
        '@type': 'Offer',
        priceCurrency: 'VND',
        price: p.price,
        availability: p.stock > 0 ? 'https://schema.org/InStock' : 'https://schema.org/OutOfStock',
        url: window.location.href,
      },
    })
    document.head.appendChild(el)
    return () => { document.head.removeChild(el) }
  }, [p])

  if (loading) return <div className="container page"><p className="muted">Đang tải…</p></div>
  if (error || !p) return <NotFound message="Sản phẩm này không còn tồn tại hoặc đã ngừng bán." />

  const gallery = [p.imageUrl, ...p.images].filter((u): u is string => !!u)
  const off = discountPercent(p.price, p.oldPrice)
  const zalo = shop.zalo ? `https://zalo.me/${shop.zalo.replace(/\D/g, '')}` : undefined

  const specs: [string, string | null][] = [
    ['Hãng', p.brandName],
    ['Dành cho', p.gender],
    ['Loại máy', movementLabel[p.movementType] ?? p.movementType],
    ['Đường kính mặt', p.caseSizeMm ? `${p.caseSizeMm} mm` : null],
    ['Chất liệu', p.material],
    ['Chống nước', p.waterResistance],
  ]

  const addToCart = () => { cart.add(p, qty); setAdded(true) }

  return (
    <div className="container page">
      <p className="breadcrumb">
        <Link to="/san-pham">Sản phẩm</Link> / <Link to={`/san-pham?brand=${p.brandSlug}`}>{p.brandName}</Link>
      </p>

      <div className="detail">
        <div className="gallery">
          <div className="gallery__main">
            <WatchImage src={gallery[active]} alt={p.name} seed={p.id} eager />
          </div>
          {gallery.length > 1 && (
            <div className="gallery__thumbs">
              {gallery.map((u, i) => (
                <button key={u} className={i === active ? 'is-active' : ''} onClick={() => setActive(i)} aria-label={`Xem ảnh ${i + 1}`}>
                  <img src={u} alt="" />
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="detail__info">
          <span className="product-card__brand">{p.brandName}</span>
          <h1>{p.name}</h1>
          <div className="price price--lg">
            <strong>{money(p.price)}</strong>
            {p.oldPrice && p.oldPrice > p.price && <s>{money(p.oldPrice)}</s>}
            {off > 0 && <span className="badge badge--sale">Giảm {off}%</span>}
          </div>
          <p className={p.stock > 0 ? 'stock stock--in' : 'stock stock--out'}>
            {p.stock > 0 ? (p.stock <= 3 ? `Chỉ còn ${p.stock} chiếc` : 'Còn hàng') : 'Tạm hết hàng'}
          </p>

          {p.stock > 0 ? (
            <div className="buy-row">
              <label className="qty">
                <span className="sr-only">Số lượng</span>
                <button type="button" onClick={() => setQty(q => Math.max(1, q - 1))} aria-label="Giảm">−</button>
                <input value={qty} readOnly aria-live="polite" />
                <button type="button" onClick={() => setQty(q => Math.min(p.stock, q + 1))} aria-label="Tăng">+</button>
              </label>
              <button className="btn btn--primary" onClick={addToCart}>Thêm vào giỏ</button>
              <button className="btn btn--dark" onClick={() => { addToCart(); nav('/gio-hang') }}>Mua ngay</button>
            </div>
          ) : (
            <p className="muted">Nhắn cho cửa hàng để được báo khi có hàng lại.</p>
          )}
          {added && <p className="notice notice--ok" role="status">Đã thêm vào giỏ. <Link to="/gio-hang">Xem giỏ hàng</Link></p>}

          <div className="ask-row">
            {zalo && <a className="btn btn--ghost" href={zalo} target="_blank" rel="noreferrer">Hỏi qua Zalo</a>}
            {shop.phone && <a className="btn btn--ghost" href={`tel:${shop.phone}`}>Gọi {shop.phone}</a>}
          </div>

          {p.description && <p className="desc">{p.description}</p>}

          <table className="specs">
            <tbody>
              {specs.filter(([, v]) => v).map(([k, v]) => (
                <tr key={k}><th scope="row">{k}</th><td>{v}</td></tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {related.data && related.data.items.filter(x => x.id !== p.id).length > 0 && (
        <section className="section">
          <h2>Cùng hãng {p.brandName}</h2>
          <div className="grid grid--4">
            {related.data.items.filter(x => x.id !== p.id).slice(0, 4).map(x => <ProductCard key={x.id} p={x} />)}
          </div>
        </section>
      )}
    </div>
  )
}
