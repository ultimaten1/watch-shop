import { useEffect, useState, type FormEvent } from 'react'
import { api } from '../api'
import type { AdminRole, AdminUser } from '../types'

const roleLabel: Record<AdminRole, string> = { Owner: 'Chủ cửa hàng', Staff: 'Nhân viên' }

export default function AdminUsers() {
  const [users, setUsers] = useState<AdminUser[] | null>(null)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ username: '', fullName: '', password: '', role: 'Staff' as AdminRole })
  const [busy, setBusy] = useState(false)

  const load = () => api.adminUsers().then(setUsers).catch(e => setError(e instanceof Error ? e.message : 'Lỗi tải danh sách'))
  useEffect(() => { load() }, [])

  const run = async (fn: () => Promise<unknown>) => {
    setError('')
    try { await fn(); await load() } catch (e) { setError(e instanceof Error ? e.message : 'Có lỗi xảy ra') }
  }

  const create = async (e: FormEvent) => {
    e.preventDefault()
    setBusy(true)
    await run(async () => {
      await api.createAdminUser(form)
      setForm({ username: '', fullName: '', password: '', role: 'Staff' })
    })
    setBusy(false)
  }

  const toggle = (u: AdminUser) =>
    run(() => api.updateAdminUser(u.id, { fullName: u.fullName, role: u.role, isActive: !u.isActive }))

  const changeRole = (u: AdminUser, role: AdminRole) =>
    run(() => api.updateAdminUser(u.id, { fullName: u.fullName, role, isActive: u.isActive }))

  const resetPassword = (u: AdminUser) => {
    const pw = window.prompt(`Mật khẩu mới cho "${u.username}" (tối thiểu 8 ký tự):`)
    if (pw) run(() => api.updateAdminUser(u.id, { fullName: u.fullName, role: u.role, isActive: u.isActive, newPassword: pw }))
  }

  const remove = (u: AdminUser) => {
    if (window.confirm(`Xoá tài khoản "${u.username}"?`)) run(() => api.deleteAdminUser(u.id))
  }

  return (
    <div>
      <h1>Tài khoản quản trị</h1>
      <p>Chủ cửa hàng được dùng mọi chức năng. Nhân viên xem và xử lý đơn hàng, tin nhắn, thêm và sửa sản phẩm, nhưng không xoá được sản phẩm, hãng, tin nhắn và không quản lý tài khoản.</p>
      {error && <p className="notice notice--error" role="alert">{error}</p>}

      <table className="table">
        <thead><tr><th>Tên đăng nhập</th><th>Họ tên</th><th>Quyền</th><th>Trạng thái</th><th /></tr></thead>
        <tbody>
          {users?.map(u => (
            <tr key={u.id}>
              <td>{u.username}</td>
              <td>{u.fullName}</td>
              <td>
                <select value={u.role} onChange={e => changeRole(u, e.target.value as AdminRole)}>
                  <option value="Owner">{roleLabel.Owner}</option>
                  <option value="Staff">{roleLabel.Staff}</option>
                </select>
              </td>
              <td>{u.isActive ? 'Đang hoạt động' : 'Đã khoá'}</td>
              <td className="table__actions">
                <button className="text-btn" onClick={() => toggle(u)}>{u.isActive ? 'Khoá' : 'Mở khoá'}</button>
                <button className="text-btn" onClick={() => resetPassword(u)}>Đặt lại mật khẩu</button>
                <button className="text-btn" onClick={() => remove(u)}>Xoá</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      <form className="stack" onSubmit={create} style={{ marginTop: '2rem', maxWidth: 420 }}>
        <h2>Thêm tài khoản</h2>
        <label>Tên đăng nhập
          <input required minLength={3} maxLength={50} autoComplete="off" value={form.username} onChange={e => setForm(f => ({ ...f, username: e.target.value }))} />
        </label>
        <label>Họ tên
          <input required maxLength={100} value={form.fullName} onChange={e => setForm(f => ({ ...f, fullName: e.target.value }))} />
        </label>
        <label>Mật khẩu (tối thiểu 8 ký tự)
          <input required type="password" minLength={8} autoComplete="new-password" value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))} />
        </label>
        <label>Quyền
          <select value={form.role} onChange={e => setForm(f => ({ ...f, role: e.target.value as AdminRole }))}>
            <option value="Staff">{roleLabel.Staff}</option>
            <option value="Owner">{roleLabel.Owner}</option>
          </select>
        </label>
        <button className="btn btn--primary" disabled={busy}>Tạo tài khoản</button>
      </form>
    </div>
  )
}
