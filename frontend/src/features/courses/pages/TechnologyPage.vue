<template>
  <div class="technology-page">
    <div class="technology-hero">
      <div class="container">
        <span class="eyebrow">Technologia</span>
        <h1 class="technology-title">{{ technologyName }}</h1>
        <p class="technology-desc">{{ technologyDescription }}</p>

        <div class="technology-toolbar">
          <div class="sort-box" ref="sortBoxRef">
            <button
              type="button"
              class="sort-trigger"
              :class="{ open: sortOpen }"
              @click="sortOpen = !sortOpen"
            >
              <span class="sort-label">Sortuj:</span>
              <span class="sort-value">{{ currentSortLabel }}</span>
              <svg class="sort-arrow" :class="{ flipped: sortOpen }" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
            </button>
            <ul v-if="sortOpen" class="sort-menu" @pointerleave="hideSortBlob">
              <span ref="sortBlobEl" class="sort-blob" aria-hidden="true"></span>
              <li
                v-for="opt in sortOptions"
                :key="opt.value"
                class="sort-option"
                :class="{ active: state.sortBy === opt.value }"
                @click="selectSort(opt.value)"
                @pointerenter="moveSortBlob($event)"
              >
                <span>{{ opt.label }}</span>
                <svg v-if="state.sortBy === opt.value" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.6" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>

    <div class="container technology-body">
      <aside class="technology-sidebar">
        <div class="filter-group glass-card">
          <div class="filter-header">
            <h3>Filtry</h3>
            <button v-if="hasActiveFilters" class="filter-reset" @click="resetFilters">
              Wyczyść
            </button>
          </div>

          <div class="filter-section">
            <h4>Poziom</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input type="radio" name="level" :checked="state.level === null" @change="setLevel(null)" />
                <span>Wszystkie</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="level" :checked="state.level === CourseLevel.Beginner" @change="setLevel(CourseLevel.Beginner)" />
                <span>Początkujący</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="level" :checked="state.level === CourseLevel.Intermediate" @change="setLevel(CourseLevel.Intermediate)" />
                <span>Średni</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="level" :checked="state.level === CourseLevel.Advanced" @change="setLevel(CourseLevel.Advanced)" />
                <span>Zaawansowany</span>
              </label>
            </div>
          </div>

          <div class="filter-section">
            <h4>Cena</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input type="radio" name="price" :checked="state.minPrice === null && state.maxPrice === null" @change="setPriceRange(null, null)" />
                <span>Dowolna</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="price" :checked="state.minPrice === 0 && state.maxPrice === 0" @change="setPriceRange(0, 0)" />
                <span>Darmowe</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="price" :checked="state.minPrice === 0 && state.maxPrice === 100" @change="setPriceRange(0, 100)" />
                <span>0 – 100 zł</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="price" :checked="state.minPrice === 100 && state.maxPrice === 500" @change="setPriceRange(100, 500)" />
                <span>100 – 500 zł</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="price" :checked="state.minPrice === 500 && state.maxPrice === null" @change="setPriceRange(500, null)" />
                <span>500+ zł</span>
              </label>
            </div>
          </div>

          <div class="filter-section">
            <h4>Język</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input type="radio" name="language" :checked="state.language === null" @change="setLanguage(null)" />
                <span>Wszystkie</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="language" :checked="state.language === 'Polski'" @change="setLanguage('Polski')" />
                <span>Polski</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="language" :checked="state.language === 'English'" @change="setLanguage('English')" />
                <span>English</span>
              </label>
            </div>
          </div>

          <div class="filter-section" v-if="categories.length">
            <h4>Kategorie</h4>
            <div class="filter-chips">
              <button
                v-for="cat in categories"
                :key="cat.id"
                type="button"
                class="filter-chip"
                :class="{ active: state.categoryIds.includes(cat.id) }"
                @click="toggleCategoryId(cat.id)"
              >
                {{ cat.name }}
              </button>
            </div>
          </div>

          <div class="filter-section">
            <h4>Minimalna ocena</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input type="radio" name="minRating" :checked="state.minRating === null" @change="setMinRating(null)" />
                <span>Dowolna</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="minRating" :checked="state.minRating === 4" @change="setMinRating(4)" />
                <span>4+ gwiazdek</span>
              </label>
              <label class="filter-option">
                <input type="radio" name="minRating" :checked="state.minRating === 3" @change="setMinRating(3)" />
                <span>3+ gwiazdek</span>
              </label>
            </div>
          </div>
        </div>
      </aside>

      <main class="technology-main">
        <div v-if="isLoading" class="technology-loading">
          <div class="spinner" />
          <p>Ładowanie kursów...</p>
        </div>

        <div v-else-if="isError" class="technology-error">
          <p>Wystąpił błąd podczas ładowania kursów.</p>
          <button class="btn btn-primary" @click="resetFilters">Spróbuj ponownie</button>
        </div>

        <div v-else-if="filteredItems.length === 0" class="technology-empty">
          <p>Brak kursów dla tej technologii spełniających wybrane kryteria.</p>
          <button class="btn btn-primary" @click="resetFilters">Wyczyść filtry</button>
        </div>

        <div v-else>
          <div class="results-header">
            <span class="results-count">Znaleziono <strong>{{ totalCount }}</strong> {{ totalCount === 1 ? 'kurs' : 'kursów' }}</span>
          </div>
          <div class="technology-grid">
            <CourseBrowseCard v-for="course in filteredItems" :key="course.id" :course="course" />
          </div>
        </div>

        <div v-if="totalPages > 1 && !isLoading" class="technology-pagination">
          <button class="page-btn" :disabled="state.pageNumber <= 1" @click="setPage(state.pageNumber - 1)">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
          </button>
          <button
            v-for="page in visiblePages"
            :key="page"
            class="page-btn"
            :class="{ active: page === state.pageNumber }"
            @click="setPage(page)"
          >
            {{ page }}
          </button>
          <button class="page-btn" :disabled="state.pageNumber >= totalPages" @click="setPage(state.pageNumber + 1)">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
          </button>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useQuery } from '@tanstack/vue-query'
