<template>
  <router-link class="path-card glass-card" :to="{ name: 'PathDetails', params: { slug: path.slug } }">
    <div class="card-thumb">
      <div class="card-thumb-fade" />
      <span class="difficulty-badge" :data-level="path.difficultyLevel">{{ difficultyLabel }}</span>
      <span class="hours-badge">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
        {{ path.estimatedHours }}h
      </span>
    </div>
    <div class="card-body">
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

const props = defineProps<{ path: LearningPathListItem }>()

const difficultyLabels: Record<PathDifficultyLevel, string> = {
  0: 'Początkujący',
  1: 'Średni',
  2: 'Zaawansowany'
} as Record<number, string> as Record<PathDifficultyLevel, string>

const difficultyLabel = computed(() => difficultyLabels[props.path.difficultyLevel] ?? '—')
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.path-card {
  --lg-r: 22px;
  --lg-blur: 0px;
  text-decoration: none;
  color: inherit;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transition: transform 0.3s, box-shadow 0.3s, border-color 0.3s;
  min-height: 360px;

  &:hover {
    transform: translateY(-6px);
    border-color: rgba(245, 158, 11, 0.35);
    box-shadow:
      0 30px 70px rgba(3, 6, 24, 0.55),
      0 6px 18px rgba(3, 6, 24, 0.4),
      0 0 0 1px rgba(245, 158, 11, 0.3),
      inset 0 1px 1px rgba(255, 255, 255, 0.18);
  }
}

.card-thumb {
  position: relative;
  height: 140px;
  background:
    radial-gradient(ellipse at 30% 30%, rgba(245, 158, 11, 0.4), transparent 60%),
    radial-gradient(ellipse at 70% 70%, rgba(139, 92, 246, 0.4), transparent 60%),
    linear-gradient(135deg, rgba(245, 158, 11, 0.15), rgba(139, 92, 246, 0.12));
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.card-thumb-fade {
  position: absolute;
  inset: 0;
  background:
    linear-gradient(180deg, transparent 60%, rgba(10, 14, 23, 0.55) 100%);
}

.difficulty-badge,
.hours-badge {
  position: absolute;
  top: 12px;
  padding: 6px 12px;
  font-size: 0.74rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  border-radius: 999px;
  background: rgba(10, 14, 23, 0.7);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.18), inset 0 0 0 1px rgba(255, 255, 255, 0.08);
  backdrop-filter: blur(8px);
}

.difficulty-badge {
  left: 12px;
  color: $color-gold;

  &[data-level="0"] { color: #4ade80; }
  &[data-level="1"] { color: $color-gold; }
  &[data-level="2"] { color: #f472b6; }
}

.hours-badge {
  right: 12px;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  color: $color-ink;
}

.card-body {
  padding: 20px 22px 22px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  flex: 1;
}

.card-title {
  font-family: $font-display;
  font-size: 1.18rem;
  font-weight: 600;
  color: $color-ink;
  letter-spacing: -0.01em;
}

.card-desc {
  font-size: 0.9rem;
  color: $color-muted;
  line-height: 1.55;
  flex: 1;
}

.card-foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
  font-size: 0.85rem;
}

.course-count {
  color: $color-muted;

  strong {
    color: $color-ink;
    font-family: $font-display;
    font-size: 1.05rem;
    font-weight: 700;
  }
}

.card-cta {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-weight: 600;
  color: $color-gold;
  transition: gap 0.2s;
}

.path-card:hover .card-cta {
  gap: 10px;
}
.path-card {
  --lg-blur: 12px;
}

.difficulty-badge,
.hours-badge {
  -webkit-backdrop-filter: blur(12px) saturate(170%);
  backdrop-filter: blur(12px) saturate(170%);
}

</style>