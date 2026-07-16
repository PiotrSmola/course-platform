import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthResponse, CurrentUser } from '@/features/auth/types/auth.types'
import { getCurrentUser } from '@/features/auth/api/auth.api'
import { hasRole, normalizeRoles } from '@/features/auth/utils/roles'

function toCurrentUser(data: AuthResponse | CurrentUser): CurrentUser {
  const raw = data as AuthResponse & { Roles?: string[] }
  return {
    id: data.id,
    email: data.email,
    firstName: data.firstName,
    lastName: data.lastName,
    roles: normalizeRoles(raw.roles ?? raw.Roles)
  }
}

const TOKEN_KEY = 'token'
const REFRESH_TOKEN_KEY = 'refreshToken'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const refreshToken = ref<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY))
  const user = ref<CurrentUser | null>(null)
  const isReady = ref(false)
  const bootstrapPromise = ref<Promise<void> | null>(null)

  const isAuthenticated = computed(() => !!token.value)
  const isInstructor = computed(() => hasRole(user.value?.roles, 'Instructor'))
  const isAdmin = computed(() => hasRole(user.value?.roles, 'Admin'))

  function setAuth(data: AuthResponse) {
    token.value = data.token
    refreshToken.value = data.refreshToken
    user.value = toCurrentUser(data)
    isReady.value = true
    bootstrapPromise.value = null
    localStorage.setItem(TOKEN_KEY, data.token)
    localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)
  }

  function setToken(data: AuthResponse) {
    token.value = data.token
    refreshToken.value = data.refreshToken
    localStorage.setItem(TOKEN_KEY, data.token)
    localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)
  }

  function setUser(data: CurrentUser) {
    user.value = toCurrentUser(data)
  }

  function logout() {
    token.value = null
    refreshToken.value = null
    user.value = null
    isReady.value = false
    bootstrapPromise.value = null
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
  }

  async function refreshUser() {
    if (!token.value) return
    try {
      const me = await getCurrentUser()
      user.value = me
    } catch {
      logout()
    }
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
    refreshToken,
    user,
    isReady,
    bootstrap,
    refreshUser,
    isAuthenticated,
    isInstructor,
    isAdmin,
    setAuth,
    setToken,
    setUser,
    logout
  }
})
