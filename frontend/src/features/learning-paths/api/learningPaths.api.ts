import client from '@/shared/api/client'
import type { CourseLevel } from '@/features/courses/types/course.types'

export enum PathDifficultyLevel {
  Beginner = 0,
  Intermediate = 1,
  Advanced = 2
}

export interface LearningPathListItem {
  id: string
  title: string
  slug: string
  shortDescription: string
  difficultyLevel: PathDifficultyLevel
  estimatedHours: number
  thumbnailUrl: string
  courseCount: number
}

export interface LearningPathCourseItem {
  id: string
  order: number
  isOptional: boolean
  courseId: string
  courseTitle: string
  courseShortDescription: string
  coursePrice: number
  courseLevel: CourseLevel
  courseThumbnailUrl: string | null
  courseLanguage: string
  instructorName: string
}

export interface LearningPathDetails {
  id: string
  title: string
  slug: string
  shortDescription: string
  description: string
  difficultyLevel: PathDifficultyLevel
  estimatedHours: number
  thumbnailUrl: string
  createdAt: string
  courses: LearningPathCourseItem[]
}

export async function getLearningPaths(): Promise<{ items: LearningPathListItem[] }> {
  const response = await client.get('/learning-paths')
  return response.data
}

export async function getLearningPathBySlug(slug: string): Promise<LearningPathDetails> {
  const response = await client.get(`/learning-paths/${slug}`)
  return response.data
}