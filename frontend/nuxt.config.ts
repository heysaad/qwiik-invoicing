import tailwindcss from '@tailwindcss/vite';

export default defineNuxtConfig({
  compatibilityDate: '2026-07-06',
  devtools: { enabled: true },
  css: ['~/assets/css/main.css'],
  runtimeConfig: {
    apiOrigin: process.env.NUXT_API_ORIGIN || 'http://localhost:5156',
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE || '/api'
    }
  },
  vite: {
    plugins: [tailwindcss() as any]
  },
  typescript: {
    typeCheck: false
  },
  app: {
    head: {
      htmlAttrs: {
        lang: 'en',
        'data-theme': 'light'
      },
      title: 'Qwiik Invoicing',
      meta: [{ name: 'viewport', content: 'width=device-width, initial-scale=1' }]
    }
  }
});
