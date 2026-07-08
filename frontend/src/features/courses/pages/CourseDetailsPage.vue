<template>
  <div class="course-details" v-if="course">
    <div class="container">
      <div class="hero-section">
        <div class="hero-content">
          <span class="eyebrow">{{ course.instructorName }}</span>
          <h1>{{ course.title }}</h1>
          <p class="description">{{ course.description }}</p>
          <div class="meta">
            <span class="badge level">{{ levelLabel }}</span>
            <span class="badge modules">{{ course.modules.length }} modułów</span>
            <span class="badge price">{{ course.price }} zł</span>
          </div>
          <div class="actions" v-if="!isInstructor">
            <template v-if="authStore.isAuthenticated && !isEnrolled">
              <button
                v-if="isPaid"
                class="btn btn-primary"
                :disabled="checkoutMutation.isPending.value"
                @click="buy"
              >
                {{ checkoutMutation.isPending.value ? 'Przekierowujemy...' : `Kup teraz — ${course.price} zł` }}
              </button>
              <button v-else class="btn btn-primary" @click="enroll">Zapisz się</button>
            </template>
            <router-link class="btn btn-primary" :to="{ name: 'MyCourses' }" v-else-if="authStore.isAuthenticated && isEnrolled">Przejdź do kursu</router-link>
            <router-link class="btn btn-primary" :to="{ name: 'Login', query: { redirect: route.fullPath } }" v-else>Zaloguj się, aby zapisać</router-link>
          </div>
        </div>
        <div class="hero-visual">
          <CourseThumbnail class="glass-card course-thumb" :url="course.thumbnailUrl" />
        </div>
      </div>

      <div class="content-grid">
        <div class="modules-section">
          <h2>Program kursu</h2>
          <div class="module-list">
            <div v-for="(module, mIdx) in course.modules" :key="module.id" class="module-item glass-card">
              <div class="module-header">
                <span class="module-num">{{ mIdx + 1 }}</span>
                <h3>{{ module.title }}</h3>
              </div>
              <div class="lessons-list">
                <template v-for="lesson in module.lessons" :key="lesson.id">
                  <router-link
                    v-if="isEnrolled"
                    class="lesson-item link"
                    :to="{ name: 'Learning', params: { courseId: course.id, lessonId: lesson.id } }"
                  >
                    <span class="lesson-icon">{{ lesson.isCompleted ? '✓' : '▶' }}</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </router-link>
                  <div v-else class="lesson-item">
                    <span class="lesson-icon">▶</span>
                    <span class="lesson-title">{{ lesson.title }}</span>
                    <span class="lesson-duration">{{ lesson.duration }} min</span>
                  </div>
                </template>
              </div>
            </div>
          </div>
        </div>

        <div class="reviews-section">
          <h2>Opinie</h2>

          <form v-if="course.canReview" class="review-form glass-card" @submit.prevent="submitReview">
            <h3>Dodaj opinię</h3>
            <div class="rating-row">
              <label>Ocena</label>
              <select v-model.number="reviewRating">
                <option v-for="n in 5" :key="n" :value="n">{{ n }} ★</option>
              </select>
            </div>
            <textarea v-model="reviewComment" rows="3" placeholder="Twoja opinia o kursie..." required />
            <button type="submit" class="btn btn-primary" :disabled="createReview.isPending.value || !reviewComment.trim()">
              Opublikuj opinię
            </button>
          </form>
          <p v-else-if="course.isEnrolled && course.hasUserReviewed" class="review-note">Dodałeś już opinię do tego kursu.</p>

          <div class="reviews-list">
            <div v-for="review in course.reviews" :key="review.id" class="review-card glass-card">
              <div class="stars">{{ '★'.repeat(review.rating) }}</div>
              <p>{{ review.comment }}</p>
              <div class="review-author">
                <span class="avatar">{{ review.authorName.charAt(0) }}</span>
                <span>{{ review.authorName }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <div v-else-if="courseQuery.isLoading" class="loading">Ładowanie...</div>
  <div v-else-if="courseQuery.isError" class="error-state">
    <div class="container">
      <h2>Nie znaleziono kursu</h2>
      <p>Kurs nie istnieje lub nie masz do niego dostępu.</p>
      <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przeglądaj kursy</router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCourseDetails } from '@/features/courses/composables/useCourses'
import { useEnroll } from '@/features/enrollment/composables/useEnrollment'
import { useCreateCheckout } from '@/features/payments/composables/usePayments'
import { useCreateReview } from '@/features/reviews/composables/useReviews'
import { CourseLevel } from '@/features/courses/types/course.types'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'

const route = useRoute()
const authStore = useAuthStore()
const courseQuery = useCourseDetails(() => route.params.id as string)
const course = computed(() => courseQuery.data.value)
const enrollMutation = useEnroll()
const reviewRating = ref(5)
const reviewComment = ref('')
const createReview = useCreateReview(() => route.params.id as string)

const checkoutMutation = useCreateCheckout()

const isInstructor = computed(() => authStore.user?.id === course.value?.instructorId)
const isEnrolled = computed(() => course.value?.isEnrolled ?? false)
const isPaid = computed(() => (course.value?.price ?? 0) > 0)

