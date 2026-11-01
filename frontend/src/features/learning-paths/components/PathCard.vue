<template>
  <router-link
    class="path-card"
    :class="[`is-${variant}`, surfaceClass]"
    :to="{ name: 'PathDetails', params: { slug: path.slug } }"
  >
    <template v-if="variant === 'featured'">
      <div class="feat-main">
        <div class="feat-top">
          <span class="feat-label">Ścieżka No. {{ number }}</span>
          <span class="tag">{{ difficultyLabel }}</span>
        </div>
        <h3 class="feat-title">{{ path.title }}</h3>
        <p class="feat-desc">{{ path.shortDescription }}</p>
        <span class="btn btn-primary feat-cta">
          Zobacz plan ścieżki
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6" /></svg>
        </span>
      </div>
      <div class="feat-sheet">
        <span class="feat-num">{{ number }}</span>
        <dl class="spec">
          <div class="spec-row">
            <dt>Poziom</dt>
            <dd>{{ difficultyLabel }}</dd>
          </div>
          <div class="spec-row">
            <dt>Czas nauki</dt>
            <dd>{{ path.estimatedHours }} h</dd>
          </div>
          <div class="spec-row">
            <dt>Kursy</dt>
            <dd>{{ path.courseCount }} {{ courseWord }}</dd>
          </div>
          <div class="spec-row">
            <dt>Kolejność</dt>
            <dd>Ustalona</dd>
          </div>
        </dl>
      </div>
    </template>

    <template v-else-if="variant === 'tall'">
      <div class="card-plate">
        <div class="plate-top">
          <span>Ścieżka nauki</span>
          <span>{{ path.courseCount }} {{ courseWord }}</span>
        </div>
        <span class="plate-num">{{ number }}</span>
      </div>
      <div class="card-body">
        <div class="card-tags">
          <span class="tag difficulty-tag" :data-level="path.difficultyLevel">{{ difficultyLabel }}</span>
          <span class="tag">{{ path.estimatedHours }} h</span>
        </div>
        <h3 class="card-title">{{ path.title }}</h3>
        <p class="card-desc">{{ path.shortDescription }}</p>
        <div class="card-foot">
          <span class="course-count">
            <strong>{{ path.courseCount }}</strong> {{ courseWord }}
          </span>
          <span class="card-cta">
            Zobacz ścieżkę
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6" /></svg>
          </span>
        </div>
      </div>
    </template>

    <template v-else-if="variant === 'outline'">
      <span class="out-num">{{ number }}</span>
      <div class="out-body">
        <span class="tag difficulty-tag" :data-level="path.difficultyLevel">{{ difficultyLabel }}</span>
        <h3 class="card-title">{{ path.title }}</h3>
        <p class="card-desc">{{ path.shortDescription }}</p>
        <ul class="out-meta">
          <li>{{ path.courseCount }} {{ courseWord }}</li>
          <li>{{ path.estimatedHours }} h nauki</li>
        </ul>
      </div>
      <span class="out-arrow" aria-hidden="true">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M7 17 17 7" /><path d="M8 7h9v9" /></svg>
      </span>
    </template>

    <template v-else>
      <div class="row-hours">
        <span class="row-hours-num">{{ path.estimatedHours }}<small>h</small></span>
        <span class="row-hours-label">szacowany czas nauki</span>
      </div>
      <div class="row-body">
        <div class="row-top">
          <span class="row-no">No. {{ number }}</span>
          <span class="tag difficulty-tag" :data-level="path.difficultyLevel">{{ difficultyLabel }}</span>
        </div>
        <h3 class="card-title">{{ path.title }}</h3>
        <p class="card-desc">{{ path.shortDescription }}</p>
        <div class="card-foot">
          <span class="course-count">
            <strong>{{ path.courseCount }}</strong> {{ courseWord }}
          </span>
          <span class="card-cta">
            Zobacz ścieżkę
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6" /></svg>
          </span>
        </div>
      </div>
    </template>
  </router-link>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { LearningPathListItem, PathDifficultyLevel } from '@/features/learning-paths/api/learningPaths.api'

type PathCardVariant = 'featured' | 'tall' | 'outline' | 'row'

const props = withDefaults(
  defineProps<{ path: LearningPathListItem; index?: number; variant?: PathCardVariant }>(),
  { index: undefined, variant: 'tall' }
)

