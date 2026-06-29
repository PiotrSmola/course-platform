import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { login, register, getCurrentUser } from '@/features/auth/api/auth.api'
import { toast } from '@/shared/toast/toast'
import { watch, computed } from 'vue'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()
  const queryClient = useQueryClient()

  const loginMutation = useMutation({
    mutationFn: login,
    onSuccess: async (data) => {
      authStore.setAuth(data)
      await queryClient.invalidateQueries({ queryKey: queryKeys.currentUser() })
      toast.success('Zalogowano pomyślnie')
      const redirect = router.currentRoute.value.query.redirect as string | undefined
      router.push(redirect || { name: 'Home' })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się zalogować')
    }
  })

  const registerMutation = useMutation({
    mutationFn: register,
    onSuccess: async (data) => {
      authStore.setAuth(data)
      await queryClient.invalidateQueries({ queryKey: queryKeys.currentUser() })
      toast.success('Konto zostało utworzone')
      const redirect = router.currentRoute.value.query.redirect as string | undefined
      router.push(redirect || { name: 'Home' })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się utworzyć konta')
    }
  })

  const currentUserQuery = useQuery({
    queryKey: queryKeys.currentUser(),
    queryFn: getCurrentUser,
    enabled: computed(() => authStore.isAuthenticated),
    retry: false
  })

  if (!authStore.isAuthenticated) {
    authStore.setReady(true)
  } else {
    watch(
      () => currentUserQuery.isFetched.value,
      (fetched) => {
        if (fetched) {
          if (currentUserQuery.data.value) {
            authStore.setUser(currentUserQuery.data.value)
          }
          if (currentUserQuery.error.value) {
            authStore.logout()
          }
          authStore.setReady(true)
        }
      },
      { immediate: true }
    )
  }

  const logout = () => {
    authStore.logout()
    toast.info('Wylogowano')
    router.push({ name: 'Home' })
  }

  return {
    login: loginMutation,
    register: registerMutation,
    currentUser: currentUserQuery,
    logout
  }
}
