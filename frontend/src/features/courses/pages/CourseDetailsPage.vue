<template>
  <div class="course-details" v-if="course.data?.value">
    <h1>{{ course.data.value.title }}</h1>
    <p>{{ course.data.value.description }}</p>
    <div class="instructor">By {{ course.data.value.instructorName }}</div>
    <div class="price">{{ course.data.value.price }}$</div>

    <div class="modules">
      <h2>Modules</h2>
      <div v-for="module in course.data.value.modules" :key="module.id" class="module">
        <h3>{{ module.title }}</h3>
        <ul>
          <li v-for="lesson in module.lessons" :key="lesson.id">{{ lesson.title }}</li>
        </ul>
      </div>
    </div>

    <div class="reviews">
      <h2>Reviews</h2>
      <div v-for="review in course.data.value.reviews" :key="review.id" class="review">
        <div class="rating">{{ review.rating }}/5</div>
        <p>{{ review.comment }}</p>
        <div class="author">{{ review.authorName }}</div>
      </div>
    </div>
  </div>
  <div v-else-if="course.isLoading">Loading...</div>
</template>

<script setup lang="ts">
import { useCourseDetails } from '@/features/courses/composables/useCourses'

const props = defineProps<{
  id: string
}>()

const course = useCourseDetails(props.id)
</script>

<style lang="scss" scoped>
.course-details {
  padding: 2rem;
  max-width: 900px;
  margin: 0 auto;
}

.module {
  margin-top: 1rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
}

.review {
  margin-top: 1rem;
  padding: 1rem;
  background: rgba(255, 255, 255, 0.05);
  border-radius: 8px;
}
</style>