import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import {
  createLessonAnswer,
  createLessonQuestion,
  deleteLessonAnswer,
  deleteLessonQuestion,
  getLessonDiscussion
} from '@/features/lesson-discussion/api/discussion.api'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

export function useLessonDiscussion(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>,
  pageNumber: MaybeRefOrGetter<number> = 1
) {
  return useQuery({
    queryKey: computed(() =>
      queryKeys.lessonDiscussion(toValue(courseId), toValue(lessonId), toValue(pageNumber))
    ),
    queryFn: () =>
      getLessonDiscussion(toValue(courseId), toValue(lessonId), toValue(pageNumber)),
    enabled: computed(() => !!toValue(courseId) && !!toValue(lessonId))
  })
}

function invalidateDiscussion(
  queryClient: ReturnType<typeof useQueryClient>,
  courseId: string,
  lessonId: string
) {
  queryClient.invalidateQueries({
    queryKey: queryKeys.lessonDiscussionAll(courseId, lessonId)
  })
}

export function useCreateLessonQuestion(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (body: string) =>
      createLessonQuestion(toValue(courseId), toValue(lessonId), body),
    onSuccess: () => {
      invalidateDiscussion(queryClient, toValue(courseId), toValue(lessonId))
      toast.success('Pytanie zostało dodane')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać pytania')
  })
}

export function useCreateLessonAnswer(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ questionId, body }: { questionId: string; body: string }) =>
      createLessonAnswer(toValue(courseId), toValue(lessonId), questionId, body),
    onSuccess: () => {
      invalidateDiscussion(queryClient, toValue(courseId), toValue(lessonId))
      toast.success('Odpowiedź została dodana')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się dodać odpowiedzi')
  })
}

export function useDeleteLessonQuestion(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (questionId: string) =>
      deleteLessonQuestion(toValue(courseId), toValue(lessonId), questionId),
    onSuccess: () => {
      invalidateDiscussion(queryClient, toValue(courseId), toValue(lessonId))
      toast.success('Pytanie zostało usunięte')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć pytania')
  })
}

export function useDeleteLessonAnswer(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ questionId, answerId }: { questionId: string; answerId: string }) =>
      deleteLessonAnswer(toValue(courseId), toValue(lessonId), questionId, answerId),
    onSuccess: () => {
      invalidateDiscussion(queryClient, toValue(courseId), toValue(lessonId))
      toast.success('Odpowiedź została usunięta')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć odpowiedzi')
  })
}
