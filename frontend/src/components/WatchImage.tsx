/** Ảnh sản phẩm. Khi chưa có ảnh thật sẽ hiện hình đồng hồ vẽ tạm để trang không bị trống. */
export default function WatchImage({
  src, alt, seed = 0, eager = false,
}: { src?: string | null; alt: string; seed?: number; eager?: boolean }) {
  if (src) {
    return <img src={src} alt={alt} loading={eager ? 'eager' : 'lazy'} className="watch-img" />
  }
  const dials = ['#0E2438', '#1F2A24', '#2B2B33', '#E9E6DE', '#34495E', '#3A2B22']
  const dial = dials[Math.abs(seed) % dials.length]
  const light = dial === '#E9E6DE'
  const mark = light ? '#2B3138' : '#E8D7B0'
  return (
    <svg className="watch-img placeholder" viewBox="0 0 200 240" role="img" aria-label={alt}>
      <rect width="200" height="240" fill="#E3E8EB" />
      <rect x="76" y="0" width="48" height="240" fill="#9AA7B1" opacity="0.55" />
      <circle cx="100" cy="120" r="62" fill="#B8863B" />
      <circle cx="100" cy="120" r="55" fill={dial} />
      {Array.from({ length: 12 }, (_, i) => (
        <line key={i} x1="100" y1="72" x2="100" y2={i % 3 === 0 ? 82 : 77}
          transform={`rotate(${i * 30} 100 120)`} stroke={mark} strokeWidth={i % 3 === 0 ? 3 : 1.5} strokeLinecap="round" />
      ))}
      <line x1="100" y1="120" x2="100" y2="92" stroke={mark} strokeWidth="3" strokeLinecap="round"
        transform={`rotate(${(seed % 12) * 30 + 40} 100 120)`} />
      <line x1="100" y1="120" x2="100" y2="80" stroke={mark} strokeWidth="2" strokeLinecap="round"
        transform={`rotate(${(seed % 7) * 50 + 120} 100 120)`} />
      <circle cx="100" cy="120" r="3" fill="#B8863B" />
    </svg>
  )
}
