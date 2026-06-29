import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { getLesson, completeLesson } from '@/features/learning/api/learning.api'
import { queryKeys } from '@/shared/queryKeys'

export function useLesson(courseId: string, lessonId: string) {
  return useQuery({
    queryKey: queryKeys.lesson(courseId, lessonId),
    queryFn: () => getLesson(courseId, lessonId),
    enabled: !!courseId && !!lessonId
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
