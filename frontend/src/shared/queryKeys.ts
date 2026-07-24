import type { CoursesFilter } from '@/features/courses/api/courses.api'

export const queryKeys = {
  currentUser: () => ['currentUser'] as const,
  userProfile: () => ['userProfile'] as const,
  enrollments: () => ['enrollments'] as const,
  wishlist: () => ['wishlist'] as const,

  categories: () => ['categories'] as const,
  technologies: () => ['technologies'] as const,
  learningPaths: () => ['learning-paths'] as const,
  learningPath: (slug: string) => ['learning-paths', slug] as const,
  businessPlans: () => ['business-plans'] as const,

  courses: () => ['courses'] as const,
  course: (id: string) => ['course', id] as const,
  courseSearch: (term: string) => ['course-search', term] as const,
  coursesBrowse: (filter: CoursesFilter) => ['courses-browse', filter] as const,
  coursesBrowseAll: () => ['courses-browse'] as const,

  lesson: (courseId: string, lessonId: string) => ['lesson', courseId, lessonId] as const,
  lessonDiscussion: (courseId: string, lessonId: string, pageNumber: number) =>
    ['lesson-discussion', courseId, lessonId, pageNumber] as const,
  lessonDiscussionAll: (courseId: string, lessonId: string) =>
    ['lesson-discussion', courseId, lessonId] as const,
  lessonQuiz: (courseId: string, lessonId: string) => ['lesson-quiz', courseId, lessonId] as const,
  lessonQuizManage: (courseId: string, lessonId: string) =>
    ['lesson-quiz-manage', courseId, lessonId] as const,
  lessonQuizAttempts: (courseId: string, lessonId: string) =>
    ['lesson-quiz-attempts', courseId, lessonId] as const,
  lessonResources: (courseId: string, lessonId: string) =>
    ['lesson-resources', courseId, lessonId] as const,
  lessonResourcesManage: (courseId: string, lessonId: string) =>
    ['lesson-resources-manage', courseId, lessonId] as const,

  paymentStatus: (sessionId: string) => ['payment-status', sessionId] as const,
  mySubscription: () => ['my-subscription'] as const,
  subscriptionOffer: () => ['subscription-offer'] as const,


  myPurchases: () => ['my-purchases'] as const,
  myCertificates: () => ['my-certificates'] as const,
  certificateVerification: (number: string) => ['certificate-verification', number] as const,

  instructorCourses: () => ['instructor-courses'] as const,
  instructorDashboard: () => ['instructor-dashboard'] as const,
  instructorAnalytics: (courseId?: string) => ['instructor-analytics', courseId ?? 'all'] as const,
  adminUsers: () => ['admin-users'] as const,
  adminCourses: () => ['admin-courses'] as const,
  adminReviews: () => ['admin-reviews'] as const,
  adminAuditLogs: (pageNumber: number, pageSize: number) => ['admin-audit-logs', pageNumber, pageSize] as const,
  adminCoupons: () => ['admin-coupons'] as const,
  adminRevenue: () => ['admin-revenue'] as const,
  catalogStats: () => ['catalog-stats'] as const,
  searchStats: () => ['admin-search-stats'] as const,
  reindexJob: () => ['admin-reindex-job'] as const,

  qaModeration: (filters: {
    courseId?: string
    unansweredByInstructorOnly?: boolean
    pageNumber?: number
    pageSize?: number
  }) => ['qa-moderation', filters] as const,
  qaModerationAll: () => ['qa-moderation'] as const
}
