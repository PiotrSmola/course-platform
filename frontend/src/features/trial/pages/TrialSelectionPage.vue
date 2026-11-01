<template>
  <div class="trial-page">
    <section class="trial-hero">
      <div class="container">
        <header class="section-head trial-head">
          <span class="eyebrow">Bezpłatny dostęp</span>
          <h1>Wybierz kurs na swój <em>trial</em></h1>
          <p>
            Poznaj sposób nauki na CoursePlatform bez opłat. W wybranym kursie odblokujemy dla Ciebie
            dwie pierwsze lekcje.
          </p>
        </header>
      </div>
    </section>

    <section class="trial-content">
      <div class="container">
        <div v-if="!authStore.isAuthenticated" class="trial-gate glass">
          <div>
            <h2>Załóż bezpłatne konto</h2>
            <p>Wybór kursu zapisujemy do Twojego konta, aby dostęp był dostępny także na innych urządzeniach.</p>
          </div>
          <router-link class="btn btn-primary" :to="{ name: 'Register', query: { redirect: '/trial' } }">
            Utwórz konto
          </router-link>
        </div>

        <div v-else-if="eligibleCoursesQuery.isLoading.value" class="state-card state-card--loading">
          <span class="spinner" aria-hidden="true" />
          <p>Przygotowujemy kursy dostępne w trialu…</p>
        </div>

        <div v-else-if="eligibleCoursesQuery.isError.value" class="state-card">
          <h2>Nie udało się pobrać kursów</h2>
          <p>Odśwież stronę lub spróbuj ponownie za chwilę.</p>
          <button type="button" class="btn btn-ghost" @click="eligibleCoursesQuery.refetch()">Spróbuj ponownie</button>
        </div>

        <div v-else-if="!eligibleCourses.length" class="state-card">
          <h2>Trial został już wykorzystany</h2>
          <p>Możesz wybrać kolejny kurs z katalogu albo wrócić do kursu, który został odblokowany wcześniej.</p>
          <router-link class="btn btn-primary" :to="{ name: 'Courses' }">Przejdź do katalogu</router-link>
        </div>

        <template v-else>
          <div class="trial-heading">
            <div>
              <h2>Wybierz jeden kurs</h2>
              <p>Trial jest jednorazowy i pozostaje przypisany do wybranego kursu.</p>
            </div>
            <span class="trial-limit">2 pierwsze lekcje</span>
          </div>

          <div class="trial-grid">
            <article v-for="course in eligibleCourses" :key="course.id" class="trial-card glass-card">
              <CourseThumbnail class="trial-card__thumbnail" :url="course.thumbnailUrl" :title="course.title" :seed="course.id" />
              <div class="trial-card__body">
                <p class="trial-card__eyebrow">
                  <span>{{ course.lessonCount }} lekcji</span>
                  <span>{{ formatPrice(course.price) }}</span>
                </p>
                <h3>{{ course.title }}</h3>
                <p class="trial-card__author">{{ course.instructorName }}</p>
                <p class="trial-card__description">{{ course.shortDescription }}</p>
                <button
                  type="button"
                  class="btn btn-primary trial-card__action"
                  :disabled="activateTrialMutation.isPending.value"
                  @click="chooseTrial(course.id)"
                >
                  {{ selectedCourseId === course.id ? 'Aktywujemy…' : 'Wybieram ten kurs' }}
                </button>
              </div>
            </article>
          </div>
        </template>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useActivateTrial, useTrialEligibleCourses } from '@/features/trial/composables/useTrial'
import CourseThumbnail from '@/shared/components/media/CourseThumbnail.vue'

const authStore = useAuthStore()
const eligibleCoursesQuery = useTrialEligibleCourses(authStore.isAuthenticated)
const activateTrialMutation = useActivateTrial()
const selectedCourseId = ref<string | null>(null)

const eligibleCourses = computed(() => eligibleCoursesQuery.data.value ?? [])

