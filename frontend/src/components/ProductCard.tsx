import { Link } from 'react-router-dom'
import type { Product } from '../types'
import { discountPercent, money } from '../format'
import WatchImage from './WatchImage'

export default function ProductCard({ p }: { p: Product }) {
  const off = discountPercent(p.price, p.oldPrice)
  return (
    <Link to={`/san-pham/${p.slug}`} className="product-card">
      <div className="product-card__media">
        <WatchImage src={p.imageUrl} alt={p.name} seed={p.id} />
        {off > 0 && <span className="badge badge--sale">Giảm {off}%</span>}
        {p.stock === 0 && <span className="badge badge--out">Hết hàng</span>}
      </div>
      <div className="product-card__body">
        <span className="product-card__brand">{p.brandName}</span>
        <h3 className="product-card__name">{p.name}</h3>
        <div className="price">
          <strong>{money(p.price)}</strong>
          {p.oldPrice && p.oldPrice > p.price && <s>{money(p.oldPrice)}</s>}
        </div>
      </div>
    </Link>
  )
}
