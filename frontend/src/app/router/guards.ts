import { useAuthStore } from '@/features/auth/stores/auth.store'
import type { RouteLocationNormalized } from 'vue-router'
import { watch } from 'vue'

function waitForAuthReady(authStore: ReturnType<typeof useAuthStore>, timeoutMs = 3000): Promise<void> {
  if (authStore.isReady) return Promise.resolve()
  return new Promise<void>((resolve) => {
    const unwatch = watch(() => authStore.isReady, (ready) => {
      if (ready) {
        unwatch()
        resolve()
      }
    }, { immediate: true })
    setTimeout(() => {
      unwatch()
      resolve()
    }, timeoutMs)
  })
}

export async function authGuard(to: RouteLocationNormalized) {
  const authStore = useAuthStore()
  await authStore.bootstrap()
  if (!authStore.isAuthenticated) {
    return { name: 'Login', query: { redirect: to.fullPath } }
  }
  await waitForAuthReady(authStore)
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
  await waitForAuthReady(authStore)
  if (!authStore.isInstructor) {
    return { name: 'Home' }
  }
}
