export interface ModerationAnswerPreviewDto {
  id: string
  body: string
  authorName: string
  isInstructorAnswer: boolean
  createdAt: string
}

export interface ModerationQuestionDto {
  id: string
  body: string
  authorName: string
  courseTitle: string
  lessonTitle: string
  courseId: string
  lessonId: string
  createdAt: string
  answersCount: number
  hasInstructorAnswer: boolean
  answersPreview: ModerationAnswerPreviewDto[]
}

export interface ModerationQueueDto {
  items: ModerationQuestionDto[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface ModerationQueueFilters {
  courseId?: string
  unansweredByInstructorOnly?: boolean
  pageNumber?: number
  pageSize?: number
}
