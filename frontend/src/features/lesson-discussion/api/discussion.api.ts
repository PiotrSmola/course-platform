import client from '@/shared/api/client'
import type { LessonDiscussionDto } from '@/features/lesson-discussion/types/discussion.types'

export async function getLessonDiscussion(
  courseId: string,
  lessonId: string,
  pageNumber = 1,
  pageSize = 20
): Promise<LessonDiscussionDto> {
  const response = await client.get<LessonDiscussionDto>(
    `/courses/${courseId}/lessons/${lessonId}/discussion`,
    { params: { pageNumber, pageSize } }
  )
  return response.data
}

export async function createLessonQuestion(
  courseId: string,
  lessonId: string,
  body: string
): Promise<string> {
  const response = await client.post<string>(
    `/courses/${courseId}/lessons/${lessonId}/discussion/questions`,
    { body }
  )
  return response.data
}

export async function createLessonAnswer(
  courseId: string,
  lessonId: string,
  questionId: string,
  body: string
): Promise<string> {
  const response = await client.post<string>(
    `/courses/${courseId}/lessons/${lessonId}/discussion/questions/${questionId}/answers`,
    { body }
  )
  return response.data
}

export async function deleteLessonQuestion(
  courseId: string,
  lessonId: string,
  questionId: string
): Promise<void> {
  await client.delete(
    `/courses/${courseId}/lessons/${lessonId}/discussion/questions/${questionId}`
  )
}

export async function deleteLessonAnswer(
  courseId: string,
  lessonId: string,
  questionId: string,
  answerId: string
): Promise<void> {
  await client.delete(
    `/courses/${courseId}/lessons/${lessonId}/discussion/questions/${questionId}/answers/${answerId}`
  )
}
