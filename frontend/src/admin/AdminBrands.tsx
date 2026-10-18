import { useState, type FormEvent } from 'react'
import { api } from '../api'
import { useAsync } from '../hooks'

export default function AdminBrands() {
  const { data, error, reload } = useAsync(() => api.brands(), [])
  const [name, setName] = useState('')
  const [msg, setMsg] = useState('')

  const add = async (e: FormEvent) => {
    e.preventDefault()
    setMsg('')
    try { await api.createBrand(name.trim()); setName(''); reload() }
    catch (err) { setMsg(err instanceof Error ? err.message : 'Không thêm được') }
  }

  const remove = async (id: number, n: string) => {
    if (!confirm(`Xoá hãng “${n}”?`)) return
    setMsg('')
    try { await api.deleteBrand(id); reload() }
    catch (err) { setMsg(err instanceof Error ? err.message : 'Không xoá được') }
  }

  return (
    <>
      <h1>Hãng đồng hồ</h1>
      <form className="inline-form" onSubmit={add}>
        <input required maxLength={100} placeholder="Tên hãng mới" aria-label="Tên hãng mới" value={name} onChange={e => setName(e.target.value)} />
        <button className="btn btn--primary">Thêm hãng</button>
      </form>
      {msg && <p className="notice notice--error" role="alert">{msg}</p>}
      {error && <p className="notice notice--error">{error}</p>}
      <table className="table">
        <thead><tr><th>Hãng</th><th>Số sản phẩm đang bán</th><th /></tr></thead>
        <tbody>
          {data?.map(b => (
            <tr key={b.id}>
              <td>{b.name}</td>
              <td>{b.productCount}</td>
              <td className="table__actions"><button className="text-btn text-btn--danger" onClick={() => remove(b.id, b.name)}>Xoá</button></td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  )
}
