import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

// Configuração do Vite para desenvolvimento e build
// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/test/setup.ts',
    css: true,
  },
})
