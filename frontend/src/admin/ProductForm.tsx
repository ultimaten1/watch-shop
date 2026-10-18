import { useEffect, useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import { useAsync } from '../hooks'
import { movementLabel } from '../format'
import type { ProductInput } from '../types'

const empty: ProductInput = {
  name: '', brandId: 0, price: 0, oldPrice: null, description: '', gender: 'Nam',
  movementType: 'Quartz', caseSizeMm: null, material: '', waterResistance: '',
  stock: 1, isFeatured: false, isActive: true, imageUrl: '', images: [],
}

export default function ProductForm() {
  const { id } = useParams()
  const editing = !!id
  const nav = useNavigate()
  const brands = useAsync(() => api.brands(), [])
  const [form, setForm] = useState<ProductInput>(empty)
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)
  const [uploading, setUploading] = useState(false)

  useEffect(() => {
    if (!id) return
    api.productById(Number(id)).then(p => setForm({
      name: p.name, brandId: p.brandId, price: p.price, oldPrice: p.oldPrice,
      description: p.description ?? '', gender: p.gender, movementType: p.movementType,
      caseSizeMm: p.caseSizeMm, material: p.material ?? '', waterResistance: p.waterResistance ?? '',
      stock: p.stock, isFeatured: p.isFeatured, isActive: p.isActive,
      imageUrl: p.imageUrl ?? '', images: p.images,
    })).catch(e => setError(e instanceof Error ? e.message : 'Không tải được sản phẩm'))
  }, [id])

  useEffect(() => {
    if (!editing && brands.data?.length && form.brandId === 0) {
      setForm(f => ({ ...f, brandId: brands.data![0].id }))
    }
  }, [brands.data, editing, form.brandId])

  const set = <K extends keyof ProductInput>(k: K, v: ProductInput[K]) => setForm(f => ({ ...f, [k]: v }))

  const onUpload = async (e: ChangeEvent<HTMLInputElement>) => {
    const files = Array.from(e.target.files ?? [])
    if (!files.length) return
    setUploading(true); setError('')
    try {
      const urls: string[] = []
      for (const f of files) urls.push((await api.upload(f)).url)
      setForm(f => {
        const main = f.imageUrl || urls[0]
        const rest = f.imageUrl ? urls : urls.slice(1)
        return { ...f, imageUrl: main, images: [...f.images, ...rest] }
      })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Tải ảnh lên thất bại')
    } finally {
      setUploading(false)
      e.target.value = ''
    }
  }

  const removeImage = (url: string) => setForm(f => {
    if (f.imageUrl === url) {
      const [next, ...rest] = f.images
      return { ...f, imageUrl: next ?? '', images: rest }
    }
    return { ...f, images: f.images.filter(u => u !== url) }
  })

  const makeMain = (url: string) => setForm(f => ({
    ...f, imageUrl: url, images: [...(f.imageUrl ? [f.imageUrl] : []), ...f.images.filter(u => u !== url)],
  }))

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setBusy(true); setError('')
    try {
      if (editing) await api.updateProduct(Number(id), form)
      else await api.createProduct(form)
      nav('/admin/san-pham')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không lưu được')
    } finally {
      setBusy(false)
    }
  }

  const allImages = [form.imageUrl, ...form.images].filter(Boolean)

  return (
    <>
      <div className="admin__head">
        <h1>{editing ? 'Sửa sản phẩm' : 'Thêm sản phẩm'}</h1>
        <Link to="/admin/san-pham" className="text-link">Quay lại danh sách</Link>
      </div>

      <form className="stack form-wide" onSubmit={submit}>
        <label>Tên sản phẩm
          <input required maxLength={200} value={form.name} onChange={e => set('name', e.target.value)} />
        </label>

        <div className="form-row">
          <label>Hãng
            <select value={form.brandId} onChange={e => set('brandId', Number(e.target.value))}>
              {brands.data?.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
            </select>
          </label>
          <label>Dành cho
            <select value={form.gender} onChange={e => set('gender', e.target.value)}>
              <option>Nam</option><option>Nữ</option><option>Unisex</option>
            </select>
          </label>
          <label>Loại máy
            <select value={form.movementType} onChange={e => set('movementType', e.target.value)}>
              {Object.entries(movementLabel).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
            </select>
          </label>
        </div>

        <div className="form-row">
          <label>Giá bán (đồng)
            <input required type="number" min={0} step={1000} value={form.price} onChange={e => set('price', Number(e.target.value))} />
          </label>
          <label>Giá gốc để gạch ngang (không bắt buộc)
            <input type="number" min={0} step={1000} value={form.oldPrice ?? ''} onChange={e => set('oldPrice', e.target.value ? Number(e.target.value) : null)} />
          </label>
          <label>Số lượng trong kho
            <input required type="number" min={0} value={form.stock} onChange={e => set('stock', Number(e.target.value))} />
          </label>
        </div>

        <div className="form-row">
          <label>Đường kính mặt (mm)
            <input type="number" min={10} max={100} value={form.caseSizeMm ?? ''} onChange={e => set('caseSizeMm', e.target.value ? Number(e.target.value) : null)} />
          </label>
          <label>Chất liệu vỏ / dây
            <input value={form.material} onChange={e => set('material', e.target.value)} />
          </label>
          <label>Chống nước
            <input placeholder="Ví dụ: 5 ATM" value={form.waterResistance} onChange={e => set('waterResistance', e.target.value)} />
          </label>
        </div>

        <label>Mô tả
          <textarea rows={5} maxLength={5000} value={form.description} onChange={e => set('description', e.target.value)} />
        </label>

        <fieldset className="images">
          <legend>Ảnh sản phẩm</legend>
          <p className="muted">Ảnh đầu tiên là ảnh đại diện. Chọn nhiều ảnh cùng lúc được. Mỗi ảnh tối đa 5MB (jpg, png, webp).</p>
          <input type="file" accept="image/jpeg,image/png,image/webp" multiple onChange={onUpload} disabled={uploading} />
          {uploading && <p className="muted">Đang tải ảnh lên…</p>}
          <div className="images__list">
            {allImages.map((u, i) => (
              <figure key={u}>
                <img src={u} alt={`Ảnh ${i + 1}`} />
                <figcaption>
                  {i === 0 ? <span>Ảnh đại diện</span> : <button type="button" className="text-btn" onClick={() => makeMain(u)}>Đặt làm ảnh đại diện</button>}
                  <button type="button" className="text-btn text-btn--danger" onClick={() => removeImage(u)}>Bỏ ảnh</button>
                </figcaption>
              </figure>
            ))}
          </div>
        </fieldset>

        <div className="checks">
          <label className="check"><input type="checkbox" checked={form.isActive} onChange={e => set('isActive', e.target.checked)} /> Đang bán (hiện trên website)</label>
          <label className="check"><input type="checkbox" checked={form.isFeatured} onChange={e => set('isFeatured', e.target.checked)} /> Nổi bật (hiện ở trang chủ)</label>
        </div>

        {error && <p className="notice notice--error" role="alert">{error}</p>}
        <div>
          <button className="btn btn--primary" disabled={busy || uploading}>{busy ? 'Đang lưu…' : 'Lưu sản phẩm'}</button>
        </div>
      </form>
    </>
  )
}
