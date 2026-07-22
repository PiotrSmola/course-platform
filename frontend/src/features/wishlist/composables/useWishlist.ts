import { computed } from 'vue'
import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { getMyWishlist, addToWishlist, removeFromWishlist } from '@/features/wishlist/api/wishlist.api'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useWishlist() {
  const authStore = useAuthStore()
  return useQuery({
    queryKey: queryKeys.wishlist(),
    queryFn: getMyWishlist,
    enabled: computed(() => authStore.isAuthenticated)
  })
}

export function useAddToWishlist() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: addToWishlist,
    onSuccess: (_data, courseId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.wishlist() })
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      toast.success('Dodano do listy życzeń')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się dodać do listy życzeń')
    }
  })
}

export function useRemoveFromWishlist() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: removeFromWishlist,
    onSuccess: (_data, courseId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.wishlist() })
      queryClient.invalidateQueries({ queryKey: queryKeys.course(courseId) })
      toast.success('Usunięto z listy życzeń')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć z listy życzeń')
    }
  })
}

export function useToggleWishlist() {
  const addMutation = useAddToWishlist()
  const removeMutation = useRemoveFromWishlist()

  function toggle(courseId: string, isOnWishlist: boolean) {
    if (isOnWishlist) {
      removeMutation.mutate(courseId)
    } else {
      addMutation.mutate(courseId)
    }
  }

  return {
    toggle,
    isPending: computed(() => addMutation.isPending.value || removeMutation.isPending.value)
  }
}
