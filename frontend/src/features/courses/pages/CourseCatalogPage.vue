<template>
  <div class="catalog">
    <h1>Course Catalog</h1>
    <div v-if="courses.isLoading">Loading...</div>
    <div v-else-if="courses.error">Error loading courses</div>
    <div v-else class="course-list">
      <CourseCard v-for="course in courses.data?.value?.items" :key="course.id" :course="course" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { useCourses } from '@/features/courses/composables/useCourses'
import CourseCard from '@/features/courses/components/CourseCard.vue'

const { courses } = useCourses()
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
