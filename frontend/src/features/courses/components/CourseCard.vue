<template>
  <div class="course-card">
    <img :src="course.thumbnailUrl" :alt="course.title" v-if="course.thumbnailUrl" />
    <div class="content">
      <h3>{{ course.title }}</h3>
      <p>{{ course.shortDescription }}</p>
      <div class="meta">
        <span>{{ course.instructorName }}</span>
        <span>{{ course.price }}$</span>
      </div>
      <router-link :to="{ name: 'CourseDetails', params: { id: course.id } }">View</router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { CourseListDto } from '@/features/courses/types/course.types'

interface Props {
  course: CourseListDto
}

defineProps<Props>()
</script>

<style lang="scss" scoped>
.course-card {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  overflow: hidden;
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.2);

  img {
    width: 100%;
    height: 180px;
    object-fit: cover;
  }

  .content {
    padding: 1rem;
  }

  .meta {
    display: flex;
    justify-content: space-between;
    margin-top: 0.5rem;
  }

  a {
    display: inline-block;
    margin-top: 0.5rem;
    color: #fff;
  }
}
</style>
