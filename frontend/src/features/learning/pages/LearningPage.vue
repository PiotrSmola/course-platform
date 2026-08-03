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
          <video
            ref="videoRef"
            :key="videoUrl ?? lesson.id"
            :src="videoUrl ?? undefined"
            controls
            crossorigin="anonymous"
            @loadedmetadata="onLoadedMetadata"
            @timeupdate="onTimeUpdate"
            @pause="savePosition"
          ></video>
          <div class="playback-controls">
            <label for="playback-rate">Prędkość</label>
            <select id="playback-rate" v-model.number="playbackRate" @change="applyPlaybackRate">
              <option :value="0.75">0.75x</option>
              <option :value="1">1x</option>
              <option :value="1.25">1.25x</option>
              <option :value="1.5">1.5x</option>
              <option :value="1.75">1.75x</option>
              <option :value="2">2x</option>
            </select>
          </div>
        </div>
        <div class="lesson-info glass">
          <div class="lesson-header">
            <h1>{{ lesson.title }}</h1>
            <div v-if="!isTrialAccess" class="progress-badge">{{ progressPercent }}%</div>
          </div>
          <div v-if="!isTrialAccess" class="progress-bar">
            <div class="progress-fill" :style="{ width: `${progressPercent}%` }"></div>
          </div>
          <p v-if="lesson.description">{{ lesson.description }}</p>
          <p v-if="isTrialAccess" class="trial-notice">Trial obejmuje odtwarzanie dwoch pierwszych lekcji.</p>
          <div class="lesson-meta">
            <span>{{ lesson.moduleTitle }}</span>
            <span>{{ lesson.duration }} min</span>
          </div>
          <button v-if="!isTrialAccess" class="btn btn-primary" @click="complete" :disabled="!!lesson?.isCompleted || isSubmitting">
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
        <LessonQuizPanel v-if="!isTrialAccess" :course-id="courseId" :lesson-id="lessonId" />
        <LessonResourcesPanel v-if="!isTrialAccess" :course-id="courseId" :lesson-id="lessonId" />
        <LessonDiscussionPanel v-if="!isTrialAccess" :course-id="courseId" :lesson-id="lessonId" />
      </div>
    </div>
  </div>
  <div v-else-if="lessonQuery.isError" class="error-state">
    <div class="container">
      <h2>Brak dostępu do lekcji</h2>
      <p>{{ lessonAccessError }}</p>
      <router-link class="btn btn-primary" :to="{ name: 'CourseDetails', params: { id: courseId } }">
        Wróć do kursu
      </router-link>
    </div>
  </div>
  <div v-else class="loading">Ładowanie...</div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import {
  useLesson,
  useCompleteLesson,
  useLessonVideoUrl,
  useUpdateLessonWatchPosition
} from '@/features/learning/composables/useLearning'
import { useCourseDetails } from '@/features/courses/composables/useCourses'
import CourseSidebar from '@/features/learning/components/CourseSidebar.vue'
import LessonDiscussionPanel from '@/features/lesson-discussion/components/LessonDiscussionPanel.vue'
import LessonResourcesPanel from '@/features/lesson-resources/components/LessonResourcesPanel.vue'
import LessonQuizPanel from '@/features/quizzes/components/LessonQuizPanel.vue'
import { toast } from '@/shared/toast/toast'
import { getApiErrorMessage } from '@/shared/api/apiError'
import type { ModuleDto, LessonListDto } from '@/features/courses/types/course.types'

const PLAYBACK_RATE_KEY = 'cp.playbackRate'
const POSITION_SAVE_INTERVAL_MS = 8000

const props = defineProps<{
  courseId: string
  lessonId: string
}>()

const router = useRouter()
const authStore = useAuthStore()
const lessonQuery = useLesson(() => props.courseId, () => props.lessonId)
const courseQuery = useCourseDetails(() => props.courseId)
const videoUrlQuery = useLessonVideoUrl(() => props.courseId, () => props.lessonId)
const lesson = computed(() => lessonQuery.data.value)
const course = computed(() => courseQuery.data.value)
const videoUrl = computed(() => videoUrlQuery.data.value?.url)
const completeMutation = useCompleteLesson()
const positionMutation = useUpdateLessonWatchPosition()

const videoRef = ref<HTMLVideoElement | null>(null)
const playbackRate = ref(Number(localStorage.getItem(PLAYBACK_RATE_KEY) || '1') || 1)
let lastSavedAt = 0
let lastSavedPosition = -1

