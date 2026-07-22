import client from '@/shared/api/client'
import type {
  LessonQuizInstructorDto,
  LessonQuizStudentDto,
  QuizAttemptAnswerInput,
  QuizAttemptListItemDto,
  QuizAttemptResultDto,
  UpsertQuizRequest
} from '@/features/quizzes/types/quiz.types'

export async function getLessonQuiz(
  courseId: string,
  lessonId: string
): Promise<LessonQuizStudentDto | null> {
  const response = await client.get<LessonQuizStudentDto | null>(
    `/courses/${courseId}/lessons/${lessonId}/quiz`
  )
  return response.data
}

export async function getLessonQuizForInstructor(
  courseId: string,
  lessonId: string
): Promise<LessonQuizInstructorDto | null> {
  const response = await client.get<LessonQuizInstructorDto | null>(
    `/courses/${courseId}/lessons/${lessonId}/quiz/manage`
  )
  return response.data
}

export async function upsertQuiz(
  courseId: string,
  lessonId: string,
  data: UpsertQuizRequest
): Promise<string> {
  const response = await client.put<string>(
    `/courses/${courseId}/lessons/${lessonId}/quiz`,
    data
  )
  return response.data
}

export async function deleteQuiz(courseId: string, lessonId: string): Promise<void> {
  await client.delete(`/courses/${courseId}/lessons/${lessonId}/quiz`)
}

export async function submitQuizAttempt(
  courseId: string,
  lessonId: string,
  answers: QuizAttemptAnswerInput[]
): Promise<QuizAttemptResultDto> {
  const response = await client.post<QuizAttemptResultDto>(
    `/courses/${courseId}/lessons/${lessonId}/quiz/attempts`,
    { answers }
  )
  return response.data
}

export async function getMyQuizAttempts(
  courseId: string,
  lessonId: string
): Promise<QuizAttemptListItemDto[]> {
  const response = await client.get<QuizAttemptListItemDto[]>(
    `/courses/${courseId}/lessons/${lessonId}/quiz/attempts/me`
  )
  return response.data
}
