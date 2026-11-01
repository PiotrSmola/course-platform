<template>
  <div class="technology-page">
    <div class="technology-hero">
      <div class="container">
        <header class="technology-head">
          <div class="technology-head__main">
            <span class="eyebrow">Technologia</span>
            <h1 class="technology-title">{{ technologyName }}</h1>
          </div>
          <div class="technology-head__aside">
            <p class="technology-count"><span>{{ String(totalCount).padStart(2, '0') }}</span> {{ totalCount === 1 ? 'kurs' : 'kursów' }}</p>
            <p class="technology-desc">{{ technologyDescription }}</p>
          </div>
        </header>

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
            {{ String(page).padStart(2, '0') }}
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
import { queryKeys } from '@/shared/queryKeys'

const route = useRoute()
const slug = computed(() => route.params.slug as string)

const { data: allTechnologies } = useQuery({
  queryKey: queryKeys.technologies(),
  queryFn: getTechnologies,
  initialData: []
})

const { data: categories } = useQuery({
  queryKey: queryKeys.categories(),
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

.technology-page {
  min-height: 100vh;
}

.technology-hero {
  padding: 32px 0 28px;
}

.technology-head {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 420px);
  column-gap: 48px;
  align-items: end;
  padding-bottom: 28px;
  margin-bottom: 24px;
  border-bottom: 1px solid $color-hairline;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
    row-gap: 18px;
  }
}

.technology-title {
  font-size: $font-size-2xl;
  margin-top: 18px;
  overflow-wrap: anywhere;
}

.technology-count {
  @include mono-label;
  color: $color-muted;
  margin-bottom: 8px;

  span {
    color: $color-accent;
  }
}

.technology-desc {
  color: $color-muted;
  font-size: 0.95rem;
  line-height: 1.6;
}

.technology-toolbar {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  flex-wrap: wrap;
  align-items: center;
}

.sort-box {
  position: relative;
  width: min(100%, 280px);
}

.sort-trigger::before {
  inset: 0;
  clip-path: none;
}

.sort-trigger {
  @include liquid-glass;
  --lg-r: #{$radius-md};
  --lg-blur: 10px;
  --lg-tint: rgba(13, 15, 19, 0.3);
  appearance: none;
  width: 100%;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.9rem;
  color: $color-ink;
  padding: 0 16px;
  height: 48px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 12px;
  text-align: left;
  transition: --lg-tint 0.3s;

  &:hover,
  &.open {
    --lg-tint: rgba(255, 255, 255, 0.07);
  }

  .sort-label {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
  }

  .sort-value {
    flex: 1;
    color: $color-ink;
    font-weight: 500;
  }

  .sort-arrow {
    color: $color-muted;
    transition: transform 0.25s;
    flex-shrink: 0;

    &.flipped {
      transform: rotate(180deg);
    }
  }
}

.sort-menu {
  @include liquid-glass;
  --lg-r: 16px;
  --lg-blur: 18px;
  --lg-tint: #{rgba($color-bg, 0.72)};
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  width: 100%;
  margin: 0;
  padding: 6px;
  list-style: none;
  z-index: 30;
}

.sort-blob {
  position: absolute;
  top: 0;
  left: 0;
  border-radius: 10px;
  background: rgba($color-ink, 0.08);
  opacity: 0;
  pointer-events: none;
  transition:
    left 0.45s $ease-jelly,
    width 0.45s $ease-jelly,
    top 0.45s $ease-jelly,
    height 0.45s $ease-jelly,
    opacity 0.2s;
}

.sort-option {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 11px 12px;
  font-size: 0.9rem;
  color: $color-muted;
  cursor: pointer;
  transition: color 0.18s;

  &:hover,
  &.active {
    color: $color-ink;
  }

  svg {
    color: $color-accent;
    flex-shrink: 0;
  }
}

