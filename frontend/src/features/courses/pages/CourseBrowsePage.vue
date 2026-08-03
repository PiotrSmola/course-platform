<template>
  <div class="browse-page">
    <div class="browse-hero">
      <div class="container">
        <h1 class="browse-title">Wszystkie kursy</h1>
        <p class="browse-subtitle">Odkryj {{ totalCount }} kursów i rozpocznij naukę już dziś</p>
        <div class="subscription-banner glass-card">
          <div>
            <span class="subscription-banner__eyebrow">All-access</span>
            <h2>Ucz się bez limitu za {{ monthlyPriceLabel }} / mies.</h2>
            <p>Jedna subskrypcja odblokowuje wszystkie opublikowane kursy i pozwala zarządzać planem w Stripe Billing Portal.</p>
          </div>
          <div class="subscription-banner__actions">
            <button
              v-if="authStore.isAuthenticated && !hasActiveSubscription"
              type="button"
              class="btn btn-primary"
              :disabled="subscriptionCheckoutMutation.isPending.value"
              @click="startSubscription"
            >
              {{ subscriptionCheckoutMutation.isPending.value ? 'Przekierowujemy...' : 'Aktywuj All-access' }}
            </button>
            <button
              v-else-if="authStore.isAuthenticated && canManageSubscription"
              type="button"
              class="btn btn-ghost"
              :disabled="billingPortalMutation.isPending.value"
              @click="openBillingPortal"
            >
              {{ billingPortalMutation.isPending.value ? 'Otwieramy...' : 'Zarządzaj subskrypcją' }}
            </button>
            <router-link v-else-if="!authStore.isAuthenticated" class="btn btn-primary" :to="{ name: 'Register' }">
              Załóż konto i subskrybuj
            </router-link>
            <span v-if="hasActiveSubscription" class="subscription-banner__badge">Aktywne dla Twojego konta</span>
          </div>
        </div>

        <div class="browse-toolbar">
          <div class="search-box glass">
            <input
              v-model="searchInput"
              type="text"
              placeholder="Szukaj kursów..."
              class="search-input"
              @keyup.enter="setSearchTerm(searchInput)"
            />
            <button class="search-btn" @click="setSearchTerm(searchInput)">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>
            </button>
          </div>

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

    <div class="container browse-body">
      <aside class="browse-sidebar">
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
                <input
                  type="radio"
                  name="level"
                  :checked="state.level === null"
                  @change="setLevel(null)"
                />
                <span>Wszystkie</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="level"
                  :checked="state.level === CourseLevel.Beginner"
                  @change="setLevel(CourseLevel.Beginner)"
                />
                <span>Początkujący</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="level"
                  :checked="state.level === CourseLevel.Intermediate"
                  @change="setLevel(CourseLevel.Intermediate)"
                />
                <span>Średni</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="level"
                  :checked="state.level === CourseLevel.Advanced"
                  @change="setLevel(CourseLevel.Advanced)"
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
                  :checked="state.minPrice === null && state.maxPrice === null"
                  @change="setPriceRange(null, null)"
                />
                <span>Dowolna</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="state.minPrice === 0 && state.maxPrice === 0"
                  @change="setPriceRange(0, 0)"
                />
                <span>Darmowe</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="state.minPrice === 0 && state.maxPrice === 100"
                  @change="setPriceRange(0, 100)"
                />
                <span>0 – 100 zł</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="state.minPrice === 100 && state.maxPrice === 500"
                  @change="setPriceRange(100, 500)"
                />
                <span>100 – 500 zł</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="price"
                  :checked="state.minPrice === 500 && state.maxPrice === null"
                  @change="setPriceRange(500, null)"
                />
                <span>500+ zł</span>
              </label>
            </div>
          </div>
          <div class="filter-section">
            <h4>Język</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input
                  type="radio"
                  name="language"
                  :checked="state.language === null"
                  @change="setLanguage(null)"
                />
                <span>Wszystkie</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="language"
                  :checked="state.language === 'Polski'"
                  @change="setLanguage('Polski')"
                />
                <span>Polski</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="language"
                  :checked="state.language === 'English'"
                  @change="setLanguage('English')"
                />
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

          <div class="filter-section" v-if="technologies.length">
            <h4>Technologie</h4>
            <div class="filter-chips">
              <button
                v-for="tech in technologies"
                :key="tech.id"
                type="button"
                class="filter-chip"
                :class="{ active: state.technologyIds.includes(tech.id) }"
                @click="toggleTechnologyId(tech.id)"
              >
                {{ tech.name }}
              </button>
            </div>
          </div>

          <div class="filter-section">
            <h4>Minimalna ocena</h4>
            <div class="filter-options">
              <label class="filter-option">
                <input
                  type="radio"
                  name="minRating"
                  :checked="state.minRating === null"
                  @change="setMinRating(null)"
                />
                <span>Dowolna</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="minRating"
                  :checked="state.minRating === 4"
                  @change="setMinRating(4)"
                />
                <span>4+ gwiazdek</span>
              </label>
              <label class="filter-option">
                <input
                  type="radio"
                  name="minRating"
                  :checked="state.minRating === 3"
                  @change="setMinRating(3)"
                />
                <span>3+ gwiazdek</span>
              </label>
            </div>
          </div>
        </div>
      </aside>

      <main class="browse-main">
        <div v-if="isLoading" class="browse-loading">
          <div class="spinner" />
          <p>Ładowanie kursów...</p>
        </div>

        <div v-else-if="isError" class="browse-error">
          <p>Wystąpił błąd podczas ładowania kursów.</p>
          <button class="btn btn-primary" @click="resetFilters">Spróbuj ponownie</button>
        </div>

        <div v-else-if="filteredItems.length === 0" class="browse-empty">
          <p>Nie znaleziono kursów spełniających kryteria.</p>
          <button class="btn btn-primary" @click="resetFilters">Wyczyść filtry</button>
        </div>

        <div v-else class="browse-grid">
          <CourseBrowseCard
            v-for="course in filteredItems"
            :key="course.id"
            :course="course"
          />
        </div>

        <div v-if="totalPages > 1 && !isLoading" class="browse-pagination">
          <button
            class="page-btn"
            :disabled="state.pageNumber <= 1"
            @click="setPage(state.pageNumber - 1)"
          >
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

          <button
            class="page-btn"
            :disabled="state.pageNumber >= totalPages"
            @click="setPage(state.pageNumber + 1)"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
          </button>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCourseBrowse, type SortOption } from '@/features/courses/composables/useCourseBrowse'
