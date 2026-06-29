import client from '@/shared/api/client'
import type { CoursesVm, CourseStatus } from '@/features/courses/types/course.types'

export interface AdminUserDto {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
  isLockedOut: boolean
}

export async function getAdminUsers(): Promise<AdminUserDto[]> {
  const response = await client.get('/admin/users')
  return response.data
}

export async function assignUserRole(userId: string, role: string): Promise<void> {
  await client.post(`/admin/users/${userId}/roles`, { userId, role })
}

export async function getAdminCourses(status?: CourseStatus): Promise<CoursesVm> {
  const params = new URLSearchParams()
  if (status !== undefined) params.append('status', status.toString())
  params.append('pageSize', '50')
  const response = await client.get(`/admin/courses?${params.toString()}`)
  return response.data
}

export async function updateCourseStatus(courseId: string, status: CourseStatus): Promise<void> {
  await client.put(`/admin/courses/${courseId}/status`, { courseId, status })
}

export async function deleteReview(reviewId: string): Promise<void> {
  await client.delete(`/admin/reviews/${reviewId}`)
}
