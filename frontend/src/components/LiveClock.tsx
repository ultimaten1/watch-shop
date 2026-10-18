import { useEffect, useState } from 'react'

/** Đồng hồ kim chạy theo giờ thật của người xem: điểm nhấn của trang chủ. */
export default function LiveClock({ label = 'Since 1990' }: { label?: string }) {
  const [now, setNow] = useState(() => new Date())

  useEffect(() => {
    const id = setInterval(() => setNow(new Date()), 1000)
    return () => clearInterval(id)
  }, [])

  const s = now.getSeconds()
  const m = now.getMinutes() + s / 60
  const h = (now.getHours() % 12) + m / 60

  const ticks = Array.from({ length: 60 }, (_, i) => i)
  const time = now.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })

  return (
    <svg
      className="live-clock"
      viewBox="-110 -110 220 220"
      role="img"
      aria-label={`Đồng hồ đang chỉ ${time}`}
    >
      <defs>
        <radialGradient id="dial" cx="35%" cy="30%" r="90%">
          <stop offset="0%" stopColor="#1E4466" />
          <stop offset="60%" stopColor="#0E2438" />
          <stop offset="100%" stopColor="#081827" />
        </radialGradient>
        <linearGradient id="bezel" x1="0" y1="0" x2="1" y2="1">
          <stop offset="0%" stopColor="#E2BE7B" />
          <stop offset="50%" stopColor="#B8863B" />
          <stop offset="100%" stopColor="#7C5A22" />
        </linearGradient>
      </defs>

      <circle r="106" fill="url(#bezel)" />
      <circle r="98" fill="#081827" />
      <circle r="94" fill="url(#dial)" />

      {ticks.map(i => {
        const major = i % 5 === 0
        const len = major ? 11 : 4
        return (
          <line
            key={i}
            x1="0" y1={-88} x2="0" y2={-88 + len}
            transform={`rotate(${i * 6})`}
            stroke={major ? '#E8D7B0' : '#6F8499'}
            strokeWidth={major ? 3 : 1}
            strokeLinecap="round"
          />
        )
      })}

      <text y="-40" textAnchor="middle" className="clock-brand">{label}</text>
      <text y="52" textAnchor="middle" className="clock-sub">Automatic</text>

      {/* kim giờ */}
      <g transform={`rotate(${h * 30})`}>
        <line y1="10" y2="-48" stroke="#E8D7B0" strokeWidth="6" strokeLinecap="round" />
        <line y1="-14" y2="-46" stroke="#0E2438" strokeWidth="2" strokeLinecap="round" />
      </g>
      {/* kim phút */}
      <g transform={`rotate(${m * 6})`}>
        <line y1="12" y2="-72" stroke="#E8D7B0" strokeWidth="4.5" strokeLinecap="round" />
        <line y1="-18" y2="-70" stroke="#0E2438" strokeWidth="1.6" strokeLinecap="round" />
      </g>
      {/* kim giây */}
      <g transform={`rotate(${s * 6})`}>
        <line y1="22" y2="-80" stroke="#8FD3B0" strokeWidth="1.5" strokeLinecap="round" />
        <circle cy="-62" r="3.2" fill="#8FD3B0" />
      </g>
      <circle r="5" fill="#B8863B" />
      <circle r="2" fill="#081827" />
    </svg>
  )
}
