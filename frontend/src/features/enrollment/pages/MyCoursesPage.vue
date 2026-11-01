<template>
  <div class="my-courses">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Moje kursy</span>
        <h2>Twoja <em>nauka</em></h2>
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
          <div class="course-thumb-wrap">
            <CourseThumbnail
              class="course-thumb"
              :url="e.courseThumbnailUrl"
              :title="e.courseTitle"
              :seed="e.courseId"
            />
            <span v-if="isComplete(e)" class="badge-done">Ukończony</span>
          </div>
          <div class="course-info">
            <p class="card-meta">
              <span class="level">{{ levelLabel(e.courseLevel) }}</span>
              <span>{{ e.completedLessons }} / {{ e.totalLessons }} lekcji</span>
              <span v-if="e.isTrial" class="badge-trial">Trial</span>
            </p>
            <h3>{{ e.courseTitle }}</h3>
            <div class="card-footer">
              <div class="progress-row">
                <div class="progress-bar" role="progressbar" :aria-valuenow="Math.round(e.progressPercentage)" aria-valuemin="0" aria-valuemax="100">
                  <div class="progress-fill" :style="{ width: e.progressPercentage + '%' }"></div>
                </div>
                <span class="progress-pct">{{ Math.round(e.progressPercentage) }}%</span>
              </div>
              <router-link
                v-if="e.continueLessonId"
                class="btn btn-ghost btn-pill continue-btn"
                :to="{ name: 'Learning', params: { courseId: e.courseId, lessonId: e.continueLessonId } }"
              >
                {{ continueLabel(e) }}
              </router-link>
              <button v-else class="btn btn-ghost btn-pill continue-btn" disabled>Brak lekcji</button>
            </div>
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
.my-courses {
  padding: 24px 0 80px;
}

.state-wrap {
  margin-top: 24px;
}

.skeletons,
.courses-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(min(320px, 100%), 1fr));
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

.badge-done {
  @include tag;
  position: absolute;
  top: 10px;
  right: 10px;
  color: $color-on-accent;
  background: $color-accent;
  box-shadow: none;
}

.course-info {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: 16px 20px 20px;

  h3 {
    margin-bottom: 18px;
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

.card-meta {
  @include mono-label;
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
  font-size: 0.64rem;
  color: $color-faint;

  > span:not(.badge-trial) + span:not(.badge-trial)::before {
    content: "/";
    margin-right: 10px;
    color: $color-hairline;
  }
}

.badge-trial {
  @include tag;
  margin-left: auto;
  color: $color-coral;
  box-shadow: inset 0 0 0 1px rgba($color-coral, 0.4);
}

.progress-row {
  flex: 1;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.progress-bar {
  height: 4px;
  border-radius: 2px;
  background: rgba($color-ink, 0.1);
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 2px;
  background: $color-accent;
  transition: width 0.5s $ease-out;
}

.progress-pct {
  font-family: $font-mono;
  font-size: 0.76rem;
  font-variant-numeric: tabular-nums;
  color: $color-ink;
}

.card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  margin-top: auto;
  padding-top: 16px;
  border-top: 1px dashed $color-hairline;
}

.continue-btn {
  padding: 10px 18px;
  font-size: 0.86rem;
}
</style>
