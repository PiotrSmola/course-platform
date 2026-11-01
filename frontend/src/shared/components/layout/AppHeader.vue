<template>
  <header class="site-header" :class="{ scrolled: isScrolled }">
    <div class="header-inner glass">
      <router-link class="logo" :to="{ name: 'Home' }" aria-label="CoursePlatform — strona główna">
        <span class="logo-word">Course<em>Platform</em></span>
      </router-link>

      <nav
        ref="navEl"
        class="main-nav"
        aria-label="Nawigacja główna"
        @pointerleave="hideBlob"
      >
        <ul ref="listEl">
          <span ref="blobEl" class="nav-blob" aria-hidden="true"></span>
          <li>
            <router-link :to="{ name: 'Home' }" @pointerenter="moveBlob($event)">Strona główna</router-link>
          </li>
          <li class="dropdown-root" @pointerenter="openCatalog" @pointerleave="closeCatalog">
            <router-link class="nav-link" :class="{ 'force-active': isCatalogRoute }" :to="{ name: 'Courses' }" @pointerenter="moveBlob($event)">Katalog</router-link>
            <div v-show="catalogOpen" class="mega-dropdown" @pointerenter="onCatalogEnter" @pointerleave="closeCatalog">
              <div class="mega-dropdown-inner">
                <div class="mega-sections">
                  <span class="mega-label">Przeglądaj</span>
                  <span ref="megaSectionBlobEl" class="mega-section-blob" aria-hidden="true"></span>
                  <router-link
                    class="mega-section"
                    :class="{ active: activeSection === 'categories' }"
                    :to="{ name: 'CategoriesList' }"
                    @pointerenter="activeSection = 'categories'; moveMegaSectionBlob($event)"
                    @pointerleave="hideMegaSectionBlob"
                    @click="catalogOpen = false"
                  >
                    <span>Kategorie</span>
                  </router-link>
                  <router-link
                    class="mega-section"
                    :class="{ active: activeSection === 'technologies' }"
                    :to="{ name: 'TechnologiesList' }"
                    @pointerenter="activeSection = 'technologies'; moveMegaSectionBlob($event)"
                    @pointerleave="hideMegaSectionBlob"
                    @click="catalogOpen = false"
                  >
                    <span>Technologie</span>
                  </router-link>
                </div>
                <div class="mega-items">
                  <span class="mega-label">{{ activeSection === 'technologies' ? 'Technologie' : 'Kategorie' }}</span>
                  <span ref="megaBlobEl" class="mega-blob" aria-hidden="true"></span>
                  <div class="mega-items-grid">
                    <router-link
                      v-for="item in activeItems"
                      :key="item.id"
                      class="mega-item"
                      :to="itemLink(item)"
                      @click="catalogOpen = false"
                      @pointerenter="moveMegaBlob($event)"
                      @pointerleave="hideMegaBlob"
                    >
                      {{ item.name }}
                    </router-link>
                  </div>
                </div>
              </div>
            </div>
          </li>
          <li>
            <router-link :to="{ name: 'PathsList' }" :class="{ 'force-active': isPathsRoute }" @pointerenter="moveBlob($event)">Ścieżki</router-link>
          </li>
          <li>
            <router-link :to="{ name: 'Business' }" :class="{ 'force-active': isBusinessRoute }" @pointerenter="moveBlob($event)">Dla firm</router-link>
          </li>
          <li>
            <router-link :to="{ name: 'Pricing' }" @pointerenter="moveBlob($event)">Cennik</router-link>
          </li>
          <li>
            <router-link :to="{ name: 'Help' }" @pointerenter="moveBlob($event)">Pomoc</router-link>
          </li>
          <li v-if="authStore.isInstructor">
            <router-link :to="{ name: 'InstructorDashboard' }" @pointerenter="moveBlob($event)">Panel instruktora</router-link>
          </li>
          <li v-if="authStore.isAdmin">
            <router-link :to="{ name: 'AdminDashboard' }" @pointerenter="moveBlob($event)">Panel admina</router-link>
          </li>
          <li v-if="authStore.isAuthenticated">
            <router-link :to="{ name: 'MyCourses' }" @pointerenter="moveBlob($event)">Moje kursy</router-link>
          </li>
        </ul>
      </nav>

      <div class="search-root">
        <button class="search-trigger" :class="{ open: searchOpen }" @click="toggleSearch" aria-label="Szukaj">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>
        </button>
        <div v-show="searchOpen" class="search-dropdown" ref="searchDropdownRef">
          <div class="search-dropdown-inner">
            <div class="search-input-wrap">
              <input
                ref="searchInputRef"
                v-model="searchQuery"
                type="text"
                placeholder="Szukaj kursów, kategorii, technologii..."
                class="search-input"
                @keydown.escape="closeSearch"
              />
              <svg class="search-input-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>
            </div>
            <div class="search-results">
              <div v-if="!debouncedQuery" class="search-placeholder">
                Wpisz nazwę kursu, kategorii lub technologii...
              </div>
              <div v-else-if="isSearchLoading" class="search-loading">
                <div class="spinner-sm" />
                <span>Szukam...</span>
              </div>
              <div v-else-if="searchResults?.items.length === 0" class="search-empty">
                Nie znaleziono kursów dla „{{ debouncedQuery }}”
              </div>
              <div v-else class="search-results-list">
                <router-link
                  v-for="course in searchResults?.items"
                  :key="course.id"
                  class="search-result-item"
                  :to="{ name: 'CourseDetails', params: { id: course.id } }"
                  @click="searchOpen = false"
                >
                  <div class="search-result-info">
                    <span class="search-result-title">{{ course.title }}</span>
                    <span class="search-result-meta">{{ course.instructorName }} · {{ course.averageRating.toFixed(1) }} ★</span>
                  </div>
                  <span v-if="course.matchedBy.length" class="search-result-matched-by">
                    {{ formatMatchedBy(course.matchedBy) }}
                  </span>
                </router-link>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="nav-actions">
        <template v-if="authStore.isInstructor || authStore.isAdmin">
          <router-link
            v-if="authStore.isInstructor"
            class="btn btn-ghost mobile-panel-link"
            :to="{ name: 'InstructorDashboard' }"
          >
            Instruktor
          </router-link>
          <router-link
            v-if="authStore.isAdmin"
            class="btn btn-ghost mobile-panel-link"
            :to="{ name: 'AdminDashboard' }"
          >
            Admin
          </router-link>
        </template>
        <template v-if="!authStore.isAuthenticated">
          <router-link class="btn btn-ghost btn-pill" :to="{ name: 'Login' }">Zaloguj</router-link>
          <router-link class="btn btn-primary btn-pill" :to="{ name: 'Register' }">Rejestracja</router-link>
        </template>
        <template v-else>
          <router-link class="avatar-btn" :to="{ name: 'Profile' }" aria-label="Konto">
            <span>{{ authStore.user?.firstName.charAt(0) }}</span>
          </router-link>
          <button class="btn btn-ghost btn-pill" @click="logout">Wyloguj</button>
        </template>
      </div>
      <button
        type="button"
        class="menu-toggle"
        :aria-expanded="mobileMenuOpen"
        aria-controls="mobile-navigation"
        aria-label="Otw&#243;rz menu"
        @click="mobileMenuOpen = !mobileMenuOpen"
      >
        <span /><span /><span />
      </button>
    </div>
    <MobileNavigationPanel v-model="mobileMenuOpen" @open-search="openMobileSearch" />
    <MobileSearchOverlay v-model="mobileSearchOpen" />
  </header>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuth } from '@/features/auth/composables/useAuth'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCategories, useTechnologies, useCourseSearch } from '@/features/courses/composables/useCourseMeta'
