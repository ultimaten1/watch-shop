import { api } from '../api'
import { useAsync } from '../hooks'
import { dateTime } from '../format'

export default function AdminMessages() {
  const { data, error, loading, reload } = useAsync(() => api.messages(), [])

  const read = async (id: number) => { await api.markMessageRead(id); reload() }
  const remove = async (id: number) => {
    if (!confirm('Xoá tin nhắn này?')) return
    await api.deleteMessage(id); reload()
  }

  return (
    <>
      <h1>Tin nhắn từ khách</h1>
      {error && <p className="notice notice--error">{error}</p>}
      {loading && <p className="muted">Đang tải…</p>}
      {data && data.length === 0 && <p className="muted">Chưa có tin nhắn nào.</p>}
      <div className="order-list">
        {data?.map(m => (
          <article key={m.id} className={`order-card ${m.isRead ? '' : 'is-unread'}`}>
            <header>
              <strong>{m.name}</strong>
              <a href={`tel:${m.phone}`}>{m.phone}</a>
              <span className="muted">{dateTime(m.createdAt)}</span>
            </header>
            {m.email && <p className="muted">{m.email}</p>}
            <p className="pre">{m.message}</p>
            <div className="table__actions">
              {!m.isRead && <button className="text-btn" onClick={() => read(m.id)}>Đánh dấu đã đọc</button>}
              <button className="text-btn text-btn--danger" onClick={() => remove(m.id)}>Xoá</button>
            </div>
          </article>
        ))}
      </div>
    </>
  )
}
