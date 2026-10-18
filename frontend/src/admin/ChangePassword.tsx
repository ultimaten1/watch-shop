import { useState, type FormEvent } from 'react'
import { api } from '../api'

export default function ChangePassword() {
  const [pw, setPw] = useState({ current: '', next: '', confirm: '' })
  const [msg, setMsg] = useState<{ ok: boolean; text: string } | null>(null)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setMsg(null)
    if (pw.next !== pw.confirm) {
      setMsg({ ok: false, text: 'Mật khẩu mới nhập lại không khớp.' })
      return
    }
    try {
      await api.changeAdminPassword(pw.current, pw.next)
      setPw({ current: '', next: '', confirm: '' })
      setMsg({ ok: true, text: 'Đã đổi mật khẩu.' })
    } catch (err) {
      setMsg({ ok: false, text: err instanceof Error ? err.message : 'Không đổi được mật khẩu.' })
    }
  }

  return (
    <form className="stack" onSubmit={submit} style={{ maxWidth: 420 }}>
      <h1>Đổi mật khẩu</h1>
      <label>Mật khẩu hiện tại
        <input required type="password" autoComplete="current-password" value={pw.current} onChange={e => setPw(p => ({ ...p, current: e.target.value }))} />
      </label>
      <label>Mật khẩu mới (tối thiểu 8 ký tự)
        <input required type="password" minLength={8} autoComplete="new-password" value={pw.next} onChange={e => setPw(p => ({ ...p, next: e.target.value }))} />
      </label>
      <label>Nhập lại mật khẩu mới
        <input required type="password" autoComplete="new-password" value={pw.confirm} onChange={e => setPw(p => ({ ...p, confirm: e.target.value }))} />
      </label>
      {msg && <p className={`notice ${msg.ok ? 'notice--ok' : 'notice--error'}`} role="status">{msg.text}</p>}
      <button className="btn btn--primary">Đổi mật khẩu</button>
    </form>
  )
}
