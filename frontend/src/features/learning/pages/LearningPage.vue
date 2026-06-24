<template>
  <div class="learning-page" v-if="lesson">
    <div class="container">
      <div class="video-section">
        <div class="video-container glass">
          <video :src="lesson.videoUrl" controls></video>
        </div>
        <div class="lesson-info glass">
          <h1>{{ lesson.title }}</h1>
          <p v-if="lesson.description">{{ lesson.description }}</p>
          <div class="lesson-meta">
            <span>{{ lesson.moduleTitle }}</span>
            <span>{{ lesson.duration }} min</span>
          </div>
          <button class="btn btn-primary" @click="complete" :disabled="!!lesson?.isCompleted || isSubmitting">
            {{ lesson.isCompleted ? 'Ukończono' : 'Oznacz jako ukończone' }}
          </button>
        </div>
      </div>
    </div>
  </div>
  <div v-else-if="lessonQuery.isLoading" class="loading">Ładowanie...</div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useLesson, useCompleteLesson } from '@/features/learning/composables/useLearning'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const lessonQuery = useLesson(props.courseId, props.lessonId)
const lesson = computed(() => lessonQuery.data.value)
const completeMutation = useCompleteLesson()

const isSubmitting = computed(() => completeMutation.isPending.value)

function complete() {
  completeMutation.mutate({ courseId: props.courseId, lessonId: props.lessonId })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.learning-page {
  padding: calc($header-height + 40px) 0 80px;
}

.video-section {
  display: grid;
  grid-template-columns: 1fr 0.35fr;
  gap: 24px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
  }
}

.video-container {
  --lg-r: 28px;
  --lg-blur: 0px;
  padding: 16px;
  overflow: hidden;

  video {
    width: 100%;
    border-radius: 20px;
    display: block;
  }
}

.lesson-info {
  --lg-r: 28px;
  --lg-blur: 0px;
  padding: 28px;
  align-self: start;

  h1 {
    font-size: 1.4rem;
    margin-bottom: 12px;
  }

  p {
    color: $color-muted;
    font-size: 0.95rem;
    margin-bottom: 20px;
  }
}

.lesson-meta {
  display: flex;
  gap: 12px;
  margin-bottom: 24px;

  span {
    padding: 6px 12px;
    border-radius: 999px;
    font-size: 0.82rem;
    background: rgba(255, 255, 255, 0.06);
    color: $color-muted;
  }
}

.btn {
  width: 100%;
  padding: 14px;
}

.loading {
  text-align: center;
  padding: 120px;
  color: $color-muted;
}
</style>
