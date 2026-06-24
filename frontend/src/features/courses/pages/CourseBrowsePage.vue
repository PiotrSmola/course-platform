<template>
  <div class="browse-page">
    <div class="browse-hero">
      <div class="container">
        <h1 class="browse-title">Wszystkie kursy</h1>
        <p class="browse-subtitle">Odkryj {{ browse.totalCount }} kursów i rozpocznij naukę już dziś</p>

        <div class="browse-toolbar">
          <div class="search-box glass">
            <input
              v-model="searchInput"
              type="text"
              placeholder="Szukaj kursów..."
              class="search-input"
              @keyup.enter="browse.setSearchTerm(searchInput)"
            />
            <button class="search-btn" @click="browse.setSearchTerm(searchInput)">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>
            </button>
          </div>

          <div class="sort-box">
            <select v-model="sortValue" class="sort-select" @change="browse.setSortBy(sortValue as SortOption)">
              <option value="newest">Najnowsze</option>
              <option value="popular">Najpopularniejsze</option>
              <option value="rating">Najwyżej oceniane</option>
              <option value="price-asc">Cena: rosnąco</option>
              <option value="price-desc">Cena: malejąco</option>
            </select>
          </div>
        </div>
      </div>
    </div>

    <div class="container browse-body">
      <aside class="browse-sidebar">
        <div class="filter-group glass-card">
          <div class="filter-header">
            <h3>Filtry</h3>
            <button v-if="hasActiveFilters" class="filter-reset" @click="browse.resetFilters">
              Wyczyść
            </button>
          </div>

          <div class="filter-section">
            <h4>Poziom</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input
                  type="radio"
                  name="level"
                  :checked="browse.state.level === null"
                  @change="browse.setLevel(null)"
                />
                <span>Wszystkie</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="level"
                  :checked="browse.state.level === CourseLevel.Beginner"
                  @change="browse.setLevel(CourseLevel.Beginner)"
                />
                <span>Początkujący</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="level"
                  :checked="browse.state.level === CourseLevel.Intermediate"
                  @change="browse.setLevel(CourseLevel.Intermediate)"
                />
                <span>Średni</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="level"
                  :checked="browse.state.level === CourseLevel.Advanced"
                  @change="browse.setLevel(CourseLevel.Advanced)"
                />
                <span>Zaawansowany</span>
              </label>
            </div>
          </div>

          <div class="filter-section">
            <h4>Cena</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="browse.state.minPrice === null && browse.state.maxPrice === null"
                  @change="browse.setPriceRange(null, null)"
                />
                <span>Dowolna</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="browse.state.minPrice === 0 && browse.state.maxPrice === 0"
                  @change="browse.setPriceRange(0, 0)"
                />
                <span>Darmowe</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="browse.state.minPrice === 0 && browse.state.maxPrice === 100"
                  @change="browse.setPriceRange(0, 100)"
                />
                <span>0 – 100 zł</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="browse.state.minPrice === 100 && browse.state.maxPrice === 500"
                  @change="browse.setPriceRange(100, 500)"
                />
                <span>100 – 500 zł</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="browse.state.minPrice === 500 && browse.state.maxPrice === null"
                  @change="browse.setPriceRange(500, null)"
                />
                <span>500+ zł</span>
              </label>
            </div>
          </div>
        </div>
      </aside>

      <main class="browse-main">
        <div v-if="browse.isLoading" class="browse-loading">
          <div class="spinner" />
          <p>Ładowanie kursów...</p>
        </div>

        <div v-else-if="browse.isError" class="browse-error">
          <p>Wystąpił błąd podczas ładowania kursów.</p>
          <button class="btn btn-primary" @click="browse.resetFilters">Spróbuj ponownie</button>
        </div>

        <div v-else-if="browse.filteredItems.length === 0" class="browse-empty">
          <p>Nie znaleziono kursów spełniających kryteria.</p>
          <button class="btn btn-primary" @click="browse.resetFilters">Wyczyść filtry</button>
        </div>

        <div v-else class="browse-grid">
          <CourseBrowseCard
            v-for="course in browse.filteredItems"
            :key="course.id"
            :course="course"
          />
        </div>

        <div v-if="browse.totalPages > 1 && !browse.isLoading" class="browse-pagination">
          <button
            class="page-btn"
            :disabled="browse.state.pageNumber <= 1"
            @click="browse.setPage(browse.state.pageNumber - 1)"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
          </button>

          <button
            v-for="page in visiblePages"
            :key="page"
            class="page-btn"
            :class="{ active: page === browse.state.pageNumber }"
            @click="browse.setPage(page)"
          >
            {{ page }}
          </button>

          <button
            class="page-btn"
            :disabled="browse.state.pageNumber >= browse.totalPages"
            @click="browse.setPage(browse.state.pageNumber + 1)"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
          </button>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useCourseBrowse, type SortOption } from '@/features/courses/composables/useCourseBrowse'
