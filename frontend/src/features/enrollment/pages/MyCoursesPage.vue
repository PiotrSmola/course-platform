<template>
  <div class="my-courses">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Moje kursy</span>
        <h2>Twoja nauka</h2>
      </div>

      <div v-if="!authStore.token" class="state-wrap">
        <EmptyState
          title="Zaloguj się, aby zobaczyć swoje kursy"
          description="Po zalogowaniu znajdziesz tu postęp i szybki powrót do nauki."
          action-label="Zaloguj się"
          action-to="Login"
        />
      </div>
      <div v-else-if="isLoading" class="state-wrap skeletons">
        <SkeletonBlock v-for="n in 3" :key="n" height="280px" />
      </div>
      <div v-else-if="isError" class="state-wrap">
        <ErrorState
          title="Nie udało się załadować kursów"
          description="Sprawdź połączenie i spróbuj ponownie za chwilę."
          action-label="Przeglądaj katalog"
          action-to="Courses"
        />
      </div>
      <div v-else-if="!enrollments.length" class="state-wrap">
        <EmptyState
          title="Nie masz jeszcze żadnych kursów"
          description="Zapisz się na darmowy kurs albo kup płatny — wszystko pojawia się tutaj."
          action-label="Przeglądaj katalog"
          action-to="Courses"
        />
      </div>
      <div v-else class="courses-grid">
        <article v-for="e in enrollments" :key="e.id" class="course-card glass-card">
          <CourseThumbnail class="course-thumb" :url="e.courseThumbnailUrl" />
          <div class="course-info">
            <div class="card-meta">
              <span class="level">{{ levelLabel(e.courseLevel) }}</span>
              <span v-if="isComplete(e)" class="badge-done">Ukończony</span>
            </div>
            <h3>{{ e.courseTitle }}</h3>
            <div class="progress-bar" role="progressbar" :aria-valuenow="Math.round(e.progressPercentage)" aria-valuemin="0" aria-valuemax="100">
              <div class="progress-fill" :style="{ width: e.progressPercentage + '%' }"></div>
            </div>
            <div class="progress-meta">
              <span>{{ e.completedLessons }} / {{ e.totalLessons }} lekcji</span>
              <span class="progress-pct">{{ Math.round(e.progressPercentage) }}%</span>
            </div>
            <router-link
              v-if="e.continueLessonId"
              class="btn btn-primary continue-btn"
              :to="{ name: 'Learning', params: { courseId: e.courseId, lessonId: e.continueLessonId } }"
            >
              {{ continueLabel(e) }}
            </router-link>
            <button v-else class="btn btn-primary continue-btn" disabled>Brak lekcji</button>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useEnrollments } from '@/features/enrollment/composables/useEnrollment'
import type { EnrollmentDto } from '@/features/enrollment/types/enrollment.types'
import { CourseLevel } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'
import EmptyState from '@/shared/components/ui/EmptyState.vue'
import ErrorState from '@/shared/components/ui/ErrorState.vue'
import SkeletonBlock from '@/shared/components/ui/SkeletonBlock.vue'

const authStore = useAuthStore()
const { data, isLoading, isError } = useEnrollments()
const enrollments = computed(() => data.value ?? [])

function isComplete(e: EnrollmentDto) {
  return e.totalLessons > 0 && e.completedLessons >= e.totalLessons
}

function continueLabel(e: EnrollmentDto) {
  if (isComplete(e)) return 'Przejrzyj kurs'
  if (e.completedLessons === 0) return 'Zacznij naukę'
  return 'Kontynuuj naukę'
}

function levelLabel(level: number) {
  switch (level) {
    case CourseLevel.Beginner: return 'Początkujący'
    case CourseLevel.Intermediate: return 'Średni'
    case CourseLevel.Advanced: return 'Zaawansowany'
    default: return 'Kurs'
  }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.my-courses {
  padding: calc($header-height + 40px) 0 80px;
}

.state-wrap {
  margin-top: 24px;
}

.skeletons {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
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
  height: 180px;
  display: block;
}

.course-info {
  padding: 24px;

  h3 {
    font-size: 1.15rem;
    margin-bottom: 16px;
  }
}

.card-meta {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.level {
  font-size: 0.78rem;
  font-weight: 600;
  color: $color-muted;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.badge-done {
  font-size: 0.75rem;
  font-weight: 700;
  padding: 4px 10px;
  border-radius: 999px;
  background: rgba(74, 222, 128, 0.15);
  color: #4ade80;
}

.progress-bar {
  height: 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.1);
  overflow: hidden;
  margin-bottom: 10px;
}

.progress-fill {
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, #a78bfa, #22d3ee);
  transition: width 0.5s ease;
}

.progress-meta {
  display: flex;
  justify-content: space-between;
  font-size: 0.85rem;
  color: $color-muted;
  margin-bottom: 20px;
}

.progress-pct {
  font-weight: 700;
  color: $color-cyan;
}

.continue-btn {
  padding: 10px 20px;
  font-size: 0.9rem;
}
</style>
