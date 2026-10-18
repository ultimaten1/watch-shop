import { Link } from 'react-router-dom'
import { usePageMeta } from '../hooks'

export default function NotFound({ message = 'Trang bạn tìm không tồn tại.' }: { message?: string }) {
  usePageMeta('Không tìm thấy trang')
  return (
    <div className="container page">
      <div className="empty">
        <h1>Không tìm thấy</h1>
        <p>{message}</p>
        <Link to="/san-pham" className="btn btn--primary">Xem sản phẩm</Link>
      </div>
    </div>
  )
}
