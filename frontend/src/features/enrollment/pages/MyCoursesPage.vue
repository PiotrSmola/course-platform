<template>
  <div class="my-courses">
    <h1>My Courses</h1>
    <div v-if="enrollments.isLoading">Loading...</div>
    <div v-else-if="enrollments.error">Error</div>
    <div v-else class="list">
      <div v-for="e in enrollments.data?.value" :key="e.id" class="course-item">
        <img :src="e.courseThumbnailUrl" v-if="e.courseThumbnailUrl" />
        <div class="info">
          <h3>{{ e.courseTitle }}</h3>
          <div class="progress-bar">
            <div class="progress" :style="{ width: e.progressPercentage + '%' }"></div>
          </div>
          <span>{{ e.completedLessons }} / {{ e.totalLessons }} lessons</span>
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
.my-courses {
  padding: 2rem;
}

.course-item {
  display: flex;
  gap: 1rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
  margin-bottom: 1rem;

  img {
    width: 120px;
    height: 80px;
    object-fit: cover;
    border-radius: 4px;
  }

  .progress-bar {
    width: 200px;
    height: 8px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 4px;
    margin-top: 0.5rem;

    .progress {
      height: 100%;
      background: #4caf50;
      border-radius: 4px;
    }
  }
}
</style>
