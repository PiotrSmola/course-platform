import { computed, onScopeDispose, ref, watch } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import {
  getReindexJob,
  getSearchStats,
  startReindex
} from '@/features/admin/api/admin.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import type { ReindexJobState } from '@/features/admin/types/search.types'

const POLLING_INTERVAL_MS = 500

export function useSearchStats() {
  return useQuery({
    queryKey: queryKeys.searchStats(),
    queryFn: getSearchStats,
    refetchInterval: POLLING_INTERVAL_MS
  })
}

export function useReindexJob() {
  const queryClient = useQueryClient()
  const previousLogsLength = ref(0)
  const newLogCount = ref(0)

  const jobQuery = useQuery({
    queryKey: queryKeys.reindexJob(),
    queryFn: getReindexJob,
    refetchInterval: () => POLLING_INTERVAL_MS,
    refetchIntervalInBackground: false
  })

  const job = computed<ReindexJobState | null>(() => jobQuery.data.value ?? null)
  const isRunning = computed(() => job.value?.status === 'Running')

  watch(
    () => job.value?.logs.length ?? 0,
    (length) => {
      if (length > previousLogsLength.value) {
        newLogCount.value += length - previousLogsLength.value
      }
      previousLogsLength.value = length
    },
    { immediate: true }
  )

  const startMutation = useMutation({
    mutationFn: startReindex,
    onSuccess: (result) => {
      queryClient.setQueryData(queryKeys.reindexJob(), result.state)
      queryClient.invalidateQueries({ queryKey: queryKeys.searchStats() })
      previousLogsLength.value = 0
      newLogCount.value = 0
      if (result.alreadyRunning) {
        toast.info('Reindex już działa — obserwuj postęp poniżej.')
      } else {
        toast.info('Rozpoczęto reindeksowanie.')
      }
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się uruchomić reindexu')
  })

  onScopeDispose(() => {
    previousLogsLength.value = 0
    newLogCount.value = 0
  })

  return {
    job,
    isRunning,
    jobQuery,
    start: startMutation,
    newLogCount
  }
}