import { getCategories, getTechnologies } from '@/features/courses/api/courses.api'
import { useCourseBrowse, type SortOption } from '@/features/courses/composables/useCourseBrowse'
import { CourseLevel, CourseStatus } from '@/features/courses/types/course.types'
import CourseBrowseCard from '@/features/courses/components/CourseBrowseCard.vue'

const route = useRoute()
const slug = computed(() => route.params.slug as string)

const { data: allTechnologies } = useQuery({
  queryKey: ['technologies'],
  queryFn: getTechnologies,
  initialData: []
})

const { data: categories } = useQuery({
  queryKey: ['categories'],
  queryFn: getCategories,
  initialData: []
})

const technology = computed(() => allTechnologies.value.find((t) => t.slug === slug.value))
const technologyName = computed(() => technology.value?.name ?? slug.value)
const technologyDescription = computed(
  () => technology.value?.description ?? `Kursy z technologii ${technologyName.value}. Rozwijaj umiejętności w praktyce.`
)
const technologyId = computed(() => technology.value?.id ?? '')

const {
  state,
  isLoading,
  isError,
  filteredItems,
  totalCount,
  totalPages,
  setLevel,
  setSortBy,
  setPage,
  setPriceRange,
  setLanguage,
  toggleCategoryId,
  setMinRating,
  resetFilters
} = useCourseBrowse({
  baseFilter: { status: CourseStatus.Published }
})

watch(
  technologyId,
  (id) => {
    if (id && !state.technologyIds.includes(id)) {
      state.technologyIds = [id]
      state.pageNumber = 1
    }
  },
  { immediate: true }
)

const sortOptions: { value: SortOption; label: string }[] = [
  { value: 'newest', label: 'Najnowsze' },
  { value: 'popular', label: 'Najpopularniejsze' },
  { value: 'rating', label: 'Najwyżej oceniane' },
  { value: 'price-asc', label: 'Cena: rosnąco' },
  { value: 'price-desc', label: 'Cena: malejąco' }
]

const sortOpen = ref(false)
const sortBoxRef = ref<HTMLElement | null>(null)
const sortBlobEl = ref<HTMLElement | null>(null)

const currentSortLabel = computed(
  () => sortOptions.find((o) => o.value === state.sortBy)?.label ?? 'Najnowsze'
)

function selectSort(value: SortOption) {
  setSortBy(value)
  sortOpen.value = false
}

function moveSortBlob(e: PointerEvent) {
  if (!sortBlobEl.value) return
  const target = e.currentTarget as HTMLElement
  const li = target.closest('li')
  if (!li) return
  const ul = li.parentElement
  if (!ul) return
  const ulRect = ul.getBoundingClientRect()
  const liRect = li.getBoundingClientRect()
  sortBlobEl.value.style.left = `${liRect.left - ulRect.left}px`
  sortBlobEl.value.style.width = `${liRect.width}px`
  sortBlobEl.value.style.top = `${liRect.top - ulRect.top}px`
  sortBlobEl.value.style.height = `${liRect.height}px`
  sortBlobEl.value.style.opacity = '1'
}

