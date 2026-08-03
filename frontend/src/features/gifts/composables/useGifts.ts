import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { createGiftCheckoutSession, redeemGiftCode, type CreateGiftCheckoutRequest } from '@/features/gifts/api/gifts.api'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useCreateGiftCheckout() {
  return useMutation({
    mutationFn: (request: CreateGiftCheckoutRequest) => createGiftCheckoutSession(request),
    onSuccess: (data) => {
      window.location.href = data.redirectUrl
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udalo sie rozpoczac zakupu prezentu')
    }
  })
}
export function useRedeemGiftCode() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: redeemGiftCode,
    onSuccess: async (data) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.course(data.courseId) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.myPurchases() }),
        queryClient.invalidateQueries({ queryKey: queryKeys.paymentHistory() })
      ])
      toast.success('Kod prezentowy zostal zrealizowany')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udalo sie zrealizowac kodu prezentowego')
    }
  })
}
