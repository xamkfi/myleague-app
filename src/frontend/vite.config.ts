import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'


// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    host: '0.0.0.0',
    port: 5173,
    watch: {
      usePolling: true,  // Required for Docker volume mounts
      interval: 100,
    },
    proxy: {
      '/api': {
        target: 'http://localhost:8080',
        changeOrigin: true,
        secure: false
      }
    }
  },
  resolve: {
    alias: {
      '@variables': path.resolve(__dirname, 'src/styles/variables.scss'),
      '@common': path.resolve(__dirname, 'src/styles/common.css'),
      '@components': path.resolve(__dirname, 'src/components'),
    },
  }
})
