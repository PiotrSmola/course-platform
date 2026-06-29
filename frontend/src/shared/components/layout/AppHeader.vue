<template>
  <header class="site-header" :class="{ scrolled: isScrolled }">
    <div class="header-inner glass">
      <router-link class="logo" :to="{ name: 'Home' }" aria-label="CoursePlatform — strona główna">
        <svg class="logo-mark" viewBox="0 0 64 64" aria-hidden="true">
          <defs>
            <linearGradient id="logo-g" x1="0" y1="0" x2="1" y2="1">
              <stop offset="0" stop-color="#f59e0b"/>
              <stop offset="1" stop-color="#ec4899"/>
            </linearGradient>
          </defs>
          <path d="M12 12h40v4H12zm0 8h28v4H12zm0 8h36v4H12zm0 12h24v4H12zm0 8h32v4H12z" fill="url(#logo-g)"/>
          <circle cx="50" cy="20" r="8" fill="url(#logo-g)" opacity="0.4"/>
        </svg>
        <span>CoursePlatform</span>
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
          <li v-if="authStore.isInstructor">
            <router-link :to="{ name: 'InstructorDashboard' }" @pointerenter="moveBlob($event)">Panel</router-link>
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
                  <span class="search-result-title">{{ course.title }}</span>
                  <span class="search-result-meta">{{ course.instructorName }} · {{ course.averageRating.toFixed(1) }} ★</span>
                </router-link>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="nav-actions">
        <template v-if="!authStore.isAuthenticated">
          <router-link class="btn btn-ghost" :to="{ name: 'Login' }">Zaloguj</router-link>
          <router-link class="btn btn-primary" :to="{ name: 'Register' }">Rejestracja</router-link>
        </template>
        <template v-else>
          <router-link class="avatar-btn" :to="{ name: 'Profile' }" aria-label="Konto">
            <span>{{ authStore.user?.firstName.charAt(0) }}</span>
          </router-link>
          <button class="btn btn-ghost" @click="logout">Wyloguj</button>
        </template>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuth } from '@/features/auth/composables/useAuth'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useCategories, useTechnologies, useCourseSearch } from '@/features/courses/composables/useCourseMeta'

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

function toggleSearch() {
  searchOpen.value = !searchOpen.value
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
  const activeLink = listEl.value.querySelector('.router-link-active') || listEl.value.querySelector('.force-active')
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
})

watch(() => route.path, async () => {
  await nextTick()
  syncBlobToActive()
  catalogOpen.value = false
  searchOpen.value = false
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
    --lg-blur: 1.5px;
    width: min($container-max, 100% - 32px);
    margin-inline: auto;
    height: $header-height;
    padding: 0 12px 0 20px;
    display: flex;
    align-items: center;
    gap: 12px;
  }

  &.scrolled .header-inner {
    --lg-tint: rgba(10, 14, 34, 0.32);
    box-shadow: 0 18px 50px rgba(3, 6, 24, 0.55), 0 2px 8px rgba(3, 6, 24, 0.3);
  }
}

.logo {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1.3rem;
  font-weight: 700;
  font-family: $font-display;
  letter-spacing: -0.02em;
  white-space: nowrap;

  .logo-mark {
    width: 30px;
    height: 30px;
    filter: drop-shadow(0 4px 12px rgba(245, 158, 11, 0.55));
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
    padding: 9px 14px;
    border-radius: 999px;
    font-size: 0.88rem;
    font-weight: 500;
    color: $color-muted;
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
  height: 36px;
  transform: translateY(-50%);
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.08);
  box-shadow:
    inset 1.5px 1.5px 1px -1px rgba(255, 255, 255, 0.6),
    inset -1.5px -1.5px 1px -1px rgba(255, 255, 255, 0.25),
    inset 0 0 0 1px rgba(255, 255, 255, 0.06);
  opacity: 0;
  pointer-events: none;
  transition:
    left 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    width 0.45s cubic-bezier(0.3, 1.55, 0.35, 1),
    opacity 0.2s;
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.avatar-btn {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  border: none;
  background: $color-grad;
  color: #0a0e17;
  font-weight: 700;
  font-size: 0.9rem;
  cursor: pointer;
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.35);
  transition: transform 0.2s;

  &:hover {
    transform: scale(1.05);
  }
}

.btn {
  padding: 9px 18px;
  font-size: 0.85rem;
}

@media (max-width: 880px) {
  .main-nav {
    display: none;
  }

  .logo span {
    display: none;
  }
}

.dropdown-root {
  position: relative;
}

