import { useMutation, useQuery } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { login, register, getCurrentUser } from '@/features/auth/api/auth.api'
import { toast } from 'vue3-toastify'
import { watch } from 'vue'

export function useAuth() {
  const authStore = useAuthStore()
  const router = useRouter()

  const loginMutation = useMutation({
    mutationFn: login,
    onSuccess: (data) => {
      authStore.setAuth(data)
      toast.success('Logged in successfully')
      router.push({ name: 'Home' })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Login failed')
    }
  })

  const registerMutation = useMutation({
    mutationFn: register,
    onSuccess: (data) => {
      authStore.setAuth(data)
      toast.success('Registered successfully')
      router.push({ name: 'Home' })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Registration failed')
    }
  })

  const currentUserQuery = useQuery({
    queryKey: ['currentUser'],
    queryFn: getCurrentUser,
    enabled: authStore.isAuthenticated,
    retry: false
  })

  watch(currentUserQuery.data, (data) => {
    if (data) authStore.setUser(data)
  })

  watch(currentUserQuery.error, (error) => {
    if (error) authStore.logout()
  })

  const logout = () => {
    authStore.logout()
    toast.info('Logged out')
    router.push({ name: 'Home' })
  }

  return {
    login: loginMutation,
    register: registerMutation,
    currentUser: currentUserQuery,
    logout
  }
}
