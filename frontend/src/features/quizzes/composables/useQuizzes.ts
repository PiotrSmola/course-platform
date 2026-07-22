import { useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import {
  deleteQuiz,
  getLessonQuiz,
  getLessonQuizForInstructor,
  getMyQuizAttempts,
  submitQuizAttempt,
  upsertQuiz
} from '@/features/quizzes/api/quizzes.api'
import type {
  QuizAttemptAnswerInput,
  UpsertQuizRequest
} from '@/features/quizzes/types/quiz.types'
import { queryKeys } from '@/shared/queryKeys'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'

export function useLessonQuiz(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  return useQuery({
    queryKey: computed(() => queryKeys.lessonQuiz(toValue(courseId), toValue(lessonId))),
    queryFn: () => getLessonQuiz(toValue(courseId), toValue(lessonId)),
    enabled: computed(() => !!toValue(courseId) && !!toValue(lessonId))
  })
}

export function useLessonQuizForInstructor(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>,
  enabled: MaybeRefOrGetter<boolean> = true
) {
  return useQuery({
    queryKey: computed(() =>
      queryKeys.lessonQuizManage(toValue(courseId), toValue(lessonId))
    ),
    queryFn: () => getLessonQuizForInstructor(toValue(courseId), toValue(lessonId)),
    enabled: computed(
      () => !!toValue(courseId) && !!toValue(lessonId) && toValue(enabled)
    )
  })
}

export function useMyQuizAttempts(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  return useQuery({
    queryKey: computed(() =>
      queryKeys.lessonQuizAttempts(toValue(courseId), toValue(lessonId))
    ),
    queryFn: () => getMyQuizAttempts(toValue(courseId), toValue(lessonId)),
    enabled: computed(() => !!toValue(courseId) && !!toValue(lessonId))
  })
}

export function useUpsertQuiz(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: UpsertQuizRequest) =>
      upsertQuiz(toValue(courseId), toValue(lessonId), data),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonQuiz(toValue(courseId), toValue(lessonId))
      })
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonQuizManage(toValue(courseId), toValue(lessonId))
      })
      toast.success('Quiz zapisany')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się zapisać quizu')
  })
}

export function useDeleteQuiz(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: () => deleteQuiz(toValue(courseId), toValue(lessonId)),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonQuiz(toValue(courseId), toValue(lessonId))
      })
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonQuizManage(toValue(courseId), toValue(lessonId))
      })
      toast.success('Quiz usunięty')
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się usunąć quizu')
  })
}

export function useSubmitQuizAttempt(
  courseId: MaybeRefOrGetter<string>,
  lessonId: MaybeRefOrGetter<string>
) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (answers: QuizAttemptAnswerInput[]) =>
      submitQuizAttempt(toValue(courseId), toValue(lessonId), answers),
    onSuccess: (result) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.lessonQuizAttempts(toValue(courseId), toValue(lessonId))
      })
      toast.success(
        result.passed
          ? `Zaliczono — ${result.scorePercent}%`
          : `Nie zaliczono — ${result.scorePercent}%`
      )
    },
    onError: (error) => toast.error(getApiErrorMessage(error) || 'Nie udało się oddać quizu')
  })
}