import MobileNavigationPanel from '@/shared/components/layout/MobileNavigationPanel.vue'
import MobileSearchOverlay from '@/shared/components/layout/MobileSearchOverlay.vue'

const authStore = useAuthStore()
const { logout } = useAuth()
const route = useRoute()

const isScrolled = ref(false)
const listEl = ref<HTMLElement | null>(null)
const blobEl = ref<HTMLElement | null>(null)
const navEl = ref<HTMLElement | null>(null)
const megaBlobEl = ref<HTMLElement | null>(null)
const megaSectionBlobEl = ref<HTMLElement | null>(null)

const catalogOpen = ref(false)
const activeSection = ref<'categories' | 'technologies'>('categories')
let catalogCloseTimer: ReturnType<typeof setTimeout> | null = null

const searchOpen = ref(false)
const searchQuery = ref('')
const debouncedQuery = ref('')
const searchInputRef = ref<HTMLInputElement | null>(null)
const searchDropdownRef = ref<HTMLElement | null>(null)
let searchTimer: ReturnType<typeof setTimeout> | null = null
const mobileMenuOpen = ref(false)
const mobileSearchOpen = ref(false)

const isCatalogRoute = computed(() => {
  const path = route.path
  return path === '/courses' || path.startsWith('/categories/') || path.startsWith('/technologies/') || /^\/courses\/[^/]+$/.test(path)
})

