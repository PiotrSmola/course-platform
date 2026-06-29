import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { getUserProfile, updateUserProfile, deleteUserAccount } from '@/features/profile/api/profile.api'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useProfile() {
  const queryClient = useQueryClient()

  const profileQuery = useQuery({
    queryKey: queryKeys.userProfile(),
    queryFn: getUserProfile,
    retry: false
  })

  const updateMutation = useMutation({
    mutationFn: updateUserProfile,
    onSuccess: () => {
      toast.success('Profil został zaktualizowany')
      queryClient.invalidateQueries({ queryKey: queryKeys.userProfile() })
      queryClient.invalidateQueries({ queryKey: queryKeys.currentUser() })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować profilu')
    }
  })

  const deleteMutation = useMutation({
    mutationFn: deleteUserAccount,
    onSuccess: () => {
      toast.success('Konto zostało usunięte')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć konta')
    }
  })

  return {
    profile: profileQuery,
    update: updateMutation,
    deleteAccount: deleteMutation
  }
}
