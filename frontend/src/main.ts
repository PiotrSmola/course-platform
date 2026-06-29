import { createApp } from 'vue'
import { createPinia } from 'pinia'
import { VueQueryPlugin } from '@tanstack/vue-query'
import App from './App.vue'
import router from './app/router'
import './assets/styles/main.scss'
import 'vue3-toastify/dist/index.css'
import { useAuthStore } from '@/features/auth/stores/auth.store'

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)
app.use(router)
app.use(VueQueryPlugin)

app.provide('toastContainerClass', 'liquid-toast-container')

await useAuthStore(pinia).bootstrap()

app.mount('#app')
