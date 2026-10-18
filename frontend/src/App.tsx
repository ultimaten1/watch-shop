import { lazy, Suspense } from 'react'
import { Route, Routes } from 'react-router-dom'
import Layout from './components/Layout'
import Home from './pages/Home'
import Products from './pages/Products'
import ProductDetail from './pages/ProductDetail'
import Cart from './pages/Cart'
import Checkout from './pages/Checkout'
import OrderResult from './pages/OrderResult'
import OrderLookup from './pages/OrderLookup'
import About from './pages/About'
import Contact from './pages/Contact'
import NotFound from './pages/NotFound'
import CustomerAuth from './pages/CustomerAuth'
import Account from './pages/Account'

// Trang quản trị tải riêng để khách không phải tải thêm code không dùng tới
const AdminApp = lazy(() => import('./admin/AdminApp'))

export default function App() {
  return (
    <Routes>
      <Route path="/admin/*" element={
        <Suspense fallback={<p className="center-note">Đang tải…</p>}>
          <AdminApp />
        </Suspense>
      } />
      <Route element={<Layout />}>
        <Route index element={<Home />} />
        <Route path="san-pham" element={<Products />} />
        <Route path="san-pham/:slug" element={<ProductDetail />} />
        <Route path="gio-hang" element={<Cart />} />
        <Route path="thanh-toan" element={<Checkout />} />
        <Route path="don-hang/:code" element={<OrderResult />} />
        <Route path="tra-cuu" element={<OrderLookup />} />
        <Route path="gioi-thieu" element={<About />} />
        <Route path="lien-he" element={<Contact />} />
        <Route path="dang-nhap" element={<CustomerAuth mode="login" />} />
        <Route path="dang-ky" element={<CustomerAuth mode="register" />} />
        <Route path="tai-khoan" element={<Account />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  )
}