.nav-link {
  position: relative;
  z-index: 2;
  display: block;
  padding: 9px 14px;
  border-radius: 999px;
  font-size: 0.88rem;
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

.mega-dropdown {
  @include liquid-glass;
  --lg-r: 20px;
  --lg-blur: 0px;
  --lg-tint: rgba(17, 24, 39, 0.55);
  position: absolute;
  top: calc(100% + 8px);
  left: 50%;
  transform: translateX(-50%);
  min-width: 560px;
  padding: 8px;
  z-index: 50;
  box-shadow:
    0 24px 60px rgba(3, 6, 24, 0.55),
    0 4px 14px rgba(3, 6, 24, 0.35),
    inset 0 1px 1px rgba(255, 255, 255, 0.18),
    inset 0 0 0 1px rgba(255, 255, 255, 0.1);
}

.mega-dropdown-inner {
  display: flex;
  border-radius: 14px;
  overflow: hidden;
}

.mega-sections {
  width: 160px;
  flex-shrink: 0;
  border-right: 1px solid rgba(255, 255, 255, 0.08);
  padding: 8px 0;
  position: relative;
}

.mega-section {
  position: relative;
  z-index: 1;
  display: block;
  padding: 10px 16px;
  font-size: 0.9rem;
  font-weight: 500;
  color: $color-muted;
  cursor: pointer;
  transition: color 0.2s;
  border-radius: 10px;
  margin: 0 6px;
  text-decoration: none;

  &:hover,
  &.active {
    color: $color-ink;
  }

  &.active {
    font-weight: 600;
  }
}

.mega-section-blob {
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

.mega-items {
  flex: 1;
  padding: 16px 20px;
  min-height: 200px;
  position: relative;
}

.mega-items-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px 16px;
}

.mega-item {
  position: relative;
  z-index: 1;
  display: block;
  padding: 8px 12px;
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 500;
  color: $color-muted;
  transition: color 0.2s;
  text-decoration: none;
  white-space: nowrap;

  &:hover {
    color: $color-ink;
  }
}

.mega-blob {
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

.search-root {
  position: relative;
  display: flex;
  align-items: center;
}

.search-trigger {
  @include liquid-glass;
  --lg-r: 999px;
  --lg-blur: 0px;
  --lg-tint: rgba(255, 255, 255, 0.04);
  width: 38px;
  height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  color: $color-muted;
  cursor: pointer;
  transition: --lg-tint 0.35s, color 0.25s, box-shadow 0.35s;

  &:hover {
    --lg-tint: rgba(245, 158, 11, 0.08);
    color: $color-ink;
    box-shadow:
      0 10px 30px rgba(3, 6, 24, 0.4),
      0 2px 8px rgba(3, 6, 24, 0.25),
      0 18px 30px -22px rgba(245, 158, 11, 0.4),
      inset 0 1px 1px rgba(255, 255, 255, 0.35),
      inset 0 0 0 1px rgba(245, 158, 11, 0.3);
  }

  &.open {
    --lg-tint: rgba(245, 158, 11, 0.12);
    color: $color-ink;
  }
}

.search-dropdown {
  @include liquid-glass;
  --lg-r: 20px;
  --lg-blur: 0px;
  --lg-tint: rgba(17, 24, 39, 0.55);
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  width: 400px;
  padding: 8px;
  z-index: 50;
  box-shadow:
    0 24px 60px rgba(3, 6, 24, 0.55),
    0 4px 14px rgba(3, 6, 24, 0.35),
    inset 0 1px 1px rgba(255, 255, 255, 0.18),
    inset 0 0 0 1px rgba(255, 255, 255, 0.1);

  @media (max-width: 520px) {
    width: 320px;
    right: -80px;
  }
}

.search-dropdown-inner {
  border-radius: 14px;
  overflow: hidden;
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
  height: 44px;
  padding: 0 44px 0 16px;
  border-radius: 12px;
  border: none;
  background: rgba(255, 255, 255, 0.04);
  box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.3), inset 0 0 0 1px rgba(255, 255, 255, 0.1);
  color: $color-ink;
  font: inherit;
  font-size: 0.94rem;
  outline: none;
  transition: background 0.3s, box-shadow 0.3s;

  &::placeholder {
    color: $color-faint;
  }

  &:focus {
    background: rgba(255, 255, 255, 0.07);
    box-shadow: inset 0 1px 1px rgba(255, 255, 255, 0.35), inset 0 0 0 1px rgba(167, 139, 250, 0.5), 0 0 0 4px rgba(139, 92, 246, 0.18);
  }
}

.search-input-icon {
  position: absolute;
  right: 14px;
  color: $color-faint;
  pointer-events: none;
}

.search-results {
  max-height: 320px;
  overflow-y: auto;
}

.search-placeholder,
.search-empty {
  padding: 16px;
  text-align: center;
  font-size: 0.85rem;
  color: $color-muted;
}

.search-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 16px;
  font-size: 0.85rem;
  color: $color-muted;
}

.spinner-sm {
  width: 18px;
  height: 18px;
  border: 2px solid rgba(255, 255, 255, 0.1);
  border-top-color: $color-gold;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.search-results-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.search-result-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: 10px 12px;
  border-radius: 10px;
  text-decoration: none;
  transition: background 0.2s;

  &:hover {
    background: rgba(255, 255, 255, 0.06);
  }
}

.search-result-title {
  font-size: 0.9rem;
  font-weight: 600;
  color: $color-ink;
}

.search-result-meta {
  font-size: 0.78rem;
  color: $color-muted;
}
</style>
