<template>
  <router-link class="path-card glass-card" :to="{ name: 'PathDetails', params: { slug: path.slug } }">
    <div class="card-plate">
      <div class="plate-top">
        <span>Ścieżka nauki</span>
        <span>CoursePlatform</span>
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
          <strong>{{ path.courseCount }}</strong> {{ path.courseCount === 1 ? 'kurs' : 'kursów' }}
        </span>
        <span class="card-cta">
          Zobacz ścieżkę
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
        </span>
      </div>
    </div>
  </router-link>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { LearningPathListItem, PathDifficultyLevel } from '@/features/learning-paths/api/learningPaths.api'

const props = defineProps<{ path: LearningPathListItem; index?: number }>()

const difficultyLabels: Record<PathDifficultyLevel, string> = {
  0: 'Początkujący',
  1: 'Średni',
  2: 'Zaawansowany'
} as Record<number, string> as Record<PathDifficultyLevel, string>

const difficultyLabel = computed(() => difficultyLabels[props.path.difficultyLevel] ?? '—')

const number = computed(() => (props.index === undefined ? '—' : String(props.index + 1).padStart(2, '0')))
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.path-card {
  --lg-blur: 10px;
  text-decoration: none;
  color: inherit;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-height: 380px;
}

.card-plate {
  position: relative;
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

.path-card:hover .plate-num {
  color: $color-accent;
}

.card-body {
  padding: 18px 22px 22px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.card-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 14px;
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
  flex: 1;
  margin-bottom: 18px;
}

.card-foot {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
  padding-top: 14px;
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
</style>
