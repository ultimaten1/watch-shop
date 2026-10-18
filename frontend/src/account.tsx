import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { api, customerAuth } from './api'
import type { Customer } from './types'

interface AccountApi {
  customer: Customer | null
  /** true khi đang kiểm tra token đã lưu */
  loading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (b: { fullName: string; email: string; password: string; phone?: string }) => Promise<void>
  update: (c: Customer) => void
  logout: () => void
}

const Ctx = createContext<AccountApi | null>(null)

export function AccountProvider({ children }: { children: ReactNode }) {
  const [customer, setCustomer] = useState<Customer | null>(null)
  const [loading, setLoading] = useState<boolean>(() => !!customerAuth.get())

  useEffect(() => {
    if (!customerAuth.get()) return
    api.customerMe()
      .then(setCustomer)
      .catch(() => customerAuth.clear())
      .finally(() => setLoading(false))
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const r = await api.customerLogin(email, password)
    customerAuth.set(r.token)
    setCustomer(r.customer)
  }, [])

  const register = useCallback(async (b: { fullName: string; email: string; password: string; phone?: string }) => {
    const r = await api.register(b)
    customerAuth.set(r.token)
    setCustomer(r.customer)
  }, [])

  const logout = useCallback(() => {
    customerAuth.clear()
    setCustomer(null)
  }, [])

  const value = useMemo(
    () => ({ customer, loading, login, register, update: setCustomer, logout }),
    [customer, loading, login, register, logout],
  )
  return <Ctx.Provider value={value}>{children}</Ctx.Provider>
}

export function useAccount() {
  const v = useContext(Ctx)
  if (!v) throw new Error('useAccount phải nằm trong AccountProvider')
  return v
}
