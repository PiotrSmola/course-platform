import axios from 'axios'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { router } from '@/app/router'
import { toast } from '@/shared/toast/toast'
import { refreshToken } from '@/features/auth/api/auth.api'

const client = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:8080/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

client.interceptors.request.use((config) => {
  const authStore = useAuthStore()
  if (authStore.token) {
    config.headers.Authorization = `Bearer ${authStore.token}`
  }
  return config
})

let isRefreshing = false
let refreshSubscribers: Array<(token: string) => void> = []

function subscribeTokenRefresh(callback: (token: string) => void) {
  refreshSubscribers.push(callback)
}

function onTokenRefreshed(token: string) {
  refreshSubscribers.forEach((callback) => callback(token))
  refreshSubscribers = []
}

client.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status !== 401 || originalRequest?._retry) {
      return Promise.reject(error)
    }

    const authStore = useAuthStore()
    const storedRefreshToken = authStore.refreshToken

    if (!storedRefreshToken) {
      authStore.logout()
      router.push({ name: 'Login' })
      return Promise.reject(error)
    }

    if (isRefreshing) {
      return new Promise((resolve) => {
        subscribeTokenRefresh((token) => {
          originalRequest.headers.Authorization = `Bearer ${token}`
          resolve(client(originalRequest))
        })
      })
    }

    originalRequest._retry = true
    isRefreshing = true

    try {
      const response = await refreshToken({ refreshToken: storedRefreshToken })
      authStore.setToken(response)
      onTokenRefreshed(response.token)
      originalRequest.headers.Authorization = `Bearer ${response.token}`
      return client(originalRequest)
    } catch {
      authStore.logout()
      router.push({ name: 'Login' })
      toast.error('Session expired. Please log in again.')
      return Promise.reject(error)
    } finally {
      isRefreshing = false
    }
  }
)

export default client
