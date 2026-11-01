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
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6" /></svg>
            Wszystkie ścieżki
          </router-link>
          <div class="path-hero-grid">
            <div class="path-hero-text">
              <span class="eyebrow">Ścieżka nauki</span>
              <h1 class="path-title">{{ path.title }}</h1>
              <p class="path-short">{{ path.shortDescription }}</p>
              <p class="path-desc">{{ path.description }}</p>
            </div>

            <aside class="path-sheet paper-card">
              <div class="sheet-top">
                <span class="sheet-label">Karta ścieżki</span>
                <span class="tag">{{ difficultyLabel }}</span>
              </div>
              <div class="sheet-hours">
                <span class="sheet-hours-num">{{ path.estimatedHours }}<small>h</small></span>
                <span class="sheet-hours-label">szacowany czas nauki</span>
              </div>
              <dl class="sheet-rows">
                <div class="sheet-row">
                  <dt>Poziom</dt>
                  <dd>{{ difficultyLabel }}</dd>
                </div>
                <div class="sheet-row">
                  <dt>Kursy</dt>
                  <dd>{{ path.courses.length }} {{ courseWord(path.courses.length) }}</dd>
                </div>
                <div v-if="optionalCount > 0" class="sheet-row">
                  <dt>Opcjonalne</dt>
                  <dd>{{ optionalCount }}</dd>
                </div>
                <div class="sheet-row">
                  <dt>Kolejność</dt>
                  <dd>Ustalona</dd>
                </div>
                <div class="sheet-row">
                  <dt>Certyfikat</dt>
                  <dd>Po każdym kursie</dd>
                </div>
              </dl>
              <router-link
                v-if="firstCourse"
                class="btn btn-primary sheet-cta"
                :to="{ name: 'CourseDetails', params: { id: firstCourse.courseId } }"
              >
                Zacznij od kursu {{ pad(firstCourse.order) }}
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6" /></svg>
              </router-link>
            </aside>
          </div>
        </div>
      </div>

      <div class="container path-body">
        <header class="section-head plan-head">
          <span class="eyebrow">Spis treści</span>
          <h2>Plan <em>nauki</em></h2>
          <p>Kursy przechodzisz w tej kolejności. Lekcje w każdym z nich otwierają się po ukończeniu poprzednich i zaliczeniu quizów.</p>
        </header>

        <ol class="timeline">
          <li
            v-for="(course, index) in sortedCourses"
            :key="course.id"
            class="tl-item"
            :class="{ 'is-first': index === 0, 'is-optional': course.isOptional }"
          >
            <span class="tl-marker">{{ pad(course.order) }}</span>
            <div class="tl-card" :class="{ 'glass-card is-static': index === 0 }">
              <div class="tl-info">
                <div class="course-tags">
                  <span v-if="index === 0" class="tag start-tag">Start</span>
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
                  class="btn btn-pill course-cta"
                  :class="index === 0 ? 'btn-primary' : 'btn-ghost'"
                  :to="{ name: 'CourseDetails', params: { id: course.courseId } }"
                >
                  Szczegóły
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6" /></svg>
                </router-link>
              </div>
            </div>
          </li>

          <li class="tl-item is-finish">
            <span class="tl-marker" aria-hidden="true">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12" /></svg>
            </span>
            <div class="tl-finish outline-card">
              <span class="finish-label">Meta</span>
              <p>Po każdym ukończonym kursie dostajesz certyfikat z unikalnym numerem i publiczną stroną weryfikacji.</p>
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

const sortedCourses = computed(() =>
  path.value ? [...path.value.courses].sort((a, b) => a.order - b.order) : []
)

const firstCourse = computed(() => sortedCourses.value[0] ?? null)

const optionalCount = computed(() => sortedCourses.value.filter((c) => c.isOptional).length)

function pad(n: number): string {
  return String(n).padStart(2, '0')
}

function courseWord(n: number): string {
  if (n === 1) return 'kurs'
  const lastDigit = n % 10
  const lastTwo = n % 100
  if (lastDigit >= 2 && lastDigit <= 4 && (lastTwo < 12 || lastTwo > 14)) return 'kursy'
  return 'kursów'
}

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

$paper-ink: #15171b;
$paper-line: rgba(21, 23, 27, 0.16);
$marker: 46px;

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
  grid-template-columns: minmax(0, 1fr) minmax(0, 400px);
  column-gap: 64px;
  row-gap: 40px;
  align-items: start;
  padding-bottom: 56px;
  border-bottom: 1px solid $color-hairline;

  @media (max-width: 900px) {
    grid-template-columns: 1fr;
  }
}

.path-hero-text {
  min-width: 0;
}

.path-title {
  font-size: $font-size-2xl;
  margin: 18px 0 18px;
}

.path-short {
  max-width: 34ch;
  font-family: $font-display;
  font-size: 1.3rem;
  font-weight: 400;
  line-height: 1.3;
  color: $color-ink;
  margin-bottom: 22px;
}

.path-desc {
  max-width: 62ch;
  padding-top: 20px;
  border-top: 1px dashed $color-hairline;
  color: $color-muted;
  font-size: 0.98rem;
  line-height: 1.7;
}

