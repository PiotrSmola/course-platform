import { useAuthStore } from '@/features/auth/stores/auth.store'
import type { RouteLocationNormalized } from 'vue-router'

export async function authGuard(to: RouteLocationNormalized) {
  const authStore = useAuthStore()
  await authStore.bootstrap()
  if (!authStore.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }
}

export async function instructorGuard(to: RouteLocationNormalized) {
  const authStore = useAuthStore()
  await authStore.bootstrap()
  if (!authStore.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }
  if (!authStore.isInstructor) {
    return { name: 'Home' }
  }
}

export async function adminGuard(to: RouteLocationNormalized) {
  const authStore = useAuthStore()
  await authStore.bootstrap()
  if (!authStore.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }
  if (!authStore.isAdmin) {
    return { name: 'Home' }
  }
}
