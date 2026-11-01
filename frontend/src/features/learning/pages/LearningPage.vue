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
            <span class="player-label">Odtwarzacz</span>
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
        <div class="lesson-info">
          <p class="lesson-kicker">
            <span v-if="lessonIndex >= 0">Lekcja {{ String(lessonIndex + 1).padStart(2, '0') }} / {{ String(totalLessons).padStart(2, '0') }}</span>
            <span>{{ lesson.moduleTitle }}</span>
            <span>{{ lesson.duration }} min</span>
          </p>
          <div class="lesson-header">
            <h1>{{ lesson.title }}</h1>
          </div>
          <p v-if="lesson.description" class="lesson-description">{{ lesson.description }}</p>
          <p v-if="isTrialAccess" class="trial-notice">Trial obejmuje dwie pierwsze lekcje kursu.</p>
          <div v-if="!isTrialAccess" class="course-progress">
            <span class="progress-label">Postęp kursu</span>
            <div class="progress-bar" role="progressbar" :aria-valuenow="progressPercent" aria-valuemin="0" aria-valuemax="100">
              <div class="progress-fill" :style="{ width: `${progressPercent}%` }"></div>
            </div>
            <span class="progress-badge">{{ progressPercent }}%</span>
          </div>
          <div class="lesson-navigation">
            <button
              type="button"
              class="btn btn-ghost nav-btn"
              :disabled="!previousLesson"
              @click="previousLesson && navigateToLesson(previousLesson.id)"
            >
              ← Poprzednia lekcja
            </button>
            <button class="btn btn-primary complete-btn" @click="complete" :disabled="!!lesson?.isCompleted || isSubmitting">
              {{ lesson.isCompleted ? 'Ukończono' : 'Oznacz jako ukończone' }}
            </button>
            <button
              type="button"
              class="btn btn-ghost nav-btn"
              :disabled="!nextLesson"
              @click="nextLesson && navigateToLesson(nextLesson.id)"
            >
              Następna lekcja →
            </button>
          </div>
        </div>
        <LessonQuizPanel :course-id="courseId" :lesson-id="lessonId" />
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
.learning-page {
  padding: 24px 0 80px;
}

.learning-layout {
  display: grid;
  grid-template-columns: 320px minmax(0, 1fr);
  gap: 32px;
  align-items: start;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
    gap: 20px;
  }
}

.video-section {
  display: flex;
  flex-direction: column;
  gap: 28px;
  min-width: 0;
}

.video-container {
  --lg-r: #{$radius-card};
  --lg-blur: 12px;
  --lg-tint: rgba(13, 15, 19, 0.28);
  padding: 12px;
  overflow: hidden;

  video {
    display: block;
    width: 100%;
    aspect-ratio: 16 / 9;
    border-radius: 14px 14px 14px 4px;
    background: #050608;
    box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.08);
  }
}

.playback-controls {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 6px 2px;

  .player-label {
    @include mono-label;
    margin-right: auto;
    font-size: 0.64rem;
    color: $color-faint;
  }

  label {
    @include mono-label;
    font-size: 0.64rem;
    color: $color-muted;
  }

  select {
    min-height: 36px;
    padding: 0 12px;
    border: none;
    border-radius: $radius-sm;
    background: rgba(13, 15, 19, 0.45);
    box-shadow: inset 0 0 0 1px $color-hairline;
    color: $color-ink;
    font-family: $font-mono;
    font-size: 0.8rem;

    &:focus {
      outline: none;
      box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
    }
  }
}

.lesson-info {
  padding: 4px 4px 28px;
  border-bottom: 1px solid $color-hairline;
}

.lesson-kicker {
  @include mono-label;
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 14px;
  font-size: 0.66rem;
  color: $color-faint;

  span:first-child {
    color: $color-accent;
  }

  span + span::before {
    content: "/";
    margin-right: 10px;
    color: $color-hairline;
  }
}

.lesson-header h1 {
  margin: 0;
  font-size: clamp(1.7rem, 2.8vw, 2.4rem);
  font-weight: 600;
  line-height: 1.08;
}

.lesson-description {
  max-width: 68ch;
  margin: 16px 0 0;
  color: $color-muted;
  font-size: 0.98rem;
  line-height: 1.6;
}

.trial-notice {
  @include mono-label;
  margin: 18px 0 0;
  color: $color-accent;
}

.course-progress {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: center;
  gap: 16px;
  margin-top: 26px;
}

.progress-label {
  @include mono-label;
  font-size: 0.64rem;
  color: $color-faint;
}

.progress-bar {
  height: 4px;
  border-radius: 2px;
  background: rgba($color-ink, 0.1);
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  border-radius: 2px;
  background: $color-accent;
  transition: width 0.4s $ease-out;
}

.progress-badge {
  font-family: $font-mono;
  font-size: 0.78rem;
  font-variant-numeric: tabular-nums;
  color: $color-ink;
}

.lesson-navigation {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 26px;

  .complete-btn {
    margin-left: auto;
  }

  .btn {
    padding: 12px 22px;
    font-size: 0.9rem;
  }
}

.loading,
.error-state {
  text-align: center;
  padding: 160px 24px 120px;
  color: $color-muted;

  h2 {
    color: $color-ink;
    margin-bottom: 12px;
  }

  p {
    margin-bottom: 24px;
  }
}

.loading {
  @include mono-label;
}

@media (max-width: 560px) {
  .learning-page {
    padding-top: 12px;
  }

  .course-progress {
    grid-template-columns: minmax(0, 1fr) auto;

    .progress-label {
      grid-column: 1 / -1;
    }
  }

  .lesson-navigation {
    flex-direction: column;
    align-items: stretch;

    .complete-btn {
      order: -1;
      margin-left: 0;
    }

    .btn {
      width: 100%;
    }
  }
}
</style>
