import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
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

export function useCourseDetails(
  courseId: MaybeRefOrGetter<string>,
  enabled: MaybeRefOrGetter<boolean> = true
) {
  return useQuery({
    queryKey: computed(() => queryKeys.course(toValue(courseId))),
    queryFn: () => getCourseDetails(toValue(courseId)),
    enabled: computed(() => !!toValue(courseId) && toValue(enabled))
  })
}

export function useCreateCourse() {
  const router = useRouter()
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: createCourse,
    onSuccess: (courseId) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.courses() })
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorCourses() })
      toast.success('Kurs został utworzony')
      router.push({ name: 'EditCourse', params: { id: courseId } })
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
      queryClient.invalidateQueries({ queryKey: queryKeys.coursesBrowseAll() })
      queryClient.invalidateQueries({ queryKey: queryKeys.course(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.instructorCourses() })
      toast.success('Kurs został zaktualizowany')
    },
    onError: (error) => {
      toast.error(getApiErrorMessage(error) || 'Nie udało się zaktualizować kursu')
    }
  })
}
