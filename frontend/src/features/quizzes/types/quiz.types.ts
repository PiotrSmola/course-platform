export interface QuizOptionStudentDto {
  id: string
  text: string
  order: number
}

export interface QuizQuestionStudentDto {
  id: string
  prompt: string
  order: number
  options: QuizOptionStudentDto[]
}

export interface LessonQuizStudentDto {
  id: string
  title: string
  passThresholdPercent: number
  questions: QuizQuestionStudentDto[]
}

export interface QuizOptionInstructorDto {
  id: string
  text: string
  isCorrect: boolean
  order: number
}

export interface QuizQuestionInstructorDto {
  id: string
  prompt: string
  order: number
  options: QuizOptionInstructorDto[]
}

export interface LessonQuizInstructorDto {
  id: string
  title: string
  passThresholdPercent: number
  questions: QuizQuestionInstructorDto[]
}

export interface QuizOptionInput {
  text: string
  isCorrect: boolean
  order: number
}

export interface QuizQuestionInput {
  prompt: string
  order: number
  options: QuizOptionInput[]
}

export interface UpsertQuizRequest {
  title: string
  passThresholdPercent: number
  questions: QuizQuestionInput[]
}

export interface QuizAttemptAnswerInput {
  questionId: string
  selectedOptionId: string
}

export interface QuizAttemptResultDto {
  attemptId: string
  scorePercent: number
  passed: boolean
  submittedAt: string
}

export interface QuizAttemptListItemDto {
  id: string
  scorePercent: number
  passed: boolean
  submittedAt: string
}
