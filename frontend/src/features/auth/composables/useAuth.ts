import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import {
  login,
  register,
  logout as logoutRequest,
  forgotPassword,
  resetPassword
} from '@/features/auth/api/auth.api'
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
    onSuccess: (data) => {
      toast.success(data.message || 'Konto utworzone. Sprawdź skrzynkę email.')
      const redirect = router.currentRoute.value.query.redirect as string | undefined
      router.push({ name: 'Login', query: redirect ? { redirect } : {} })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się utworzyć konta')
    }
  })

  const forgotPasswordMutation = useMutation({
    mutationFn: forgotPassword,
    onSuccess: () => {
      toast.success('Jeśli konto istnieje, wysłaliśmy email')
    },
    onError: () => {
      toast.success('Jeśli konto istnieje, wysłaliśmy email')
    }
  })

  const resetPasswordMutation = useMutation({
    mutationFn: resetPassword,
    onSuccess: () => {
      toast.success('Hasło zostało zmienione')
      router.push({ name: 'Login' })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się zmienić hasła')
    }
  })

  const logout = async () => {
    const token = authStore.refreshToken
    if (token) {
      try {
        await logoutRequest(token)
      } catch {
        // Server-side revocation is best-effort; always clear local state regardless.
      }
    }
    authStore.logout()
    queryClient.clear()
    toast.info('Wylogowano')
    router.push({ name: 'Home' })
  }

  return {
    login: loginMutation,
    register: registerMutation,
    forgotPassword: forgotPasswordMutation,
    resetPassword: resetPasswordMutation,
    logout
  }
}
