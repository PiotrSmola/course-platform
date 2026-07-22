export interface LessonAnswerDto {
  id: string
  body: string
  authorId: string
  authorName: string
  isInstructorAnswer: boolean
  createdAt: string
  canDelete: boolean
}

export interface LessonQuestionDto {
  id: string
  body: string
  authorId: string
  authorName: string
  createdAt: string
  canDelete: boolean
  answers: LessonAnswerDto[]
}

export interface LessonDiscussionDto {
  items: LessonQuestionDto[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}
