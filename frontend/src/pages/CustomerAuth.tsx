import { useState, type FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { useAccount } from '../account'
import { usePageMeta } from '../hooks'

/** Đăng nhập / đăng ký cho khách mua hàng */
export default function CustomerAuth({ mode }: { mode: 'login' | 'register' }) {
  const isRegister = mode === 'register'
  usePageMeta(isRegister ? 'Đăng ký tài khoản' : 'Đăng nhập')
  const { customer, login, register } = useAccount()
  const nav = useNavigate()
  const loc = useLocation()
  const back = (loc.state as { from?: string } | null)?.from ?? '/tai-khoan'

  const [form, setForm] = useState({ fullName: '', email: '', phone: '', password: '', confirm: '' })
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  if (customer) return <Navigate to={back} replace />

  const bind = (k: keyof typeof form) => ({
    value: form[k],
    onChange: (e: { target: { value: string } }) => setForm(f => ({ ...f, [k]: e.target.value })),
  })

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (isRegister && form.password !== form.confirm) {
      setError('Mật khẩu nhập lại không khớp.')
      return
    }
    setBusy(true)
    try {
      if (isRegister) {
        await register({
          fullName: form.fullName,
          email: form.email,
          password: form.password,
          phone: form.phone.replace(/[\s.-]/g, '') || undefined,
        })
      } else {
        await login(form.email, form.password)
      }
      nav(back, { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Không thực hiện được, vui lòng thử lại.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <div className="container page">
      <form className="stack" onSubmit={submit} style={{ maxWidth: 420, marginInline: 'auto' }}>
        <h1>{isRegister ? 'Đăng ký tài khoản' : 'Đăng nhập'}</h1>
        {isRegister && (
          <label>Họ tên
            <input required maxLength={100} autoComplete="name" {...bind('fullName')} />
          </label>
        )}
        <label>Email
          <input required type="email" autoComplete="email" {...bind('email')} />
        </label>
        {isRegister && (
          <label>Số điện thoại (không bắt buộc)
            <input type="tel" inputMode="tel" autoComplete="tel" placeholder="0901234567" {...bind('phone')} />
          </label>
        )}
        <label>Mật khẩu {isRegister && <small>(tối thiểu 8 ký tự)</small>}
          <input required type="password" minLength={isRegister ? 8 : undefined}
            autoComplete={isRegister ? 'new-password' : 'current-password'} {...bind('password')} />
        </label>
        {isRegister && (
          <label>Nhập lại mật khẩu
            <input required type="password" autoComplete="new-password" {...bind('confirm')} />
          </label>
        )}
        {error && <p className="notice notice--error" role="alert">{error}</p>}
        <button className="btn btn--primary" disabled={busy}>
          {busy ? 'Vui lòng đợi…' : isRegister ? 'Tạo tài khoản' : 'Đăng nhập'}
        </button>
        <p>
          {isRegister
            ? <>Đã có tài khoản? <Link className="text-link" to="/dang-nhap" state={loc.state}>Đăng nhập</Link></>
            : <>Chưa có tài khoản? <Link className="text-link" to="/dang-ky" state={loc.state}>Đăng ký</Link></>}
        </p>
      </form>
    </div>
  )
}
