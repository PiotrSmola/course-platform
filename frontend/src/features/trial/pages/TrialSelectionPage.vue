<template>
  <div class="trial-page">
    <section class="trial-hero">
      <div class="container">
        <span class="eyebrow">Bezpłatny dostęp</span>
        <h1>Wybierz kurs na swój trial</h1>
        <p>
          Poznaj sposób nauki na CoursePlatform bez opłat. W wybranym kursie odblokujemy dla Ciebie
          dwie pierwsze lekcje.
        </p>
      </div>
    </section>

    <section class="trial-content">
      <div class="container">
        <div v-if="!authStore.isAuthenticated" class="trial-gate glass-card">
          <div>
            <h2>Załóż bezpłatne konto</h2>
            <p>Wybór kursu zapisujemy do Twojego konta, aby dostęp był dostępny także na innych urządzeniach.</p>
          </div>
          <router-link class="btn btn-primary" :to="{ name: 'Register', query: { redirect: '/trial' } }">
            Utwórz konto
          </router-link>
        </div>

        <div v-else-if="eligibleCoursesQuery.isLoading.value" class="state-card glass-card">
          <span class="spinner" aria-hidden="true" />
          <p>Przygotowujemy kursy dostępne w trialu…</p>
        </div>

        <div v-else-if="eligibleCoursesQuery.isError.value" class="state-card glass-card">
          <h2>Nie udało się pobrać kursów</h2>
          <p>Odśwież stronę lub spróbuj ponownie za chwilę.</p>
          <button type="button" class="btn btn-ghost" @click="eligibleCoursesQuery.refetch()">Spróbuj ponownie</button>
        </div>

        <div v-else-if="!eligibleCourses.length" class="state-card glass-card">
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
              <CourseThumbnail class="trial-card__thumbnail" :url="course.thumbnailUrl" />
              <div class="trial-card__body">
                <p class="trial-card__eyebrow">{{ course.lessonCount }} lekcji · {{ formatPrice(course.price) }}</p>
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

.trial-page {
  min-height: 100%;
}

.trial-hero {
  padding: 44px 0 38px;
  text-align: center;
  border-bottom: 1px solid $color-hairline;

  .eyebrow {
    margin-bottom: 18px;
  }

  h1 {
    max-width: 680px;
    margin: 0 auto 14px;
    font-family: $font-display;
    font-size: $font-size-xl;
    line-height: 1.08;
    letter-spacing: -0.03em;
  }

  p {
    max-width: 640px;
    margin: 0 auto;
    color: $color-muted;
    line-height: 1.65;
  }
}

.trial-content {
  padding: 48px 0 96px;
}

.trial-gate,
.state-card {
  --lg-r: 26px;
  --lg-blur: 2px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 28px;
  max-width: 820px;
  margin: 0 auto;
  padding: 32px;

  h2 {
    font-family: $font-display;
    font-size: 1.35rem;
    margin-bottom: 8px;
  }

  p {
    color: $color-muted;
    line-height: 1.55;
  }

  @media (max-width: 620px) {
    flex-direction: column;
    align-items: flex-start;
    padding: 26px 22px;

    .btn {
      width: 100%;
    }
  }
}

.state-card {
  flex-direction: column;
  justify-content: center;
  text-align: center;
  padding: 52px 32px;
}

.spinner {
  width: 34px;
  height: 34px;
  border: 3px solid rgba(255, 255, 255, 0.12);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: trial-spin 0.8s linear infinite;
}

.trial-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 24px;
  margin-bottom: 26px;

  h2 {
    font-family: $font-display;
    font-size: clamp(1.45rem, 3vw, 2rem);
    letter-spacing: -0.02em;
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
  flex-shrink: 0;
  border-radius: 999px;
  padding: 8px 13px;
  color: $color-gold;
  font-size: 0.82rem;
  font-weight: 700;
  background: rgba(245, 158, 11, 0.1);
  box-shadow: inset 0 0 0 1px rgba(245, 158, 11, 0.24);
}

.trial-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 20px;

  @media (max-width: 980px) {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  @media (max-width: 620px) {
    grid-template-columns: 1fr;
  }
}

.trial-card {
  --lg-r: 22px;
  --lg-blur: 1.5px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.trial-card__thumbnail {
  width: calc(100% - 32px);
  aspect-ratio: 16 / 9;
  margin: 16px 16px 0;
  border-radius: 16px;
}

.trial-card__body {
  display: flex;
  flex: 1;
  flex-direction: column;
  padding: 18px 18px 20px;
}

.trial-card__eyebrow {
  margin-bottom: 8px;
  color: $color-gold;
  font-size: 0.76rem;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.trial-card h3 {
  font-family: $font-display;
  font-size: 1.1rem;
  line-height: 1.3;
}

.trial-card__author {
  margin-top: 5px;
  color: $color-faint;
  font-size: 0.85rem;
}

.trial-card__description {
  display: -webkit-box;
  margin: 14px 0 20px;
  overflow: hidden;
  color: $color-muted;
  font-size: 0.9rem;
  line-height: 1.55;
  -webkit-box-orient: vertical;
  -webkit-line-clamp: 3;
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
