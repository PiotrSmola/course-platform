<template>
  <div class="list-page">
    <div class="list-hero">
      <div class="container">
        <span class="eyebrow">Wszystkie kategorie</span>
        <h1 class="list-title">Przeglądaj kategorie kursów</h1>
        <p class="list-subtitle">
          Wybierz obszar, który Cię interesuje. Każda kategoria gromadzi kursy pogrupowane tematycznie — od backendu po AI.
        </p>
      </div>
    </div>

    <div class="container list-body">
      <div v-if="isLoading" class="list-loading">
        <div class="spinner" />
        <p>Ładowanie kategorii...</p>
      </div>

      <div v-else-if="isError" class="list-empty">
        <p>Nie udało się pobrać kategorii. Spróbuj ponownie.</p>
      </div>

      <div v-else-if="items.length === 0" class="list-empty">
        <p>Na platformie nie ma jeszcze żadnych kategorii.</p>
      </div>

      <div v-else class="list-grid">
        <router-link
          v-for="item in items"
          :key="item.id"
          class="list-card glass-card"
          :to="{ name: 'CategoryDetails', params: { slug: item.slug } }"
        >
          <div class="card-head">
            <span class="card-icon" :data-slug="item.slug">{{ initials(item.name) }}</span>
            <div class="card-meta">
              <h3 class="card-title">{{ item.name }}</h3>
              <span class="card-slug">/{{ item.slug }}</span>
            </div>
          </div>
          <p class="card-desc">{{ item.description || `Kursy z zakresu ${item.name.toLowerCase()}.` }}</p>
          <div class="card-foot">
            <span class="card-count">
              <strong>{{ counts[item.id] ?? '…' }}</strong>
              <span>kursów</span>
            </span>
            <span class="card-cta">
              Zobacz kursy
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
            </span>
          </div>
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { getCategories, getCourses } from '@/features/courses/api/courses.api'
import type { CategoryDto } from '@/features/courses/api/courses.api'
import { CourseStatus } from '@/features/courses/types/course.types'
import { queryKeys } from '@/shared/queryKeys'

const { isLoading, isError, data } = useQuery<CategoryDto[]>({
  queryKey: queryKeys.categories(),
  queryFn: getCategories,
  initialData: []
})

const items = computed(() => data.value ?? [])

const counts = reactive<Record<string, number>>({})

function initials(name: string): string {
  const parts = name.trim().split(/\s+/).slice(0, 2)
  return parts.map((p) => p.charAt(0).toUpperCase()).join('') || '·'
}

async function fetchCount(id: string) {
  try {
    const result = await getCourses({ categoryIds: [id], status: CourseStatus.Published, pageSize: 1 })
    counts[id] = result.totalCount
  } catch {
    counts[id] = 0
  }
}

items.value.forEach((cat) => {
  if (counts[cat.id] === undefined) {
    void fetchCount(cat.id)
  }
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.list-page {
  padding-top: $header-height;
  min-height: 100vh;
}

.list-hero {
  padding: 56px 0 36px;
  border-bottom: 1px solid $color-hairline;
  text-align: center;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  font-size: 0.78rem;
  font-weight: 600;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: $color-ink;
  background: rgba(245, 158, 11, 0.12);
  border: 1px solid rgba(245, 158, 11, 0.25);
  border-radius: 999px;
  margin-bottom: 16px;
}

.list-title {
  font-family: $font-display;
  font-size: $font-size-xl;
  font-weight: 600;
  letter-spacing: -0.02em;
  margin-bottom: 12px;
  background: $color-grad-text;
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
}

.list-subtitle {
  color: $color-muted;
  font-size: 1.02rem;
  max-width: 640px;
  margin: 0 auto;
  line-height: 1.55;
}

.list-body {
  padding-top: 48px;
  padding-bottom: 80px;
}

.list-loading,
.list-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 80px 20px;
  text-align: center;
  color: $color-muted;
}

.list-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.list-card {
  --lg-r: 20px;
  --lg-blur: 0px;
  padding: 24px;
  text-decoration: none;
  color: inherit;
  display: flex;
  flex-direction: column;
  gap: 16px;
  min-height: 220px;
  transition: transform 0.25s, box-shadow 0.25s, border-color 0.25s;

  &:hover {
    transform: translateY(-4px);
    border-color: rgba(245, 158, 11, 0.35);
    box-shadow:
      0 24px 60px rgba(3, 6, 24, 0.55),
      0 4px 14px rgba(3, 6, 24, 0.35),
      0 0 0 1px rgba(245, 158, 11, 0.3),
      inset 0 1px 1px rgba(255, 255, 255, 0.18);
  }
}

.card-head {
  display: flex;
  align-items: center;
  gap: 14px;
}

.card-icon {
  width: 48px;
  height: 48px;
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.25), rgba(236, 72, 153, 0.18));
  box-shadow:
    inset 1.5px 1.5px 2px -1px rgba(255, 255, 255, 0.35),
    inset -1.5px -1.5px 2px -1px rgba(255, 255, 255, 0.08),
    inset 0 -8px 16px -10px rgba(245, 158, 11, 0.25);
  font-family: $font-display;
  font-weight: 700;
  font-size: 1.05rem;
  color: $color-ink;
  letter-spacing: -0.02em;

  &[data-slug="ai"] { background: linear-gradient(135deg, rgba(34, 211, 238, 0.28), rgba(139, 92, 246, 0.22)); }
  &[data-slug="mobile"] { background: linear-gradient(135deg, rgba(236, 72, 153, 0.25), rgba(244, 63, 94, 0.18)); }
  &[data-slug="devops"] { background: linear-gradient(135deg, rgba(20, 184, 166, 0.25), rgba(34, 211, 238, 0.18)); }
  &[data-slug="databases"] { background: linear-gradient(135deg, rgba(139, 92, 246, 0.25), rgba(236, 72, 153, 0.18)); }
  &[data-slug="frontend"] { background: linear-gradient(135deg, rgba(245, 158, 11, 0.3), rgba(245, 158, 11, 0.18)); }
  &[data-slug="backend"] { background: linear-gradient(135deg, rgba(34, 211, 238, 0.28), rgba(20, 184, 166, 0.18)); }
}

.card-meta {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}

.card-title {
  font-family: $font-display;
  font-size: 1.1rem;
  font-weight: 600;
  color: $color-ink;
}

.card-slug {
  font-size: 0.78rem;
  color: $color-faint;
  font-family: ui-monospace, "SF Mono", Menlo, monospace;
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
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.card-count {
  display: flex;
  align-items: baseline;
  gap: 6px;
  font-size: 0.85rem;
  color: $color-muted;

  strong {
    color: $color-gold;
    font-family: $font-display;
    font-size: 1.1rem;
    font-weight: 700;
  }
}

.card-cta {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 0.85rem;
  font-weight: 600;
  color: $color-gold;
  transition: gap 0.2s;
}

.list-card:hover .card-cta {
  gap: 10px;
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
</style>