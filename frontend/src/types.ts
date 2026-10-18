export interface Product {
  id: number
  name: string
  slug: string
  brandId: number
  brandName: string
  brandSlug: string
  price: number
  oldPrice: number | null
  description: string | null
  gender: string
  movementType: string
  caseSizeMm: number | null
  material: string | null
  waterResistance: string | null
  stock: number
  isFeatured: boolean
  isActive: boolean
  imageUrl: string | null
  images: string[]
}

export interface ProductInput {
  name: string
  brandId: number
  price: number
  oldPrice: number | null
  description: string
  gender: string
  movementType: string
  caseSizeMm: number | null
  material: string
  waterResistance: string
  stock: number
  isFeatured: boolean
  isActive: boolean
  imageUrl: string
  images: string[]
}

export interface Paged<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface Brand {
  id: number
  name: string
  slug: string
  productCount: number
}

export interface ShopInfo {
  name: string
  slogan: string
  phone: string
  zalo: string
  email: string
  address: string
  openingHours: string
  facebookUrl: string
  mapEmbedUrl: string
}

export type PaymentMethod = 'BankTransfer' | 'Cod' | 'ContactLater'
export type OrderStatus = 'New' | 'Confirmed' | 'Paid' | 'Shipping' | 'Completed' | 'Cancelled'

export interface OrderItem {
  productId: number
  productName: string
  unitPrice: number
  quantity: number
}

export interface PaymentInfo {
  bankName: string
  accountNumber: string
  accountName: string
  transferContent: string
  qrUrl: string
}

export interface Order {
  id: number
  code: string
  customerName: string
  phone: string
  email: string | null
  address: string
  note: string | null
  paymentMethod: PaymentMethod
  status: OrderStatus
  total: number
  createdAt: string
  items: OrderItem[]
  payment: PaymentInfo | null
}

export interface ContactMessage {
  id: number
  name: string
  phone: string
  email: string | null
  message: string
  createdAt: string
  isRead: boolean
}

export interface AdminStats {
  newOrders: number
  unreadMessages: number
  products: number
  lowStock: number
}
