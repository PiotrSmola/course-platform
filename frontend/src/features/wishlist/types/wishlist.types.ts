import type { CourseLevel } from '@/features/courses/types/course.types'

export interface WishlistItemDto {
  id: string
  courseId: string
  courseTitle: string
  courseThumbnailUrl: string | null
  courseLevel: CourseLevel
  price: number
  addedAt: string
}
