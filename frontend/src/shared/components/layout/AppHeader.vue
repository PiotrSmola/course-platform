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
            <span class="nav-link" :class="{ active: catalogOpen }">Katalog</span>
            <div v-show="catalogOpen" class="mega-dropdown" @pointerenter="onCatalogEnter" @pointerleave="closeCatalog">
              <div class="mega-dropdown-inner">
                <div class="mega-sections">
                  <div
                    class="mega-section"
                    :class="{ active: activeSection === 'categories' }"
                    @pointerenter="activeSection = 'categories'"
                  >
                    <span>Kategorie</span>
                  </div>
                  <div
                    class="mega-section"
                    :class="{ active: activeSection === 'technologies' }"
                    @pointerenter="activeSection = 'technologies'"
                  >
                    <span>Technologie</span>
                  </div>
                </div>
                <div class="mega-items">
                  <div class="mega-items-grid">
                    <router-link
                      v-for="item in activeItems"
                      :key="item.id"
                      class="mega-item"
                      :to="itemLink(item)"
                      @click="catalogOpen = false"
                    >
                      {{ item.name }}
                    </router-link>
                  </div>
                </div>
              </div>
            </div>
          </li>
          <li>
            <a href="#" @pointerenter="moveBlob($event)">Ścieżki</a>
          </li>
          <li>
            <a href="#" @pointerenter="moveBlob($event)">Dla firm</a>
          </li>
          <li v-if="authStore.isInstructor">
            <router-link :to="{ name: 'InstructorDashboard' }" @pointerenter="moveBlob($event)">Panel</router-link>
          </li>
          <li v-if="authStore.isAuthenticated">
            <router-link :to="{ name: 'MyCourses' }" @pointerenter="moveBlob($event)">Moje kursy</router-link>
          </li>
        </ul>
      </nav>

      <div class="nav-actions">
        <template v-if="!authStore.isAuthenticated">
          <router-link class="btn btn-ghost" :to="{ name: 'Login' }">Zaloguj</router-link>
          <router-link class="btn btn-primary" :to="{ name: 'Register' }">Rejestracja</router-link>
        </template>
        <template v-else>
          <button class="avatar-btn" aria-label="Konto">
            <span>{{ authStore.user?.firstName.charAt(0) }}</span>
          </button>
          <button class="btn btn-ghost" @click="logout">Wyloguj</button>
        </template>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick, computed } from 'vue'
import { useAuth } from '@/features/auth/composables/useAuth'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { useQuery } from '@tanstack/vue-query'
import { getCategories, getTechnologies } from '@/features/courses/api/courses.api'
import type { CategoryDto, TechnologyDto } from '@/features/courses/api/courses.api'

const authStore = useAuthStore()
const { logout } = useAuth()

const isScrolled = ref(false)
const listEl = ref<HTMLElement | null>(null)
const blobEl = ref<HTMLElement | null>(null)
const navEl = ref<HTMLElement | null>(null)

const catalogOpen = ref(false)
const activeSection = ref<'categories' | 'technologies'>('categories')
let catalogCloseTimer: ReturnType<typeof setTimeout> | null = null

const { data: categories } = useQuery<CategoryDto[]>({
  queryKey: ['categories'],
  queryFn: getCategories,
  initialData: []
})

const { data: technologies } = useQuery<TechnologyDto[]>({
  queryKey: ['technologies'],
  queryFn: getTechnologies,
  initialData: []
})

const activeItems = computed(() => {
  if (activeSection.value === 'technologies') return technologies.value
  return categories.value
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

function itemLink(item: CategoryDto | TechnologyDto) {
  const isTech = activeSection.value === 'technologies'
  return {
    name: 'Courses',
    query: isTech ? { technologyIds: item.id } : { categoryIds: item.id }
  }
}

const onScroll = () => {
  isScrolled.value = window.scrollY > 24
}

function moveBlob(e: PointerEvent) {
  if (!blobEl.value) return
  const target = e.currentTarget as HTMLElement
  const li = target.closest('li')
  if (!li) return
  const ul = li.parentElement
  if (!ul) return
  const ulRect = ul.getBoundingClientRect()
  const liRect = li.getBoundingClientRect()
  blobEl.value.style.left = `${liRect.left - ulRect.left}px`
  blobEl.value.style.width = `${liRect.width}px`
  blobEl.value.style.opacity = '1'
}

function hideBlob() {
  if (!blobEl.value) return
  blobEl.value.style.opacity = '0'
}

onMounted(async () => {
  window.addEventListener('scroll', onScroll, { passive: true })
  onScroll()
  await nextTick()
})

onUnmounted(() => {
  window.removeEventListener('scroll', onScroll)
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
  &.active {
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
}

.mega-section {
  padding: 10px 16px;
  font-size: 0.9rem;
  font-weight: 500;
  color: $color-muted;
  cursor: pointer;
  transition: color 0.2s, background 0.2s;
  border-radius: 10px;
  margin: 0 6px;

  &:hover,
  &.active {
    color: $color-ink;
    background: rgba(255, 255, 255, 0.06);
  }

  &.active {
    font-weight: 600;
  }
}

.mega-items {
  flex: 1;
  padding: 16px 20px;
  min-height: 200px;
}

.mega-items-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px 16px;
}

.mega-item {
  display: block;
  padding: 8px 12px;
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 500;
  color: $color-muted;
  transition: color 0.2s, background 0.2s;
  text-decoration: none;
  white-space: nowrap;

  &:hover {
    color: $color-ink;
    background: rgba(255, 255, 255, 0.06);
  }
}
</style>