.path-sheet {
  padding: 26px 28px 28px;
  transform: rotate(1deg);
  margin-top: 12px;

  @media (max-width: 900px) {
    max-width: 480px;
    transform: rotate(0.5deg);
  }

  @media (max-width: 560px) {
    padding: 22px 20px 24px;
  }
}

.sheet-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 16px;
  border-bottom: 1px solid $paper-ink;
}

.sheet-label {
  @include mono-label;
  color: rgba($paper-ink, 0.6);
}

.sheet-hours {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 16px;
  padding: 22px 0 18px;
}

.sheet-hours-num {
  font-family: $font-display;
  font-size: 5rem;
  font-weight: 600;
  line-height: 0.8;
  letter-spacing: -0.015em;
  color: $paper-ink;

  small {
    margin-left: 4px;
    font-size: 1.6rem;
    font-weight: 500;
    color: rgba($paper-ink, 0.6);
  }
}

.sheet-hours-label {
  @include mono-label;
  max-width: 12ch;
  font-size: 0.6rem;
  line-height: 1.5;
  text-align: right;
  color: rgba($paper-ink, 0.55);
}

.sheet-rows {
  margin: 0 0 24px;
}

.sheet-row {
  display: flex;
  align-items: baseline;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px dashed $paper-line;

  &:first-child {
    border-top: 1px dashed $paper-line;
  }

  dt {
    @include mono-label;
    font-size: 0.62rem;
    color: rgba($paper-ink, 0.55);
  }

  dd {
    margin: 0 0 0 auto;
    font-family: $font-mono;
    font-size: 0.82rem;
    font-weight: 500;
    color: $paper-ink;
    text-align: right;
  }
}

.sheet-cta {
  width: 100%;
}

.path-body {
  padding-top: 88px;
  padding-bottom: 96px;
}

.timeline {
  position: relative;
  list-style: none;
  padding: 0;
  margin: 0;

  &::before {
    content: "";
    position: absolute;
    top: 8px;
    bottom: 30px;
    left: calc(#{$marker} / 2);
    width: 1px;
    background: rgba($color-ink, 0.2);
  }
}

.tl-item {
  position: relative;
  display: grid;
  grid-template-columns: $marker minmax(0, 1fr);
  gap: 28px;
  padding-bottom: 40px;

  &.is-finish {
    padding-bottom: 0;
  }

  @media (max-width: 560px) {
    grid-template-columns: 36px minmax(0, 1fr);
    gap: 16px;
  }
}

.tl-marker {
  position: relative;
  display: grid;
  place-items: center;
  width: $marker;
  height: $marker;
  border-radius: 4px;
  background: $color-bg;
  box-shadow: inset 0 0 0 1px rgba($color-ink, 0.28);
  font-family: $font-display;
  font-size: 1.15rem;
  font-weight: 600;
  color: $color-ink;

  .is-first & {
    color: $color-on-accent;
    background: $color-accent;
    box-shadow: 0 0 0 6px rgba($color-accent, 0.14);
  }

  .is-optional & {
    box-shadow: none;
    outline: 1px dashed rgba($color-ink, 0.4);
    outline-offset: -1px;
    color: $color-muted;
  }

  .is-finish & {
    color: $color-accent;
    box-shadow: inset 0 0 0 1px rgba($color-accent, 0.6);
  }

  @media (max-width: 560px) {
    width: 36px;
    height: 36px;
    font-size: 0.95rem;
  }
}

.timeline::before {
  @media (max-width: 560px) {
    left: 18px;
  }
}

.tl-card {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: start;
  gap: 28px;
  min-width: 0;
  padding: 4px 0 40px;
  border-bottom: 1px solid $color-hairline;

  &.glass-card {
    --lg-blur: 10px;
    padding: 26px 28px 28px;
    border-bottom: none;
  }

  @media (max-width: 720px) {
    grid-template-columns: minmax(0, 1fr);
    gap: 18px;

    &.glass-card {
      padding: 20px 18px 22px;
    }
  }
}

.tl-info {
  min-width: 0;
}

.course-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 12px;
}

.start-tag {
  color: $color-on-accent;
  background: $color-accent;
  box-shadow: none;
}

.level-tag {
  &[data-level="0"] { color: $color-success; }
  &[data-level="1"] { color: $color-accent; }
  &[data-level="2"] { color: $color-coral; }
}

.course-title {
  font-family: $font-display;
  font-size: 1.45rem;
  font-weight: 500;
  line-height: 1.12;
  margin-bottom: 8px;
  color: $color-ink;

  .is-first & {
    font-size: clamp(1.5rem, 2.4vw, 1.9rem);
  }
}

.course-desc {
  max-width: 68ch;
  font-size: 0.92rem;
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

  @media (max-width: 720px) {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
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

.tl-finish {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 8px 20px;
  padding: 14px 20px;

  p {
    flex: 1 1 280px;
    font-size: 0.92rem;
    line-height: 1.55;
    color: $color-muted;
  }
}

.finish-label {
  @include mono-label;
  color: $color-accent;
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
