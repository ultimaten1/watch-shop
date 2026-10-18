import { Link } from 'react-router-dom'
import { api } from '../api'
import { useAsync } from '../hooks'

export default function Dashboard() {
  const { data, error } = useAsync(() => api.stats(), [])
  return (
    <>
      <h1>Tổng quan</h1>
      {error && <p className="notice notice--error">{error}</p>}
      {data && (
        <div className="stat-grid">
          <Link to="/admin/don-hang" className="stat"><strong>{data.newOrders}</strong><span>đơn mới chờ xử lý</span></Link>
          <Link to="/admin/tin-nhan" className="stat"><strong>{data.unreadMessages}</strong><span>tin nhắn chưa đọc</span></Link>
          <Link to="/admin/san-pham" className="stat"><strong>{data.products}</strong><span>sản phẩm</span></Link>
          <Link to="/admin/san-pham" className="stat"><strong>{data.lowStock}</strong><span>sản phẩm sắp hết hàng</span></Link>
        </div>
      )}
    </>
  )
}
