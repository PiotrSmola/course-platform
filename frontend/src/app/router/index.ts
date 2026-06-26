import { createRouter, createWebHistory } from 'vue-router'
import type { RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { authGuard, instructorGuard } from './guards'

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    component: () => import('@/app/layouts/DefaultLayout.vue'),
    children: [
      { path: '', component: () => import('@/features/courses/pages/CourseCatalogPage.vue'), name: 'Home' },
      { path: 'courses', component: () => import('@/features/courses/pages/CourseBrowsePage.vue'), name: 'Courses' },
      { path: 'courses/:id', component: () => import('@/features/courses/pages/CourseDetailsPage.vue'), name: 'CourseDetails', props: true },
      { path: 'categories', component: () => import('@/features/courses/pages/CategoriesListPage.vue'), name: 'CategoriesList' },
      { path: 'categories/:slug', component: () => import('@/features/courses/pages/CategoryPage.vue'), name: 'CategoryDetails', props: true },
      { path: 'technologies', component: () => import('@/features/courses/pages/TechnologiesListPage.vue'), name: 'TechnologiesList' },
      { path: 'technologies/:slug', component: () => import('@/features/courses/pages/TechnologyPage.vue'), name: 'TechnologyDetails', props: true },
      { path: 'paths', component: () => import('@/features/learning-paths/pages/PathsListPage.vue'), name: 'PathsList' },
      { path: 'paths/:slug', component: () => import('@/features/learning-paths/pages/PathDetailsPage.vue'), name: 'PathDetails', props: true },
      { path: 'business', component: () => import('@/features/business/pages/BusinessPage.vue'), name: 'Business' }
    ]
  },
  {
    path: '/login',
    component: () => import('@/app/layouts/AuthLayout.vue'),
    children: [
      { path: '', component: () => import('@/features/auth/pages/LoginPage.vue'), name: 'Login', meta: { guest: true } }
    ]
  },
  {
    path: '/register',
    component: () => import('@/app/layouts/AuthLayout.vue'),
    children: [
      { path: '', component: () => import('@/features/auth/pages/RegisterPage.vue'), name: 'Register', meta: { guest: true } }
    ]
  },
  {
    path: '/my-courses',
    component: () => import('@/app/layouts/DashboardLayout.vue'),
    beforeEnter: authGuard,
    children: [
      { path: '', component: () => import('@/features/enrollment/pages/MyCoursesPage.vue'), name: 'MyCourses' }
    ]
  },
  {
    path: '/learn/:courseId/:lessonId',
    component: () => import('@/app/layouts/DashboardLayout.vue'),
    beforeEnter: authGuard,
    children: [
      { path: '', component: () => import('@/features/learning/pages/LearningPage.vue'), name: 'Learning', props: true }
    ]
  },
  {
    path: '/instructor',
    component: () => import('@/app/layouts/DashboardLayout.vue'),
    beforeEnter: [authGuard, instructorGuard],
    children: [
      { path: '', component: () => import('@/features/instructor/pages/InstructorDashboardPage.vue'), name: 'InstructorDashboard' },
      { path: 'courses/new', component: () => import('@/features/instructor/pages/CourseEditorPage.vue'), name: 'NewCourse', props: () => ({ isNew: true }) },
      { path: 'courses/:id', component: () => import('@/features/instructor/pages/CourseEditorPage.vue'), name: 'EditCourse', props: true }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior() {
    return { top: 0 }
  }
})

router.beforeEach((to) => {
  const authStore = useAuthStore()
  if (to.meta.guest && authStore.isAuthenticated) {
    return { name: 'Home' }
  }
})

export default router
export { router }
