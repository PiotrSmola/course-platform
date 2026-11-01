<template>
  <div class="instructor-dashboard">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Panel instruktora</span>
        <h2>{{ heading }}</h2>
      </div>

      <div class="toolbar">
        <div class="tabs-scroll">
          <div class="tabs glass">
            <button type="button" :class="{ active: tab === 'courses' }" @click="tab = 'courses'">
              Kursy
            </button>
            <button type="button" :class="{ active: tab === 'analytics' }" @click="tab = 'analytics'">
              Analytics
            </button>
            <button type="button" :class="{ active: tab === 'qa' }" @click="tab = 'qa'">
              Q&A
            </button>
          </div>
        </div>
        <router-link v-if="tab === 'courses'" class="btn btn-primary new-course-btn" :to="{ name: 'NewCourse' }">
          + Nowy kurs
        </router-link>
      </div>

      <template v-if="tab === 'courses'">
        <section v-if="!dashboardLoading && dashboard" class="stats-grid">
          <div class="stat-card">
            <span class="stat-value">{{ dashboard.totalStudents }}</span>
            <span class="stat-label">Studentów</span>
          </div>
          <div class="stat-card">
            <span class="stat-value">{{ formatCurrency(dashboard.totalRevenue) }}</span>
            <span class="stat-label">Przychód</span>
          </div>
          <div class="stat-card">
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
            <div class="course-thumb-wrap">
              <CourseThumbnail class="course-thumb" :url="course.thumbnailUrl" :title="course.title" :seed="course.id" />
            </div>
            <div class="course-info">
              <p class="course-kicker">
                <span>{{ course.moduleCount }} mod.</span>
                <span>{{ course.lessonCount }} lek.</span>
                <span class="status" :class="statusClass(course.status)">{{ statusLabel(course.status) }}</span>
              </p>
              <h3>{{ course.title }}</h3>
              <dl class="course-stats">
                <div>
                  <dt>Studenci</dt>
                  <dd>{{ course.enrollmentCount }}</dd>
                </div>
                <div v-if="courseStats(course.id)">
                  <dt>Przychód</dt>
                  <dd>{{ formatCurrency(courseStats(course.id)!.revenue) }}</dd>
                </div>
                <div v-if="courseStats(course.id)">
                  <dt>Ukończono</dt>
                  <dd>{{ courseStats(course.id)!.completionRate }}%</dd>
                </div>
              </dl>
              <div class="course-actions">
                <router-link class="btn btn-ghost btn-sm" :to="{ name: 'EditCourse', params: { id: course.id } }">
                  Edytuj
                </router-link>
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
                  class="btn btn-sm btn-danger"
                  :disabled="deleteCourseMutation.isPending.value"
                  @click="confirmDelete(course.id, course.title)"
                >
                  Usuń
                </button>
              </div>
            </div>
          </article>
        </div>
      </template>

      <section v-else-if="tab === 'analytics'" class="panel">
        <InstructorAnalyticsPanel />
      </section>

      <section v-else class="panel glass-card">
        <QaModerationPanel :course-options="courseFilterOptions" />
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useInstructorCourses, useInstructorDashboard, useUpdateCourseStatus, useDeleteCourse } from '@/features/instructor/composables/useInstructor'
import { CourseStatus } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'
import QaModerationPanel from '@/features/lesson-discussion/components/QaModerationPanel.vue'
import InstructorAnalyticsPanel from '@/features/analytics/components/InstructorAnalyticsPanel.vue'

type InstructorTab = 'courses' | 'analytics' | 'qa'
const tab = ref<InstructorTab>('courses')

const heading = computed(() => {
  if (tab.value === 'analytics') return 'Analytics'
  if (tab.value === 'qa') return 'Moderacja Q&A'
  return 'Twoje kursy'
})

const { data, isLoading } = useInstructorCourses()
const { data: dashboardData, isLoading: dashboardLoading } = useInstructorDashboard()
const updateStatus = useUpdateCourseStatus()
const deleteCourseMutation = useDeleteCourse()
const courses = computed(() => data.value ?? [])
const dashboard = computed(() => dashboardData.value)
const courseFilterOptions = computed(() =>
  courses.value.map((course) => ({ id: course.id, title: course.title }))
)

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
.instructor-dashboard {
  padding: 24px 0 80px;
}

.toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 36px;

  @media (max-width: 560px) {
    flex-direction: column;
    align-items: stretch;
  }
}

.tabs-scroll {
  min-width: 0;
  margin: -8px -8px -14px;
  padding: 8px 8px 14px;
  overflow-x: auto;
  scrollbar-width: none;

  &::-webkit-scrollbar {
    display: none;
  }
}

.tabs {
  --lg-r: 999px;
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.3);
  display: inline-flex;
  gap: 2px;
  padding: 5px;

  button {
    padding: 9px 20px;
    border: none;
    border-radius: 999px;
    background: transparent;
    color: $color-muted;
    cursor: pointer;
    font: inherit;
    font-size: 0.88rem;
    font-weight: 500;
    white-space: nowrap;
    transition: background 0.25s, color 0.25s;

    &:hover {
      color: $color-ink;
    }

    &.active {
      color: $color-ink;
      background: rgba($color-ink, 0.12);
      box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.12);
    }
  }
}

.new-course-btn {
  flex-shrink: 0;
  padding: 11px 22px;
  font-size: 0.88rem;
}

.panel.glass-card {
  --lg-blur: 12px;
  padding: 24px;

  &:hover {
    transform: none;
  }

  @media (max-width: 560px) {
    padding: 18px 14px;
  }
}

.stats-grid {
  display: flex;
  flex-wrap: wrap;
  overflow: hidden;
  margin-bottom: 44px;
  border-top: 1px solid $color-hairline;
  border-bottom: 1px solid $color-hairline;
}

.stat-card {
  flex: 1 1 200px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin: -1px 0 0 -1px;
  padding: 26px 24px 22px;
  border-top: 1px solid $color-hairline;
  border-left: 1px solid $color-hairline;
}

.stat-value {
  font-family: $font-display;
  font-size: clamp(2rem, 3.4vw, 2.8rem);
  font-weight: 600;
  line-height: 1;
  letter-spacing: -0.015em;
  font-variant-numeric: lining-nums;
  color: $color-ink;
  white-space: nowrap;
}

.stat-label {
  @include mono-label;
  font-size: 0.64rem;
  color: $color-faint;
}

.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(300px, 100%), 1fr));
  gap: 24px;
}

.course-card {
  --lg-blur: 10px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.course-thumb-wrap {
  position: relative;
  margin: 12px 12px 0;
}

.course-thumb {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  border-radius: 14px 14px 14px 4px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.08);
}

.status {
  @include tag;
  margin-left: auto;

  &.published {
    color: $color-on-accent;
    background: $color-accent;
    box-shadow: none;
  }

  &.hidden {
    color: $color-coral;
    box-shadow: inset 0 0 0 1px rgba($color-coral, 0.45);
  }
}

.course-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: 16px 20px 20px;

  h3 {
    margin-bottom: 16px;
    font-family: $font-display;
    font-size: 1.08rem;
    font-weight: 600;
    line-height: 1.25;
    letter-spacing: -0.005em;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
}

.course-kicker {
  @include mono-label;
  display: flex;
  gap: 10px;
  margin-bottom: 10px;
  font-size: 0.64rem;
  color: $color-faint;

  align-items: center;

  span:not(.status) + span:not(.status)::before {
    content: "/";
    margin-right: 10px;
    color: $color-hairline;
  }
}

.course-stats {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  margin: 0 0 18px;
  border-top: 1px solid $color-hairline;

  > div {
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding: 12px 10px 0 0;
    min-width: 0;

    & + div {
      padding-left: 12px;
      border-left: 1px solid $color-hairline;
    }
  }

  dt {
    @include mono-label;
    font-size: 0.6rem;
    color: $color-faint;
  }

  dd {
    margin: 0;
    font-family: $font-display;
    font-size: 1.1rem;
    font-weight: 600;
    line-height: 1.1;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    color: $color-ink;
  }
}

.course-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: auto;
  padding-top: 16px;
  border-top: 1px dashed $color-hairline;
}

.btn-sm {
  padding: 8px 16px;
  font-size: 0.82rem;
}

.btn-danger {
  --lg-tint: #{rgba($color-danger, 0.22)};
  margin-left: auto;

  &:hover {
    --lg-tint: #{rgba($color-danger, 0.32)};
  }
}

.loading,
.empty {
  text-align: center;
  padding: 48px;
  color: $color-muted;
}

.loading {
  @include mono-label;
}
</style>
