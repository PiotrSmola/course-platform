export interface UserStatisticsDto {
  totalEnrollments: number
  completedCourses: number
  totalLessonsCompleted: number
  totalLessonsAvailable: number
  averageProgressPercentage: number
  totalLearningTimeSeconds: number
  certificatesEarned: number
  lastActivityAt: string | null
}

export interface UserProfileDto {
  id: string
  email: string | null
  firstName: string
  lastName: string
  createdAt: string
  statistics: UserStatisticsDto
}

export interface UpdateProfileRequest {
  firstName: string
  lastName: string
}

export interface UserProfileSummaryDto {
  id: string
  email: string | null
  firstName: string
  lastName: string
}
