<template>
  <div class="path-page">
    <div v-if="isLoading" class="path-loading">
      <div class="spinner" />
      <p>Ładowanie ścieżki...</p>
    </div>

    <div v-else-if="isError || !path" class="path-error">
      <h2>Nie znaleziono ścieżki</h2>
      <p>Sprawdź poprawność adresu lub wróć do listy ścieżek.</p>
      <router-link class="btn btn-primary" :to="{ name: 'PathsList' }">Wszystkie ścieżki</router-link>
    </div>

    <template v-else>
      <div class="path-hero">
        <div class="container">
          <router-link class="back-link" :to="{ name: 'PathsList' }">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
            Wszystkie ścieżki
          </router-link>
          <div class="path-hero-grid">
            <div class="path-hero-text">
              <div class="path-meta">
                <span class="difficulty-badge" :data-level="path.difficultyLevel">{{ difficultyLabel }}</span>
                <span class="hours-badge">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                  {{ path.estimatedHours }}h nauki
                </span>
                <span class="course-count-badge">
                  {{ path.courses.length }} {{ path.courses.length === 1 ? 'kurs' : 'kursów' }}
                </span>
              </div>
              <h1 class="path-title">{{ path.title }}</h1>
              <p class="path-short">{{ path.shortDescription }}</p>
              <p class="path-desc">{{ path.description }}</p>
            </div>
          </div>
        </div>
      </div>

      <div class="container path-body">
        <h2 class="section-title">Plan nauki</h2>

        <ol class="path-courses">
          <li v-for="course in path.courses" :key="course.id" class="path-course glass-card">
            <div class="course-step">
              <span class="step-number">{{ course.order }}</span>
              <span v-if="course.isOptional" class="step-optional">Opcjonalny</span>
            </div>
            <div class="course-thumb" :data-level="course.courseLevel">
              <span>{{ levelLabel(course.courseLevel) }}</span>
            </div>
            <div class="course-info">
              <h3 class="course-title">{{ course.courseTitle }}</h3>
              <p class="course-desc">{{ course.courseShortDescription }}</p>
              <div class="course-meta">
                <span>{{ course.instructorName }}</span>
                <span class="dot">·</span>
                <span>{{ course.courseLanguage }}</span>
                <span class="dot">·</span>
                <span class="course-price">{{ course.coursePrice === 0 ? 'Darmowy' : `${course.coursePrice.toFixed(2).replace(/\.00$/, '')} zł` }}</span>
              </div>
            </div>
            <router-link
              class="course-cta"
              :to="{ name: 'CourseDetails', params: { id: course.courseId } }"
            >
              Szczegóły
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
            </router-link>
          </li>
        </ol>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useLearningPathBySlug } from '@/features/learning-paths/composables/useLearningPaths'
import type { PathDifficultyLevel } from '@/features/learning-paths/api/learningPaths.api'
import { CourseLevel } from '@/features/courses/types/course.types'

const route = useRoute()
const slug = computed(() => route.params.slug as string)
const { isLoading, isError, data } = useLearningPathBySlug(() => slug.value)
const path = computed(() => data.value ?? null)

const difficultyLabels: Record<PathDifficultyLevel, string> = {
  0: 'Początkujący',
  1: 'Średni',
  2: 'Zaawansowany'
} as Record<number, string> as Record<PathDifficultyLevel, string>

const difficultyLabel = computed(() =>
  path.value ? difficultyLabels[path.value.difficultyLevel] ?? '—' : '—'
)

function levelLabel(level: CourseLevel): string {
  switch (level) {
    case CourseLevel.Beginner: return 'BEG'
    case CourseLevel.Intermediate: return 'INT'
    case CourseLevel.Advanced: return 'ADV'
    default: return ''
  }
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.path-page {
  padding-top: $header-height;
  min-height: 100vh;
}

.path-loading,
.path-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 120px 20px;
  text-align: center;
  color: $color-muted;
  min-height: 50vh;
}

.path-hero {
  padding: 32px 0 40px;
  border-bottom: 1px solid $color-hairline;
}

.back-link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.85rem;
  color: $color-muted;
  text-decoration: none;
  margin-bottom: 24px;
  transition: color 0.2s;

  &:hover {
    color: $color-gold;
  }
}

.path-meta {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 18px;
}

.difficulty-badge,
.hours-badge,
.course-count-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  font-size: 0.74rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  border-radius: 999px;
}