const isSubmitting = computed(() => completeMutation.isPending.value)
const isTrialAccess = computed(() => {
  const currentCourse = course.value
  return Boolean(
    currentCourse?.canAccessContent &&
    !currentCourse.isEnrolled &&
    !currentCourse.hasSubscriptionAccess &&
    currentCourse.instructorId !== authStore.user?.id &&
    !authStore.isAdmin
  )
})

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
  for (let i = lessonIndex.value - 1; i >= 0; i--) {
    const lessonItem = allLessons.value[i]
    if (lessonItem && !lessonItem.isLocked) return lessonItem
  }
  return null
})

const nextLesson = computed<LessonListDto | null>(() => {
  for (let i = lessonIndex.value + 1; i < allLessons.value.length; i++) {
    const lessonItem = allLessons.value[i]
    if (lessonItem && !lessonItem.isLocked) return lessonItem
  }
  return null
})

const nextLockedLesson = computed<LessonListDto | null>(() => {
  return allLessons.value[lessonIndex.value + 1] ?? null
})

const lessonAccessError = computed(() => {
  const message = getApiErrorMessage(lessonQuery.error.value)
  if (message?.toLowerCase().includes('previous') || message?.toLowerCase().includes('quiz')) {
    return 'Ta lekcja jest jeszcze zablokowana. Ukończ poprzednią lekcję i zalicz quiz, jeśli jest wymagany.'
  }
  return message || 'Musisz być zapisany na kurs, aby oglądać tę lekcję.'
})

function navigateToLesson(lessonId: string) {
  router.push({ name: 'Learning', params: { courseId: props.courseId, lessonId } })
}

function applyPlaybackRate() {
  localStorage.setItem(PLAYBACK_RATE_KEY, String(playbackRate.value))
  if (videoRef.value) {
    videoRef.value.playbackRate = playbackRate.value
  }
}

function onLoadedMetadata() {
  const video = videoRef.value
  if (!video) return
  video.playbackRate = playbackRate.value
  const position = lesson.value?.lastPositionSeconds ?? 0
  if (position > 0 && Number.isFinite(video.duration) && position < video.duration - 5) {
    video.currentTime = position
  }
}

function savePosition() {
  const video = videoRef.value
  if (isTrialAccess.value) return
  if (!video || !Number.isFinite(video.currentTime)) return
  const positionSeconds = Math.floor(video.currentTime)
  if (positionSeconds === lastSavedPosition) return
  lastSavedPosition = positionSeconds
  lastSavedAt = Date.now()
  positionMutation.mutate({
    courseId: props.courseId,
    lessonId: props.lessonId,
    positionSeconds
  })
}

function onTimeUpdate() {
  if (Date.now() - lastSavedAt < POSITION_SAVE_INTERVAL_MS) return
  savePosition()
}

function complete() {
  if (isTrialAccess.value) return
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
    if (!success) return
    if (nextLesson.value) {
      navigateToLesson(nextLesson.value.id)
      return
    }
    if (nextLockedLesson.value?.isLocked) {
      toast.info(nextLockedLesson.value.lockReason || 'Następna lekcja jest jeszcze zablokowana.')
    }
  }
)

watch(
  () => props.lessonId,
  () => {
    lastSavedAt = 0
    lastSavedPosition = -1
  }
)

function onBeforeUnload() {
  savePosition()
}

onMounted(() => {
  window.addEventListener('beforeunload', onBeforeUnload)
})

onBeforeUnmount(() => {
  window.removeEventListener('beforeunload', onBeforeUnload)
  savePosition()
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

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

.playback-controls {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 12px;

  label {
    color: $color-muted;
    font-size: 0.85rem;
  }

  select {
    border-radius: 10px;
    border: 1px solid rgba(255, 255, 255, 0.12);
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    padding: 6px 10px;
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

.trial-notice {
  margin: 0 0 20px;
  color: $color-gold !important;
  font-size: 0.9rem !important;
  line-height: 1.5;
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
.learning-layout {
  grid-template-columns: 320px minmax(0, 1fr);
}

.video-container,
.lesson-info {
  --lg-blur: 12px;
}

.playback-controls select {
  min-height: 40px;
  border-radius: 12px;
  background: rgba(15, 23, 42, 0.7);
}

.progress-badge,
.lesson-meta span {
  @include liquid-glass;
  --lg-r: 999px;
  --lg-blur: 10px;
  --lg-tint: rgba(255, 255, 255, 0.06);
  background: transparent;
}

@media (max-width: 880px) {
  .learning-layout {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 560px) {
  .learning-page {
    padding-top: calc($header-height + 20px);
  }

  .lesson-info {
    padding: 20px;
  }

  .lesson-header,
  .lesson-navigation,
  .playback-controls {
    align-items: stretch;
    flex-direction: column;
  }

  .lesson-navigation .btn,
  .playback-controls select {
    width: 100%;
    max-width: none;
  }
}

</style>
