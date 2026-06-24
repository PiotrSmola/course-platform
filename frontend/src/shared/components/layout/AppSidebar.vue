<template>
  <aside class="sidebar">
    <nav>
      <router-link :to="{ name: 'MyCourses' }">My Courses</router-link>
      <router-link v-if="authStore.isInstructor" :to="{ name: 'InstructorDashboard' }">Dashboard</router-link>
      <router-link v-if="authStore.isInstructor" :to="{ name: 'NewCourse' }">New Course</router-link>
    </nav>
  </aside>
</template>

<script setup lang="ts">
import { useAuthStore } from '@/features/auth/stores/auth.store'

const authStore = useAuthStore()
</script>

<style lang="scss" scoped>
.sidebar {
  width: 240px;
  padding: 2rem;
  background: rgba(0, 0, 0, 0.3);
  backdrop-filter: blur(10px);

  nav {
    display: flex;
    flex-direction: column;
    gap: 1rem;
  }

  a {
    color: white;
    text-decoration: none;
  }
}
</style>