import CourseBrowseCard from '@/features/courses/components/CourseBrowseCard.vue'
import { useCreateBillingPortalSession, useCreateSubscriptionCheckout, useMySubscription, useSubscriptionOffer } from '@/features/payments/composables/usePayments'
import { CourseLevel } from '@/features/courses/types/course.types'
import { getCategories, getTechnologies } from '@/features/courses/api/courses.api'
import { queryKeys } from '@/shared/queryKeys'

const authStore = useAuthStore()
const subscriptionCheckoutMutation = useCreateSubscriptionCheckout()
const billingPortalMutation = useCreateBillingPortalSession()
const subscriptionQuery = useMySubscription(() => authStore.isAuthenticated)
const subscriptionOfferQuery = useSubscriptionOffer()
const monthlyPriceLabel = computed(() => {
  const price = subscriptionOfferQuery.data.value?.monthlyPricePln ?? 399
  return `${price} zł`
})

const {
  state,
  isLoading,
  isError,
  filteredItems,
  totalCount,
  totalPages,
  setSearchTerm,
  setLevel,
  setSortBy,
  setPage,
  setPriceRange,
  setLanguage,
  toggleCategoryId,
  toggleTechnologyId,
  setMinRating,
  resetFilters
} = useCourseBrowse()

const { data: categories } = useQuery({
  queryKey: queryKeys.categories(),
  queryFn: getCategories,
  initialData: []
})

const { data: technologies } = useQuery({
  queryKey: queryKeys.technologies(),
  queryFn: getTechnologies,
  initialData: []
})

const hasActiveSubscription = computed(() => subscriptionQuery.data.value?.hasActiveAccess ?? false)
const canManageSubscription = computed(() => subscriptionQuery.data.value?.canManageInPortal ?? false)

const searchInput = ref(state.searchTerm)

watch(() => state.searchTerm, (val) => {
  searchInput.value = val
})

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
  return state.level !== null ||
    state.minPrice !== null ||
    state.maxPrice !== null ||
    state.searchTerm !== '' ||
    state.language !== null ||
    state.categoryIds.length > 0 ||
    state.technologyIds.length > 0 ||
    state.minRating !== null
})

