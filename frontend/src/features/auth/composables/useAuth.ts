import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { login, register } from '@/features/auth/api/auth.api'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()
  const queryClient = useQueryClient()

  async function invalidateAuthDependentQueries() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() }),
      queryClient.invalidateQueries({ queryKey: queryKeys.userProfile() }),
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorCourses() }),
      queryClient.invalidateQueries({ queryKey: queryKeys.adminUsers() }),
      queryClient.invalidateQueries({ queryKey: queryKeys.adminCourses() })
    ])
  }

  const loginMutation = useMutation({
    mutationFn: login,
    onSuccess: async (data) => {
      authStore.setAuth(data)
      await invalidateAuthDependentQueries()
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
      await invalidateAuthDependentQueries()
      toast.success('Konto zostało utworzone')
      const redirect = router.currentRoute.value.query.redirect as string | undefined
      router.push(redirect || { name: 'Home' })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się utworzyć konta')
    }
  })

  const logout = () => {
    authStore.logout()
    queryClient.clear()
    toast.info('Wylogowano')
    router.push({ name: 'Home' })
  }

  return {
    login: loginMutation,
    register: registerMutation,
    logout
  }
}
