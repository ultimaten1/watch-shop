import { Link } from 'react-router-dom'
import { usePageMeta } from '../hooks'
import { useShop } from '../shop'

export default function About() {
  const shop = useShop()
  usePageMeta(`Về ${shop.name}`, `Câu chuyện và cam kết của ${shop.name}.`)

  return (
    <div className="container page narrow prose">
      <h1>Về {shop.name}</h1>
      {/* NỘI DUNG MẪU: hãy thay bằng câu chuyện thật của gia đình bạn (bắt đầu bán từ năm nào, ai trông cửa hàng, vì sao chọn đồng hồ…) */}
      <p>
        Cửa hàng bắt đầu từ một quầy nhỏ của gia đình. Chúng tôi bán đồng hồ theo cách đơn giản:
        chọn những mẫu mình tin dùng, nói thật về từng chiếc và ở lại bảo hành sau khi bán.
      </p>
      <h2>Chúng tôi cam kết</h2>
      <ul>
        <li>Đồng hồ chính hãng, có phiếu bảo hành của cửa hàng.</li>
        <li>Mô tả đúng tình trạng, ảnh chụp là ảnh thật của sản phẩm.</li>
        <li>Hỗ trợ chỉnh dây, thay pin, vệ sinh đồng hồ đã mua tại cửa hàng.</li>
      </ul>
      <h2>Gặp chúng tôi</h2>
      <p>{shop.address}</p>
      <p>{shop.openingHours}</p>
      <p><Link to="/lien-he" className="btn btn--primary">Liên hệ cửa hàng</Link></p>
    </div>
  )
}
