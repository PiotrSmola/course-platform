<template>
  <Transition name="mobile-panel">
    <div v-if="modelValue" class="mobile-panel-backdrop" @click.self="close">
      <aside
        ref="panelRef"
        id="mobile-navigation"
        class="mobile-panel glass"
        role="dialog"
        aria-modal="true"
        aria-label="Menu główne"
        tabindex="-1"
        @keydown.escape="close"
      >
        <div class="mobile-panel__top">
          <span>Menu</span>
          <button type="button" class="mobile-panel__close" aria-label="Zamknij menu" @click="close">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
              <path d="M6 6l12 12M18 6 6 18" />
            </svg>
          </button>
        </div>

        <button type="button" class="mobile-panel__search" @click="openSearch">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" aria-hidden="true">
            <circle cx="11" cy="11" r="7" />
            <path d="m20 20-4.2-4.2" />
          </svg>
          <span>Szukaj kursów, kategorii i technologii</span>
        </button>

        <nav class="mobile-panel__nav" aria-label="Nawigacja mobilna">
          <router-link :class="{ active: isHomeRoute }" :to="{ name: 'Home' }" @click="close">Strona główna</router-link>
          <router-link :class="{ active: isCatalogRoute }" :to="{ name: 'Courses' }" @click="close">Katalog</router-link>
          <router-link :class="{ active: isPathsRoute }" :to="{ name: 'PathsList' }" @click="close">Ścieżki</router-link>
          <router-link :class="{ active: isBusinessRoute }" :to="{ name: 'Business' }" @click="close">Dla firm</router-link>
          <router-link v-if="authStore.isAuthenticated" :class="{ active: route.path === '/my-courses' }" :to="{ name: 'MyCourses' }" @click="close">Moje kursy</router-link>
          <router-link v-if="authStore.isInstructor" :class="{ active: route.path === '/instructor' }" :to="{ name: 'InstructorDashboard' }" @click="close">Panel instruktora</router-link>
          <router-link v-if="authStore.isAdmin" :class="{ active: route.path === '/admin' }" :to="{ name: 'AdminDashboard' }" @click="close">Panel administratora</router-link>
        </nav>

        <div class="mobile-panel__actions">
          <template v-if="!authStore.isAuthenticated">
            <router-link class="btn btn-ghost" :to="{ name: 'Login' }" @click="close">Zaloguj</router-link>
            <router-link class="btn btn-primary" :to="{ name: 'Register' }" @click="close">Załóż konto</router-link>
          </template>
          <template v-else>
            <router-link class="mobile-panel__account" :to="{ name: 'Profile' }" @click="close">
              <span class="mobile-panel__avatar">{{ initial }}</span>
              <span>
                <strong>{{ fullName }}</strong>
                <small>Ustawienia konta</small>
              </span>
            </router-link>
            <button type="button" class="btn btn-ghost" @click="handleLogout">Wyloguj</button>
          </template>
        </div>
      </aside>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { useAuth } from '@/features/auth/composables/useAuth'
import { useAuthStore } from '@/features/auth/stores/auth.store'

const props = defineProps<{
  modelValue: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  openSearch: []
}>()

const route = useRoute()
const authStore = useAuthStore()
const { logout } = useAuth()
const panelRef = ref<HTMLElement | null>(null)

const isHomeRoute = computed(() => route.path === '/')
const isCatalogRoute = computed(() => {
  const path = route.path
  return path === '/courses' || path.startsWith('/categories/') || path.startsWith('/technologies/') || /^\/courses\/[^/]+$/.test(path)
})
const isPathsRoute = computed(() => route.path === '/paths' || route.path.startsWith('/paths/'))
const isBusinessRoute = computed(() => route.path === '/business')
const initial = computed(() => authStore.user?.firstName?.charAt(0).toUpperCase() || 'U')
const fullName = computed(() => [authStore.user?.firstName, authStore.user?.lastName].filter(Boolean).join(' ') || 'Twoje konto')

function close() {
  emit('update:modelValue', false)
}

function openSearch() {
  close()
  emit('openSearch')
}

async function handleLogout() {
  close()
  await logout()
}

watch(() => props.modelValue, async (isOpen) => {
  document.body.classList.toggle('is-overlay-open', isOpen)
  if (isOpen) {
    await nextTick()
    panelRef.value?.focus()
  }
})

watch(() => route.path, close)

onBeforeUnmount(() => {
  document.body.classList.remove('is-overlay-open')
})
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.mobile-panel-backdrop {
  position: fixed;
  inset: 0;
  z-index: 220;
  display: flex;
  justify-content: flex-end;
  background: rgba(3, 6, 18, 0.62);
  -webkit-backdrop-filter: blur(8px);
  backdrop-filter: blur(8px);
}

.mobile-panel {
  --lg-r: 28px 0 0 28px;
  --lg-blur: 4px;
  display: flex;
  width: min(390px, calc(100vw - 36px));
  min-height: 100%;
  flex-direction: column;
  padding: 18px;
  outline: none;
}

.mobile-panel__top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 4px 16px 8px;
  color: $color-muted;
  font-family: $font-display;
  font-size: 0.88rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.mobile-panel__close {
  display: grid;
  width: 40px;
  height: 40px;
  place-items: center;
  border: 0;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.07);
  color: $color-ink;
  cursor: pointer;

  svg {
    width: 20px;
    height: 20px;
  }
}

.mobile-panel__search {
  display: flex;
  align-items: center;
  gap: 11px;
  width: 100%;
  padding: 14px;
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.05);
  color: $color-muted;
  font: inherit;
  font-size: 0.9rem;
  text-align: left;
  cursor: pointer;

  svg {
    width: 19px;
    height: 19px;
    flex-shrink: 0;
    color: $color-gold;
  }
}

.mobile-panel__nav {
  display: grid;
  gap: 4px;
  margin: 20px 0;

  a {
    padding: 13px 14px;
    border-radius: 14px;
    color: $color-muted;
    font-size: 1rem;
    font-weight: 600;
    text-decoration: none;
    transition: background 160ms ease, color 160ms ease;

    &:hover,
    &.active {
      background: rgba(255, 255, 255, 0.08);
      color: $color-ink;
    }

    &.active {
      box-shadow: inset 0 0 0 1px rgba(245, 158, 11, 0.18);
    }
  }
}

.mobile-panel__actions {
  display: grid;
  gap: 10px;
  margin-top: auto;
  padding-top: 18px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);

  .btn {
    width: 100%;
  }
}

.mobile-panel__account {
  display: flex;
  align-items: center;
  gap: 11px;
  padding: 10px;
  color: $color-ink;
  text-decoration: none;

  > span:last-child {
    display: grid;
    gap: 2px;
  }

  strong {
    font-size: 0.9rem;
  }

  small {
    color: $color-faint;
    font-size: 0.76rem;
  }
}

.mobile-panel__avatar {
  display: grid;
  width: 40px;
  height: 40px;
  place-items: center;
  border-radius: 50%;
  background: $color-grad;
  color: #111827;
  font-family: $font-display;
  font-weight: 800;
}

.mobile-panel-enter-active,
.mobile-panel-leave-active {
  transition: opacity 180ms ease;

  .mobile-panel {
    transition: transform 180ms ease;
  }
}

.mobile-panel-enter-from,
.mobile-panel-leave-to {
  opacity: 0;

  .mobile-panel {
    transform: translateX(100%);
  }
}
</style>
