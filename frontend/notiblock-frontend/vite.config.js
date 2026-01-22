import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(),
  tailwindcss(),
  ],
  server: {
    historyApiFallback: true,
  },
  preview: {
    historyApiFallback: true,
  },
  resolve: {
    alias: {
      '@blockchain': path.resolve(__dirname, '../../blockchain'),
    }
  }
})
