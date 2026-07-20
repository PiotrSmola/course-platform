<template>
  <div class="instructor-dashboard">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Panel instruktora</span>
        <h2>Twoje kursy</h2>
      </div>

      <div class="actions">
        <router-link class="btn btn-primary" :to="{ name: 'NewCourse' }">
          + Nowy kurs
        </router-link>
      </div>

      <section v-if="!dashboardLoading && dashboard" class="stats-grid">
        <div class="stat-card glass-card">
          <span class="stat-value">{{ dashboard.totalStudents }}</span>
          <span class="stat-label">Studentów</span>
        </div>
        <div class="stat-card glass-card">
          <span class="stat-value">{{ formatCurrency(dashboard.totalRevenue) }}</span>
          <span class="stat-label">Przychód</span>
        </div>
        <div class="stat-card glass-card">
          <span class="stat-value">{{ dashboard.averageCompletionRate }}%</span>
          <span class="stat-label">Średni postęp</span>
        </div>
      </section>

      <div v-if="isLoading" class="loading">Ładowanie...</div>
      <div v-else-if="!courses?.length" class="empty glass-card">
        <p>Nie masz jeszcze kursów. Utwórz pierwszy kurs i dodaj moduły z lekcjami.</p>
      </div>
      <div v-else class="courses-grid">
        <article v-for="course in courses" :key="course.id" class="course-card glass-card">
          <CourseThumbnail class="course-thumb" :url="course.thumbnailUrl" />
          <div class="course-info">
            <h3>{{ course.title }}</h3>
            <div class="course-stats">
              <span>{{ course.enrollmentCount }} studentów</span>
              <span>{{ course.moduleCount }} mod. · {{ course.lessonCount }} lek.</span>
              <span class="status" :class="statusClass(course.status)">{{ statusLabel(course.status) }}</span>
              <span v-if="courseStats(course.id)">{{ formatCurrency(courseStats(course.id)!.revenue) }}</span>
              <span v-if="courseStats(course.id)">{{ courseStats(course.id)!.completionRate }}% ukończono</span>
            </div>
            <router-link class="btn btn-ghost" :to="{ name: 'EditCourse', params: { id: course.id } }">
              Edytuj
            </router-link>
            <div class="course-actions">
              <button
                v-if="course.status === CourseStatus.Draft || course.status === CourseStatus.Hidden"
                class="btn btn-ghost btn-sm"
                :disabled="updateStatus.isPending.value"
                @click="publishCourse(course.id)"
              >
                Opublikuj
              </button>
              <button
                v-if="course.status === CourseStatus.Published"
                class="btn btn-ghost btn-sm"
                :disabled="updateStatus.isPending.value"
                @click="hideCourse(course.id)"
              >
                Ukryj
              </button>
              <button
                class="btn btn-ghost btn-sm btn-danger"
                :disabled="deleteCourseMutation.isPending.value"
                @click="confirmDelete(course.id, course.title)"
              >
                Usuń
              </button>
            </div>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useInstructorCourses, useInstructorDashboard, useUpdateCourseStatus, useDeleteCourse } from '@/features/instructor/composables/useInstructor'
import { CourseStatus } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'

const { data, isLoading } = useInstructorCourses()
const { data: dashboardData, isLoading: dashboardLoading } = useInstructorDashboard()
const updateStatus = useUpdateCourseStatus()
const deleteCourseMutation = useDeleteCourse()
const courses = computed(() => data.value ?? [])
const dashboard = computed(() => dashboardData.value)

function statusLabel(status: CourseStatus) {
  switch (status) {
    case CourseStatus.Published: return 'Opublikowany'
    case CourseStatus.Hidden: return 'Ukryty'
    default: return 'Szkic'
  }
}

function statusClass(status: CourseStatus) {
  switch (status) {
    case CourseStatus.Published: return 'published'
    case CourseStatus.Hidden: return 'hidden'
    default: return 'draft'
  }
}

function courseStats(courseId: string) {
  return dashboard.value?.courses.find((c) => c.id === courseId)
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(value)
}

function publishCourse(courseId: string) {
  updateStatus.mutate({ courseId, status: CourseStatus.Published })
}

function hideCourse(courseId: string) {
  updateStatus.mutate({ courseId, status: CourseStatus.Hidden })
}

function confirmDelete(courseId: string, title: string) {
  if (window.confirm(`Czy na pewno chcesz usunąć kurs „${title}"? Tej operacji nie można cofnąć.`)) {
    deleteCourseMutation.mutate(courseId)
  }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.instructor-dashboard {
  padding: calc($header-height + 40px) 0 80px;
}

.actions {
  margin-bottom: 40px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 20px;
  margin-bottom: 40px;
}

.stat-card {
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.stat-value {
  font-size: 2rem;
  font-weight: 700;
  color: $color-ink;
}

.stat-label {
  font-size: 0.9rem;
  color: $color-muted;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.course-card {
  padding: 0;
  overflow: hidden;
}

.course-thumb {
  height: 160px;
  background: linear-gradient(160deg, rgba(139, 92, 246, 0.2), rgba(34, 211, 238, 0.1));
  background-size: cover;
  background-position: center;
}

.course-info {
  padding: 24px;

  h3 {
    font-size: 1.15rem;
    margin-bottom: 12px;
  }
}

.course-stats {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-bottom: 16px;
  font-size: 0.85rem;
  color: $color-muted;

  .status {
    padding: 4px 10px;
    border-radius: 999px;
    font-weight: 600;

    &.published { background: rgba(74, 222, 128, 0.15); color: #4ade80; }
    &.draft { background: rgba(251, 191, 36, 0.15); color: #fbbf24; }
    &.hidden { background: rgba(248, 113, 113, 0.15); color: #f87171; }
  }
}

.course-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 12px;
}

.btn-sm {
  padding: 8px 16px;
  font-size: 0.85rem;
  height: auto;
}

.btn-danger {
  color: #f87171;

  &:hover {
    --lg-tint: rgba(248, 113, 113, 0.12);
  }
}

.loading,
.empty {
  text-align: center;
  padding: 48px;
  color: $color-muted;
}

.empty {
  border-radius: 20px;
}
</style>
