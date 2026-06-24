<template>
  <div class="catalog">
    <h1>Course Catalog</h1>
    <div v-if="isLoading">Loading...</div>
    <div v-else-if="isError">Error: {{ error?.message }}</div>
    <div v-else-if="data">
      <div class="course-list">
        <CourseCard v-for="course in data.items" :key="course.id" :course="course" />
      </div>
    </div>
    <div v-else>No data</div>
  </div>
</template>

<script setup lang="ts">
import { useCourses } from '@/features/courses/composables/useCourses'
import CourseCard from '@/features/courses/components/CourseCard.vue'

const { isLoading, isError, error, data } = useCourses()
</script>

<style lang="scss" scoped>
.catalog {
  padding: 2rem;
}

.course-list {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
  margin-top: 1.5rem;
}
</style>
