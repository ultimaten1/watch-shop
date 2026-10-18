import { useEffect, useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { api } from '../api'
import { useAccount } from '../account'
import { usePageMeta } from '../hooks'
import { dateTime, money, paymentLabel, statusLabel } from '../format'
import type { Order } from '../types'

export default function Account() {
  usePageMeta('Tài khoản của tôi')
  const { customer, loading, update, logout } = useAccount()
  const nav = useNavigate()

  const [profile, setProfile] = useState({ fullName: '', phone: '', address: '' })
  const [profileMsg, setProfileMsg] = useState<{ ok: boolean; text: string } | null>(null)
  const [pw, setPw] = useState({ current: '', next: '', confirm: '' })
  const [pwMsg, setPwMsg] = useState<{ ok: boolean; text: string } | null>(null)
  const [orders, setOrders] = useState<Order[] | null>(null)
  const [ordersError, setOrdersError] = useState('')

  useEffect(() => {
    if (!customer) return
    setProfile({ fullName: customer.fullName, phone: customer.phone ?? '', address: customer.address ?? '' })
    api.myOrders().then(setOrders).catch(e => setOrdersError(e instanceof Error ? e.message : 'Không tải được đơn hàng'))
  }, [customer?.id]) // eslint-disable-line react-hooks/exhaustive-deps

  if (loading) return <p className="center-note">Đang tải…</p>
  if (!customer) return <Navigate to="/dang-nhap" state={{ from: '/tai-khoan' }} replace />

  const saveProfile = async (e: FormEvent) => {
    e.preventDefault()
    setProfileMsg(null)
    try {
      const c = await api.updateProfile({
        fullName: profile.fullName,
        phone: profile.phone.replace(/[\s.-]/g, '') || undefined,
        address: profile.address || undefined,
      })
      update(c)
      setProfileMsg({ ok: true, text: 'Đã lưu thông tin.' })
    } catch (err) {
      setProfileMsg({ ok: false, text: err instanceof Error ? err.message : 'Không lưu được.' })
    }
  }

  const changePassword = async (e: FormEvent) => {
    e.preventDefault()
    setPwMsg(null)
    if (pw.next !== pw.confirm) {
      setPwMsg({ ok: false, text: 'Mật khẩu mới nhập lại không khớp.' })
      return
    }
    try {
      await api.changeCustomerPassword(pw.current, pw.next)
      setPw({ current: '', next: '', confirm: '' })
      setPwMsg({ ok: true, text: 'Đã đổi mật khẩu.' })
    } catch (err) {
      setPwMsg({ ok: false, text: err instanceof Error ? err.message : 'Không đổi được mật khẩu.' })
    }
  }

  return (
    <div className="container page">
      <h1>Tài khoản của tôi</h1>
      <p>Đăng nhập bằng <strong>{customer.email}</strong> · <button className="text-btn" onClick={() => { logout(); nav('/') }}>Đăng xuất</button></p>

      <h2>Đơn hàng của tôi</h2>
      {ordersError && <p className="notice notice--error" role="alert">{ordersError}</p>}
      {orders === null && !ordersError && <p>Đang tải…</p>}
      {orders && orders.length === 0 && (
        <div className="empty">
          <p>Bạn chưa có đơn hàng nào.</p>
          <Link to="/san-pham" className="btn btn--primary">Xem sản phẩm</Link>
        </div>
      )}
      {orders && orders.length > 0 && (
        <table className="table">
          <thead><tr><th>Mã đơn</th><th>Ngày đặt</th><th>Sản phẩm</th><th>Thanh toán</th><th>Tổng</th><th>Trạng thái</th></tr></thead>
          <tbody>
            {orders.map(o => (
              <tr key={o.id}>
                <td><Link className="text-link" to={`/don-hang/${o.code}`} onClick={() => sessionStorage.setItem(`order_${o.code}`, JSON.stringify(o))}>{o.code}</Link></td>
                <td>{dateTime(o.createdAt)}</td>
                <td>{o.items.map(i => `${i.productName} × ${i.quantity}`).join(', ')}</td>
                <td>{paymentLabel[o.paymentMethod]}</td>
                <td>{money(o.total)}</td>
                <td>{statusLabel[o.status]}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      <div className="checkout" style={{ marginTop: '2rem' }}>
        <form className="stack checkout__form" onSubmit={saveProfile}>
          <h2>Thông tin cá nhân</h2>
          <label>Họ tên
            <input required maxLength={100} value={profile.fullName} onChange={e => setProfile(p => ({ ...p, fullName: e.target.value }))} />
          </label>
          <label>Số điện thoại
            <input type="tel" inputMode="tel" value={profile.phone} onChange={e => setProfile(p => ({ ...p, phone: e.target.value }))} />
          </label>
          <label>Địa chỉ nhận hàng mặc định
            <input maxLength={300} value={profile.address} onChange={e => setProfile(p => ({ ...p, address: e.target.value }))} />
          </label>
          {profileMsg && <p className={`notice ${profileMsg.ok ? 'notice--ok' : 'notice--error'}`} role="status">{profileMsg.text}</p>}
          <button className="btn btn--primary">Lưu thông tin</button>
        </form>

        <form className="stack summary" onSubmit={changePassword}>
          <h2>Đổi mật khẩu</h2>
          <label>Mật khẩu hiện tại
            <input required type="password" autoComplete="current-password" value={pw.current} onChange={e => setPw(p => ({ ...p, current: e.target.value }))} />
          </label>
          <label>Mật khẩu mới (tối thiểu 8 ký tự)
            <input required type="password" minLength={8} autoComplete="new-password" value={pw.next} onChange={e => setPw(p => ({ ...p, next: e.target.value }))} />
          </label>
          <label>Nhập lại mật khẩu mới
            <input required type="password" autoComplete="new-password" value={pw.confirm} onChange={e => setPw(p => ({ ...p, confirm: e.target.value }))} />
          </label>
          {pwMsg && <p className={`notice ${pwMsg.ok ? 'notice--ok' : 'notice--error'}`} role="status">{pwMsg.text}</p>}
          <button className="btn btn--dark">Đổi mật khẩu</button>
        </form>
      </div>
    </div>
  )
}