const isPathsRoute = computed(() => route.path === '/paths' || route.path.startsWith('/paths/'))
const isBusinessRoute = computed(() => route.path === '/business')

const { data: categories } = useCategories()
const { data: technologies } = useTechnologies()

const activeItems = computed(() => {
  if (activeSection.value === 'technologies') return technologies.value
  return categories.value
})

const { isLoading: isSearchLoading, data: searchResults } = useCourseSearch(debouncedQuery)

watch(searchQuery, (val) => {
  if (searchTimer) clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    debouncedQuery.value = val.trim()
  }, 500)
})

watch(searchOpen, async (open) => {
  if (open) {
    await nextTick()
    searchInputRef.value?.focus()
  }
})

function openCatalog() {
  if (catalogCloseTimer) {
    clearTimeout(catalogCloseTimer)
    catalogCloseTimer = null
  }
  catalogOpen.value = true
}

function closeCatalog() {
  catalogCloseTimer = setTimeout(() => {
    catalogOpen.value = false
  }, 150)
}

function onCatalogEnter() {
  if (catalogCloseTimer) {
    clearTimeout(catalogCloseTimer)
    catalogCloseTimer = null
  }
}

function itemLink(item: { slug: string }) {
  const isTech = activeSection.value === 'technologies'
  return {
    name: isTech ? 'TechnologyDetails' : 'CategoryDetails',
    params: { slug: item.slug }
  }
}

const matchedByLabels: Record<string, string> = {
  title: 'Tytuł',
  instructor: 'Autor',
  tags: 'Tagi',
  description: 'Opis'
}

function formatMatchedBy(matchedBy: string[]): string {
  return matchedBy
    .map((key) => matchedByLabels[key] || key)
    .join(' · ')
}

function toggleSearch() {
  if (window.matchMedia('(max-width: 1120px)').matches) {
    mobileMenuOpen.value = false
    mobileSearchOpen.value = true
    return
  }
  searchOpen.value = !searchOpen.value
}

function openMobileSearch() {
  mobileMenuOpen.value = false
  mobileSearchOpen.value = true
}

function closeSearch() {
  searchOpen.value = false
  searchQuery.value = ''
  debouncedQuery.value = ''
}

function handleClickOutside(event: MouseEvent) {
  if (searchOpen.value && searchDropdownRef.value && !searchDropdownRef.value.contains(event.target as Node)) {
    const trigger = document.querySelector('.search-trigger')
    if (trigger && !trigger.contains(event.target as Node)) {
      closeSearch()
    }
  }
  if (catalogOpen.value) {
    const dropdownRoot = document.querySelector('.dropdown-root')
    if (dropdownRoot && !dropdownRoot.contains(event.target as Node)) {
      catalogOpen.value = false
    }
  }
}

const onScroll = () => {
  isScrolled.value = window.scrollY > 24
}

function moveBlobToElement(el: HTMLElement) {
  if (!blobEl.value || !listEl.value) return
  const li = el.closest('li')
  if (!li) return
  const ulRect = listEl.value.getBoundingClientRect()
  const liRect = li.getBoundingClientRect()
  blobEl.value.style.left = `${liRect.left - ulRect.left}px`
  blobEl.value.style.width = `${liRect.width}px`
  blobEl.value.style.opacity = '1'
}

function moveBlob(e: PointerEvent) {
  const target = e.currentTarget as HTMLElement
  moveBlobToElement(target)
}

function syncBlobToActive() {
  if (!listEl.value) return
  const activeLink = listEl.value.querySelector('.force-active') || listEl.value.querySelector('.router-link-exact-active')
  if (activeLink) {
    moveBlobToElement(activeLink as HTMLElement)
  } else {
    if (blobEl.value) blobEl.value.style.opacity = '0'
  }
}

function hideBlob() {
  syncBlobToActive()
}

