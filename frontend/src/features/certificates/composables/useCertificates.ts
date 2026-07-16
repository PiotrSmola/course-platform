import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { useMutation, useQuery } from '@tanstack/vue-query'
import {
  getCertificateDownloadUrl,
  getMyCertificates,
  verifyCertificate
} from '@/features/certificates/api/certificates.api'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useMyCertificates() {
  const authStore = useAuthStore()
  return useQuery({
    queryKey: queryKeys.myCertificates(),
    queryFn: getMyCertificates,
    enabled: computed(() => authStore.isAuthenticated)
  })
}

export function useCertificateVerification(number: MaybeRefOrGetter<string>) {
  return useQuery({
    queryKey: computed(() => queryKeys.certificateVerification(toValue(number))),
    queryFn: () => verifyCertificate(toValue(number)),
    enabled: computed(() => toValue(number).length > 0),
    retry: false
  })
}

export function useDownloadCertificate() {
  return useMutation({
    mutationFn: getCertificateDownloadUrl,
    onSuccess: (data) => {
      window.open(data.url, '_blank', 'noopener')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się pobrać certyfikatu')
    }
  })
}
