import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5189',
        changeOrigin: true,
        secure: false,
        // remove rewrite so /api stays intact when forwarded
        // rewrite: undefined
      }
    }
  }
})