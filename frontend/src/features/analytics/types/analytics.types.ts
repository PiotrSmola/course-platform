export interface LessonDropOffDto {
  lessonId: string
  lessonTitle: string
  moduleId: string
  moduleTitle: string
  orderIndex: number
  reachedCount: number
  completedCount: number
  dropOffPercent: number
}

export interface InstructorCourseAnalyticsDto {
  courseId: string
  title: string
  enrollmentCount: number
  revenue: number
  completionRate: number
  dropOff: LessonDropOffDto[]
}

export interface InstructorAnalyticsDto {
  totalRevenue: number
  averageCompletionRate: number
  courses: InstructorCourseAnalyticsDto[]
}

export interface AdminRevenueOverviewDto {
  courseSalesRevenue: number
  subscriptionRevenue: number
  totalRevenue: number
  completedCoursePayments: number
  paidSubscriptionInvoices: number
  activeSubscriptions: number
}
