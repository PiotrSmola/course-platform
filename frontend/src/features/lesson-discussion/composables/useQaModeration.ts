import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { getModerationQueue } from '@/features/lesson-discussion/api/moderation.api'
import {
  createLessonAnswer,
  deleteLessonAnswer,
  deleteLessonQuestion
} from '@/features/lesson-discussion/api/discussion.api'
import type { ModerationQueueFilters } from '@/features/lesson-discussion/types/moderation.types'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

export function useModerationQueue(filters: MaybeRefOrGetter<ModerationQueueFilters>) {
  return useQuery({
    queryKey: computed(() => queryKeys.qaModeration(toValue(filters))),
    queryFn: () => getModerationQueue(toValue(filters))
  })
}

function invalidateModerationQueue(queryClient: ReturnType<typeof useQueryClient>) {
  queryClient.invalidateQueries({ queryKey: queryKeys.qaModerationAll() })
}

export function useModerationMutations() {
  const queryClient = useQueryClient()

  const replyToQuestion = useMutation({
    mutationFn: ({
      courseId,
      lessonId,
      questionId,
      body
    }: {
      courseId: string
      lessonId: string
      questionId: string
      body: string
    }) => createLessonAnswer(courseId, lessonId, questionId, body),
    onSuccess: (_data, variables) => {
      invalidateModerationQueue(queryClient)
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonDiscussionAll(variables.courseId, variables.lessonId)
      })
      toast.success('Odpowiedź została dodana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać odpowiedzi')
  })

  const removeQuestion = useMutation({
    mutationFn: ({
      courseId,
      lessonId,
      questionId
    }: {
      courseId: string
      lessonId: string
      questionId: string
    }) => deleteLessonQuestion(courseId, lessonId, questionId),
    onSuccess: (_data, variables) => {
      invalidateModerationQueue(queryClient)
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonDiscussionAll(variables.courseId, variables.lessonId)
      })
      toast.success('Pytanie zostało usunięte')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć pytania')
  })

  const removeAnswer = useMutation({
    mutationFn: ({
      courseId,
      lessonId,
      questionId,
      answerId
    }: {
      courseId: string
      lessonId: string
      questionId: string
      answerId: string
    }) => deleteLessonAnswer(courseId, lessonId, questionId, answerId),
    onSuccess: (_data, variables) => {
      invalidateModerationQueue(queryClient)
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonDiscussionAll(variables.courseId, variables.lessonId)
      })
      toast.success('Odpowiedź została usunięta')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć odpowiedzi')
  })

  return { replyToQuestion, removeQuestion, removeAnswer }
}
