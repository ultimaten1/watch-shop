import { useState, type FormEvent } from 'react'
import { api } from '../api'
import { usePageMeta } from '../hooks'
import { useShop } from '../shop'

export default function Contact() {
  const shop = useShop()
  usePageMeta(`Liên hệ ${shop.name}`, `Địa chỉ, số điện thoại, Zalo và giờ mở cửa của ${shop.name}.`)
  const [form, setForm] = useState({ name: '', phone: '', email: '', message: '' })
  const [state, setState] = useState<'idle' | 'busy' | 'done'>('idle')
  const [error, setError] = useState('')

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setState('busy'); setError('')
    try {
      await api.contact({ ...form, phone: form.phone.replace(/[\s.-]/g, ''), email: form.email || undefined })
      setState('done')
      setForm({ name: '', phone: '', email: '', message: '' })
    } catch (err) {
      setState('idle')
      setError(err instanceof Error ? err.message : 'Không gửi được, vui lòng thử lại.')
    }
  }

  const zalo = shop.zalo ? `https://zalo.me/${shop.zalo.replace(/\D/g, '')}` : undefined

  return (
    <div className="container page">
      <h1>Liên hệ</h1>
      <div className="contact">
        <div className="stack">
          <h2>Gửi lời nhắn</h2>
          {state === 'done' ? (
            <p className="notice notice--ok" role="status">Đã gửi. Cửa hàng sẽ gọi lại cho bạn sớm nhất.</p>
          ) : (
            <form className="stack" onSubmit={submit}>
              <label>Họ tên
                <input required maxLength={100} value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} />
              </label>
              <label>Số điện thoại
                <input required type="tel" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
              </label>
              <label>Email (không bắt buộc)
                <input type="email" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} />
              </label>
              <label>Bạn cần hỏi gì?
                <textarea required rows={5} maxLength={2000} value={form.message} onChange={e => setForm({ ...form, message: e.target.value })} />
              </label>
              {error && <p className="notice notice--error" role="alert">{error}</p>}
              <button className="btn btn--primary" disabled={state === 'busy'}>
                {state === 'busy' ? 'Đang gửi…' : 'Gửi lời nhắn'}
              </button>
            </form>
          )}
        </div>

        <div className="stack">
          <h2>Thông tin cửa hàng</h2>
          <p>{shop.address}</p>
          <p>{shop.openingHours}</p>
          {shop.phone && <p>Điện thoại: <a href={`tel:${shop.phone}`}>{shop.phone}</a></p>}
          {zalo && <p>Zalo: <a href={zalo} target="_blank" rel="noreferrer">{shop.zalo}</a></p>}
          {shop.email && <p>Email: <a href={`mailto:${shop.email}`}>{shop.email}</a></p>}
          {shop.mapEmbedUrl && (
            <iframe title="Bản đồ cửa hàng" src={shop.mapEmbedUrl} className="map" loading="lazy" referrerPolicy="no-referrer-when-downgrade" />
          )}
        </div>
      </div>
    </div>
  )
}
