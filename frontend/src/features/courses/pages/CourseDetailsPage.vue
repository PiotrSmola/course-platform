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
          <div class="actions" v-if="authStore.isAuthenticated && !isInstructor">
            <button class="btn btn-primary" @click="enroll" v-if="!isEnrolled">Zapisz się</button>
            <router-link class="btn btn-primary" :to="{ name: 'MyCourses' }" v-else>Przejdź do kursu</router-link>
          </div>
        </div>
        <div class="hero-visual">
          <div class="glass-card course-thumb" :style="{ backgroundImage: `url(${course.thumbnailUrl})` }"></div>
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
                <div v-for="lesson in module.lessons" :key="lesson.id" class="lesson-item">
                  <span class="lesson-icon">▶</span>
                  <span class="lesson-title">{{ lesson.title }}</span>
                  <span class="lesson-duration">{{ lesson.duration }} min</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="reviews-section">
          <h2>Opinie</h2>
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
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCourseDetails } from '@/features/courses/composables/useCourses'
import { CourseLevel } from '@/features/courses/types/course.types'

const route = useRoute()
const authStore = useAuthStore()
const courseQuery = useCourseDetails(route.params.id as string)
const course = computed(() => courseQuery.data.value)

const isInstructor = computed(() => authStore.user?.id === course.value?.instructorId)
const isEnrolled = false // TODO: check enrollment

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
  // TODO: implement enrollment
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.course-details {
  padding: calc($header-height + 40px) 0 80px;
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
  background-size: cover;
  background-position: center;
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