function moveMegaBlob(e: PointerEvent) {
  if (!megaBlobEl.value || !listEl.value) return
  const target = e.currentTarget as HTMLElement
  const container = target.closest('.mega-items') as HTMLElement | null
  if (!container) return
  const containerRect = container.getBoundingClientRect()
  const targetRect = target.getBoundingClientRect()
  megaBlobEl.value.style.left = `${targetRect.left - containerRect.left}px`
  megaBlobEl.value.style.top = `${targetRect.top - containerRect.top}px`
  megaBlobEl.value.style.width = `${targetRect.width}px`
  megaBlobEl.value.style.height = `${targetRect.height}px`
  megaBlobEl.value.style.opacity = '1'
}

function hideMegaBlob() {
  if (!megaBlobEl.value) return
  megaBlobEl.value.style.opacity = '0'
}

function moveMegaSectionBlob(e: PointerEvent) {
  if (!megaSectionBlobEl.value) return
  const target = e.currentTarget as HTMLElement
  const container = target.closest('.mega-sections') as HTMLElement | null
  if (!container) return
  const containerRect = container.getBoundingClientRect()
  const targetRect = target.getBoundingClientRect()
  megaSectionBlobEl.value.style.left = `${targetRect.left - containerRect.left}px`
  megaSectionBlobEl.value.style.top = `${targetRect.top - containerRect.top}px`
  megaSectionBlobEl.value.style.width = `${targetRect.width}px`
  megaSectionBlobEl.value.style.height = `${targetRect.height}px`
  megaSectionBlobEl.value.style.opacity = '1'
}

function hideMegaSectionBlob() {
  if (!megaSectionBlobEl.value) return
  megaSectionBlobEl.value.style.opacity = '0'
}

onMounted(async () => {
  window.addEventListener('scroll', onScroll, { passive: true })
  document.addEventListener('mousedown', handleClickOutside)
  onScroll()
  await nextTick()
  syncBlobToActive()
  document.fonts?.ready.then(syncBlobToActive)
})

watch(() => route.path, async () => {
  await nextTick()
  syncBlobToActive()
  catalogOpen.value = false
  searchOpen.value = false
  mobileMenuOpen.value = false
  mobileSearchOpen.value = false
})

