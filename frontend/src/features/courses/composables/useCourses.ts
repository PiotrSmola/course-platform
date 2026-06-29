import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { useRouter } from 'vue-router'
import { getCourses, getCourseDetails, createCourse, updateCourse } from '@/features/courses/api/courses.api'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import { queryKeys } from '@/shared/queryKeys'

export function useCourses() {
  const { isLoading, isError, error, data } = useQuery({
    queryKey: queryKeys.courses(),
    queryFn: () => getCourses()
  })

  return { isLoading, isError, error, data }
}

export function useCourseDetails(courseId: string, enabled: boolean = true) {
  return useQuery({
    queryKey: queryKeys.course(courseId),
    queryFn: () => getCourseDetails(courseId),
    enabled: !!courseId && enabled
  })
}

export function useCreateCourse() {
  const router = useRouter()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createCourse,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      toast.success('Kurs został utworzony')
      router.push({ name: 'InstructorDashboard' })
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się utworzyć kursu')
    }
  })
}

export function useUpdateCourse() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: updateCourse,
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.course(variables.id) })
      toast.success('Kurs został zaktualizowany')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować kursu')
    }
  })
}