function formatPrice(price: number): string {
  return new Intl.NumberFormat('pl-PL', {
    style: 'currency',
    currency: 'PLN',
    maximumFractionDigits: 0
  }).format(price)
}

async function chooseTrial(courseId: string) {
  selectedCourseId.value = courseId
  try {
    await activateTrialMutation.mutateAsync({ courseId })
  } finally {
    selectedCourseId.value = null
  }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.trial-page {
  min-height: 100%;
}

.trial-hero {
  padding: 32px 0 0;

  h1 {
    font-size: clamp(2.4rem, 4.6vw, 3.8rem);
    margin-top: 18px;
  }
}

.trial-head {
  margin-bottom: 40px;
}

.trial-content {
  padding: 0 0 96px;
}

.trial-gate {
  --lg-r: 18px;
  --lg-blur: 10px;
  --lg-tint: rgba(13, 15, 19, 0.3);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 28px;
  max-width: 920px;
  padding: 24px 24px 24px 28px;

  h2 {
    font-size: 1.4rem;
    font-weight: 600;
    margin-bottom: 6px;
  }

  p {
    color: $color-muted;
    line-height: 1.55;
  }

  @media (max-width: 620px) {
    flex-direction: column;
    align-items: flex-start;
    padding: 24px 20px;

    .btn {
      width: 100%;
    }
  }
}

.state-card {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: 12px;
  max-width: 720px;
  padding: 32px 0;
  border-top: 1px solid $color-hairline;

  h2 {
    font-size: 1.6rem;
    font-weight: 600;
  }

  p {
    color: $color-muted;
    line-height: 1.55;
  }

  .btn {
    margin-top: 8px;
  }
}

.state-card--loading {
  flex-direction: row;
  align-items: center;

  p {
    @include mono-label;
    color: $color-faint;
  }
}

.spinner {
  width: 22px;
  height: 22px;
  border: 2px solid rgba($color-ink, 0.1);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: trial-spin 0.8s linear infinite;
}

.trial-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 24px;
  padding-bottom: 20px;
  margin-bottom: 28px;
  border-bottom: 1px solid $color-hairline;

  h2 {
    font-size: clamp(1.6rem, 3vw, 2.2rem);
    margin-bottom: 8px;
  }

  p {
    color: $color-muted;
  }

  @media (max-width: 620px) {
    align-items: flex-start;
    flex-direction: column;
  }
}

.trial-limit {
  @include tag;
  flex-shrink: 0;
  color: $color-accent;
  box-shadow: inset 0 0 0 1px rgba($color-accent, 0.4);
}

.trial-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 24px;

  @media (max-width: 980px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 620px) {
    grid-template-columns: 1fr;
  }
}

.trial-card {
  --lg-blur: 10px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.trial-card__thumbnail {
  position: relative;
  aspect-ratio: 16 / 9;
  margin: 12px 12px 0;
  border-radius: 14px 14px 14px 4px;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.08);
}

.trial-card__body {
  display: flex;
  flex: 1;
  flex-direction: column;
  padding: 16px 20px 20px;
}

.trial-card__eyebrow {
  @include mono-label;
  display: flex;
  gap: 10px;
  margin-bottom: 10px;
  font-size: 0.64rem;
  color: $color-faint;

  span + span::before {
    content: "/";
    margin-right: 10px;
    color: $color-hairline;
  }
}

.trial-card h3 {
  font-size: 1.28rem;
  font-weight: 600;
  line-height: 1.2;
}

.trial-card__author {
  margin-top: 6px;
  color: $color-muted;
  font-size: 0.84rem;
}

.trial-card__description {
  display: -webkit-box;
  margin: 14px 0 20px;
  padding-top: 14px;
  overflow: hidden;
  color: $color-muted;
  font-size: 0.9rem;
  line-height: 1.55;
  border-top: 1px dashed $color-hairline;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 4;
}

.trial-card__action {
  width: 100%;
  margin-top: auto;
  padding-inline: 16px;
}

@keyframes trial-spin {
  to { transform: rotate(360deg); }
}
</style>
