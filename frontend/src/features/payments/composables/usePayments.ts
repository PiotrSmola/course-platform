import { computed, readonly, ref, toValue, type MaybeRefOrGetter } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import {
  createBillingPortalSession,
  createCheckoutSession,
  createSubscriptionCheckoutSession,
  getMyPurchases,
  getMyPaymentHistory,
  getMySubscription,
  getPaymentStatus,
  getSubscriptionOffer,
  previewCoupon
} from '@/features/payments/api/payments.api'
import { PaymentStatus, SubscriptionStatus } from '@/features/payments/types/payment.types'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useCreateCheckout() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ courseId, couponCode }: { courseId: string; couponCode?: string | null }) =>
      createCheckoutSession(courseId, couponCode),
    onSuccess: (data, variables) => {
      if (data.enrolled) {
        queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() })
        queryClient.invalidateQueries({ queryKey: queryKeys.course(variables.courseId) })
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

export function usePreviewCoupon() {
  return useMutation({
    mutationFn: ({ courseId, couponCode }: { courseId: string; couponCode: string }) =>
      previewCoupon(courseId, couponCode),
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nieprawidłowy kod rabatowy')
    }
  })
}

export function useCreateSubscriptionCheckout() {
  return useMutation({
    mutationFn: createSubscriptionCheckoutSession,
    onSuccess: (data) => {
      window.location.href = data.redirectUrl
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się rozpocząć subskrypcji')
    }
  })
}

export function useCreateBillingPortalSession() {
  return useMutation({
    mutationFn: createBillingPortalSession,
    onSuccess: (data) => {
      window.location.href = data.redirectUrl
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się otworzyć panelu rozliczeń')
    }
  })
}

export function useMyPurchases(limit = 5) {
  return useQuery({
    queryKey: queryKeys.myPurchases(),
    queryFn: () => getMyPurchases(limit)
  })
}

export function useMyPaymentHistory(limit = 5) {
  return useQuery({
    queryKey: queryKeys.paymentHistory(),
    queryFn: () => getMyPaymentHistory(limit)
  })
}

const MAX_POLL_ATTEMPTS = 30
const POLL_INTERVAL_MS = 2000

export function usePaymentStatus(sessionId: MaybeRefOrGetter<string>) {
  const queryClient = useQueryClient()
  const isPollingTimeout = ref(false)
  let attempts = 0

  const query = useQuery({
    queryKey: computed(() => queryKeys.paymentStatus(toValue(sessionId))),
    queryFn: () => getPaymentStatus(toValue(sessionId)),
    enabled: computed(() => toValue(sessionId).length > 0),
    refetchInterval: (q) => {
      if (isPollingTimeout.value) return false
      if (q.state.data?.status !== PaymentStatus.Pending) return false
      if (attempts >= MAX_POLL_ATTEMPTS) {
        isPollingTimeout.value = true
        return false
      }
      attempts++
      return POLL_INTERVAL_MS
    }
  })

  const refetch = async () => {
    attempts = 0
    isPollingTimeout.value = false
    return query.refetch()
  }

  return {
    ...query,
    isPollingTimeout: readonly(isPollingTimeout),
    refetch
  }
}

export function useMySubscription(
  enabled: MaybeRefOrGetter<boolean> = true,
  pollUntilResolved = false
) {
  const isPollingTimeout = ref(false)
  let attempts = 0

  const query = useQuery({
    queryKey: queryKeys.mySubscription(),
    queryFn: getMySubscription,
    enabled: computed(() => toValue(enabled)),
    refetchInterval: (q) => {
      if (!pollUntilResolved || isPollingTimeout.value) return false
      const data = q.state.data
      if (data?.hasActiveAccess) return false
      if (data?.status === SubscriptionStatus.Canceled) return false
      if (attempts >= MAX_POLL_ATTEMPTS) {
        isPollingTimeout.value = true
        return false
      }
      attempts++
      return POLL_INTERVAL_MS
    }
  })

  const refetch = async () => {
    attempts = 0
    isPollingTimeout.value = false
    return query.refetch()
  }

  return {
    ...query,
    isPollingTimeout: readonly(isPollingTimeout),
    refetch
  }
}

export function useSubscriptionOffer() {
  return useQuery({
    queryKey: queryKeys.subscriptionOffer(),
    queryFn: getSubscriptionOffer
  })
}