const difficultyLabels: Record<PathDifficultyLevel, string> = {
  0: 'Początkujący',
  1: 'Średni',
  2: 'Zaawansowany'
} as Record<number, string> as Record<PathDifficultyLevel, string>

const difficultyLabel = computed(() => difficultyLabels[props.path.difficultyLevel] ?? '—')

const number = computed(() => (props.index === undefined ? '—' : String(props.index + 1).padStart(2, '0')))

const surfaceClass = computed(() => {
  switch (props.variant) {
    case 'featured': return 'paper-card'
    case 'outline': return 'outline-card'
    default: return 'glass-card'
  }
})

const courseWord = computed(() => {
  const n = props.path.courseCount
  if (n === 1) return 'kurs'
  const lastDigit = n % 10
  const lastTwo = n % 100
  if (lastDigit >= 2 && lastDigit <= 4 && (lastTwo < 12 || lastTwo > 14)) return 'kursy'
  return 'kursów'
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

$paper-ink: #15171b;
$paper-line: rgba(21, 23, 27, 0.16);

.path-card {
  text-decoration: none;
  color: inherit;
  min-width: 0;
}

.difficulty-tag {
  &[data-level="0"] {
    color: $color-success;
    box-shadow: inset 0 0 0 1px rgba($color-success, 0.35);
  }

  &[data-level="1"] {
    color: $color-accent;
    box-shadow: inset 0 0 0 1px rgba($color-accent, 0.35);
  }

  &[data-level="2"] {
    color: $color-coral;
    box-shadow: inset 0 0 0 1px rgba($color-coral, 0.35);
  }
}

.card-title {
  font-family: $font-display;
  font-size: 1.32rem;
  font-weight: 500;
  line-height: 1.1;
  color: $color-ink;
  margin-bottom: 10px;
}

.card-desc {
  font-size: 0.9rem;
  color: $color-muted;
  line-height: 1.55;
}

.card-foot {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
  padding-top: 14px;
  margin-top: auto;
  border-top: 1px dashed $color-hairline;
}

.course-count {
  @include mono-label;
  font-size: 0.62rem;
  color: $color-faint;

  strong {
    margin-right: 4px;
    font-family: $font-display;
    font-size: 1.7rem;
    font-weight: 500;
    letter-spacing: 0;
    color: $color-ink;
  }
}

.card-cta {
  @include mono-label;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.64rem;
  color: $color-muted;
  transition: gap 0.2s, color 0.2s;
}

.path-card:hover .card-cta {
  gap: 10px;
  color: $color-accent;
}

.is-featured {
  display: grid;
  grid-template-columns: minmax(0, 1.35fr) minmax(0, 1fr);
  gap: 40px;
  padding: 34px 36px 36px;
  color: $paper-ink;
  transform: rotate(-0.6deg);
  transition: transform 0.5s $ease-out, box-shadow 0.35s;

  &:hover {
    transform: rotate(0deg) translateY(-4px);
  }

  @media (max-width: 720px) {
    grid-template-columns: 1fr;
    gap: 28px;
    padding: 26px 22px 26px;
    transform: none;

    &:hover {
      transform: translateY(-2px);
    }
  }
}

.feat-main {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  min-width: 0;
}

.feat-top {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
  margin-bottom: 26px;
}

.feat-label {
  @include mono-label;
  color: rgba($paper-ink, 0.6);
}

.feat-title {
  font-family: $font-display;
  font-size: clamp(1.9rem, 3vw, 2.7rem);
  font-weight: 600;
  line-height: 1.04;
  letter-spacing: -0.015em;
  color: $paper-ink;
  margin-bottom: 16px;
}

.feat-desc {
  max-width: 46ch;
  font-size: 1rem;
  line-height: 1.6;
  margin-bottom: 30px;
}

.feat-cta {
  margin-top: auto;
}

.feat-sheet {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 18px;
  min-width: 0;
  padding-left: 32px;
  border-left: 1px solid $paper-line;

  @media (max-width: 720px) {
    padding-left: 0;
    padding-top: 22px;
    border-left: none;
    border-top: 1px solid $paper-line;
  }
}

.feat-num {
  font-family: $font-display;
  font-size: clamp(5rem, 9vw, 8rem);
  font-weight: 600;
  line-height: 0.8;
  letter-spacing: -0.015em;
  color: $paper-ink;
}

.spec {
  margin: 0;
}

.spec-row {
  display: flex;
  align-items: baseline;
  gap: 10px;
  padding: 9px 0;
  border-bottom: 1px dashed $paper-line;

  &:first-child {
    border-top: 1px solid $paper-ink;
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

.is-tall {
  --lg-blur: 10px;
  display: flex;
  flex-direction: column;
  min-height: 100%;
  overflow: hidden;
}

.card-plate {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 150px;
  margin: 12px 12px 0;
  padding: 14px 18px 10px;
  border-radius: 14px 14px 14px 4px;
  background:
    linear-gradient(rgba(242, 239, 230, 0.05) 1px, transparent 1px) 0 0 / 100% 18px,
    rgba(13, 15, 19, 0.62);
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.07);
}

.plate-top {
  @include mono-label;
  display: flex;
  justify-content: space-between;
  gap: 12px;
  font-size: 0.62rem;
  color: $color-faint;
}

.plate-num {
  font-family: $font-display;
  font-size: 4.4rem;
  font-weight: 600;
  line-height: 0.85;
  letter-spacing: -0.015em;
  color: $color-ink;
  transition: color 0.3s;
}

.is-tall:hover .plate-num {
  color: $color-accent;
}

.card-body {
  display: flex;
  flex-direction: column;
  flex: 1;
  padding: 18px 22px 22px;

  .card-desc {
    margin-bottom: 18px;
  }
}

.card-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 14px;
}

.is-outline {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr) auto;
  align-items: start;
  gap: 26px;
  padding: 28px 28px 26px;

  .card-title {
    margin-top: 14px;
  }

  @media (max-width: 560px) {
    grid-template-columns: minmax(0, 1fr) auto;
    gap: 16px;
    padding: 22px 20px;

    .out-num {
      grid-column: 1;
    }

    .out-body {
      grid-column: 1 / -1;
      grid-row: 2;
    }
  }
}

.out-num {
  font-family: $font-display;
  font-size: 4rem;
  font-weight: 600;
  line-height: 0.82;
  letter-spacing: -0.015em;
  color: transparent;
  -webkit-text-stroke: 1px rgba($color-ink, 0.55);
  transition: color 0.3s, -webkit-text-stroke-color 0.3s;
}

.is-outline:hover .out-num {
  color: $color-accent;
  -webkit-text-stroke-color: $color-accent;
}

.out-body {
  min-width: 0;
}

.out-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 18px;
  list-style: none;
  padding: 14px 0 0;
  margin: 16px 0 0;
  border-top: 1px dashed $color-hairline;

  li {
    @include mono-label;
    font-size: 0.64rem;
    color: $color-faint;

    &::before {
      content: "— ";
      color: $color-accent;
    }
  }
}

