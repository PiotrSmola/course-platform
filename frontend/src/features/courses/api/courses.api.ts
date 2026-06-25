import client from '@/shared/api/client'
import type { CoursesVm, CourseDetailsDto, CourseLevel, CourseStatus } from '@/features/courses/types/course.types'

export interface CoursesFilter {
  searchTerm?: string
  level?: CourseLevel
  status?: CourseStatus
  sortBy?: string
  minPrice?: number
  maxPrice?: number
  pageNumber?: number
  pageSize?: number
}

export async function getCourses(filter: CoursesFilter = {}): Promise<CoursesVm> {
  const params = new URLSearchParams()
  if (filter.searchTerm) params.append('searchTerm', filter.searchTerm)
  if (filter.level !== undefined) params.append('level', filter.level.toString())
  if (filter.status !== undefined) params.append('status', filter.status.toString())
  if (filter.sortBy) params.append('sortBy', filter.sortBy)
  if (filter.minPrice !== undefined) params.append('minPrice', filter.minPrice.toString())
  if (filter.maxPrice !== undefined) params.append('maxPrice', filter.maxPrice.toString())
  if (filter.pageNumber !== undefined) params.append('pageNumber', filter.pageNumber.toString())
  if (filter.pageSize !== undefined) params.append('pageSize', filter.pageSize.toString())
  
  const response = await client.get(`/courses?${params.toString()}`)
  return response.data
}

export async function getCourseDetails(id: string): Promise<CourseDetailsDto> {
  const response = await client.get(`/courses/${id}`)
  return response.data
}

export interface CreateCourseRequest {
  title: string
  description: string
  shortDescription: string
  price: number
  level: CourseLevel
  thumbnailUrl: string
}

export async function createCourse(data: CreateCourseRequest): Promise<string> {
  const response = await client.post('/courses', data)
  return response.data
}

export interface UpdateCourseRequest {
  id: string
  title: string
  description: string
  shortDescription: string
  price: number
  level: CourseLevel
  status: CourseStatus
  thumbnailUrl: string
}

export async function updateCourse(data: UpdateCourseRequest): Promise<void> {
  await client.put(`/courses/${data.id}`, data)
}
