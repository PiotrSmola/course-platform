import client from '@/shared/api/client'
import type { CourseListDto } from '@/features/courses/types/course.types'

export async function getPopularCourses(): Promise<CourseListDto[]> {
  const response = await client.get('/courses/popular')
  return response.data
}
