import { createContext, useContext, useEffect, useState, type ReactNode } from 'react'
import { api } from './api'
import type { ShopInfo } from './types'

const fallback: ShopInfo = {
  name: 'Đồng Hồ Gia Đình', slogan: '', phone: '', zalo: '', email: '',
  address: '', openingHours: '', facebookUrl: '', mapEmbedUrl: '',
}

const Ctx = createContext<ShopInfo>(fallback)

export function ShopProvider({ children }: { children: ReactNode }) {
  const [shop, setShop] = useState<ShopInfo>(fallback)
  useEffect(() => {
    api.shop().then(setShop).catch(() => { /* giữ giá trị mặc định */ })
  }, [])
  return <Ctx.Provider value={shop}>{children}</Ctx.Provider>
}

export const useShop = () => useContext(Ctx)