import CourseBrowseCard from '@/features/courses/components/CourseBrowseCard.vue'
import { CourseLevel } from '@/features/courses/types/course.types'

const browse = useCourseBrowse()

const searchInput = ref(browse.state.searchTerm)

watch(() => browse.state.searchTerm, (val) => {
  searchInput.value = val
})

const sortValue = computed({
  get: () => browse.state.sortBy,
  set: (val: SortOption) => browse.setSortBy(val)
})

const hasActiveFilters = computed(() => {
  return browse.state.level !== null ||
    browse.state.minPrice !== null ||
    browse.state.maxPrice !== null ||
    browse.state.searchTerm !== ''
})

const visiblePages = computed(() => {
  const total = browse.totalPages.value
  const current = browse.state.pageNumber
  const pages: number[] = []
  const range = 2

  let start = Math.max(1, current - range)
  let end = Math.min(total, current + range)

  if (end - start < range * 2) {
    if (start === 1) {
      end = Math.min(total, start + range * 2)
    } else if (end === total) {
      start = Math.max(1, end - range * 2)
    }
  }

  for (let i = start; i <= end; i++) {
    pages.push(i)
  }
  return pages
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.browse-page {
  padding-top: $header-height;
  min-height: 100vh;
}

.browse-hero {
  padding: 40px 0 32px;
  border-bottom: 1px solid $color-hairline;
}

.browse-title {
  font-family: $font-display;
  font-size: $font-size-xl;
  font-weight: 600;
  letter-spacing: -0.02em;
  margin-bottom: 8px;
}

.browse-subtitle {
  color: $color-muted;
  font-size: 0.95rem;
  margin-bottom: 24px;
}

.browse-toolbar {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
}

.search-box {
  @include liquid-glass;
  --lg-r: 999px;
  --lg-blur: 0px;
  flex: 1;
  min-width: 260px;
  display: flex;
  align-items: center;
  padding: 4px 4px 4px 18px;
  gap: 8px;
}

.search-input {
  flex: 1;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.95rem;
  color: $color-ink;
  outline: none;

  &::placeholder {
    color: $color-faint;
  }
}

.search-btn {
  @include liquid-glass;
  --lg-r: 999px;
  --lg-blur: 0px;
  --lg-tint: rgba(245, 158, 11, 0.15);
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  color: $color-ink;
  cursor: pointer;
  flex-shrink: 0;
  transition: transform 0.2s;

  &:hover {
    transform: scale(1.05);
  }
}

.sort-box {
  position: relative;
}

.sort-select {
  @include liquid-glass;
  --lg-r: 12px;
  --lg-blur: 0px;
  appearance: none;
  background: none;
  border: none;
  font: inherit;
  font-size: 0.9rem;
  color: $color-ink;
  padding: 12px 40px 12px 16px;
  cursor: pointer;
  outline: none;
  min-width: 180px;
}

.browse-body {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: 32px;
  padding-top: 32px;
  padding-bottom: 80px;

  @media (max-width: 860px) {
    grid-template-columns: 1fr;
  }
}

.browse-sidebar {
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
  color: $color-gold;
  cursor: pointer;
  padding: 0;
  text-decoration: underline;

  &:hover {
    color: lighten($color-gold, 10%);
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
      border-color: $color-gold;

      &::after {
        content: "";
        position: absolute;
        inset: 3px;
        border-radius: 50%;
        background: $color-gold;
      }
    }
  }
}

.browse-main {
  min-width: 0;
}

.browse-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 24px;

  @media (max-width: 600px) {
    grid-template-columns: 1fr;
  }
}

.browse-loading,
.browse-error,
.browse-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  padding: 80px 20px;
  text-align: center;
  color: $color-muted;
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
  to {
    transform: rotate(360deg);
  }
}

.browse-pagination {
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
    --lg-tint: rgba(245, 158, 11, 0.18);
    box-shadow: 0 0 0 1px rgba(245, 158, 11, 0.4);
  }
}
</style>
