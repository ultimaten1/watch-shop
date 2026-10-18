import { useEffect, useState } from 'react'
import { NavLink, Navigate, Outlet, Route, Routes, useNavigate } from 'react-router-dom'
import { api, auth } from '../api'
import { usePageMeta } from '../hooks'
import Login from './Login'
import Dashboard from './Dashboard'
import AdminOrders from './AdminOrders'
import AdminProducts from './AdminProducts'
import ProductForm from './ProductForm'
import AdminBrands from './AdminBrands'
import AdminMessages from './AdminMessages'

function Guard() {
  const nav = useNavigate()
  const [ok, setOk] = useState<boolean | null>(auth.get() ? null : false)

  useEffect(() => {
    if (!auth.get()) return
    api.me().then(() => setOk(true)).catch(() => { auth.clear(); setOk(false) })
  }, [])

  if (ok === false) return <Navigate to="/admin/login" replace />
  if (ok === null) return <p className="center-note">Đang kiểm tra đăng nhập…</p>

  return (
    <div className="admin">
      <aside className="admin__nav">
        <strong className="admin__title">Quản trị cửa hàng</strong>
        <NavLink end to="/admin">Tổng quan</NavLink>
        <NavLink to="/admin/don-hang">Đơn hàng</NavLink>
        <NavLink to="/admin/san-pham">Sản phẩm</NavLink>
        <NavLink to="/admin/hang">Hãng</NavLink>
        <NavLink to="/admin/tin-nhan">Tin nhắn</NavLink>
        <a href="/" target="_blank" rel="noreferrer">Xem website</a>
        <button className="text-btn" onClick={() => { auth.clear(); nav('/admin/login') }}>Đăng xuất</button>
      </aside>
      <section className="admin__main">
        <Outlet />
      </section>
    </div>
  )
}

export default function AdminApp() {
  usePageMeta('Quản trị')
  return (
    <Routes>
      <Route path="login" element={<Login />} />
      <Route element={<Guard />}>
        <Route index element={<Dashboard />} />
        <Route path="don-hang" element={<AdminOrders />} />
        <Route path="san-pham" element={<AdminProducts />} />
        <Route path="san-pham/moi" element={<ProductForm />} />
        <Route path="san-pham/:id" element={<ProductForm />} />
        <Route path="hang" element={<AdminBrands />} />
        <Route path="tin-nhan" element={<AdminMessages />} />
      </Route>
    </Routes>
  )
}
