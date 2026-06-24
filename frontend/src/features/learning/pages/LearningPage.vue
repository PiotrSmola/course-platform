<template>
  <div class="learning-page" v-if="lesson.data?.value">
    <h1>{{ lesson.data.value.title }}</h1>
    <video :src="lesson.data.value.videoUrl" controls></video>
    <button @click="complete">Mark as Complete</button>
  </div>
  <div v-else-if="lesson.isLoading">Loading...</div>
</template>

<script setup lang="ts">
import { useLesson, useCompleteLesson } from '@/features/learning/composables/useLearning'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const lesson = useLesson(props.courseId, props.lessonId)
const completeMutation = useCompleteLesson()

function complete() {
  completeMutation.mutate({ courseId: props.courseId, lessonId: props.lessonId })
}
</script>

<style lang="scss" scoped>
.learning-page {
  padding: 2rem;

  video {
    width: 100%;
    max-width: 900px;
    margin-top: 1rem;
  }

  button {
    margin-top: 1rem;
    padding: 0.75rem 1.5rem;
    cursor: pointer;
  }
}
</style>