const visiblePages = computed(() => {
  const total = totalPages.value
  const current = state.pageNumber
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

function startSubscription() {
  subscriptionCheckoutMutation.mutate()
}

function openBillingPortal() {
  billingPortalMutation.mutate()
}
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;
@use "sass:color";

@property --lg-tint {
  syntax: "<color>";
  inherits: true;
  initial-value: rgba(255, 255, 255, 0.04);
}

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
  margin-bottom: 20px;
}

.subscription-banner {
  --lg-r: 24px;
  --lg-blur: 0px;
  display: flex;
  justify-content: space-between;
  gap: 24px;
  padding: 22px 24px;
  margin-bottom: 24px;

  @media (max-width: 780px) {
    flex-direction: column;
    align-items: flex-start;
  }
}

.subscription-banner__eyebrow {
  display: inline-flex;
  margin-bottom: 10px;
  font-size: 0.76rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: $color-gold;
}

.subscription-banner h2 {
  font-size: 1.3rem;
  margin-bottom: 8px;
}

.subscription-banner p {
  color: $color-muted;
  max-width: 720px;
}

.subscription-banner__actions {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  justify-content: center;
  gap: 10px;

  @media (max-width: 780px) {
    align-items: flex-start;
  }
}

.subscription-banner__badge {
  font-size: 0.85rem;
  font-weight: 600;
  color: #86efac;
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
  --lg-tint: rgba(245, 158, 11, 0.85);
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  color: #1a1208;
  cursor: pointer;
  flex-shrink: 0;
  transition: transform 0.2s, box-shadow 0.2s;

  &:hover {
    --lg-tint: rgba(245, 158, 11, 1);
    transform: scale(1.05);
    box-shadow:
      0 10px 24px rgba(245, 158, 11, 0.35),
      0 2px 8px rgba(3, 6, 24, 0.25);
  }
}

.sort-box {
  position: relative;
}

.sort-trigger:focus-visible,
.sort-option:focus-visible {
  outline: none;
}

.sort-trigger::before {
  inset: 0;
  clip-path: none;
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
    --lg-tint: rgba(245, 158, 11, 0.08);
    box-shadow:
      0 10px 30px rgba(3, 6, 24, 0.4),
      0 2px 8px rgba(3, 6, 24, 0.25),
      0 18px 30px -22px rgba(245, 158, 11, 0.4),
      inset 0 1px 1px rgba(255, 255, 255, 0.35),
      inset 0 0 0 1px rgba(245, 158, 11, 0.3);
  }

  &.open {
    --lg-tint: rgba(245, 158, 11, 0.1);
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
    color: $color-gold;
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
  transition: --lg-tint 0.35s, box-shadow 0.35s;
}

.sort-menu::before {
  content: "";
  position: absolute;
  inset: 0;
  border-radius: inherit;
  pointer-events: none;
  background:
    radial-gradient(180px 100px at 18% 0%, rgba(255, 255, 255, 0.07), transparent 65%),
    radial-gradient(120px 80px at 100% 100%, rgba(245, 158, 11, 0.05), transparent 70%);
}

.sort-blob {
  position: absolute;
  top: 0;
  left: 0;
  border-radius: 12px;
  background: linear-gradient(135deg, rgba(245, 158, 11, 0.16), rgba(245, 158, 11, 0.08));
  box-shadow:
    inset 1.5px 1.5px 2px -1px rgba(255, 255, 255, 0.35),
    inset -1.5px -1.5px 2px -1px rgba(255, 255, 255, 0.08),
    inset 0 -8px 16px -10px rgba(245, 158, 11, 0.25);
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
    color: $color-gold;
    flex-shrink: 0;
  }

  &.active {
    color: $color-ink;
    font-weight: 600;
  }
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
    color: color.adjust($color-gold, $lightness: 10%);
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
    background: rgba(245, 158, 11, 0.15);
    border-color: rgba(245, 158, 11, 0.4);
    color: #fbbf24;
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
.subscription-banner {
  --lg-blur: 12px;
  gap: 18px;
  padding: 18px 20px;

  h2 {
    font-size: 1.12rem;
  }

  p {
    font-size: 0.9rem;
  }
}

.search-box {
  --lg-blur: 12px;
}

.sort-box {
  width: min(100%, 280px);
}

.sort-trigger {
  width: 100%;
  min-width: 0;
  --lg-blur: 12px;
}

.sort-menu {
  width: 100%;
  min-width: 0;
  --lg-blur: 12px;
}

.browse-body {
  grid-template-columns: 330px minmax(0, 1fr);
  gap: 36px;
}

.filter-group {
  --lg-blur: 12px;
  padding: 26px;
}

@media (max-width: 860px) {
  .browse-body {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 620px) {
  .browse-toolbar {
    flex-direction: column;
  }

  .search-box,
  .sort-box {
    width: 100%;
    min-width: 0;
  }

  .subscription-banner__actions {
    width: 100%;
    align-items: stretch;
  }
}

</style>
