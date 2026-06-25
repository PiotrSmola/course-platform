export interface EnrollmentDto {
  id: string
  courseId: string
  courseTitle: string
  courseThumbnailUrl: string
  courseLevel: number
  enrolledAt: string
  completedLessons: number
  totalLessons: number
  progressPercentage: number
  firstLessonId: string | null
}
