<template>
  <div class="list-page">
    <div class="list-hero">
      <div class="container">
        <header class="section-head list-head">
          <span class="eyebrow">Wszystkie kategorie</span>
          <h1 class="list-title">Przeglądaj <em>kategorie</em> kursów</h1>
          <p class="list-subtitle">
            Wybierz obszar, który Cię interesuje. Każda kategoria gromadzi kursy pogrupowane tematycznie — od backendu po AI.
          </p>
        </header>
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
          v-for="(item, index) in items"
          :key="item.id"
          class="list-card glass-card"
          :to="{ name: 'CategoryDetails', params: { slug: item.slug } }"
        >
          <div class="card-head">
            <span class="card-num">No. {{ String(index + 1).padStart(2, '0') }}</span>
            <span class="card-slug">/{{ item.slug }}</span>
          </div>
          <h3 class="card-title">{{ item.name }}</h3>
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
  min-height: 100vh;
}

.list-hero {
  padding: 32px 0 0;
}

.list-head {
  margin-bottom: 40px;
}

.list-title {
  font-size: $font-size-2xl;
  margin-top: 18px;
}

.list-subtitle {
  font-size: 0.98rem;
  line-height: 1.6;
}

.list-body {
  padding-bottom: 96px;
}

.list-loading,
.list-empty {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 48px 0;
  color: $color-muted;
}

.list-loading p {
  @include mono-label;
  color: $color-faint;
}

.list-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.list-card {
  --lg-blur: 10px;
  padding: 22px 24px 20px;
  text-decoration: none;
  color: inherit;
  display: flex;
  flex-direction: column;
  min-height: 230px;
}

.card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding-bottom: 14px;
  margin-bottom: 18px;
  border-bottom: 1px solid $color-hairline;
}

.card-num {
  @include mono-label;
  color: $color-accent;
}

.card-slug {
  font-family: $font-mono;
  font-size: 0.7rem;
  color: $color-faint;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.card-title {
  font-family: $font-display;
  font-size: 1.4rem;
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
  padding-top: 14px;
  border-top: 1px dashed $color-hairline;
}

.card-count {
  display: flex;
  align-items: baseline;
  gap: 8px;

  strong {
    font-family: $font-display;
    font-size: 1.6rem;
    font-weight: 500;
    line-height: 1;
    color: $color-ink;
  }

  span {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
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

.list-card:hover .card-cta {
  gap: 10px;
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
