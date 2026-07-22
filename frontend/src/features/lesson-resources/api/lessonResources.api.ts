import client from '@/shared/api/client'
import type {
  ConfirmLessonResourceUploadRequest,
  LessonResourceDownloadResponse,
  LessonResourceDto,
  PresignLessonResourceUploadResponse
} from '@/features/lesson-resources/types/lessonResource.types'

export async function getLessonResources(
  courseId: string,
  lessonId: string
): Promise<LessonResourceDto[]> {
  const response = await client.get<LessonResourceDto[]>(
    `/courses/${courseId}/lessons/${lessonId}/resources`
  )
  return response.data
}

export async function getLessonResourcesForInstructor(
  courseId: string,
  lessonId: string
): Promise<LessonResourceDto[]> {
  const response = await client.get<LessonResourceDto[]>(
    `/courses/${courseId}/lessons/${lessonId}/resources/manage`
  )
  return response.data
}

export async function presignLessonResourceUpload(
  courseId: string,
  lessonId: string,
  contentType: string
): Promise<PresignLessonResourceUploadResponse> {
  const response = await client.post<PresignLessonResourceUploadResponse>(
    `/courses/${courseId}/lessons/${lessonId}/resources/presign`,
    { contentType }
  )
  return response.data
}

export async function confirmLessonResourceUpload(
  courseId: string,
  lessonId: string,
  data: ConfirmLessonResourceUploadRequest
): Promise<string> {
  const response = await client.post<string>(
    `/courses/${courseId}/lessons/${lessonId}/resources/confirm`,
    data
  )
  return response.data
}

export async function deleteLessonResource(
  courseId: string,
  lessonId: string,
  resourceId: string
): Promise<void> {
  await client.delete(`/courses/${courseId}/lessons/${lessonId}/resources/${resourceId}`)
}

export async function getLessonResourceDownloadUrl(
  courseId: string,
  lessonId: string,
  resourceId: string
): Promise<LessonResourceDownloadResponse> {
  const response = await client.get<LessonResourceDownloadResponse>(
    `/courses/${courseId}/lessons/${lessonId}/resources/${resourceId}/download`
  )
  return response.data
}
