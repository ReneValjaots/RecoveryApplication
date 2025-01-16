// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2024-04-03',
  devtools: { enabled: true },
  // devServer: {
  //   https: {
  //     key: './server.key', 
  //     cert: './server.crt'
  //   }
  // },
  modules: ['@nuxt/ui', '@pinia/nuxt',],
  imports: {dirs: ['types/*.ts']},
  runtimeConfig: {
    public: { apiUrl: "https://localhost:5129/api/" },
  },
});