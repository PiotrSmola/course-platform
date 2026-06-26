import { z } from 'zod'
import { sharedSchemas } from '@/shared/validation'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'

export const courseSchema = z.object({
  title: sharedSchemas.courseTitle,
  description: sharedSchemas.courseDescription,
  shortDescription: sharedSchemas.shortDescription,
  price: sharedSchemas.price,
  level: z.nativeEnum(CourseLevel),
  status: z.nativeEnum(CourseStatus).optional(),
  thumbnailUrl: sharedSchemas.thumbnailUrl,
  language: sharedSchemas.language,
  categoryIds: z.array(z.string()).optional(),
  technologyIds: z.array(z.string()).optional()
})

export const createCourseSchema = courseSchema
export const updateCourseSchema = courseSchema

export type CourseFormValues = z.infer<typeof courseSchema>
export type CreateCourseFormValues = z.infer<typeof createCourseSchema>
export type UpdateCourseFormValues = z.infer<typeof updateCourseSchema>
