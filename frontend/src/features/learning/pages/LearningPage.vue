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
          <video :key="videoUrl ?? lesson.id" :src="videoUrl ?? undefined" controls crossorigin="anonymous"></video>
        </div>
        <div class="lesson-info glass">
          <div class="lesson-header">
            <h1>{{ lesson.title }}</h1>
            <div class="progress-badge">{{ progressPercent }}%</div>
          </div>
          <div class="progress-bar">
            <div class="progress-fill" :style="{ width: `${progressPercent}%` }"></div>
          </div>
          <p v-if="lesson.description">{{ lesson.description }}</p>
          <div class="lesson-meta">
            <span>{{ lesson.moduleTitle }}</span>
            <span>{{ lesson.duration }} min</span>
          </div>
          <button class="btn btn-primary" @click="complete" :disabled="!!lesson?.isCompleted || isSubmitting">
            {{ lesson.isCompleted ? 'Ukończono' : 'Oznacz jako ukończone' }}
          </button>
          <div class="lesson-navigation">
            <button
              type="button"
              class="btn btn-ghost"
              :disabled="!previousLesson"
              @click="previousLesson && navigateToLesson(previousLesson.id)"
            >
              ← Poprzednia lekcja
            </button>
            <button
              type="button"
              class="btn btn-ghost"
              :disabled="!nextLesson"
              @click="nextLesson && navigateToLesson(nextLesson.id)"
            >
              Następna lekcja →
            </button>
          </div>
        </div>
        <LessonQuizPanel :course-id="courseId" :lesson-id="lessonId" />
        <LessonDiscussionPanel :course-id="courseId" :lesson-id="lessonId" />
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
import { computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useLesson, useCompleteLesson, useLessonVideoUrl } from '@/features/learning/composables/useLearning'
import { useCourseDetails } from '@/features/courses/composables/useCourses'
import CourseSidebar from '@/features/learning/components/CourseSidebar.vue'
import LessonDiscussionPanel from '@/features/lesson-discussion/components/LessonDiscussionPanel.vue'
import LessonQuizPanel from '@/features/quizzes/components/LessonQuizPanel.vue'
import { toast } from '@/shared/toast/toast'
import type { ModuleDto, LessonListDto } from '@/features/courses/types/course.types'

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const router = useRouter()
const lessonQuery = useLesson(() => props.courseId, () => props.lessonId)
const courseQuery = useCourseDetails(() => props.courseId)
const videoUrlQuery = useLessonVideoUrl(() => props.courseId, () => props.lessonId)
const lesson = computed(() => lessonQuery.data.value)
const course = computed(() => courseQuery.data.value)
const videoUrl = computed(() => videoUrlQuery.data.value?.url)
const completeMutation = useCompleteLesson()

const isSubmitting = computed(() => completeMutation.isPending.value)

const allLessons = computed<LessonListDto[]>(() => {
  return course.value?.modules.flatMap((m) => m.lessons) ?? []
})

const completedCount = computed(() => {
  return allLessons.value.filter((l) => l.isCompleted).length
})

const totalLessons = computed(() => allLessons.value.length)

const progressPercent = computed(() => {
  if (totalLessons.value === 0) return 0
  return Math.round((completedCount.value / totalLessons.value) * 100)
})

const lessonIndex = computed(() => {
  return allLessons.value.findIndex((l) => l.id === props.lessonId)
})

const previousLesson = computed<LessonListDto | null>(() => {
  return allLessons.value[lessonIndex.value - 1] ?? null
})

const nextLesson = computed<LessonListDto | null>(() => {
  return allLessons.value[lessonIndex.value + 1] ?? null
})

function navigateToLesson(lessonId: string) {
  router.push({ name: 'Learning', params: { courseId: props.courseId, lessonId } })
}

function complete() {
  completeMutation.mutate(
    { courseId: props.courseId, lessonId: props.lessonId },
    {
      onSuccess: () => {
        toast.success('Lekcja ukończona')
        if (progressPercent.value >= 100) {
          toast.success('Gratulacje! Ukończyłeś kurs')
        }
      }
    }
  )
}

watch(
  () => completeMutation.isSuccess.value,
  (success) => {
    if (success && nextLesson.value) {
      navigateToLesson(nextLesson.value.id)
    }
  }
)
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
    margin-bottom: 0;
  }

  p {
    color: $color-muted;
    font-size: 0.95rem;
    margin-bottom: 20px;
  }
}

.lesson-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.progress-badge {
  font-size: 0.85rem;
  font-weight: 700;
  color: $color-gold;
  padding: 6px 12px;
  border-radius: 999px;
  background: rgba(245, 158, 11, 0.12);
}

.progress-bar {
  height: 6px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
  overflow: hidden;
  margin-bottom: 20px;
}

.progress-fill {
  height: 100%;
  border-radius: 999px;
  background: linear-gradient(90deg, $color-gold, #fbbf24);
  transition: width 0.4s ease;
}

.lesson-navigation {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  margin-top: 20px;

  button {
    flex: 1;
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
