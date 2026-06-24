<template>
  <div class="auth-page">
    <h1>Login</h1>
    <LoginForm />
  </div>
</template>

<script setup lang="ts">
import LoginForm from '@/features/auth/components/LoginForm.vue'
</script>

<style lang="scss" scoped>
.auth-page {
  max-width: 400px;
  margin: 0 auto;
  padding: 2rem;
}
</style>
