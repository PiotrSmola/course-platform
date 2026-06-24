<template>
  <header class="header">
    <div class="logo">
      <router-link :to="{ name: 'Home' }">CoursePlatform</router-link>
    </div>
    <nav>
      <router-link :to="{ name: 'Courses' }">Courses</router-link>
      <router-link v-if="authStore.isAuthenticated" :to="{ name: 'MyCourses' }">My Courses</router-link>
      <router-link v-if="authStore.isInstructor" :to="{ name: 'InstructorDashboard' }">Instructor</router-link>
      <template v-if="!authStore.isAuthenticated">
        <router-link :to="{ name: 'Login' }">Login</router-link>
        <router-link :to="{ name: 'Register' }">Register</router-link>
      </template>
      <template v-else>
        <span>{{ authStore.user?.firstName }}</span>
        <button @click="logout">Logout</button>
      </template>
    </nav>
  </header>
</template>

<script setup lang="ts">
import { useAuth } from '@/features/auth/composables/useAuth'
import { useAuthStore } from '@/features/auth/stores/auth.store'

const authStore = useAuthStore()
const { logout } = useAuth()
</script>

<style lang="scss" scoped>
.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 2rem;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(10px);

  nav {
    display: flex;
    gap: 1rem;
    align-items: center;
  }

  a {
    color: white;
    text-decoration: none;
  }

  button {
    background: transparent;
    border: 1px solid white;
    color: white;
    padding: 0.5rem 1rem;
    cursor: pointer;
  }
}
</style>