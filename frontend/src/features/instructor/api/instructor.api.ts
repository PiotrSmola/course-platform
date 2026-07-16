import client from '@/shared/api/client'
import type { InstructorCourseDto, CourseStatus } from '@/features/courses/types/course.types'

export async function getInstructorCourses(): Promise<InstructorCourseDto[]> {
  const response = await client.get('/courses/instructor/my')
  return response.data
}

export interface InstructorCourseStatsDto {
  id: string
  title: string
  status: CourseStatus
  enrollmentCount: number
  revenue: number
  completionRate: number
}

export interface InstructorDashboardDto {
  totalStudents: number
  totalRevenue: number
  averageCompletionRate: number
  courses: InstructorCourseStatsDto[]
}

export async function getInstructorDashboard(): Promise<InstructorDashboardDto> {
  const response = await client.get('/courses/instructor/dashboard')
  return response.data
}

export interface CreateModuleRequest {
  courseId: string
  title: string
  order: number
}

export interface UpdateModuleRequest {
  courseId: string
  moduleId: string
  title: string
  order: number
}

export interface CreateLessonRequest {
  courseId: string
  moduleId: string
  title: string
  description?: string
  duration: number
  order: number
}

export interface UpdateLessonRequest {
  courseId: string
  moduleId: string
  lessonId: string
  title: string
  description?: string
  duration: number
  order: number
}

export async function createModule(data: CreateModuleRequest): Promise<string> {
  const response = await client.post(`/courses/${data.courseId}/modules`, data)
  return response.data
}

export async function updateModule(data: UpdateModuleRequest): Promise<void> {
  await client.put(`/courses/${data.courseId}/modules/${data.moduleId}`, data)
}

export async function deleteModule(courseId: string, moduleId: string): Promise<void> {
  await client.delete(`/courses/${courseId}/modules/${moduleId}`)
}

export async function createLesson(data: CreateLessonRequest): Promise<string> {
  const response = await client.post(`/courses/${data.courseId}/modules/${data.moduleId}/lessons`, data)
  return response.data
}

export async function updateLesson(data: UpdateLessonRequest): Promise<void> {
  await client.put(`/courses/${data.courseId}/modules/${data.moduleId}/lessons/${data.lessonId}`, data)
}

export interface InitiateLessonVideoUploadResponse {
  objectKey: string
  uploadId: string
  partSizeBytes: number
  maxParts: number
}

export async function initiateLessonVideoUpload(courseId: string, lessonId: string, contentType: string): Promise<InitiateLessonVideoUploadResponse> {
  const response = await client.post(`/courses/${courseId}/lessons/${lessonId}/video/uploads`, {
    courseId,
    lessonId,
    contentType
  })
  return response.data
}

export async function presignLessonVideoPart(courseId: string, lessonId: string, uploadId: string, partNumber: number): Promise<{ url: string }> {
  const response = await client.post(`/courses/${courseId}/lessons/${lessonId}/video/uploads/${uploadId}/parts/presign`, { partNumber })
  return response.data
}

export async function completeLessonVideoUpload(
  courseId: string,
  lessonId: string,
  uploadId: string,
  parts: { partNumber: number; eTag: string }[]
): Promise<void> {
  await client.post(`/courses/${courseId}/lessons/${lessonId}/video/uploads/${uploadId}/complete`, { parts })
}

export async function abortLessonVideoUpload(courseId: string, lessonId: string, uploadId: string): Promise<void> {
  await client.delete(`/courses/${courseId}/lessons/${lessonId}/video/uploads/${uploadId}`)
}

export async function deleteLesson(courseId: string, moduleId: string, lessonId: string): Promise<void> {
  await client.delete(`/courses/${courseId}/modules/${moduleId}/lessons/${lessonId}`)
}
