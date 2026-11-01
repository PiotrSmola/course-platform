<template>
  <div class="path-page">
    <div v-if="isLoading" class="path-loading container">
      <div class="spinner" />
      <p>Ładowanie ścieżki...</p>
    </div>

    <div v-else-if="isError || !path" class="path-error container">
      <span class="eyebrow">Ścieżki nauki</span>
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
              <span class="eyebrow">Ścieżka nauki</span>
              <h1 class="path-title">{{ path.title }}</h1>
              <p class="path-short">{{ path.shortDescription }}</p>
            </div>
            <div class="path-hero-aside">
              <dl class="path-meta">
                <div class="meta-item">
                  <dt>Poziom</dt>
                  <dd class="difficulty" :data-level="path.difficultyLevel">{{ difficultyLabel }}</dd>
                </div>
                <div class="meta-item">
                  <dt>Czas nauki</dt>
                  <dd><span class="meta-num">{{ path.estimatedHours }}</span> h</dd>
                </div>
                <div class="meta-item">
                  <dt>Kursy</dt>
                  <dd><span class="meta-num">{{ path.courses.length }}</span> {{ path.courses.length === 1 ? 'kurs' : 'kursów' }}</dd>
                </div>
              </dl>
              <p class="path-desc">{{ path.description }}</p>
            </div>
          </div>
        </div>
      </div>

      <div class="container path-body">
        <header class="plan-head">
          <span class="eyebrow">Spis treści</span>
          <h2 class="section-title">Plan <em>nauki</em></h2>
        </header>

        <ol class="path-courses">
          <li v-for="course in path.courses" :key="course.id" class="path-course">
            <div class="course-step">
              <span class="step-number">{{ String(course.order).padStart(2, '0') }}</span>
            </div>
            <div class="course-info">
              <div class="course-tags">
                <span class="tag level-tag" :data-level="course.courseLevel">{{ levelLabel(course.courseLevel) }}</span>
                <span v-if="course.isOptional" class="tag">Opcjonalny</span>
              </div>
              <h3 class="course-title">{{ course.courseTitle }}</h3>
              <p class="course-desc">{{ course.courseShortDescription }}</p>
              <div class="course-meta">
                <span>{{ course.instructorName }}</span>
                <span>{{ course.courseLanguage }}</span>
              </div>
            </div>
            <div class="course-side">
              <span class="course-price">{{ course.coursePrice === 0 ? 'Darmowy' : `${course.coursePrice.toFixed(2).replace(/\.00$/, '')} zł` }}</span>
              <router-link
                class="btn btn-ghost btn-pill course-cta"
                :to="{ name: 'CourseDetails', params: { id: course.courseId } }"
              >
                Szczegóły
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
              </router-link>
            </div>
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
@use "@/assets/styles/abstracts/mixins" as *;

.path-page {
  min-height: 100vh;
}

.path-loading {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 96px 0;
  min-height: 50vh;

  p {
    @include mono-label;
    color: $color-faint;
  }
}

.path-error {
  padding: 96px 0;
  min-height: 50vh;

  h2 {
    margin: 16px 0 12px;
  }

  p {
    color: $color-muted;
    margin-bottom: 24px;
  }
}

.path-hero {
  padding: 24px 0 0;
}

.back-link {
  @include mono-label;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.64rem;
  color: $color-faint;
  text-decoration: none;
  margin-bottom: 36px;
  transition: color 0.2s;

  &:hover {
    color: $color-accent;
  }
}

.path-hero-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 440px);
  column-gap: 56px;
  row-gap: 28px;
  align-items: end;
  padding-bottom: 36px;
  border-bottom: 1px solid $color-hairline;

  @media (max-width: 900px) {
    grid-template-columns: 1fr;
  }
}

.path-title {
  font-size: $font-size-2xl;
  margin: 18px 0 16px;
}

.path-short {
  max-width: 34ch;
  font-family: $font-display;
  font-size: 1.3rem;
  font-weight: 400;
  line-height: 1.3;
  color: $color-muted;
}

.path-meta {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  margin: 0 0 20px;
  border-top: 1px solid $color-hairline;
  border-bottom: 1px solid $color-hairline;
}

.meta-item {
  padding: 12px 14px 14px;
  min-width: 0;

  &:first-child {
    padding-left: 0;
  }

  & + & {
    border-left: 1px solid $color-hairline;
  }

  dt {
    @include mono-label;
    font-size: 0.6rem;
    color: $color-faint;
    margin-bottom: 8px;
  }

  dd {
    margin: 0;
    font-family: $font-mono;
    font-size: 0.82rem;
    color: $color-muted;
    white-space: nowrap;
  }
}

.meta-num {
  font-family: $font-display;
  font-weight: 600;
  font-size: 1.7rem;
  line-height: 1;
  color: $color-ink;
}

.difficulty {
  &[data-level="0"] { color: $color-success; }
  &[data-level="1"] { color: $color-accent; }
  &[data-level="2"] { color: $color-coral; }
}

.path-desc {
  color: $color-muted;
  font-size: 0.95rem;
  line-height: 1.65;
}

.path-body {
  padding-top: 56px;
  padding-bottom: 96px;
}

.plan-head {
  margin-bottom: 28px;
}

.section-title {
  font-size: clamp(2rem, 3.2vw, 2.6rem);
  margin-top: 14px;
}

.path-courses {
  list-style: none;
  padding: 0;
  margin: 0;
  border-top: 1px solid $color-ink;
}

.path-course {
  display: grid;
  grid-template-columns: 96px minmax(0, 1fr) auto;
  align-items: start;
  gap: 24px;
  padding: 26px 0;
  border-bottom: 1px solid $color-hairline;

  @media (max-width: 720px) {
    grid-template-columns: 56px minmax(0, 1fr);
    gap: 16px;

    .course-side {
      grid-column: 2;
      align-items: flex-start;
    }
  }
}

.step-number {
  font-family: $font-display;
  font-size: 3rem;
  font-weight: 600;
  line-height: 0.85;
  color: $color-accent;

  @media (max-width: 720px) {
    font-size: 2.4rem;
  }
}

.course-info {
  min-width: 0;
}

.course-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 12px;
}

.level-tag {
  &[data-level="0"] { color: $color-success; }
  &[data-level="1"] { color: $color-accent; }
  &[data-level="2"] { color: $color-coral; }
}

.course-title {
  font-family: $font-display;
  font-size: 1.35rem;
  font-weight: 500;
  line-height: 1.12;
  margin-bottom: 8px;
  color: $color-ink;
}

.course-desc {
  max-width: 68ch;
  font-size: 0.9rem;
  color: $color-muted;
  line-height: 1.55;
  margin-bottom: 12px;
}

.course-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
  font-family: $font-mono;
  font-size: 0.7rem;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: $color-faint;

  span + span::before {
    content: "/";
    margin-right: 10px;
    color: $color-hairline;
  }
}

.course-side {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 14px;
}

.course-price {
  font-family: $font-display;
  font-weight: 600;
  font-size: 1.7rem;
  line-height: 1;
  white-space: nowrap;
}

.course-cta {
  padding: 9px 16px;
  font-size: 0.84rem;
}

.spinner {
  width: 22px;
  height: 22px;
  border: 2px solid rgba($color-ink, 0.1);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
