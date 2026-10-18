import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// Khi chạy dev, mọi request /api và /uploads được chuyển sang backend .NET (cổng 5080)
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5080',
      '/uploads': 'http://localhost:5080',
    },
  },
})
