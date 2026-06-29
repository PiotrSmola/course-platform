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

      <div v-if="isLoading" class="loading">Ładowanie...</div>
      <div v-else-if="!courses?.length" class="empty glass-card">
        <p>Nie masz jeszcze kursów. Utwórz pierwszy kurs i dodaj moduły z lekcjami.</p>
      </div>
      <div v-else class="courses-grid">
        <article v-for="course in courses" :key="course.id" class="course-card glass-card">
          <div
            class="course-thumb"
            :style="course.thumbnailUrl ? { backgroundImage: `url(${course.thumbnailUrl})` } : undefined"
          />
          <div class="course-info">
            <h3>{{ course.title }}</h3>
            <div class="course-stats">
              <span>{{ course.enrollmentCount }} studentów</span>
              <span>{{ course.moduleCount }} mod. · {{ course.lessonCount }} lek.</span>
              <span class="status" :class="statusClass(course.status)">{{ statusLabel(course.status) }}</span>
            </div>
            <router-link class="btn btn-ghost" :to="{ name: 'EditCourse', params: { id: course.id } }">
              Edytuj
            </router-link>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useInstructorCourses } from '@/features/instructor/composables/useInstructor'
import { CourseStatus } from '@/features/courses/types/course.types'

const { data, isLoading } = useInstructorCourses()
const courses = computed(() => data.value ?? [])

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
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.instructor-dashboard {
  padding: calc($header-height + 40px) 0 80px;
}

.actions {
  margin-bottom: 40px;
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
