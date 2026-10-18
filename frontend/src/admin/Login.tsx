import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api, auth } from '../api'

export default function Login() {
  const nav = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setBusy(true); setError('')
    try {
      const r = await api.login(username, password)
      auth.set(r.token)
      nav('/admin', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không đăng nhập được')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="login-wrap">
      <form className="login stack" onSubmit={submit}>
        <h1>Đăng nhập quản trị</h1>
        <label>Tên đăng nhập
          <input required autoComplete="username" value={username} onChange={e => setUsername(e.target.value)} />
        </label>
        <label>Mật khẩu
          <input required type="password" autoComplete="current-password" value={password} onChange={e => setPassword(e.target.value)} />
        </label>
        {error && <p className="notice notice--error" role="alert">{error}</p>}
        <button className="btn btn--primary" disabled={busy}>{busy ? 'Đang đăng nhập…' : 'Đăng nhập'}</button>
      </form>
    </div>
  )
}
