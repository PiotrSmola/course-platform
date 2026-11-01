<template>
  <aside class="sidebar">
    <nav>
      <router-link :to="{ name: 'MyCourses' }">My Courses</router-link>
      <router-link :to="{ name: 'Wishlist' }">Wishlist</router-link>
      <router-link v-if="authStore.isInstructor" :to="{ name: 'InstructorDashboard' }">Instructor</router-link>
      <router-link v-if="authStore.isAdmin" :to="{ name: 'AdminDashboard' }">Admin</router-link>
      <router-link v-if="authStore.isInstructor" :to="{ name: 'NewCourse' }">New Course</router-link>
    </nav>
  </aside>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/features/auth/stores/auth.store'

const authStore = useAuthStore()
</script>

<style lang="scss" scoped>
@use "@/assets/styles/abstracts/variables" as *;

.sidebar {
  width: 240px;
  padding: 2rem 1.5rem;
  border-right: 1px solid $color-hairline;

  nav {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  a {
    padding: 9px 12px;
    border-radius: $radius-sm;
    color: $color-muted;
    font-size: 0.92rem;
    text-decoration: none;
    transition: background 160ms ease, color 160ms ease;

    &:hover,
    &.router-link-active {
      background: rgba($color-ink, 0.08);
      color: $color-ink;
    }
  }
}
</style>
