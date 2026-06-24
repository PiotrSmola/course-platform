import { useQuery, useMutation } from '@tanstack/vue-query'
import { getLesson, completeLesson } from '@/features/learning/api/learning.api'

export function useLesson(courseId: string, lessonId: string) {
  return useQuery({
    queryKey: ['lesson', courseId, lessonId],
    queryFn: () => getLesson(courseId, lessonId),
    enabled: !!courseId && !!lessonId
  })
}

export function useCompleteLesson() {
  return useMutation({
    mutationFn: ({ courseId, lessonId }: { courseId: string; lessonId: string }) =>
      completeLesson(courseId, lessonId)
  })
}
