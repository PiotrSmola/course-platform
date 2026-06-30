import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { getLesson, completeLesson } from '@/features/learning/api/learning.api'
import { queryKeys } from '@/shared/queryKeys'

export function useLesson(courseId: MaybeRefOrGetter<string>, lessonId: MaybeRefOrGetter<string>) {
  return useQuery({
    queryKey: computed(() => queryKeys.lesson(toValue(courseId), toValue(lessonId))),
    queryFn: () => getLesson(toValue(courseId), toValue(lessonId)),
    enabled: computed(() => !!toValue(courseId) && !!toValue(lessonId))
  })
}

export function useCompleteLesson() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ courseId, lessonId }: { courseId: string; lessonId: string }) =>
      completeLesson(courseId, lessonId),
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.lesson(variables.courseId, variables.lessonId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.enrollments() })
      queryClient.invalidateQueries({ queryKey: queryKeys.course(variables.courseId) })
    }
  })
}
