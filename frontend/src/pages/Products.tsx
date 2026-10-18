import { useSearchParams } from 'react-router-dom'
import { api } from '../api'
import { useAsync, usePageMeta } from '../hooks'
import { movementLabel } from '../format'
import ProductCard from '../components/ProductCard'

const PRICE_RANGES: { label: string; min?: number; max?: number }[] = [
  { label: 'Tất cả mức giá' },
  { label: 'Dưới 3 triệu', max: 3_000_000 },
  { label: '3 – 7 triệu', min: 3_000_000, max: 7_000_000 },
  { label: '7 – 15 triệu', min: 7_000_000, max: 15_000_000 },
  { label: 'Trên 15 triệu', min: 15_000_000 },
]

export default function Products() {
  usePageMeta('Đồng hồ nam, nữ chính hãng', 'Danh sách đồng hồ nam, nữ, máy cơ, năng lượng ánh sáng. Lọc theo hãng, giá, loại máy.')
  const [sp, setSp] = useSearchParams()

  const get = (k: string) => sp.get(k) ?? ''
  const page = Number(get('page')) || 1
  const priceIdx = Number(get('gia')) || 0
  const range = PRICE_RANGES[priceIdx] ?? PRICE_RANGES[0]

  const brands = useAsync(() => api.brands(), [])
  const list = useAsync(
    () => api.products({
      search: get('q'), brand: get('brand'), gender: get('gender'), movement: get('movement'),
      minPrice: range.min, maxPrice: range.max, sort: get('sort'), page, pageSize: 12,
    }),
    [sp.toString()],
  )

  const set = (k: string, v: string) => {
    const next = new URLSearchParams(sp)
    if (v) next.set(k, v); else next.delete(k)
    if (k !== 'page') next.delete('page')
    setSp(next)
  }

  const total = list.data?.total ?? 0
  const pages = Math.max(1, Math.ceil(total / 12))

  return (
    <div className="container page">
      <h1>Sản phẩm</h1>

      <form className="filters" onSubmit={e => e.preventDefault()} role="search">
        <input
          type="search" placeholder="Tìm theo tên hoặc hãng" aria-label="Tìm sản phẩm"
          defaultValue={get('q')} key={get('q')}
          onKeyDown={e => { if (e.key === 'Enter') set('q', (e.target as HTMLInputElement).value.trim()) }}
          onBlur={e => { if (e.target.value.trim() !== get('q')) set('q', e.target.value.trim()) }}
        />
        <select aria-label="Hãng" value={get('brand')} onChange={e => set('brand', e.target.value)}>
          <option value="">Tất cả hãng</option>
          {brands.data?.map(b => <option key={b.id} value={b.slug}>{b.name}</option>)}
        </select>
        <select aria-label="Dành cho" value={get('gender')} onChange={e => set('gender', e.target.value)}>
          <option value="">Nam và nữ</option>
          <option value="Nam">Đồng hồ nam</option>
          <option value="Nữ">Đồng hồ nữ</option>
          <option value="Unisex">Unisex</option>
        </select>
        <select aria-label="Loại máy" value={get('movement')} onChange={e => set('movement', e.target.value)}>
          <option value="">Mọi loại máy</option>
          {Object.entries(movementLabel).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
        <select aria-label="Mức giá" value={priceIdx} onChange={e => set('gia', e.target.value === '0' ? '' : e.target.value)}>
          {PRICE_RANGES.map((r, i) => <option key={i} value={i}>{r.label}</option>)}
        </select>
        <select aria-label="Sắp xếp" value={get('sort')} onChange={e => set('sort', e.target.value)}>
          <option value="">Mới nhất</option>
          <option value="price_asc">Giá thấp đến cao</option>
          <option value="price_desc">Giá cao đến thấp</option>
          <option value="name">Tên A–Z</option>
        </select>
      </form>

      {list.loading && <p className="muted">Đang tải…</p>}
      {list.error && <p className="notice notice--error">{list.error}</p>}
      {list.data && (
        <>
          <p className="muted result-count">{total} sản phẩm</p>
          {list.data.items.length === 0 ? (
            <div className="empty">
              <p>Không có sản phẩm nào khớp với bộ lọc.</p>
              <button className="btn btn--ghost" onClick={() => setSp(new URLSearchParams())}>Xoá bộ lọc</button>
            </div>
          ) : (
            <div className="grid grid--4">
              {list.data.items.map(p => <ProductCard key={p.id} p={p} />)}
            </div>
          )}
          {pages > 1 && (
            <nav className="pager" aria-label="Phân trang">
              <button className="btn btn--ghost" disabled={page <= 1} onClick={() => set('page', String(page - 1))}>Trang trước</button>
              <span>Trang {page} / {pages}</span>
              <button className="btn btn--ghost" disabled={page >= pages} onClick={() => set('page', String(page + 1))}>Trang sau</button>
            </nav>
          )}
        </>
      )}
    </div>
  )
}
