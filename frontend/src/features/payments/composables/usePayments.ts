import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { createCheckoutSession, getPaymentStatus } from '@/features/payments/api/payments.api'
import { PaymentStatus } from '@/features/payments/types/payment.types'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useCreateCheckout() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createCheckoutSession,
    onSuccess: (data, courseId) => {
      if (data.enrolled) {
        queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() })
        queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
        toast.success('Zapisano na kurs')
      } else if (data.redirectUrl) {
        window.location.href = data.redirectUrl
      }
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się rozpocząć płatności')
    }
  })
}

export function usePaymentStatus(sessionId: MaybeRefOrGetter<string>) {
  return useQuery({
    queryKey: computed(() => queryKeys.paymentStatus(toValue(sessionId))),
    queryFn: () => getPaymentStatus(toValue(sessionId)),
    enabled: computed(() => toValue(sessionId).length > 0),
    refetchInterval: (query) =>
      query.state.data?.status === PaymentStatus.Pending ? 2000 : false
  })
}
