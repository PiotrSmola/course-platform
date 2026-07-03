export enum CourseLevel {
  Beginner = 0,
  Intermediate = 1,
  Advanced = 2
}

export enum CourseStatus {
  Draft = 0,
  Published = 1,
  Hidden = 2
}

export interface CourseListDto {
  id: string
  title: string
  shortDescription: string
  price: number
  level: CourseLevel
  status: CourseStatus
  thumbnailUrl: string | null
  instructorName: string
  language: string
  categoryNames: string[]
  technologyNames: string[]
  moduleCount: number
  lessonCount: number
  averageRating: number
  reviewCount: number
}

export interface CoursesVm {
  items: CourseListDto[]
  totalCount: number
}

export interface LessonDto {
  id: string
  title: string
  description: string | null
  duration: number
  order: number
  videoObjectKey: string
  moduleId?: string
  moduleTitle?: string
  courseId?: string
  courseTitle?: string
  isCompleted?: boolean
}

export interface LessonListDto {
  id: string
  title: string
  description: string | null
  duration: number
  order: number
  isCompleted?: boolean
  videoObjectKey?: string | null
}

export interface ModuleDto {
  id: string
  title: string
  order: number
  lessons: LessonListDto[]
}

export interface ReviewDto {
  id: string
  rating: number
  comment: string
  authorName: string
  createdAt: string
}

export interface CourseDetailsDto {
  id: string
  title: string
  description: string
  shortDescription: string
  price: number
  level: CourseLevel
  status: CourseStatus
  thumbnailUrl: string | null
  language: string
  instructorId: string
  instructorName: string
  createdAt: string
  categoryNames: string[]
  technologyNames: string[]
  modules: ModuleDto[]
  averageRating: number
  reviewCount: number
  reviews: ReviewDto[]
  isEnrolled: boolean
  hasUserReviewed: boolean
  canReview: boolean
}

export interface InstructorCourseDto {
  id: string
  title: string
  status: CourseStatus
  price: number
  thumbnailUrl: string | null
  enrollmentCount: number
  moduleCount: number
  lessonCount: number
  createdAt: string
}