onUnmounted(() => {
  window.removeEventListener('scroll', onScroll)
  document.removeEventListener('mousedown', handleClickOutside)
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;
@use "@/assets/styles/abstracts/mixins" as *;

.site-header {
  position: fixed;
  top: 14px;
  left: 0;
  right: 0;
  z-index: 100;

  .header-inner {
    --lg-r: 999px;
    --lg-blur: 1px;
    --lg-sat: 160%;
    --lg-tint: rgba(13, 15, 19, 0.08);
    --lg-glow: 0.08;
    width: min($container-max, 100% - 32px);
    margin-inline: auto;
    height: 74px;
    padding: 0 12px 0 32px;
    display: flex;
    align-items: center;
    gap: 12px;
    transition: box-shadow 0.35s;
  }

  &.scrolled .header-inner {
    --lg-tint: rgba(13, 15, 19, 0.2);
    box-shadow:
      0 18px 44px -12px rgba(0, 0, 0, 0.6),
      0 2px 8px rgba(0, 0, 0, 0.25);
  }
}

.logo {
  display: flex;
  align-items: baseline;
  flex-shrink: 0;
  color: $color-ink;
  text-decoration: none;
  white-space: nowrap;
  text-shadow: 0 1px 8px rgba(0, 0, 0, 0.45);
}

.logo-word {
  font-family: $font-display;
  font-size: 1.5rem;
  font-weight: 600;
  letter-spacing: -0.01em;
  line-height: 1;

  em {
    font-style: normal;
    color: $color-muted;
  }
}

.main-nav {
  margin-inline: auto;

  ul {
    position: relative;
    display: flex;
    gap: 2px;
    padding: 4px;
    align-items: center;
  }

  li {
    position: relative;
    z-index: 1;
  }

  a {
    position: relative;
    z-index: 2;
    display: block;
    padding: 10px 18px;
    border-radius: 999px;
    font-family: $font-family;
    font-size: 1rem;
    font-weight: 500;
    color: rgba($color-ink, 0.82);
    text-shadow: 0 1px 8px rgba(0, 0, 0, 0.45);
    transition: color 0.25s;
    white-space: nowrap;

    &:hover {
      color: $color-ink;
    }

    &.router-link-active {
      color: $color-ink;
    }
  }
}

.nav-blob {
  position: absolute;
  top: 50%;
  height: 42px;
  transform: translateY(-50%);
  border-radius: 999px;
  background: rgba($color-ink, 0.1);
  box-shadow: inset 0 0 0 1px rgba($color-ink, 0.1);
  opacity: 0;
  pointer-events: none;
  transition:
    left 0.45s $ease-jelly,
    width 0.45s $ease-jelly,
    opacity 0.2s;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.avatar-btn {
  display: grid;
  place-items: center;
  width: 46px;
  height: 46px;
  border-radius: 50%;
  border: none;
  background: rgba($color-ink, 0.1);
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-ink;
  font-family: $font-display;
  font-size: 1.12rem;
  font-weight: 600;
  line-height: 1;
  text-decoration: none;
  cursor: pointer;
  transition: background 0.2s, box-shadow 0.2s;

  &:hover {
    background: rgba($color-ink, 0.16);
    box-shadow: inset 0 0 0 1px rgba($color-ink, 0.22);
  }
}

.btn {
  padding: 12px 24px;
  font-size: 0.97rem;
}

.mobile-panel-link {
  display: none;
}

.dropdown-root {
  position: relative;
}

.nav-link {
  position: relative;
  z-index: 2;
  display: block;
  padding: 10px 18px;
  border-radius: 999px;
  font-size: 1rem;
  font-weight: 500;
  color: $color-muted;
  transition: color 0.25s;
  white-space: nowrap;
  cursor: pointer;

  &:hover,
  &.active,
  &.force-active {
    color: $color-ink;
  }
}

.mega-label {
  @include mono-label;
  display: block;
  font-size: 0.62rem;
  color: $color-faint;
}

.mega-dropdown {
  @include liquid-glass;
  --lg-r: #{$radius-card};
  --lg-blur: 14px;
  --lg-sat: 140%;
  --lg-tint: rgba(13, 15, 19, 0.62);
  --lg-glow: 0.05;
  position: absolute;
  top: calc(100% + 14px);
  left: 50%;
  transform: translateX(-50%);
  min-width: 580px;
  padding: 8px;
  z-index: 50;
  box-shadow:
    0 28px 60px -16px rgba(0, 0, 0, 0.65),
    0 4px 14px rgba(0, 0, 0, 0.3);
}

.mega-dropdown-inner {
  display: flex;
  border-radius: 14px;
  overflow: hidden;
}

.mega-sections {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: 2px;
  width: 176px;
  flex-shrink: 0;
  padding: 14px 8px;
  border-right: 1px solid $color-hairline;

  .mega-label {
    padding: 0 10px 10px;
  }
}

.mega-section {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: baseline;
  gap: 10px;
  padding: 10px 12px;
  border-radius: 10px;
  font-family: $font-display;
  font-size: 1.08rem;
  font-weight: 500;
  color: $color-muted;
  cursor: pointer;
  transition: color 0.2s;
  text-decoration: none;

  &:hover,
  &.active {
    color: $color-ink;
  }
}

.mega-section-blob,
.mega-blob {
  position: absolute;
  top: 0;
  left: 0;
  border-radius: 10px;
  background: rgba($color-ink, 0.08);
  box-shadow: inset 0 0 0 1px rgba($color-ink, 0.08);
  opacity: 0;
  pointer-events: none;
  transition:
    left 0.45s $ease-jelly,
    width 0.45s $ease-jelly,
    top 0.45s $ease-jelly,
    height 0.45s $ease-jelly,
    opacity 0.2s;
}

.mega-items {
  flex: 1;
  padding: 14px 16px 16px;
  min-height: 200px;
  position: relative;

  > .mega-label {
    padding: 0 12px 10px;
    margin-bottom: 6px;
    border-bottom: 1px dashed $color-hairline;
  }
}

.mega-items-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 2px 12px;
}

.mega-item {
  position: relative;
  z-index: 1;
  display: block;
  padding: 9px 12px;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 500;
  color: $color-muted;
  transition: color 0.2s;
  text-decoration: none;
  white-space: nowrap;

  &:hover {
    color: $color-ink;
  }
}

.search-root {
  position: relative;
  display: flex;
  align-items: center;
}

.search-trigger {
  width: 46px;
  height: 46px;
  display: flex;
  align-items: center;
  justify-content: center;
  appearance: none;
  padding: 0;
  border: none;
  border-radius: 50%;
  background: rgba($color-ink, 0.05);
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-muted;
  cursor: pointer;
  transition: background 0.25s, color 0.25s, box-shadow 0.25s;

  &:hover,
  &.open {
    background: rgba($color-ink, 0.1);
    box-shadow: inset 0 0 0 1px rgba($color-ink, 0.2);
    color: $color-ink;
  }
}

.search-dropdown {
  @include liquid-glass;
  --lg-r: #{$radius-card};
  --lg-blur: 14px;
  --lg-sat: 140%;
  --lg-tint: rgba(13, 15, 19, 0.62);
  --lg-glow: 0.05;
  position: absolute;
  top: calc(100% + 20px);
  right: 0;
  width: 420px;
  padding: 10px;
  z-index: 50;
  box-shadow:
    0 28px 60px -16px rgba(0, 0, 0, 0.65),
    0 4px 14px rgba(0, 0, 0, 0.3);

  @media (max-width: 520px) {
    width: 320px;
    right: -80px;
  }
}

.search-dropdown-inner {
  border-radius: 14px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.search-input-wrap {
  position: relative;
  display: flex;
  align-items: center;
}

.search-input {
  width: 100%;
  height: 46px;
  padding: 0 44px 0 16px;
  border-radius: $radius-md;
  border: none;
  background: rgba(13, 15, 19, 0.45);
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-ink;
  font: inherit;
  font-size: 0.94rem;
  outline: none;
  transition: box-shadow 0.2s;

  &::placeholder {
    color: $color-faint;
  }

  &:focus {
    box-shadow: inset 0 0 0 1px $color-hairline, 0 0 0 2px rgba($color-accent, 0.6);
  }
}

.search-input-icon {
  position: absolute;
  right: 15px;
  color: $color-faint;
  pointer-events: none;
}

.search-results {
  max-height: 320px;
  overflow-y: auto;
}

.search-placeholder,
.search-empty {
  padding: 14px 16px 16px;
  font-size: 0.85rem;
  color: $color-muted;
}

.search-loading {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 14px 16px 16px;
  font-size: 0.85rem;
  color: $color-muted;
}

.spinner-sm {
  width: 16px;
  height: 16px;
  border: 2px solid rgba($color-ink, 0.12);
  border-top-color: $color-accent;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.search-results-list {
  display: flex;
  flex-direction: column;
}

.search-result-item {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 11px 12px;
  border-radius: 10px;
  text-decoration: none;
  transition: background 0.2s;

  & + & {
    box-shadow: 0 -1px 0 $color-hairline;
  }

  &:hover {
    background: rgba($color-ink, 0.06);
  }
}

.search-result-info {
  display: flex;
  flex-direction: column;
  gap: 3px;
  min-width: 0;
}

.search-result-title {
  font-family: $font-display;
  font-size: 0.95rem;
  font-weight: 500;
  line-height: 1.25;
  color: $color-ink;
}

.search-result-meta {
  font-family: $font-mono;
  font-size: 0.7rem;
  color: $color-faint;
}

.search-result-matched-by {
  @include tag;
  flex-shrink: 0;
}

.menu-toggle {
  display: none;
  width: 40px;
  height: 40px;
  flex-shrink: 0;
  place-content: center;
  gap: 5px;
  padding: 0;
  border: none;
  border-radius: 50%;
  background: rgba($color-ink, 0.05);
  box-shadow: inset 0 0 0 1px $color-hairline;
  color: $color-ink;
  cursor: pointer;

  span {
    display: block;
    width: 16px;
    height: 1.5px;
    margin-inline: auto;
    border-radius: 2px;
    background: currentColor;
  }
}

@media (max-width: 1320px) {
  .site-header .header-inner {
    padding-left: 22px;
    gap: 8px;
  }

  .main-nav a,
  .nav-link {
    padding: 10px 12px;
    font-size: 0.94rem;
  }

  .btn {
    padding: 11px 18px;
  }
}

@media (max-width: 1120px) {
  .site-header {
    top: 10px;
  }

  .site-header .header-inner {
    width: min($container-max, 100% - 24px);
    height: 64px;
    padding: 0 10px 0 22px;
    gap: 8px;
  }

  .main-nav,
  .nav-actions {
    display: none;
  }

  .logo {
    margin-right: auto;
  }

  .menu-toggle {
    display: grid;
  }

  .search-dropdown {
    display: none !important;
  }
}

@media (max-width: 420px) {
  .site-header .header-inner {
    width: calc(100% - 16px);
  }

  .logo-word {
    font-size: 1.12rem;
  }
}
</style>
