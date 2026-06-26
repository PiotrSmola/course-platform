import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { getUserProfile, updateUserProfile, deleteUserAccount } from '@/features/profile/api/profile.api'
import { toast } from '@/shared/toast/toast'

export function useProfile() {
  const queryClient = useQueryClient()

  const profileQuery = useQuery({
    queryKey: ['userProfile'],
    queryFn: getUserProfile,
    retry: false
  })

  const updateMutation = useMutation({
    mutationFn: updateUserProfile,
    onSuccess: () => {
      toast.success('Profil został zaktualizowany')
      queryClient.invalidateQueries({ queryKey: ['userProfile'] })
      queryClient.invalidateQueries({ queryKey: ['currentUser'] })
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Nie udało się zaktualizować profilu')
    }
  })

  const deleteMutation = useMutation({
    mutationFn: deleteUserAccount,
    onSuccess: () => {
      toast.success('Konto zostało usunięte')
    },
    onError: (error: any) => {
      toast.error(error.response?.data?.error || 'Nie udało się usunąć konta')
    }
  })

  return {
    profile: profileQuery,
    update: updateMutation,
    deleteAccount: deleteMutation
  }
}
