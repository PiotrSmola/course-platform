import { useAuthStore } from '@/features/auth/stores/auth.store'
import type { RouteLocationNormalized } from 'vue-router'

export function authGuard(to: RouteLocationNormalized) {
  const authStore = useAuthStore()
  if (!authStore.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }
}

export function instructorGuard() {
  const authStore = useAuthStore()
  if (!authStore.isInstructor) {
    return { name: 'Home' }
  }
}
