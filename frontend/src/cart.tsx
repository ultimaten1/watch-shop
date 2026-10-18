import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import type { Product } from './types'

export interface CartLine {
  productId: number
  slug: string
  name: string
  brandName: string
  price: number
  imageUrl: string | null
  stock: number
  quantity: number
}

interface CartApi {
  lines: CartLine[]
  count: number
  total: number
  add: (p: Product, qty?: number) => void
  setQty: (productId: number, qty: number) => void
  remove: (productId: number) => void
  clear: () => void
}

const KEY = 'ws_cart_v1'
const Ctx = createContext<CartApi | null>(null)

export function CartProvider({ children }: { children: ReactNode }) {
  const [lines, setLines] = useState<CartLine[]>(() => {
    try {
      return JSON.parse(localStorage.getItem(KEY) ?? '[]') as CartLine[]
    } catch {
      return []
    }
  })

  useEffect(() => {
    try { localStorage.setItem(KEY, JSON.stringify(lines)) } catch { /* bỏ qua */ }
  }, [lines])

  const api = useMemo<CartApi>(() => ({
    lines,
    count: lines.reduce((n, l) => n + l.quantity, 0),
    total: lines.reduce((s, l) => s + l.price * l.quantity, 0),
    add: (p, qty = 1) =>
      setLines(prev => {
        const found = prev.find(l => l.productId === p.id)
        if (found) {
          return prev.map(l =>
            l.productId === p.id ? { ...l, stock: p.stock, quantity: Math.min(l.quantity + qty, Math.max(p.stock, 1)) } : l)
        }
        return [...prev, {
          productId: p.id, slug: p.slug, name: p.name, brandName: p.brandName,
          price: p.price, imageUrl: p.imageUrl, stock: p.stock, quantity: Math.min(qty, Math.max(p.stock, 1)),
        }]
      }),
    setQty: (id, qty) =>
      setLines(prev => prev.map(l => (l.productId === id ? { ...l, quantity: Math.max(1, Math.min(qty, Math.max(l.stock, 1))) } : l))),
    remove: id => setLines(prev => prev.filter(l => l.productId !== id)),
    clear: () => setLines([]),
  }), [lines])

  return <Ctx.Provider value={api}>{children}</Ctx.Provider>
}

export function useCart() {
  const c = useContext(Ctx)
  if (!c) throw new Error('useCart phải nằm trong CartProvider')
  return c
}