function hideSortBlob() {
  if (!sortBlobEl.value) return
  sortBlobEl.value.style.opacity = '0'
}

function handleClickOutside(event: MouseEvent) {
  if (sortOpen.value && sortBoxRef.value && !sortBoxRef.value.contains(event.target as Node)) {
    sortOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', handleClickOutside)
})

const hasActiveFilters = computed(() => {
  return (
    state.level !== null ||
    state.minPrice !== null ||
    state.maxPrice !== null ||
    state.language !== null ||
    state.categoryIds.length > 0 ||
    state.minRating !== null
  )
})

const visiblePages = computed(() => {
  const total = totalPages.value
  const current = state.pageNumber
  const pages: number[] = []
  const range = 2
  let start = Math.max(1, current - range)
  let end = Math.min(total, current + range)
  if (end - start < range * 2) {
    if (start === 1) end = Math.min(total, start + range * 2)
    else if (end === total) start = Math.max(1, end - range * 2)
  }
  for (let i = start; i <= end; i++) pages.push(i)
  return pages
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;
@use "sass:color";

.technology-page {
  padding-top: $header-height;
  min-height: 100vh;
}

.technology-hero {
  padding: 48px 0 28px;
  border-bottom: 1px solid $color-hairline;
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
  background: rgba(34, 211, 238, 0.12);
  border: 1px solid rgba(34, 211, 238, 0.28);
  border-radius: 999px;
  margin-bottom: 16px;
}

.technology-title {
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

.technology-desc {
  color: $color-muted;
  font-size: 1rem;
  max-width: 720px;
  margin-bottom: 24px;
  line-height: 1.55;
}

.technology-toolbar {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
  align-items: center;
}

.sort-box {
  position: relative;
}

.sort-trigger {
  @include liquid-glass;
  --lg-r: 14px;
  --lg-blur: 0px;
  --lg-tint: rgba(255, 255, 255, 0.04);
  appearance: none;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.9rem;
  color: $color-ink;
  padding: 13px 16px;
  cursor: pointer;
  outline: none;
  min-width: 240px;
  display: inline-flex;
  align-items: center;
  gap: 10px;
  text-align: left;
  box-shadow:
    0 10px 30px rgba(3, 6, 24, 0.35),
    0 2px 8px rgba(3, 6, 24, 0.22),
    0 18px 30px -22px rgba(170, 200, 255, 0.35),
    inset 0 1px 1px rgba(255, 255, 255, 0.3),
    inset 0 0 0 1px rgba(255, 255, 255, 0.1);
  transition: --lg-tint 0.35s, box-shadow 0.35s, transform 0.2s;

  &:hover {
    --lg-tint: rgba(34, 211, 238, 0.08);
    box-shadow:
      0 10px 30px rgba(3, 6, 24, 0.4),
      0 2px 8px rgba(3, 6, 24, 0.25),
      0 18px 30px -22px rgba(34, 211, 238, 0.4),
      inset 0 1px 1px rgba(255, 255, 255, 0.35),
      inset 0 0 0 1px rgba(34, 211, 238, 0.3);
  }

  &.open {
    --lg-tint: rgba(34, 211, 238, 0.1);
  }

  .sort-label {
    color: $color-faint;
    font-size: 0.78rem;
    font-weight: 500;
    text-transform: uppercase;
    letter-spacing: 0.06em;
  }

  .sort-value {
    flex: 1;
    color: $color-ink;
    font-weight: 600;
  }

  .sort-arrow {
    color: $color-cyan;
    transition: transform 0.25s;
    flex-shrink: 0;

    &.flipped {
      transform: rotate(180deg);
    }
  }

  &.open .sort-arrow {
    color: $color-ink;
  }
}

.sort-menu {
  @include liquid-glass;
  --lg-r: 16px;
  --lg-blur: 2px;
  --lg-tint: rgba(17, 24, 39, 0.55);
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 260px;
  margin: 0;
  padding: 8px;
  list-style: none;
  z-index: 30;
  box-shadow:
    0 24px 60px rgba(3, 6, 24, 0.55),
    0 4px 14px rgba(3, 6, 24, 0.35),
    inset 0 1px 1px rgba(255, 255, 255, 0.18),
    inset 0 0 0 1px rgba(255, 255, 255, 0.1);
}

.sort-blob {
  position: absolute;
  top: 0;
  left: 0;
  border-radius: 12px;
  background: linear-gradient(135deg, rgba(34, 211, 238, 0.16), rgba(34, 211, 238, 0.08));
  box-shadow:
    inset 1.5px 1.5px 2px -1px rgba(255, 255, 255, 0.35),
    inset -1.5px -1.5px 2px -1px rgba(255, 255, 255, 0.08),
    inset 0 -8px 16px -10px rgba(34, 211, 238, 0.25);
  opacity: 0;
  pointer-events: none;
  transition:
    left 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    width 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    top 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    height 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    opacity 0.2s;
}

.sort-option {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 14px;
  font-size: 0.9rem;
  color: $color-ink;
  cursor: pointer;
  transition: color 0.18s;

  svg {
    color: $color-cyan;
    flex-shrink: 0;
  }

  &.active {
    color: $color-ink;
    font-weight: 600;
  }
}

.technology-body {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 32px;
  padding-top: 32px;
  padding-bottom: 80px;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
  }
}

.technology-sidebar {
  @media (max-width: 860px) {
    order: -1;
  }
}

.filter-group {
  --lg-r: 20px;
  --lg-blur: 0px;
  padding: 24px;
  position: sticky;
  top: calc($header-height + 24px);
}

.filter-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 20px;

  h3 {
    font-family: $font-display;
    font-size: 1.05rem;
    font-weight: 600;
  }
}

.filter-reset {
  background: none;
  border: none;
  font: inherit;
  font-size: 0.82rem;
  color: $color-cyan;
  cursor: pointer;
  padding: 0;
  text-decoration: underline;

  &:hover {
    color: color.adjust($color-cyan, $lightness: 10%);
  }
}

.filter-section {
  margin-bottom: 24px;

  &:last-child {
    margin-bottom: 0;
  }

  h4 {
    font-size: 0.82rem;
    font-weight: 600;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: $color-faint;
    margin-bottom: 12px;
  }
}

.filter-options {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.filter-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.filter-chip {
  appearance: none;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 999px;
  padding: 6px 14px;
  font: inherit;
  font-size: 0.82rem;
  color: $color-muted;
  cursor: pointer;
  transition: background 0.2s, color 0.2s, border-color 0.2s;

  &:hover {
    background: rgba(255, 255, 255, 0.08);
    color: $color-ink;
  }

  &.active {
    background: rgba(34, 211, 238, 0.15);
    border-color: rgba(34, 211, 238, 0.4);
    color: $color-cyan;
  }
}

.filter-option {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 0.9rem;
  color: $color-muted;
  cursor: pointer;
  transition: color 0.2s;

  &:hover {
    color: $color-ink;
  }

  input[type="radio"] {
    appearance: none;
    width: 16px;
    height: 16px;
    border: 2px solid $color-faint;
    border-radius: 50%;
    flex-shrink: 0;
    cursor: pointer;
    position: relative;
    transition: border-color 0.2s;

    &:checked {
      border-color: $color-cyan;

      &::after {
        content: "";
        position: absolute;
        inset: 3px;
        border-radius: 50%;
        background: $color-cyan;
      }
    }
  }
}

.technology-main {
  min-width: 0;
}

.results-header {
  margin-bottom: 16px;
  font-size: 0.9rem;
  color: $color-muted;

  strong {
    color: $color-ink;
    font-weight: 700;
  }
}

.technology-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;

  @media (max-width: 600px) {
    grid-template-columns: 1fr;
  }
}

.technology-loading,
.technology-error,
.technology-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 80px 20px;
  text-align: center;
  color: $color-muted;
}

.technology-pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  margin-top: 48px;
}

.page-btn {
  @include liquid-glass;
  --lg-r: 10px;
  --lg-blur: 0px;
  min-width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  font: inherit;
  font-size: 0.9rem;
  font-weight: 600;
  color: $color-ink;
  cursor: pointer;
  padding: 0 12px;
  transition: transform 0.15s, box-shadow 0.2s;

  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(3, 6, 24, 0.4);
  }

  &:disabled {
    opacity: 0.4;
    cursor: not-allowed;
  }

  &.active {
    --lg-tint: rgba(34, 211, 238, 0.18);
    box-shadow: 0 0 0 1px rgba(34, 211, 238, 0.4);
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
</style>