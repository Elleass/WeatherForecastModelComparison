import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        // target: 'http://localhost:5189',
        target: 'http://192.168.0.184:5000',
        changeOrigin: true,
        secure: false,
        // remove rewrite so /api stays intact when forwarded
        // rewrite: undefined
      }
    }
  }
})