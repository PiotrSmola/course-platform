export interface TrialEligibleCourseDto {
  id: string
  title: string
  shortDescription: string
  thumbnailUrl: string | null
  instructorName: string
  price: number
  lessonCount: number
}

export interface ActivateTrialRequest {
  courseId: string
}

export interface TrialAccessDto {
  courseId: string
}
