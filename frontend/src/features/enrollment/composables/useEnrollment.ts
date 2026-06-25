import { ref, watch } from 'vue'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { getMyEnrollments, enroll } from '@/features/enrollment/api/enrollment.api'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { toast } from 'vue3-toastify'
import type { EnrollmentDto } from '@/features/enrollment/types/enrollment.types'

export function useEnrollments() {
  const authStore = useAuthStore()
  const data = ref<EnrollmentDto[]>([])
  const isLoading = ref(false)
  const error = ref<unknown>(null)

  async function load() {
    isLoading.value = true
    error.value = null
    try {
      data.value = await getMyEnrollments()
    } catch (e) {
      error.value = e
    } finally {
      isLoading.value = false
    }
  }

  watch(
    () => authStore.token,
    (token) => {
      if (token) load()
      else { data.value = [] }
    },
    { immediate: true }
  )

  return { data, isLoading, error, refetch: load }
}

export function useEnroll() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: enroll,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['enrollments'] })
      toast.success('Zapisano na kurs')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Nie udało się zapisać na kurs')
    }
  })
}