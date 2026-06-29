import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthResponse, CurrentUser } from '@/features/auth/types/auth.types'
import { getCurrentUser } from '@/features/auth/api/auth.api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<CurrentUser | null>(null)
  const isReady = ref(false)
  const bootstrapPromise = ref<Promise<void> | null>(null)

  const isAuthenticated = computed(() => !!token.value)
  const isInstructor = computed(() => user.value?.roles.includes('Instructor') || user.value?.roles.includes('Admin') || false)
  const isAdmin = computed(() => user.value?.roles.includes('Admin') || false)

  function setAuth(data: AuthResponse) {
    token.value = data.token
    user.value = {
      id: data.id,
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      roles: data.roles
    }
    localStorage.setItem('token', data.token)
  }

  function setUser(data: CurrentUser) {
    user.value = data
  }

  function setReady(ready: boolean) {
    isReady.value = ready
  }

  function logout() {
    token.value = null
    user.value = null
    isReady.value = false
    localStorage.removeItem('token')
  }

  async function bootstrap() {
    if (bootstrapPromise.value) return bootstrapPromise.value

    bootstrapPromise.value = (async () => {
      if (!token.value) {
        isReady.value = true
        return
      }

      try {
        const me = await getCurrentUser()
        user.value = me
      } catch {
        logout()
      } finally {
        isReady.value = true
      }
    })()

    return bootstrapPromise.value
  }

  return {
    token,
    user,
    isReady,
    bootstrap,
    isAuthenticated,
    isInstructor,
    isAdmin,
    setAuth,
    setUser,
    setReady,
    logout
  }
})
