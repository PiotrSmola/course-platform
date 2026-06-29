export const queryKeys = {
  currentUser: () => ['currentUser'] as const,
  userProfile: () => ['userProfile'] as const,
  enrollments: () => ['enrollments'] as const,

  categories: () => ['categories'] as const,
  technologies: () => ['technologies'] as const,

  courses: () => ['courses'] as const,
  course: (id: string) => ['course', id] as const,
  courseSearch: (term: string) => ['course-search', term] as const,
  coursesBrowse: (filter: unknown) => ['courses-browse', filter] as const,

  lesson: (courseId: string, lessonId: string) => ['lesson', courseId, lessonId] as const
}

