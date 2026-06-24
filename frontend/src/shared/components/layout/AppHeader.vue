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

      <nav class="main-nav" aria-label="Nawigacja główna">
        <ul>
          <li><router-link :to="{ name: 'Courses' }">Kursy</router-link></li>
          <li><router-link v-if="authStore.isAuthenticated" :to="{ name: 'MyCourses' }">Moje kursy</router-link></li>
          <li><router-link v-if="authStore.isInstructor" :to="{ name: 'InstructorDashboard' }">Panel</router-link></li>
        </ul>
      </nav>

      <div class="nav-actions">
        <template v-if="!authStore.isAuthenticated">
          <router-link class="btn btn-ghost" :to="{ name: 'Login' }">Zaloguj</router-link>
          <router-link class="btn btn-primary" :to="{ name: 'Register' }">Rejestracja</router-link>
        </template>
        <template v-else>
          <span class="user-name">{{ authStore.user?.firstName }}</span>
          <button class="btn btn-ghost" @click="logout">Wyloguj</button>
        </template>
      </div>
    </div>
  </header>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useAuth } from '@/features/auth/composables/useAuth'
import { useAuthStore } from '@/features/auth/stores/auth.store'

const authStore = useAuthStore()
const { logout } = useAuth()

const isScrolled = ref(false)

const onScroll = () => {
  isScrolled.value = window.scrollY > 24
}

onMounted(() => {
  window.addEventListener('scroll', onScroll, { passive: true })
  onScroll()
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
    gap: 16px;
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
  font-size: 1.35rem;
  font-weight: 700;
  letter-spacing: 0.02em;

  .logo-mark {
    width: 30px;
    height: 30px;
    filter: drop-shadow(0 4px 12px rgba(245, 158, 11, 0.55));
  }
}

.main-nav {
  margin-inline: auto;

  ul {
    display: flex;
    gap: 4px;
  }

  a {
    position: relative;
    z-index: 1;
    display: block;
    padding: 9px 15px;
    border-radius: 999px;
    font-size: 0.94rem;
    color: $color-muted;
    transition: color 0.25s, background 0.25s;

    &:hover {
      color: $color-ink;
    }

    &.router-link-active {
      color: $color-ink;
      background: rgba(255, 255, 255, 0.1);
    }
  }
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.user-name {
  font-size: 0.94rem;
  color: $color-muted;
  padding: 0 8px;
}

.btn {
  padding: 9px 18px;
  font-size: 0.88rem;
}
</style>