.technology-body {
  display: grid;
  grid-template-columns: 290px minmax(0, 1fr);
  gap: 36px;
  padding-top: 12px;
  padding-bottom: 96px;

  @media (max-width: 860px) {
    grid-template-columns: minmax(0, 1fr);
  }
}

.technology-sidebar {
  min-width: 0;

  @media (max-width: 860px) {
    order: -1;
  }
}

.filter-group {
  --lg-blur: 10px;
  padding: 22px 22px 24px;
  position: sticky;
  top: calc($header-height + 24px);

  &:hover {
    transform: none;
  }
}

.filter-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-bottom: 14px;
  margin-bottom: 18px;
  border-bottom: 1px solid $color-hairline;

  h3 {
    font-family: $font-display;
    font-size: 1.2rem;
    font-weight: 500;
  }
}

.filter-reset {
  @include mono-label;
  background: none;
  border: none;
  font-size: 0.62rem;
  color: $color-accent;
  cursor: pointer;
  padding: 0 0 2px;
  border-bottom: 1px dashed rgba($color-accent, 0.5);
}

.filter-section {
  padding-bottom: 18px;
  margin-bottom: 18px;
  border-bottom: 1px dashed $color-hairline;

  &:last-child {
    padding-bottom: 0;
    margin-bottom: 0;
    border-bottom: none;
  }

  h4 {
    @include mono-label;
    font-size: 0.62rem;
    color: $color-faint;
    margin-bottom: 12px;
  }
}

.filter-options {
  display: flex;
  flex-direction: column;
  gap: 9px;
}

.filter-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.filter-chip {
  @include tag;
  appearance: none;
  border: none;
  padding: 6px 9px;
  cursor: pointer;
  transition: color 0.2s, background 0.2s, box-shadow 0.2s;

  &:hover {
    color: $color-ink;
    box-shadow: inset 0 0 0 1px rgba($color-ink, 0.3);
  }

  &.active {
    color: $color-accent;
    background: rgba($color-accent, 0.1);
    box-shadow: inset 0 0 0 1px rgba($color-accent, 0.6);
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

  &:hover,
  &:has(input:checked) {
    color: $color-ink;
  }

  input[type="radio"] {
    appearance: none;
    width: 14px;
    height: 14px;
    margin: 0;
    border-radius: 50%;
    box-shadow: inset 0 0 0 1px rgba($color-ink, 0.35);
    flex-shrink: 0;
    cursor: pointer;
    transition: box-shadow 0.2s, background 0.2s;

    &:checked {
      background: $color-accent;
      box-shadow:
        inset 0 0 0 1px $color-accent,
        inset 0 0 0 4px rgba(13, 15, 19, 0.9);
    }
  }
}

.technology-main {
  min-width: 0;
}

.results-header {
  @include mono-label;
  margin-bottom: 16px;
  color: $color-faint;

  strong {
    color: $color-accent;
    font-weight: 500;
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
  align-items: flex-start;
  gap: 16px;
  padding: 48px 0;
  color: $color-muted;
  border-top: 1px solid $color-hairline;
}

.technology-loading {
  flex-direction: row;
  align-items: center;

  p {
    @include mono-label;
    color: $color-faint;
  }
}

.technology-pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 4px;
  margin-top: 56px;
  padding-top: 24px;
  border-top: 1px solid $color-hairline;
}

.page-btn {
  min-width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: $radius-xs;
  background: none;
  font-family: $font-mono;
  font-size: 0.82rem;
  letter-spacing: 0.06em;
  color: $color-muted;
  cursor: pointer;
  padding: 0 10px;
  transition: color 0.2s, background 0.2s;

  &:hover:not(:disabled) {
    color: $color-ink;
    background: rgba($color-ink, 0.06);
  }

  &:disabled {
    color: $color-faint;
    cursor: not-allowed;
  }

  &.active {
    color: $color-on-accent;
    background: $color-accent;
  }
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

@media (max-width: 620px) {
  .sort-box {
    width: 100%;
  }
}
</style>
