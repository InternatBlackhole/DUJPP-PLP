import { fileURLToPath, URL } from 'node:url'

import { defineConfig, HmrOptions, UserConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'

const confObj: UserConfig = {
  plugins: [
    vue(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    cors: {
      origin: '*',
      methods: ['GET', 'HEAD', 'PUT', 'PATCH', 'POST', 'DELETE', 'OPTIONS'],
      allowedHeaders: ['Content-Type', 'Authorization', 'X-Requested-With'],
      exposedHeaders: ['Content-Range', 'X-Content-Range'],
      credentials: true,
      maxAge: 3600
    },
  }
}

if(process.env.VITE_HOST) {
  const [host , port] = process.env.VITE_HOST.split(":")
  console.info("VITE_HOST: " + process.env.VITE_HOST)
  confObj.server!.hmr = {
    protocol: "ws",
    host,
    port: Number.parseInt(port) || undefined
  } satisfies HmrOptions
  console.info("hmr: " + JSON.stringify(confObj.server?.hmr) )
}

// https://vite.dev/config/
export default defineConfig(confObj)
