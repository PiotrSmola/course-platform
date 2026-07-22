import { computed } from 'vue'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { joinWaitlist, leaveWaitlist } from '@/features/waitlist/api/waitlist.api'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useJoinWaitlist() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: joinWaitlist,
    onSuccess: (_data, courseId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      toast.success('Zapisano na listę oczekujących')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się dołączyć do listy oczekujących')
    }
  })
}

export function useLeaveWaitlist() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: leaveWaitlist,
    onSuccess: (_data, courseId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      toast.success('Wypisano z listy oczekujących')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się wypisać z listy oczekujących')
    }
  })
}

export function useToggleWaitlist() {
  const joinMutation = useJoinWaitlist()
  const leaveMutation = useLeaveWaitlist()

  function toggle(courseId: string, isOnWaitlist: boolean) {
    if (isOnWaitlist) {
      leaveMutation.mutate(courseId)
    } else {
      joinMutation.mutate(courseId)
    }
  }

  return {
    toggle,
    isPending: computed(() => joinMutation.isPending.value || leaveMutation.isPending.value)
  }
}
