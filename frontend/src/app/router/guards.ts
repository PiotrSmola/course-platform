import { useAuthStore } from '@/features/auth/stores/auth.store'
import type { NavigationGuard } from 'vue-router'

export const authGuard: NavigationGuard = (to, _from, next) => {
  const authStore = useAuthStore()
  if (!authStore.isAuthenticated) {
    next({ name: 'Login', query: { redirect: to.fullPath } })
  } else {
    next()
  }
}

export const instructorGuard: NavigationGuard = (_to, _from, next) => {
  const authStore = useAuthStore()
  if (!authStore.isInstructor) {
    next({ name: 'Home' })
  } else {
    next()
  }
}
