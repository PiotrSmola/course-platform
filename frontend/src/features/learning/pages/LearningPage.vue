<template>
  <div class="learning-page" v-if="lesson && course">
    <div class="container learning-layout">
      <CourseSidebar
        :course-id="courseId"
        :current-lesson-id="lessonId"
        :modules="course.modules"
      />
      <div class="video-section">
        <div class="video-container glass">
          <video :key="lesson.videoUrl" :src="lesson.videoUrl" controls crossorigin="anonymous"></video>
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
  <div v-else-if="lessonQuery.isError" class="error-state">
    <div class="container">
      <h2>Brak dostępu do lekcji</h2>
      <p>Musisz być zapisany na kurs, aby oglądać tę lekcję.</p>
      <router-link class="btn btn-primary" :to="{ name: 'CourseDetails', params: { id: courseId } }">
        Wróć do kursu
      </router-link>
    </div>
  </div>
  <div v-else class="loading">Ładowanie...</div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useLesson, useCompleteLesson } from '@/features/learning/composables/useLearning'
import { useCourseDetails } from '@/features/courses/composables/useCourses'
import CourseSidebar from '@/features/learning/components/CourseSidebar.vue'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const lessonQuery = useLesson(() => props.courseId, () => props.lessonId)
const courseQuery = useCourseDetails(() => props.courseId)
const lesson = computed(() => lessonQuery.data.value)
const course = computed(() => courseQuery.data.value)
const completeMutation = useCompleteLesson()

const isSubmitting = computed(() => completeMutation.isPending.value)

function complete() {
  completeMutation.mutate({ courseId: props.courseId, lessonId: props.lessonId })
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.learning-page {
  padding: calc($header-height + 40px) 0 80px;
}

.learning-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 24px;
  align-items: start;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
  }
}

.video-section {
  display: flex;
  flex-direction: column;
  gap: 24px;
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
  max-width: 320px;
  padding: 14px;
}

.loading,
.error-state {
  text-align: center;
  padding: 120px 24px;
  color: $color-muted;

  h2 {
    color: $color-ink;
    margin-bottom: 12px;
  }

  p {
    margin-bottom: 24px;
  }
}
</style>
