import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import App from './App'
import { CartProvider } from './cart'
import { ShopProvider } from './shop'
import './styles.css'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <ShopProvider>
        <CartProvider>
          <App />
        </CartProvider>
      </ShopProvider>
    </BrowserRouter>
  </StrictMode>,
)
