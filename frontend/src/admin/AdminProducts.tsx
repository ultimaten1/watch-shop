import { useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { useAsync } from '../hooks'
import { money } from '../format'
import WatchImage from '../components/WatchImage'

export default function AdminProducts() {
  const [search, setSearch] = useState('')
  const { data, error, loading, reload } = useAsync(
    () => api.products({ includeInactive: true, pageSize: 60, search }),
    [search],
  )
  const [msg, setMsg] = useState('')

  const remove = async (id: number, name: string) => {
    if (!confirm(`Xoá vĩnh viễn “${name}”? Muốn tạm ẩn thì hãy sửa và tắt mục “Đang bán”.`)) return
    try { await api.deleteProduct(id); reload() } catch (e) { setMsg(e instanceof Error ? e.message : 'Không xoá được') }
  }

  return (
    <>
      <div className="admin__head">
        <h1>Sản phẩm</h1>
        <Link to="/admin/san-pham/moi" className="btn btn--primary">Thêm sản phẩm</Link>
      </div>
      <input type="search" placeholder="Tìm theo tên hoặc hãng" aria-label="Tìm sản phẩm" value={search} onChange={e => setSearch(e.target.value)} />
      {msg && <p className="notice notice--error" role="alert">{msg}</p>}
      {error && <p className="notice notice--error">{error}</p>}
      {loading && <p className="muted">Đang tải…</p>}

      <table className="table">
        <thead>
          <tr><th>Sản phẩm</th><th>Giá</th><th>Tồn kho</th><th>Trạng thái</th><th /></tr>
        </thead>
        <tbody>
          {data?.items.map(p => (
            <tr key={p.id}>
              <td>
                <div className="table__product">
                  <div className="thumb"><WatchImage src={p.imageUrl} alt="" seed={p.id} /></div>
                  <div><strong>{p.name}</strong><br /><span className="muted">{p.brandName}</span></div>
                </div>
              </td>
              <td>{money(p.price)}</td>
              <td>{p.stock}</td>
              <td>{p.isActive ? 'Đang bán' : 'Đang ẩn'}{p.isFeatured && ' · Nổi bật'}</td>
              <td className="table__actions">
                <Link to={`/admin/san-pham/${p.id}`}>Sửa</Link>
                <button className="text-btn text-btn--danger" onClick={() => remove(p.id, p.name)}>Xoá</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {data && data.items.length === 0 && <p className="muted">Chưa có sản phẩm nào.</p>}
    </>
  )
}
