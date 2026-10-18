import type {
  AdminMe, AdminStats, AdminUser, Brand, ContactMessage, Customer, CustomerAuthResult, Order, OrderStatus, Paged, PaymentMethod,
  Product, ProductInput, ShopInfo,
} from './types'

const BASE = import.meta.env.VITE_API_BASE ?? ''
const TOKEN_KEY = 'ws_admin_token'
const CUSTOMER_KEY = 'ws_customer_token'

export const auth = {
  get: () => localStorage.getItem(TOKEN_KEY),
  set: (t: string) => localStorage.setItem(TOKEN_KEY, t),
  clear: () => localStorage.removeItem(TOKEN_KEY),
}

export const customerAuth = {
  get: () => localStorage.getItem(CUSTOMER_KEY),
  set: (t: string) => localStorage.setItem(CUSTOMER_KEY, t),
  clear: () => localStorage.removeItem(CUSTOMER_KEY),
}

// Các API dành cho khách dùng token khách; còn lại dùng token quản trị
const isCustomerPath = (path: string, method?: string) =>
  path.startsWith('/api/account') || path.startsWith('/api/orders/mine') ||
  (path === '/api/orders' && method === 'POST')

export class ApiError extends Error {
  status: number
  constructor(message: string, status: number) {
    super(message)
    this.status = status
  }
}

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  const asCustomer = isCustomerPath(path, init.method)
  const store = asCustomer ? customerAuth : auth
  const token = store.get()
  if (token) headers.set('Authorization', `Bearer ${token}`)
  if (init.body && !(init.body instanceof FormData)) headers.set('Content-Type', 'application/json')

  let res: Response
  try {
    res = await fetch(BASE + path, { ...init, headers })
  } catch {
    throw new ApiError('Không kết nối được máy chủ. Vui lòng thử lại sau.', 0)
  }

  if (res.status === 401 && token) {
    store.clear()
  }
  if (res.status === 429) throw new ApiError('Bạn thao tác quá nhanh, vui lòng đợi một phút rồi thử lại.', 429)
  if (!res.ok) {
    let message = 'Có lỗi xảy ra, vui lòng thử lại.'
    try {
      const body = await res.json()
      if (body?.error) message = body.error
      else if (body?.errors) message = Object.values(body.errors as Record<string, string[]>).flat().join(' ')
    } catch { /* body không phải JSON */ }
    throw new ApiError(message, res.status)
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

const qs = (params: Record<string, string | number | boolean | undefined | null>) => {
  const sp = new URLSearchParams()
  Object.entries(params).forEach(([k, v]) => {
    if (v !== undefined && v !== null && v !== '') sp.set(k, String(v))
  })
  const s = sp.toString()
  return s ? `?${s}` : ''
}

export interface ProductFilters {
  search?: string
  brand?: string
  gender?: string
  movement?: string
  minPrice?: number
  maxPrice?: number
  featured?: boolean
  sort?: string
  page?: number
  pageSize?: number
  includeInactive?: boolean
}

export const api = {
  shop: () => request<ShopInfo>('/api/shop'),
  brands: () => request<Brand[]>('/api/brands'),
  products: (f: ProductFilters = {}) => request<Paged<Product>>('/api/products' + qs({ ...f })),
  product: (slug: string) => request<Product>(`/api/products/${encodeURIComponent(slug)}`),
  contact: (body: { name: string; phone: string; email?: string; message: string }) =>
    request<{ ok: boolean }>('/api/contact', { method: 'POST', body: JSON.stringify(body) }),
  createOrder: (body: {
    customerName: string; phone: string; email?: string; address: string; note?: string
    paymentMethod: PaymentMethod; items: { productId: number; quantity: number }[]
  }) => request<Order>('/api/orders', { method: 'POST', body: JSON.stringify(body) }),
  lookupOrder: (code: string, phone: string) =>
    request<Order>('/api/orders/lookup' + qs({ code, phone })),

  // Tài khoản khách
  register: (b: { fullName: string; email: string; password: string; phone?: string }) =>
    request<CustomerAuthResult>('/api/account/register', { method: 'POST', body: JSON.stringify(b) }),
  customerLogin: (email: string, password: string) =>
    request<CustomerAuthResult>('/api/account/login', { method: 'POST', body: JSON.stringify({ email, password }) }),
  customerMe: () => request<Customer>('/api/account/me'),
  updateProfile: (b: { fullName: string; phone?: string; address?: string }) =>
    request<Customer>('/api/account/me', { method: 'PUT', body: JSON.stringify(b) }),
  changeCustomerPassword: (currentPassword: string, newPassword: string) =>
    request<void>('/api/account/password', { method: 'PUT', body: JSON.stringify({ currentPassword, newPassword }) }),
  myOrders: () => request<Order[]>('/api/orders/mine'),

  // Quản trị
  login: (username: string, password: string) =>
    request<{ token: string; expiresAt: string }>('/api/auth/login', {
      method: 'POST', body: JSON.stringify({ username, password }),
    }),
  me: () => request<AdminMe>('/api/auth/me'),
  changeAdminPassword: (currentPassword: string, newPassword: string) =>
    request<void>('/api/auth/password', { method: 'PUT', body: JSON.stringify({ currentPassword, newPassword }) }),
  adminUsers: () => request<AdminUser[]>('/api/admin/users'),
  createAdminUser: (b: { username: string; fullName: string; password: string; role: string }) =>
    request<AdminUser>('/api/admin/users', { method: 'POST', body: JSON.stringify(b) }),
  updateAdminUser: (id: number, b: { fullName: string; role: string; isActive: boolean; newPassword?: string }) =>
    request<AdminUser>(`/api/admin/users/${id}`, { method: 'PUT', body: JSON.stringify(b) }),
  deleteAdminUser: (id: number) => request<void>(`/api/admin/users/${id}`, { method: 'DELETE' }),
  stats: () => request<AdminStats>('/api/admin/stats'),
  productById: (id: number) => request<Product>(`/api/products/by-id/${id}`),
  createProduct: (b: ProductInput) =>
    request<Product>('/api/products', { method: 'POST', body: JSON.stringify(b) }),
  updateProduct: (id: number, b: ProductInput) =>
    request<Product>(`/api/products/${id}`, { method: 'PUT', body: JSON.stringify(b) }),
  deleteProduct: (id: number) => request<void>(`/api/products/${id}`, { method: 'DELETE' }),
  createBrand: (name: string) =>
    request<Brand>('/api/brands', { method: 'POST', body: JSON.stringify({ name }) }),
  deleteBrand: (id: number) => request<void>(`/api/brands/${id}`, { method: 'DELETE' }),
  orders: (status?: OrderStatus) => request<Order[]>('/api/orders' + qs({ status })),
  setOrderStatus: (id: number, status: OrderStatus) =>
    request<Order>(`/api/orders/${id}/status`, { method: 'PUT', body: JSON.stringify({ status }) }),
  messages: () => request<ContactMessage[]>('/api/contact'),
  markMessageRead: (id: number) => request<void>(`/api/contact/${id}/read`, { method: 'PUT' }),
  deleteMessage: (id: number) => request<void>(`/api/contact/${id}`, { method: 'DELETE' }),
  upload: async (file: File) => {
    const fd = new FormData()
    fd.append('file', file)
    return request<{ url: string }>('/api/uploads', { method: 'POST', body: fd })
  },
}
