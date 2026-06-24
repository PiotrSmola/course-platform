<template>
  <div class="my-courses">
    <div class="container">
      <div class="section-head">
        <span class="eyebrow">Moje kursy</span>
        <h2>Twoja nauka</h2>
      </div>

      <div v-if="enrollments.isLoading" class="loading">Ładowanie...</div>
      <div v-else-if="enrollments.error" class="error">Błąd</div>
      <div v-else-if="enrollments.data && Array.isArray(enrollments.data)" class="courses-grid">
        <div v-for="e in enrollments.data" :key="e.id" class="course-card glass-card">
          <div class="course-thumb" :style="{ backgroundImage: `url(${e.courseThumbnailUrl})` }"></div>
          <div class="course-info">
            <h3>{{ e.courseTitle }}</h3>
            <div class="progress-bar">
              <div class="progress-fill" :style="{ width: e.progressPercentage + '%' }"></div>
            </div>
            <div class="progress-meta">
              <span>{{ e.completedLessons }} / {{ e.totalLessons }} lekcji</span>
              <span class="progress-pct">{{ Math.round(e.progressPercentage) }}%</span>
            </div>
            <router-link class="btn btn-primary continue-btn" :to="{ name: 'Learning', params: { courseId: e.courseId, lessonId: 'first' } }">
              Kontynuuj
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useEnrollments } from '@/features/enrollment/composables/useEnrollment'

const enrollments = useEnrollments()
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.my-courses {
  padding: calc($header-height + 40px) 0 80px;
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
  background-size: cover;
  background-position: center;
}

.course-info {
  padding: 24px;

  h3 {
    font-size: 1.15rem;
    margin-bottom: 16px;
  }
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

.loading,
.error {
  text-align: center;
  padding: 60px;
  color: $color-muted;
}
</style>