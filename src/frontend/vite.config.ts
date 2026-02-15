import { defineConfig } from 'vite'
import { execSync } from 'child_process'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

// Auto-generate version as yyyy-MM-dd.{git-short-sha}
// Falls back to date-only when git is unavailable (e.g. inside Docker)
let gitSha = 'unknown'
try {
  gitSha = execSync('git rev-parse --short HEAD').toString().trim()
} catch {
  // git not available (e.g. Docker container without .git directory)
}
const buildDate = new Date().toISOString().split('T')[0]
const appVersion = `${buildDate}.${gitSha}`

// https://vite.dev/config/
export default defineConfig({
  define: {
    __APP_VERSION__: JSON.stringify(appVersion),
  },
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
