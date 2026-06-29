import client from '@/shared/api/client'
import type { InstructorCourseDto } from '@/features/courses/types/course.types'

export async function getInstructorCourses(): Promise<InstructorCourseDto[]> {
  const response = await client.get('/courses/instructor/my')
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
  videoUrl: string
  duration: number
  order: number
}

export interface UpdateLessonRequest {
  courseId: string
  moduleId: string
  lessonId: string
  title: string
  description?: string
  videoUrl: string
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

export async function deleteLesson(courseId: string, moduleId: string, lessonId: string): Promise<void> {
  await client.delete(`/courses/${courseId}/modules/${moduleId}/lessons/${lessonId}`)
}