.out-arrow {
  display: grid;
  place-items: center;
  width: 40px;
  height: 40px;
  border-radius: 2px;
  border: 1px solid $color-hairline;
  color: $color-muted;
  transition: color 0.3s, border-color 0.3s, transform 0.3s $ease-out;
}

.is-outline:hover .out-arrow {
  color: $color-accent;
  border-color: rgba($color-accent, 0.6);
  transform: translate(2px, -2px);
}

.is-row {
  --lg-blur: 10px;
  display: grid;
  grid-template-columns: 150px minmax(0, 1fr);
  overflow: hidden;

  @media (max-width: 560px) {
    grid-template-columns: 1fr;
  }
}

.row-hours {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 16px;
  padding: 24px 20px;
  border-right: 1px dashed $color-hairline;

  @media (max-width: 560px) {
    flex-direction: row;
    align-items: baseline;
    justify-content: flex-start;
    border-right: none;
    border-bottom: 1px dashed $color-hairline;
    padding: 20px 20px 16px;
  }
}

.row-hours-num {
  font-family: $font-display;
  font-size: 3.6rem;
  font-weight: 600;
  line-height: 0.85;
  letter-spacing: -0.015em;
  color: $color-accent;

  small {
    margin-left: 2px;
    font-size: 1.3rem;
    font-weight: 500;
    color: $color-muted;
  }
}

.row-hours-label {
  @include mono-label;
  font-size: 0.6rem;
  line-height: 1.5;
  color: $color-faint;
}

.row-body {
  display: flex;
  flex-direction: column;
  min-width: 0;
  padding: 24px 24px 22px;

  .card-desc {
    margin-bottom: 18px;
  }
}

.row-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 14px;
}

.row-no {
  @include mono-label;
  color: $color-faint;
}
</style>
