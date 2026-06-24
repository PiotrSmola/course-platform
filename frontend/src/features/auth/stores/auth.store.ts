import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { AuthResponse, CurrentUser } from '@/features/auth/types/auth.types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const user = ref<CurrentUser | null>(null)

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

  function logout() {
    token.value = null
    user.value = null
    localStorage.removeItem('token')
  }

  return {
    token,
    user,
    isAuthenticated,
    isInstructor,
    isAdmin,
    setAuth,
    setUser,
    logout
  }
})
