import client from '@/shared/api/client'
import type { LessonDto, PresignedUrlDto } from '@/features/courses/types/course.types'

export async function getLesson(courseId: string, lessonId: string): Promise<LessonDto> {
  const response = await client.get(`/courses/${courseId}/lessons/${lessonId}`)
  return response.data
}

export async function completeLesson(courseId: string, lessonId: string): Promise<void> {
  await client.post(`/courses/${courseId}/lessons/${lessonId}/complete`)
}

export async function getLessonVideoUrl(courseId: string, lessonId: string): Promise<PresignedUrlDto> {
  const response = await client.get(`/courses/${courseId}/lessons/${lessonId}/video`)
  return response.data
}
