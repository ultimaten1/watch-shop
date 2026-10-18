import type { OrderStatus, PaymentMethod } from './types'

const vnd = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 })
export const money = (n: number) => vnd.format(n)

export const dateTime = (iso: string) =>
  new Date(iso.endsWith('Z') ? iso : iso + 'Z').toLocaleString('vi-VN', {
    day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit',
  })

export const statusLabel: Record<OrderStatus, string> = {
  New: 'Mới đặt',
  Confirmed: 'Đã xác nhận',
  Paid: 'Đã thanh toán',
  Shipping: 'Đang giao',
  Completed: 'Hoàn tất',
  Cancelled: 'Đã huỷ',
}

export const paymentLabel: Record<PaymentMethod, string> = {
  BankTransfer: 'Chuyển khoản',
  Cod: 'Thanh toán khi nhận hàng',
  ContactLater: 'Liên hệ để thống nhất',
}

export const movementLabel: Record<string, string> = {
  Automatic: 'Máy cơ (Automatic)',
  Quartz: 'Máy pin (Quartz)',
  Solar: 'Năng lượng ánh sáng (Solar)',
}

export const discountPercent = (price: number, old: number | null) =>
  old && old > price ? Math.round(((old - price) / old) * 100) : 0
