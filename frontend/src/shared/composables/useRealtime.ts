import { watch } from 'vue'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import type { HubConnection } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/vue-query'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { toast } from '@/shared/toast/toast'
import { queryKeys } from '@/shared/queryKeys'
import type { ReindexJobState } from '@/features/admin/types/search.types'

interface UserNotification {
  type: string
  title: string
  message: string
  link: string | null
}

const hubUrl = `${(import.meta.env.VITE_API_URL || 'http://localhost:8080/api').replace(/\/api\/?$/, '')}/hubs/notifications`

export function useRealtime() {
  const authStore = useAuthStore()
  const queryClient = useQueryClient()
  let connection: HubConnection | null = null

  const start = async () => {
    if (connection) return

    connection = new HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => authStore.token ?? '' })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build()

    connection.on('reindex', (state: ReindexJobState) => {
      queryClient.setQueryData(queryKeys.reindexJob(), state)
    })

    connection.on('notification', (notification: UserNotification) => {
      toast.success(`${notification.title} — ${notification.message}`)

      if (notification.type === 'purchase-completed') {
        queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() })
        queryClient.invalidateQueries({ queryKey: queryKeys.myPurchases() })
      }

      if (notification.type === 'certificate-issued') {
        queryClient.invalidateQueries({ queryKey: queryKeys.myCertificates() })
      }
    })

    try {
      await connection.start()
    } catch {
      const failed = connection
      connection = null
      await failed.stop().catch(() => {})
    }
  }

  const stop = async () => {
    if (!connection) return
    const current = connection
    connection = null
    await current.stop().catch(() => {})
  }

  watch(
    () => authStore.isAuthenticated,
    (authenticated) => {
      if (authenticated) start()
      else stop()
    },
    { immediate: true }
  )
}
