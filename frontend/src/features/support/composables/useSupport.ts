import { useMutation } from '@tanstack/vue-query'
import { sendSupportMessage } from '@/features/support/api/support.api'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { toast } from '@/shared/toast/toast'

export function useSendSupportMessage() {
  return useMutation({
    mutationFn: sendSupportMessage,
    onSuccess: () => {
      toast.success('Wiadomość została wysłana. Odpowiemy tak szybko, jak to możliwe.')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się wysłać wiadomości')
    }
  })
}