.difficulty-badge {
  background: rgba(245, 158, 11, 0.14);
  border: 1px solid rgba(245, 158, 11, 0.3);
  color: $color-gold;

  &[data-level="0"] { color: #4ade80; background: rgba(74, 222, 128, 0.12); border-color: rgba(74, 222, 128, 0.3); }
  &[data-level="1"] { color: $color-gold; background: rgba(245, 158, 11, 0.14); border-color: rgba(245, 158, 11, 0.3); }
  &[data-level="2"] { color: #f472b6; background: rgba(244, 114, 182, 0.12); border-color: rgba(244, 114, 182, 0.3); }
}

.hours-badge {
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: $color-ink;
}

.course-count-badge {
  background: rgba(139, 92, 246, 0.14);
  border: 1px solid rgba(139, 92, 246, 0.3);
  color: #c4b5fd;
}

.path-title {
  font-family: $font-display;
  font-size: $font-size-xl;
  font-weight: 600;
  letter-spacing: -0.02em;
  margin-bottom: 14px;
  background: $color-grad-text;
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.path-short {
  font-size: 1.05rem;
  color: $color-ink;
  margin-bottom: 12px;
  font-weight: 500;
}

.path-desc {
  color: $color-muted;
  font-size: 0.98rem;
  max-width: 760px;
  line-height: 1.6;
}

.path-body {
  padding-top: 40px;
  padding-bottom: 80px;
}

.section-title {
  font-family: $font-display;
  font-size: 1.4rem;
  font-weight: 600;
  margin-bottom: 20px;
  color: $color-ink;
}

.path-courses {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 16px;
  counter-reset: step;
}

.path-course {
  --lg-r: 18px;
  --lg-blur: 0px;
  display: grid;
  grid-template-columns: auto 80px 1fr auto;
  align-items: center;
  gap: 20px;
  padding: 18px 22px;
  transition: transform 0.2s, box-shadow 0.2s, border-color 0.2s;

  &:hover {
    transform: translateX(4px);
    border-color: rgba(245, 158, 11, 0.3);
    box-shadow:
      0 18px 44px rgba(3, 6, 24, 0.5),
      0 4px 12px rgba(3, 6, 24, 0.3),
      inset 0 1px 1px rgba(255, 255, 255, 0.18);
  }

  @media (max-width: 720px) {
    grid-template-columns: auto 1fr;
    gap: 14px;

    .course-thumb { display: none; }
    .course-cta { grid-column: 1 / -1; justify-self: stretch; text-align: center; }
  }
}

.course-step {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.step-number {
  width: 38px;
  height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  font-family: $font-display;
  font-weight: 700;
  font-size: 1rem;
  color: $color-ink;
  background:
    linear-gradient(135deg, rgba(245, 158, 11, 0.35), rgba(236, 72, 153, 0.25));
  box-shadow: inset 1.5px 1.5px 2px -1px rgba(255, 255, 255, 0.4), inset 0 0 0 1px rgba(245, 158, 11, 0.4);
}

.step-optional {
  font-size: 0.7rem;
  color: $color-faint;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.course-thumb {
  width: 80px;
  height: 80px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(34, 211, 238, 0.25), rgba(139, 92, 246, 0.18));
  font-family: $font-display;
  font-weight: 700;
  font-size: 0.85rem;
  letter-spacing: 0.08em;
  color: $color-ink;
  box-shadow: inset 1.5px 1.5px 2px -1px rgba(255, 255, 255, 0.35);

  &[data-level="0"] { background: linear-gradient(135deg, rgba(74, 222, 128, 0.3), rgba(34, 211, 238, 0.18)); }
  &[data-level="1"] { background: linear-gradient(135deg, rgba(245, 158, 11, 0.3), rgba(245, 158, 11, 0.18)); }
  &[data-level="2"] { background: linear-gradient(135deg, rgba(244, 114, 182, 0.3), rgba(244, 63, 94, 0.18)); }
}

.course-info {
  min-width: 0;
}

.course-title {
  font-family: $font-display;
  font-size: 1.05rem;
  font-weight: 600;
  margin-bottom: 4px;
  color: $color-ink;
}

.course-desc {
  font-size: 0.85rem;
  color: $color-muted;
  line-height: 1.45;
  margin-bottom: 6px;
}

.course-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
  font-size: 0.78rem;
  color: $color-faint;

  .dot { color: $color-faint; opacity: 0.5; }
  .course-price { color: $color-gold; font-weight: 600; }
}

.course-cta {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.06);
  color: $color-ink;
  text-decoration: none;
  font-size: 0.85rem;
  font-weight: 600;
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.2), inset 0 0 0 1px rgba(255, 255, 255, 0.08);
  transition: background 0.2s, transform 0.2s;

  &:hover {
    background: rgba(245, 158, 11, 0.18);
    color: $color-gold;
    transform: translateX(2px);
  }
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid rgba(255, 255, 255, 0.1);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
.path-course {
  --lg-blur: 12px;
}

.difficulty-badge,
.hours-badge,
.course-count-badge,
.course-cta {
  -webkit-backdrop-filter: blur(12px) saturate(170%);
  backdrop-filter: blur(12px) saturate(170%);
}

.course-cta {
  background: rgba(255, 255, 255, 0.07);
}

</style>