const levelLabel = computed(() => {
  if (!course.value) return ''
  switch (course.value.level) {
    case CourseLevel.Beginner: return 'Początkujący'
    case CourseLevel.Intermediate: return 'Średni'
    case CourseLevel.Advanced: return 'Zaawansowany'
    default: return ''
  }
})

const enroll = () => {
  if (!course.value) return
  enrollMutation.mutate(course.value.id)
}

const buy = () => {
  if (!course.value) return
  checkoutMutation.mutate(course.value.id)
}

function submitReview() {
  if (!reviewComment.value.trim()) return
  createReview.mutate(
    { rating: reviewRating.value, comment: reviewComment.value },
    { onSuccess: () => { reviewComment.value = '' } }
  )
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.course-details {
  padding: calc($header-height + 40px) 0 80px;
}

.error-state {
  padding: calc($header-height + 80px) 0 80px;
  text-align: center;

  h2 {
    font-size: 1.5rem;
    margin-bottom: 12px;
  }

  p {
    color: $color-muted;
    margin-bottom: 24px;
  }
}

.hero-section {
  display: grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 60px;
  align-items: center;
  margin-bottom: 80px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
  }
}

.hero-content {
  h1 {
    font-size: clamp(2rem, 4vw, 3.2rem);
    margin: 16px 0 20px;
  }

  .description {
    color: $color-muted;
    font-size: 1.05rem;
    line-height: 1.7;
    margin-bottom: 24px;
  }
}

.meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  margin-bottom: 32px;
}

.badge {
  padding: 6px 14px;
  border-radius: 999px;
  font-size: 0.82rem;
  font-weight: 600;
  background: rgba(255, 255, 255, 0.06);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.25), 0 0 0 1px rgba(255, 255, 255, 0.08);
}

.price {
  background: rgba(139, 92, 246, 0.2);
  color: #d6c9ff;
}

.actions .btn {
  padding: 14px 32px;
}

.hero-visual {
  display: flex;
  justify-content: center;
}

.course-thumb {
  width: 100%;
  max-width: 420px;
  aspect-ratio: 16/10;
  border-radius: 28px;
  box-shadow: 0 40px 90px rgba(3, 6, 24, 0.6), 0 4px 14px rgba(3, 6, 24, 0.35);
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 0.4fr;
  gap: 40px;

  @media (max-width: 880px) {
    grid-template-columns: 1fr;
  }
}

.reviews-section h2 {
  font-size: 1.5rem;
  margin-bottom: 28px;
}

.review-form {
  padding: 24px;
  margin-bottom: 24px;

  h3 {
    font-size: 1rem;
    margin-bottom: 16px;
  }

  textarea {
    width: 100%;
    margin: 12px 0 16px;
    padding: 12px;
    border-radius: 12px;
    border: none;
    background: rgba(255, 255, 255, 0.04);
    color: $color-ink;
    font: inherit;
    resize: vertical;
  }

  select {
    margin-left: 12px;
    padding: 6px 12px;
    border-radius: 8px;
    background: rgba(255, 255, 255, 0.06);
    color: $color-ink;
    border: none;
  }
}

.review-note {
  color: $color-muted;
  margin-bottom: 20px;
  font-size: 0.9rem;
}

.lesson-item.link {
  text-decoration: none;
  color: inherit;
  transition: background 0.2s;

  &:hover {
    background: rgba(255, 255, 255, 0.06);
  }
}

.modules-section h2,
.reviews-section h2 {
  font-size: 1.5rem;
  margin-bottom: 28px;
}

.module-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.module-item {
  padding: 24px;
}

.module-header {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 16px;

  .module-num {
    width: 32px;
    height: 32px;
    display: grid;
    place-items: center;
    border-radius: 50%;
    background: rgba(139, 92, 246, 0.25);
    font-size: 0.85rem;
    font-weight: 700;
  }

  h3 {
    font-size: 1.1rem;
  }
}

.lessons-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding-left: 46px;
}

.lesson-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 14px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.03);
  font-size: 0.92rem;

  .lesson-icon {
    color: $color-gold;
    font-size: 0.8rem;
  }

  .lesson-title {
    flex: 1;
  }

  .lesson-duration {
    color: $color-faint;
    font-size: 0.82rem;
  }
}

.reviews-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.review-card {
  padding: 24px;

  .stars {
    color: #fcd34d;
    font-size: 1.1rem;
    margin-bottom: 12px;
  }

  p {
    color: $color-muted;
    font-size: 0.95rem;
    margin-bottom: 16px;
  }

  .review-author {
    display: flex;
    align-items: center;
    gap: 10px;

    .avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      display: grid;
      place-items: center;
      font-size: 0.8rem;
      font-weight: 700;
      background: linear-gradient(135deg, rgba(139, 92, 246, 0.35), rgba(34, 211, 238, 0.25));
    }

    span:last-child {
      font-size: 0.9rem;
      color: $color-muted;
    }
  }
}

.loading {
  text-align: center;
  padding: 120px;
  color: $color-muted;
}
</style>
