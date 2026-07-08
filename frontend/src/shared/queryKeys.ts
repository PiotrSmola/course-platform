import type { CoursesFilter } from '@/features/courses/api/courses.api'

export const queryKeys = {
  currentUser: () => ['currentUser'] as const,
  userProfile: () => ['userProfile'] as const,
  enrollments: () => ['enrollments'] as const,

  categories: () => ['categories'] as const,
  technologies: () => ['technologies'] as const,

  courses: () => ['courses'] as const,
  course: (id: string) => ['course', id] as const,
  courseSearch: (term: string) => ['course-search', term] as const,
  coursesBrowse: (filter: CoursesFilter) => ['courses-browse', filter] as const,
  coursesBrowseAll: () => ['courses-browse'] as const,

  lesson: (courseId: string, lessonId: string) => ['lesson', courseId, lessonId] as const,

  paymentStatus: (sessionId: string) => ['payment-status', sessionId] as const,

  instructorCourses: () => ['instructor-courses'] as const,
  adminUsers: () => ['admin-users'] as const,
  adminCourses: () => ['admin-courses'] as const,
  catalogStats: () => ['catalog-stats'] as const
}
